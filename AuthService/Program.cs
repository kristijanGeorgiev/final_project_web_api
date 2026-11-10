
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
// CONFIGURATION
// ============================================================

// WebApplication.CreateBuilder automatically loads:
// - appsettings.json
// - appsettings.{Environment}.json
// - User Secrets in Development (when configured)
// - Environment variables
// - Command-line arguments

// ============================================================
// JWT CONFIGURATION
// ============================================================

var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtIssuer))
{
    throw new InvalidOperationException(
        "JWT issuer missing. Configure Jwt:Issuer.");
}

if (string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException(
        "JWT audience missing. Configure Jwt:Audience.");
}

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "JWT signing key missing. Configure Jwt:Key " +
        "using User Secrets or environment variables.");
}

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "JWT signing key must contain at least 32 UTF-8 bytes.");
}

var signingKey = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(jwtKey));

// ============================================================
// CONTROLLERS
// ============================================================

builder.Services.AddControllers();

// ============================================================
// SWAGGER + JWT AUTHORIZATION
// ============================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Cloud Client Management - AuthService API",
        Version = "v1",
        Description =
            "Authentication and authorization API using " +
            "ASP.NET Core Identity and JWT Bearer tokens."
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
            Description =
                "Enter the JWT access token obtained from " +
                "POST /api/auth/login."
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
// SQL SERVER DATABASE
// ============================================================

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ============================================================
// ASP.NET CORE IDENTITY
// ============================================================

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        // Password requirements

        options.Password.RequiredLength = 12;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;

        // User settings

        options.User.RequireUniqueEmail = true;

        // Sign-in requirements

        // This application does not currently implement
        // email or phone confirmation workflows.

        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
        options.SignIn.RequireConfirmedAccount = false;

        // Account lockout

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
        // Preserve JWT claim names without inbound mapping.
        // TokenService must emit matching claim types.

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

                // These must match the claims generated
                // by TokenService.

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

builder.Services.AddScoped<
    ITokenService,
    TokenService>();

// ============================================================
// CORS - ANGULAR FRONTEND
// ============================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
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
    // Apply pending migrations automatically
    // in the Development environment only.
    //
    // Production migrations should be executed
    // as a controlled deployment operation.

    if (app.Environment.IsDevelopment())
    {
        var dbContext = scope.ServiceProvider
            .GetRequiredService<AuthDbContext>();

        await dbContext.Database.MigrateAsync();
    }

    // Ensure Admin/User roles exist and create
    // the initial administrator if configured.
    //
    // Existing administrator passwords are
    // intentionally not overwritten.

    await IdentitySeeder.SeedAsync(
        scope.ServiceProvider,
        app.Configuration);
}

// ============================================================
// SWAGGER MIDDLEWARE
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
// PUBLIC ROOT ENDPOINT
// GET: /
// ============================================================

app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        service = "Cloud Client Management - AuthService",
        status = "Running",
        version = "1.0",
        authentication = "JWT Bearer",
        swagger = "/swagger/index.html",
        endpoints = new
        {
            login = "POST /api/auth/login",
            register = "POST /api/auth/register",
            currentUser = "GET /api/auth/me",
            changePassword = "POST /api/auth/change-password",
            health = "GET /health"
        }
    });
})
.AllowAnonymous();

// ============================================================
// PUBLIC HEALTH CHECK
// GET: /health
// ============================================================

app.MapGet("/health", () =>
{
    return Results.Ok(new
    {
        status = "Healthy",
        service = "AuthService",
        timestamp = DateTime.UtcNow
    });
})
.AllowAnonymous();

// ============================================================
// API CONTROLLERS
// ============================================================

app.MapControllers();

// ============================================================
// START APPLICATION
// ============================================================

app.Run();
