using StatisticsService.DTOs;
using StatisticsService.Repositories;

namespace StatisticsService.Services
{
    public class StatisticsManagementService : IStatisticsService
    {
        private readonly IStatisticsRepository _repository;

        public StatisticsManagementService(
            IStatisticsRepository repository)
        {
            _repository = repository;
        }

        public async Task<StatisticsDto> GetStatisticsAsync(
            CancellationToken ct = default)
        {
            return await _repository.GetStatisticsAsync(ct);
        }
    }
}