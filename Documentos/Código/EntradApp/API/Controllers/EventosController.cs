using EntradApp.BusinessLogic.Interfaces;
using EntradApp.Shared.DTOs.Evento;
using EntradApp.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EntradApp.API.Controllers;

[ApiController]
[Route("api/v1/eventos")]
public class EventosController : ControllerBase
{
    private readonly IEventoService _eventoService;
    private readonly ISectorService _sectorService;

    public EventosController(IEventoService eventoService, ISectorService sectorService)
    {
        _eventoService = eventoService;
        _sectorService = sectorService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<EventoResumenDTO>>> GetEventos([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _eventoService.GetAprobadosAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EventoResponseDTO>> GetEvento(Guid id)
    {
        var result = await _eventoService.GetByIdAsync(id);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpGet("{id}/sectores")]
    public async Task<ActionResult<List<SectorResponseDTO>>> GetSectores(Guid id)
    {
        var result = await _eventoService.GetSectoresAsync(id);
        return Ok(result);
    }

    [Authorize(Policy = "Usuario")]
    [HttpPost]
    public async Task<ActionResult<EventoResponseDTO>> CrearEvento(EventoCreateDTO dto)
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")?.Value ?? throw new UnauthorizedAccessException());
        var result = await _eventoService.CrearAsync(usuarioId, dto);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return CreatedAtAction(nameof(GetEvento), new { id = result.Value!.Id }, result.Value);
    }

    [Authorize(Policy = "Usuario")]
    [HttpGet("mis-eventos")]
    public async Task<ActionResult<PagedResult<EventoResumenDTO>>> GetMisEventos([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")!.Value);
        var result = await _eventoService.GetMisEventosAsync(usuarioId, page, pageSize);
        return Ok(result);
    }

    [Authorize(Policy = "Usuario")]
    [HttpPut("{id}")]
    public async Task<ActionResult<EventoResponseDTO>> ActualizarEvento(Guid id, EventoUpdateDTO dto)
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")!.Value);
        var result = await _eventoService.ActualizarAsync(usuarioId, id, dto);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Authorize(Policy = "Usuario")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> EliminarEvento(Guid id)
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")!.Value);
        var result = await _eventoService.EliminarAsync(usuarioId, id);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return NoContent();
    }
}