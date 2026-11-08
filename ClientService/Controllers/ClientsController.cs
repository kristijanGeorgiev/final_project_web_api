using Microsoft.AspNetCore.Mvc;
using ClientService.DTOs;
using ClientService.Services;
using Microsoft.AspNetCore.Authorization;
namespace ClientService.Controllers;
[ApiController]
[Route("api/clients")]
public class ClientsController(IClientService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ClientListDto>>> GetAll(CancellationToken ct) => Ok(await service.GetAllAsync(ct));
    [HttpGet("count")]
    public async Task<ActionResult<int>> GetCount(CancellationToken ct) => Ok(await service.CountAsync(ct));
    [HttpGet("by-client-type/{clientTypeId:int}")]
    public async Task<ActionResult<List<ClientListDto>>> GetByClientTypeId(int clientTypeId, CancellationToken ct) =>
        Ok(await service.GetByClientTypeIdAsync(clientTypeId, ct));
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClientDetailDto>> GetById(int id, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }
    [HttpPost]
    public async Task<ActionResult<ClientDetailDto>> Create([FromBody] ClientCreateDto dto, CancellationToken ct)
    {
        var result = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ClientUpdateDto dto, CancellationToken ct) =>
        await service.UpdateAsync(id, dto, ct) ? NoContent() : NotFound();
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) =>
        await service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
