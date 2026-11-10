using System.Security.Claims;
using System.Text;
using AuthService.Data;
using AuthService.Entities;
using AuthService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// JWT CONFIGURATION
// ============================================================

var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtIssuer))
    throw new InvalidOperationException(
        "JWT issuer missing. Configure Jwt:Issuer.");

if (string.IsNullOrWhiteSpace(jwtAudience))
    throw new InvalidOperationException(
        "JWT audience missing. Configure Jwt:Audience.");

if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException(
        "JWT signing key missing. Configure Jwt:Key.");

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException(
        "JWT signing key must contain at least 32 UTF-8 bytes.");

var signingKey = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(jwtKey));

// ============================================================
// CONTROLLERS
// ============================================================

builder.Services.AddControllers();

// ============================================================
// SWAGGER + JWT
// ============================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Cloud Client Management - AuthService API",
        Version = "v1",
        Description = "Authentication and authorization API."
    });

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter the JWT access token."
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
});

// ============================================================
// DATABASE
// ============================================================

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "DefaultConnection is not configured.");
}

builder.Services.AddDbContext<AuthDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

// ============================================================
// IDENTITY
// ============================================================

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequiredLength = 12;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;

        options.User.RequireUniqueEmail = true;

        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
        options.SignIn.RequireConfirmedAccount = false;

        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<IdentityRole>()
    .AddSignInManager()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

// ============================================================
// JWT AUTHENTICATION
// ============================================================

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateLifetime = true,
                RequireExpirationTime = true,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                ClockSkew = TimeSpan.FromSeconds(30),

                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role
            };
    });

// ============================================================
// AUTHORIZATION
// ============================================================

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy =
        new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
});

// ============================================================
// APPLICATION SERVICES
// ============================================================

builder.Services.AddScoped<
    IAuthService,
    global::AuthService.Services.AuthService>();

builder.Services.AddScoped<ITokenService, TokenService>();

// ============================================================
// CORS
// ============================================================

var allowedOrigins = new List<string>
{
    "http://localhost:4200"
};

allowedOrigins.AddRange(
    builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? Array.Empty<string>());

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        policy
            .WithOrigins(
                allowedOrigins
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray())
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ============================================================
// BUILD APPLICATION
// ============================================================

var app = builder.Build();

// ============================================================
// DATABASE MIGRATIONS + IDENTITY SEEDING
// ============================================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // Preserve the existing migration policy.
    // Production migrations should be applied separately.
    if (app.Environment.IsDevelopment())
    {
        var dbContext =
            services.GetRequiredService<AuthDbContext>();

        await dbContext.Database.MigrateAsync();
    }

    // Initialize existing Identity data and roles.
    await IdentitySeeder.SeedAsync(
        services,
        app.Configuration);

    // Bootstrap the first administrator when configured.
    await AdminSeeder.SeedAsync(
        services,
        app.Configuration);
}

// ============================================================
// SWAGGER
// ============================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "AuthService API v1");

        options.RoutePrefix = "swagger";
    });
}
else
{
    app.UseHsts();
}

// ============================================================
// HTTP PIPELINE
// ============================================================

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("AngularClient");

app.UseAuthentication();
app.UseAuthorization();

// ============================================================
// ROOT ENDPOINT
// ============================================================

app.MapGet("/", () => Results.Ok(new
{
    service = "Cloud Client Management - AuthService",
    status = "Running",
    version = "1.0",
    authentication = "JWT Bearer",
    endpoints = new
    {
        login = "POST /api/auth/login",
        register = "POST /api/auth/register",
        currentUser = "GET /api/auth/me",
        changePassword = "POST /api/auth/change-password",
        health = "GET /health"
    }
}))
.AllowAnonymous();

// ============================================================
// HEALTH CHECK
// ============================================================

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "AuthService",
    timestamp = DateTime.UtcNow
}))
.AllowAnonymous();

// ============================================================
// CONTROLLERS
// ============================================================

app.MapControllers();

app.Run();
