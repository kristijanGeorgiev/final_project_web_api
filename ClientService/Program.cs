
using ClientService.Repositories;
using ClientService.Services;
using Microsoft.EntityFrameworkCore;
using ClientService.Data;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// Repositories
builder.Services.AddScoped<IClientRepository, ClientRepository>();

// Services
builder.Services.AddScoped<IClientService, ClientManagementService>();

// CORS - Allow Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularCorsPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Middleware
app.UseRouting();

// Enable Angular CORS policy
app.UseCors("AngularCorsPolicy");

app.UseAuthorization();
app.MapGet("/api/health", () =>
{
    return Results.Ok(new
    {
        status = "Healthy",
        service = "ClientService"
    });
});
app.MapControllers();

app.Run();