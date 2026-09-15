using EntradApp.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EntradApp.API.Controllers;

[ApiController]
[Route("api/v1/eventos/{eventoId:guid}")]
[Authorize(Policy = "Usuario")]
public class ReportesController : ControllerBase
{
    private readonly IEventoService _eventoService;
    private readonly ICompraService _compraService;

    public ReportesController(IEventoService eventoService, ICompraService compraService)
    {
        _eventoService = eventoService;
        _compraService = compraService;
    }

    [HttpGet("transacciones")]
    public async Task<ActionResult> GetTransacciones(
        Guid eventoId,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] string? estado,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")!.Value);
        
        // Verificar que el evento pertenece al usuario
        var evento = await _eventoService.GetByIdAsync(eventoId);
        if (!evento.IsSuccess) return NotFound(new { error = evento.Error });
        if (evento.Value!.UsuarioCreadorId != usuarioId) return Forbid();

        // En producción, usar servicio dedicado de reportes
        var result = await _compraService.GetByUsuarioAsync(usuarioId, page, pageSize);
        return Ok(result);
    }

    [HttpGet("estadisticas")]
    public async Task<ActionResult> GetEstadisticas(Guid eventoId)
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")!.Value);
        var evento = await _eventoService.GetByIdAsync(eventoId);
        if (!evento.IsSuccess) return NotFound(new { error = evento.Error });
        if (evento.Value!.UsuarioCreadorId != usuarioId) return Forbid();

        var sectores = evento.Value.Sectores;
        return Ok(new
        {
            TotalVendidas = sectores.Sum(s => s.EntradasVendidas),
            TotalDisponibles = sectores.Sum(s => s.EntradasDisponibles),
            TotalCapacidad = sectores.Sum(s => s.Capacidad),
            IngresosTotales = sectores.Sum(s => s.EntradasVendidas * s.PrecioActual),
            PorcentajeOcupacionGlobal = sectores.Sum(s => s.Capacidad) > 0 
                ? (double)sectores.Sum(s => s.EntradasVendidas) / sectores.Sum(s => s.Capacidad) * 100 
                : 0,
            PorSector = sectores.Select(s => new
            {
                s.Id,
                s.Nombre,
                s.Capacidad,
                s.EntradasVendidas,
                s.EntradasDisponibles,
                s.PorcentajeOcupacion,
                s.PrecioActual,
                Ingresos = s.EntradasVendidas * s.PrecioActual
            })
        });
    }

    [HttpGet("ocupacion")]
    public async Task<ActionResult> GetOcupacion(Guid eventoId)
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")!.Value);
        var evento = await _eventoService.GetByIdAsync(eventoId);
        if (!evento.IsSuccess) return NotFound(new { error = evento.Error });
        if (evento.Value!.UsuarioCreadorId != usuarioId) return Forbid();

        var sectores = evento.Value.Sectores;
        return Ok(sectores.Select(s => new
        {
            s.Id,
            s.Nombre,
            s.Capacidad,
            s.EntradasVendidas,
            s.EntradasDisponibles,
            s.PorcentajeOcupacion,
            s.PrecioBase,
            s.PrecioActual,
            PrecioIncrementado = s.PrecioActual > s.PrecioBase
        }));
    }
}