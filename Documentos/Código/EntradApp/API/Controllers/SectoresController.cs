using EntradApp.BusinessLogic.Interfaces;
using EntradApp.Shared.DTOs.Sector;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EntradApp.API.Controllers;

[ApiController]
[Route("api/v1/eventos/{eventoId:guid}/sectores")]
[Authorize(Policy = "Usuario")]
public class SectoresController : ControllerBase
{
    private readonly ISectorService _sectorService;

    public SectoresController(ISectorService sectorService) => _sectorService = sectorService;

    [HttpPost]
    public async Task<ActionResult<SectorResponseDTO>> CrearSector(Guid eventoId, SectorCreateDTO dto)
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")!.Value);
        var result = await _sectorService.CrearAsync(usuarioId, eventoId, dto);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return CreatedAtAction(nameof(CrearSector), new { eventoId, id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SectorResponseDTO>> ActualizarSector(Guid eventoId, Guid id, SectorUpdateDTO dto)
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")!.Value);
        var result = await _sectorService.ActualizarAsync(usuarioId, id, dto);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> EliminarSector(Guid eventoId, Guid id)
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")!.Value);
        var result = await _sectorService.EliminarAsync(usuarioId, id);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return NoContent();
    }
}