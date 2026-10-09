using EntradApp.BusinessLogic.Interfaces;
using EntradApp.Shared.Common;
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

    private static readonly string[] SortByValidos = ["fecha", "nombre", "precio"];
    private const int MaxPageSize = 100;

    public EventosController(IEventoService eventoService, ISectorService sectorService)
    {
        _eventoService = eventoService;
        _sectorService = sectorService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<EventoResumenDTO>>> GetEventos(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null,
        [FromQuery] string? lugar = null,
        [FromQuery] decimal? precioMin = null,
        [FromQuery] decimal? precioMax = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false)
    {
        var errorValidacion = ValidarParametrosListado(page, pageSize, fechaDesde, fechaHasta, precioMin, precioMax, sortBy);
        if (errorValidacion != null) return BadRequest(new { error = errorValidacion });

        var result = await _eventoService.GetAprobadosConFiltrosAsync(
            fechaDesde, fechaHasta, lugar, precioMin, precioMax, sortBy, sortDesc, page, pageSize);
        return Ok(result);
    }

    private static string? ValidarParametrosListado(
        int page, int pageSize,
        DateTime? fechaDesde, DateTime? fechaHasta,
        decimal? precioMin, decimal? precioMax,
        string? sortBy)
    {
        if (page < 1) return "El número de página debe ser mayor a 0";
        if (pageSize < 1) return "El tamaño de página debe ser mayor a 0";
        if (pageSize > MaxPageSize) return $"El tamaño de página no puede exceder {MaxPageSize}";

        if (fechaDesde.HasValue && fechaHasta.HasValue && fechaDesde > fechaHasta)
            return "La fecha 'desde' no puede ser posterior a la fecha 'hasta'";

        if (precioMin.HasValue && precioMax.HasValue && precioMin > precioMax)
            return "El precio mínimo no puede ser mayor al precio máximo";

        if (!string.IsNullOrWhiteSpace(sortBy) && !SortByValidos.Contains(sortBy.ToLower()))
            return $"El campo 'sortBy' debe ser uno de: {string.Join(", ", SortByValidos)}";

        return null;
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
        var errorValidacion = ValidarPaginacion(page, pageSize);
        if (errorValidacion != null) return BadRequest(new { error = errorValidacion });

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

    [Authorize(Policy = "Usuario")]
    [HttpPut("{id}/cancelar")]
    public async Task<ActionResult> CancelarEvento(Guid id)
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")!.Value);
        var result = await _eventoService.CancelarAsync(usuarioId, id);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    private static string? ValidarPaginacion(int page, int pageSize)
    {
        if (page < 1) return "El número de página debe ser mayor a 0";
        if (pageSize < 1) return "El tamaño de página debe ser mayor a 0";
        if (pageSize > MaxPageSize) return $"El tamaño de página no puede exceder {MaxPageSize}";
        return null;
    }
}