using StatisticsService.DTOs;

namespace StatisticsService.Repositories
{
    public interface IStatisticsRepository
    {
        Task<StatisticsDto> GetStatisticsAsync(CancellationToken ct);
    }
}
