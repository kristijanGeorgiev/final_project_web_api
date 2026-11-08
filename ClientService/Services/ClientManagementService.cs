using ClientService.DTOs;
using ClientService.Entities;
using ClientService.Repositories;
namespace ClientService.Services;
public class ClientManagementService(IClientRepository repository) : IClientService
{
    private static ClientListDto ToListDto(Client x) => new() { Id = x.Id, ClientTypeId = x.ClientTypeId, Name = x.Name, Address = x.Address, City = x.City, Phone = x.Phone, Email = x.Email, TaxNumber = x.TaxNumber };
    private static ClientDetailDto ToDetailDto(Client x) => new() { Id = x.Id, ClientTypeId = x.ClientTypeId, Name = x.Name, Address = x.Address, City = x.City, Phone = x.Phone, Email = x.Email, TaxNumber = x.TaxNumber };
    public async Task<List<ClientListDto>> GetAllAsync(CancellationToken ct) => (await repository.GetAllAsync(ct)).Select(ToListDto).ToList();
    public async Task<ClientDetailDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var entity = await repository.GetByIdAsync(id, ct);
        return entity is null ? null : ToDetailDto(entity);
    }
    public async Task<List<ClientListDto>> GetByClientTypeIdAsync(int clientTypeId, CancellationToken ct) =>
        (await repository.GetByClientTypeIdAsync(clientTypeId, ct)).Select(ToListDto).ToList();
    public Task<int> CountAsync(CancellationToken ct) => repository.CountAsync(ct);
    public async Task<ClientDetailDto> CreateAsync(ClientCreateDto dto, CancellationToken ct)
    {
        var entity = new Client();
        entity.ClientTypeId = dto.ClientTypeId;
        entity.Name = dto.Name;
        entity.Address = dto.Address;
        entity.City = dto.City;
        entity.Phone = dto.Phone;
        entity.Email = dto.Email;
        entity.TaxNumber = dto.TaxNumber;
        await repository.AddAsync(entity, ct);
        await repository.SaveChangesAsync(ct);
        return ToDetailDto(entity);
    }
    public async Task<bool> UpdateAsync(int id, ClientUpdateDto dto, CancellationToken ct)
    {
        var entity = await repository.GetByIdAsync(id, ct);
        if (entity is null) return false;
        entity.ClientTypeId = dto.ClientTypeId;
        entity.Name = dto.Name;
        entity.Address = dto.Address;
        entity.City = dto.City;
        entity.Phone = dto.Phone;
        entity.Email = dto.Email;
        entity.TaxNumber = dto.TaxNumber;
        await repository.SaveChangesAsync(ct);
        return true;
    }
    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await repository.GetByIdAsync(id, ct);
        if (entity is null) return false;
        repository.Delete(entity);
        await repository.SaveChangesAsync(ct);
        return true;
    }
}
