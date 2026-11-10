
using StatisticsService.Repositories;
using StatisticsService.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Verify effective downstream URLs
var clientServiceUrl =
    builder.Configuration["Services:ClientService"];

var clientTypeServiceUrl =
    builder.Configuration["Services:ClientTypeService"];

if (!Uri.TryCreate(
        clientServiceUrl,
        UriKind.Absolute,
        out var clientUri) ||
    clientUri.Scheme != Uri.UriSchemeHttp)
{
    throw new InvalidOperationException(
        "Services:ClientService must be a valid HTTP URL.");
}

if (!Uri.TryCreate(
        clientTypeServiceUrl,
        UriKind.Absolute,
        out var clientTypeUri) ||
    clientTypeUri.Scheme != Uri.UriSchemeHttp)
{
    throw new InvalidOperationException(
        "Services:ClientTypeService must be a valid HTTP URL.");
}

Console.WriteLine(
    $"ClientService: {clientServiceUrl}");

Console.WriteLine(
    $"ClientTypeService: {clientTypeServiceUrl}");

// HTTP communication with downstream services
builder.Services.AddHttpClient<
    IStatisticsRepository,
    StatisticsRepository>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(15);
    });

// Business service
builder.Services.AddScoped<
    IStatisticsService,
    StatisticsManagementService>();

// Angular CORS
const string AngularCorsPolicy = "AngularCorsPolicy";

var allowedOrigins = new List<string>
{
    "http://localhost:4200",
    "https://final-angular-ui.blackrock-2676f93e.northeurope.azurecontainerapps.io"
};

allowedOrigins.AddRange(
    builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? Array.Empty<string>());

builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularCorsPolicy, policy =>
    {
        policy
            .WithOrigins(
                allowedOrigins
                    .Where(origin => !string.IsNullOrWhiteSpace(origin))
                    .Select(origin => origin.Trim().TrimEnd('/'))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray())
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AngularPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/api/health", () =>
{
    return Results.Ok(new
    {
        status = "Healthy",
        service = "StatisticsService"
    });
});
app.Run();