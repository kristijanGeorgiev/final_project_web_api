using Microsoft.EntityFrameworkCore;
using ClientService.Data;
using ClientService.Entities;
namespace ClientService.Repositories;
public class ClientRepository(ApplicationDbContext db) : IClientRepository
{
    public Task<List<Client>> GetAllAsync(CancellationToken ct) => db.Clients.AsNoTracking().OrderBy(x => x.Id).ToListAsync(ct);
    public Task<Client?> GetByIdAsync(int id, CancellationToken ct) => db.Clients.FirstOrDefaultAsync(x => x.Id == id, ct);
    public Task<List<Client>> GetByClientTypeIdAsync(int clientTypeId, CancellationToken ct) => db.Clients.AsNoTracking().Where(x => x.ClientTypeId == clientTypeId).OrderBy(x => x.Id).ToListAsync(ct);
    public Task<int> CountAsync(CancellationToken ct) => db.Clients.CountAsync(ct);
    public async Task AddAsync(Client entity, CancellationToken ct) { await db.Clients.AddAsync(entity, ct); }
    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
    public void Delete(Client entity) => db.Clients.Remove(entity);
}
