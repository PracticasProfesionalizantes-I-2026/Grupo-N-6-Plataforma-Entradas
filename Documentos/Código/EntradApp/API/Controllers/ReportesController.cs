using EntradApp.BusinessLogic.Interfaces;
using EntradApp.Shared.Common;
using EntradApp.Shared.DTOs.Compra;
using EntradApp.Shared.DTOs.Sector;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace EntradApp.API.Controllers;

[ApiController]
[Route("api/v1/reportes/eventos/{eventoId:guid}")]
[Authorize(Policy = "Usuario")]
public class ReportesController : ControllerBase
{
    private readonly IEventoService _eventoService;
    private readonly ICompraService _compraService;

    private static readonly string[] FormatosValidos = ["csv", "json"];
    private static readonly string[] EstadosCompraValidos = ["PendientePago", "Aprobado", "Cancelada"];
    private const int MaxPageSize = 100;

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
        [FromQuery] int pageSize = 20,
        [FromQuery] string? format = null)
    {
        var errorValidacion = ValidarParametrosReporte(page, pageSize, fechaDesde, fechaHasta, estado, format);
        if (errorValidacion != null) return BadRequest(new { error = errorValidacion });

        var usuarioId = Guid.Parse(User.FindFirst("sub")!.Value);

        var evento = await _eventoService.GetByIdAsync(eventoId);
        if (!evento.IsSuccess) return NotFound(new { error = evento.Error });
        if (evento.Value!.UsuarioCreadorId != usuarioId) return Forbid();

        var result = await _compraService.GetByEventoAsync(eventoId, fechaDesde, fechaHasta, estado, page, pageSize);

        if (format?.ToLower() == "csv")
        {
            var csv = GenerarCsvTransacciones(result);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"transacciones-evento-{eventoId}-{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        return Ok(result);
    }

    [HttpGet("estadisticas")]
    public async Task<ActionResult> GetEstadisticas(
        Guid eventoId,
        [FromQuery] string? format = null)
    {
        var errorValidacion = ValidarFormato(format);
        if (errorValidacion != null) return BadRequest(new { error = errorValidacion });

        var usuarioId = Guid.Parse(User.FindFirst("sub")!.Value);
        var evento = await _eventoService.GetByIdAsync(eventoId);
        if (!evento.IsSuccess) return NotFound(new { error = evento.Error });
        if (evento.Value!.UsuarioCreadorId != usuarioId) return Forbid();

        var sectores = evento.Value.Sectores;
        var stats = new
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
        };

        if (format?.ToLower() == "csv")
        {
            var csv = GenerarCsvEstadisticas(sectores);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"estadisticas-evento-{eventoId}-{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        return Ok(stats);
    }

    [HttpGet("ocupacion")]
    public async Task<ActionResult> GetOcupacion(
        Guid eventoId,
        [FromQuery] string? format = null)
    {
        var errorValidacion = ValidarFormato(format);
        if (errorValidacion != null) return BadRequest(new { error = errorValidacion });

        var usuarioId = Guid.Parse(User.FindFirst("sub")!.Value);
        var evento = await _eventoService.GetByIdAsync(eventoId);
        if (!evento.IsSuccess) return NotFound(new { error = evento.Error });
        if (evento.Value!.UsuarioCreadorId != usuarioId) return Forbid();

        var sectores = evento.Value.Sectores;
        var ocupacion = sectores.Select(s => new
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
        });

        if (format?.ToLower() == "csv")
        {
            var csv = GenerarCsvOcupacion(sectores);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"ocupacion-evento-{eventoId}-{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        return Ok(ocupacion);
    }

    private static string? ValidarParametrosReporte(
        int page, int pageSize,
        DateTime? fechaDesde, DateTime? fechaHasta,
        string? estado, string? format)
    {
        if (page < 1) return "El número de página debe ser mayor a 0";
        if (pageSize < 1) return "El tamaño de página debe ser mayor a 0";
        if (pageSize > MaxPageSize) return $"El tamaño de página no puede exceder {MaxPageSize}";

        if (fechaDesde.HasValue && fechaHasta.HasValue && fechaDesde > fechaHasta)
            return "La fecha 'desde' no puede ser posterior a la fecha 'hasta'";

        if (!string.IsNullOrWhiteSpace(estado) && !EstadosCompraValidos.Contains(estado, StringComparer.OrdinalIgnoreCase))
            return $"El estado debe ser uno de: {string.Join(", ", EstadosCompraValidos)}";

        return ValidarFormato(format);
    }

    private static string? ValidarFormato(string? format)
    {
        if (!string.IsNullOrWhiteSpace(format) && !FormatosValidos.Contains(format.ToLower()))
            return $"El formato debe ser uno de: {string.Join(", ", FormatosValidos)}";
        return null;
    }

    private static string GenerarCsvTransacciones(PagedResult<CompraResponseDTO> result)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id,UsuarioId,EventoId,SectorId,Cantidad,Total,Estado,FechaCreacion,FechaPago");
        foreach (var c in result.Items)
        {
            sb.AppendLine($"{c.Id},{c.UsuarioId},{c.EventoId},{c.SectorId},{c.Cantidad},{c.Total},{c.Estado},{c.FechaCreacion:yyyy-MM-dd HH:mm},{c.FechaPago?.ToString("yyyy-MM-dd HH:mm") ?? ""}");
        }
        return sb.ToString();
    }

    private static string GenerarCsvEstadisticas(List<EntradApp.Shared.DTOs.Evento.SectorResponseDTO> sectores)
    {
        var sb = new StringBuilder();
        sb.AppendLine("SectorId,Nombre,Capacidad,EntradasVendidas,EntradasDisponibles,PorcentajeOcupacion,PrecioActual,Ingresos");
        foreach (var s in sectores)
        {
            sb.AppendLine($"{s.Id},{s.Nombre},{s.Capacidad},{s.EntradasVendidas},{s.EntradasDisponibles},{s.PorcentajeOcupacion:F2},{s.PrecioActual},{s.EntradasVendidas * s.PrecioActual}");
        }
        sb.AppendLine($",TOTAL,,{sectores.Sum(s => s.EntradasVendidas)},{sectores.Sum(s => s.EntradasDisponibles)},,{sectores.Sum(s => s.EntradasVendidas * s.PrecioActual)}");
        return sb.ToString();
    }

    private static string GenerarCsvOcupacion(List<EntradApp.Shared.DTOs.Evento.SectorResponseDTO> sectores)
    {
        var sb = new StringBuilder();
        sb.AppendLine("SectorId,Nombre,Capacidad,EntradasVendidas,EntradasDisponibles,PorcentajeOcupacion,PrecioBase,PrecioActual,PrecioIncrementado");
        foreach (var s in sectores)
        {
            sb.AppendLine($"{s.Id},{s.Nombre},{s.Capacidad},{s.EntradasVendidas},{s.EntradasDisponibles},{s.PorcentajeOcupacion:F2},{s.PrecioBase},{s.PrecioActual},{s.PrecioActual > s.PrecioBase}");
        }
        return sb.ToString();
    }
}