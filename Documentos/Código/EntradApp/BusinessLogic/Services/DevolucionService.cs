using EntradApp.BusinessLogic.Interfaces;
using EntradApp.DataAccess.Entities;
using EntradApp.DataAccess.Repositories.Interfaces;
using EntradApp.Shared.Common;
using EntradApp.Shared.DTOs.Devolucion;
using EntradApp.Shared.Enums;
using EntradApp.Shared.Exceptions;

namespace EntradApp.BusinessLogic.Services;

public class DevolucionService : IDevolucionService
{
    private readonly IDevolucionRepository _devolucionRepository;
    private readonly IEntradaRepository _entradaRepository;
    private readonly ISectorRepository _sectorRepository;
    private readonly IEventoRepository _eventoRepository;

    public DevolucionService(
        IDevolucionRepository devolucionRepository,
        IEntradaRepository entradaRepository,
        ISectorRepository sectorRepository,
        IEventoRepository eventoRepository)
    {
        _devolucionRepository = devolucionRepository;
        _entradaRepository = entradaRepository;
        _sectorRepository = sectorRepository;
        _eventoRepository = eventoRepository;
    }

    public async Task<Result<DevolucionResponseDTO>> SolicitarDevolucionAsync(Guid usuarioId, DevolucionCreateDTO dto)
    {
        var entrada = await _entradaRepository.GetByIdAsync(dto.EntradaId);
        if (entrada == null) return Result.Failure<DevolucionResponseDTO>("Entrada no encontrada");
        if (entrada.Compra == null || entrada.Compra.UsuarioId != usuarioId)
            return Result.Failure<DevolucionResponseDTO>("No tiene permisos para devolver esta entrada");

        if (entrada.Estado != EstadoEntrada.Activa)
            return Result.Failure<DevolucionResponseDTO>("La entrada no está activa para devolución");

        var evento = await _eventoRepository.GetByIdAsync(entrada.Compra.EventoId);
        if (evento?.FechaInicio <= DateTime.UtcNow)
            return Result.Failure<DevolucionResponseDTO>("No se puede devolver entradas de eventos ya iniciados");

        var sector = await _sectorRepository.GetByIdAsync(entrada.SectorId);
        if (sector == null) return Result.Failure<DevolucionResponseDTO>("Sector no encontrado");

        var montoReembolso = Math.Round(sector.PrecioActual * 0.8m, 2);

        var devolucion = new Devolucion
        {
            Id = Guid.NewGuid(),
            EntradaId = dto.EntradaId,
            MontoReembolsado = montoReembolso,
            FechaSolicitud = DateTime.UtcNow,
            Estado = "Solicitada"
        };

        await _devolucionRepository.CreateAsync(devolucion);

        return Result.Success(new DevolucionResponseDTO
        {
            Id = devolucion.Id,
            EntradaId = devolucion.EntradaId,
            CodigoUnicoEntrada = entrada.CodigoUnico,
            MontoReembolsado = devolucion.MontoReembolsado,
            FechaSolicitud = devolucion.FechaSolicitud,
            Estado = devolucion.Estado
        });
    }

    public async Task<Result<DevolucionResponseDTO>> CompletarDevolucionAsync(Guid devolucionId)
    {
        var devolucion = await _devolucionRepository.GetByIdAsync(devolucionId);
        if (devolucion == null) return Result.Failure<DevolucionResponseDTO>("Devolución no encontrada");
        if (devolucion.Estado == "Completada") return Result.Failure<DevolucionResponseDTO>("La devolución ya fue completada");

        var entrada = await _entradaRepository.GetByIdAsync(devolucion.EntradaId);
        if (entrada == null) return Result.Failure<DevolucionResponseDTO>("Entrada asociada no encontrada");

        devolucion.Estado = "Completada";
        devolucion.FechaProcesamiento = DateTime.UtcNow;

        await _devolucionRepository.UpdateAsync(devolucion);
        await _entradaRepository.UpdateEstadoAsync(devolucion.EntradaId, EstadoEntrada.Devuelta);
        await _sectorRepository.IncrementarVendidasAsync(entrada.SectorId, -1);

        return Result.Success(new DevolucionResponseDTO
        {
            Id = devolucion.Id,
            EntradaId = devolucion.EntradaId,
            CodigoUnicoEntrada = entrada.CodigoUnico,
            MontoReembolsado = devolucion.MontoReembolsado,
            FechaSolicitud = devolucion.FechaSolicitud,
            FechaProcesamiento = devolucion.FechaProcesamiento,
            Estado = devolucion.Estado
        });
    }

    public async Task<PagedResult<DevolucionResumenDTO>> GetMisDevolucionesAsync(Guid usuarioId, int page, int pageSize)
    {
        var result = await _devolucionRepository.GetByUsuarioAsync(usuarioId, page, pageSize);
        var items = result.Items.Select(MapToResumenDTO).ToList();
        return new PagedResult<DevolucionResumenDTO>(items, result.PageNumber, result.PageSize, result.TotalCount);
    }

    private static DevolucionResumenDTO MapToResumenDTO(Devolucion d) => new()
    {
        Id = d.Id,
        EventoNombre = d.Entrada?.Compra?.Evento?.Nombre ?? "",
        CodigoUnicoEntrada = d.Entrada?.CodigoUnico ?? "",
        MontoReembolsado = d.MontoReembolsado,
        FechaSolicitud = d.FechaSolicitud
    };
}