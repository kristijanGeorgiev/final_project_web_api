using ClientTypeService.Entities;

namespace ClientTypeService.Repositories
{
    public interface IClientTypeRepository
    {
        Task<List<ClientType>> GetAllAsync(CancellationToken ct);
        Task<ClientType?> GetByIdAsync(int id, CancellationToken ct);
        Task<int> CountAsync(CancellationToken ct);
        Task AddAsync(ClientType entity, CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
        void Delete(ClientType entity);
    }
}
