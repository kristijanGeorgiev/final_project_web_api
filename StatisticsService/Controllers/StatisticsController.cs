
using Microsoft.AspNetCore.Mvc;
using StatisticsService.DTOs;
using StatisticsService.Services;

namespace StatisticsService.Controllers;

[ApiController]
[Route("api/statistics")]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _service;
    private readonly ILogger<StatisticsController> _logger;
    private readonly IWebHostEnvironment _environment;

    public StatisticsController(
        IStatisticsService service,
        ILogger<StatisticsController> logger,
        IWebHostEnvironment environment)
    {
        _service = service;
        _logger = logger;
        _environment = environment;
    }

    [HttpGet]
    public async Task<ActionResult<StatisticsDto>> GetStatistics(
        CancellationToken ct)
    {
        try
        {
            _logger.LogInformation(
                "Statistics request received.");

            var statistics = await _service.GetStatisticsAsync(ct);

            _logger.LogInformation(
                "Statistics retrieved successfully.");

            return Ok(statistics);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "A dependent service is unavailable.");

            if (_environment.IsDevelopment())
            {
                return StatusCode(503, new
                {
                    Message = "A dependent service is unavailable.",
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message,
                    StatusCode = ex.StatusCode.HasValue
                        ? (int)ex.StatusCode.Value
                        : (int?)null
                });
            }

            return StatusCode(503, new
            {
                Message = "A dependent service is unavailable."
            });
        }
        catch (TaskCanceledException ex)
            when (!ct.IsCancellationRequested)
        {
            _logger.LogError(
                ex,
                "A dependent service request timed out.");

            if (_environment.IsDevelopment())
            {
                return StatusCode(504, new
                {
                    Message = "A dependent service timed out.",
                    Error = ex.Message
                });
            }

            return StatusCode(504, new
            {
                Message = "A dependent service timed out."
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(
                ex,
                "StatisticsService configuration or response error.");

            if (_environment.IsDevelopment())
            {
                return StatusCode(500, new
                {
                    Message = "StatisticsService encountered a configuration or response error.",
                    Error = ex.Message
                });
            }

            return StatusCode(500, new
            {
                Message = "An internal server error occurred."
            });
        }
        catch (Exception ex) when (
            ex is not OperationCanceledException)
        {
            _logger.LogError(
                ex,
                "Unexpected error while retrieving statistics.");

            if (_environment.IsDevelopment())
            {
                return StatusCode(500, new
                {
                    Message = "An unexpected error occurred.",
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message
                });
            }

            return StatusCode(500, new
            {
                Message = "An internal server error occurred."
            });
        }
    }
}