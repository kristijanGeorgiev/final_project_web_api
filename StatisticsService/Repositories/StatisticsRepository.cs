using System.Net;
using System.Text.Json;
using StatisticsService.DTOs;

namespace StatisticsService.Repositories
{
    public class StatisticsRepository : IStatisticsRepository
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StatisticsRepository> _logger;

        public StatisticsRepository(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<StatisticsRepository> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<StatisticsDto> GetStatisticsAsync(
            CancellationToken ct = default)
        {
            var clientUrl = GetServiceUrl("ClientService");
            var clientTypeUrl = GetServiceUrl("ClientTypeService");

            _logger.LogInformation(
                "Retrieving statistics from ClientService and ClientTypeService.");

            var totalClients = await GetCountAsync(
                "ClientService",
                clientUrl,
                "/api/clients/count",
                ct);

            var totalClientTypes = await GetCountAsync(
                "ClientTypeService",
                clientTypeUrl,
                "/api/clienttypes/count",
                ct);

            _logger.LogInformation(
                "Statistics retrieved successfully. TotalClients: {TotalClients}, TotalClientTypes: {TotalClientTypes}",
                totalClients,
                totalClientTypes);

            return new StatisticsDto
            {
                TotalClients = totalClients,
                TotalClientTypes = totalClientTypes
            };
        }

        private string GetServiceUrl(string serviceName)
        {
            var url = _configuration[$"Services:{serviceName}"];

            if (string.IsNullOrWhiteSpace(url))
            {
                _logger.LogError(
                    "Configuration missing for service: {ServiceName}",
                    serviceName);

                throw new InvalidOperationException(
                    $"Services:{serviceName} configuration is missing.");
            }

            if (!Uri.TryCreate(
                    url,
                    UriKind.Absolute,
                    out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp &&
                 uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new InvalidOperationException(
                    $"Invalid URL configured for {serviceName}.");
            }

            return url.TrimEnd('/');
        }

        private async Task<int> GetCountAsync(
            string serviceName,
            string baseUrl,
            string endpoint,
            CancellationToken ct)
        {
            var requestUrl = $"{baseUrl}{endpoint}";

            try
            {
                _logger.LogInformation(
                    "Calling {ServiceName}: {RequestUrl}",
                    serviceName,
                    requestUrl);

                using var response = await _httpClient.GetAsync(
                    requestUrl,
                    HttpCompletionOption.ResponseHeadersRead,
                    ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "{ServiceName} returned HTTP {StatusCode}",
                        serviceName,
                        (int)response.StatusCode);

                    throw new HttpRequestException(
                        $"{serviceName} returned HTTP {(int)response.StatusCode}.",
                        null,
                        response.StatusCode);
                }

                var count = await response.Content
                    .ReadFromJsonAsync<int>(
                        cancellationToken: ct);

                if (count < 0)
                {
                    throw new JsonException(
                        $"{serviceName} returned a negative count.");
                }

                _logger.LogInformation(
                    "{ServiceName} returned count: {Count}",
                    serviceName,
                    count);

                return count;
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                _logger.LogError(
                    "{ServiceName} request timed out: {RequestUrl}",
                    serviceName,
                    requestUrl);

                throw new HttpRequestException(
                    $"{serviceName} did not respond within the configured timeout.",
                    null,
                    HttpStatusCode.ServiceUnavailable);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "HTTP communication failed with {ServiceName} at {RequestUrl}",
                    serviceName,
                    requestUrl);

                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    ex,
                    "Invalid count response from {ServiceName}",
                    serviceName);

                throw new InvalidOperationException(
                    $"{serviceName} returned an invalid count response.",
                    ex);
            }
        }
    }
}