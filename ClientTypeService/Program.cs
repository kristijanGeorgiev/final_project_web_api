
using ClientTypeService.Repositories;
using ClientTypeService.Services;
using ClientTypeService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// Configuration
// --------------------------------------------------

Console.WriteLine("========== CLIENT TYPE SERVICE ==========");
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"Content Root: {builder.Environment.ContentRootPath}");

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ClientTypeService: DefaultConnection is missing.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
// --------------------------------------------------
// Controllers and Swagger
// --------------------------------------------------

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --------------------------------------------------
// Database
// --------------------------------------------------

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        connectionString,
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);

            sqlOptions.CommandTimeout(30);
        });
});

// --------------------------------------------------
// Repositories
// --------------------------------------------------

builder.Services.AddScoped<
    IClientTypeRepository,
    ClientTypeRepository>();

// --------------------------------------------------
// Services
// --------------------------------------------------

builder.Services.AddScoped<
    IClientTypeService,
    ClientTypeManagementService>();

// --------------------------------------------------
// CORS - Angular Frontend
// --------------------------------------------------

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// --------------------------------------------------
// Build Application
// --------------------------------------------------

var app = builder.Build();

// --------------------------------------------------
// HTTP Pipeline
// --------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AngularPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();