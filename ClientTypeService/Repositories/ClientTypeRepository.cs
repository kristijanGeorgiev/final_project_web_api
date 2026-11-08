using Microsoft.EntityFrameworkCore;
using ClientTypeService.Data;
using ClientTypeService.Entities;
namespace ClientTypeService.Repositories;
public class ClientTypeRepository(ApplicationDbContext db) : IClientTypeRepository
{
    public Task<List<ClientType>> GetAllAsync(CancellationToken ct) => db.ClientTypes.AsNoTracking().OrderBy(x => x.Id).ToListAsync(ct);
    public Task<ClientType?> GetByIdAsync(int id, CancellationToken ct) => db.ClientTypes.FirstOrDefaultAsync(x => x.Id == id, ct);
    public Task<int> CountAsync(CancellationToken ct) => db.ClientTypes.CountAsync(ct);
    public async Task AddAsync(ClientType entity, CancellationToken ct) { await db.ClientTypes.AddAsync(entity, ct); }
    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
    public void Delete(ClientType entity) => db.ClientTypes.Remove(entity);
}
