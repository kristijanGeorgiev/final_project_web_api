using StatisticsService.DTOs;

namespace StatisticsService.Services
{
    public interface IStatisticsService
    {
        Task<StatisticsDto> GetStatisticsAsync(CancellationToken ct);
    }
}
