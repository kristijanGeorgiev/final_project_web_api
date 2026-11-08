using ClientService.DTOs;
namespace ClientService.Services;
public interface IClientService
{
    Task<List<ClientListDto>> GetAllAsync(CancellationToken ct);
    Task<ClientDetailDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<List<ClientListDto>> GetByClientTypeIdAsync(int clientTypeId, CancellationToken ct);
    Task<int> CountAsync(CancellationToken ct);
    Task<ClientDetailDto> CreateAsync(ClientCreateDto dto, CancellationToken ct);
    Task<bool> UpdateAsync(int id, ClientUpdateDto dto, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
