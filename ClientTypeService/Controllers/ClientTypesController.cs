using Microsoft.AspNetCore.Mvc;
using ClientTypeService.DTOs;
using ClientTypeService.Services;
namespace ClientTypeService.Controllers;
[ApiController]
[Route("api/clienttypes")]
public class ClientTypesController(IClientTypeService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ClientTypeListDto>>> GetAll(CancellationToken ct) => Ok(await service.GetAllAsync(ct));
    [HttpGet("count")]
    public async Task<ActionResult<int>> GetCount(CancellationToken ct) => Ok(await service.CountAsync(ct));
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClientTypeDetailDto>> GetById(int id, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }
    [HttpPost]
    public async Task<ActionResult<ClientTypeDetailDto>> Create([FromBody] ClientTypeCreateDto dto, CancellationToken ct)
    {
        var result = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ClientTypeUpdateDto dto, CancellationToken ct) =>
        await service.UpdateAsync(id, dto, ct) ? NoContent() : NotFound();
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) =>
        await service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
