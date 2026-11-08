using ClientService.Entities;
namespace ClientService.Repositories;
public interface IClientRepository
{
    Task<List<Client>> GetAllAsync(CancellationToken ct);
    Task<Client?> GetByIdAsync(int id, CancellationToken ct);
    Task<List<Client>> GetByClientTypeIdAsync(int clientTypeId, CancellationToken ct);
    Task<int> CountAsync(CancellationToken ct);
    Task AddAsync(Client entity, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
    void Delete(Client entity);
}
