
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
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AngularPolicy");
app.UseAuthorization();
app.MapControllers();

app.Run();