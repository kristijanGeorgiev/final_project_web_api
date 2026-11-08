using ClientTypeService.DTOs;

namespace ClientTypeService.Services
{
    public interface IClientTypeService
    {
        Task<List<ClientTypeListDto>> GetAllAsync(CancellationToken ct);
        Task<ClientTypeDetailDto?> GetByIdAsync(int id, CancellationToken ct);
        Task<int> CountAsync(CancellationToken ct);
        Task<ClientTypeDetailDto> CreateAsync(ClientTypeCreateDto dto, CancellationToken ct);
        Task<bool> UpdateAsync(int id, ClientTypeUpdateDto dto, CancellationToken ct);
        Task<bool> DeleteAsync(int id, CancellationToken ct);
    }
}
