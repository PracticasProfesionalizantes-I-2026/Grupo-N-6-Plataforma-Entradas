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
        if (entrada.Compra.UsuarioId != dto.EntradaId) return Result.Failure<DevolucionResponseDTO>("No tiene permisos para devolver esta entrada");
        
        if (entrada.Estado != EstadoEntrada.Activa)
            return Result.Failure<DevolucionResponseDTO>("La entrada no está activa para devolución");
        
        var evento = await _eventoRepository.GetByIdAsync(entrada.Compra.EventoId);
        if (evento?.FechaInicio <= DateTime.UtcNow)
            return Result.Failure<DevolucionResponseDTO>("No se puede devolver entradas de eventos ya iniciados");

        var compra = await _entradaRepository.GetByIdAsync(entrada.CompraId);
        var montoReembolso = compra!.Total * 0.8m / compra.Cantidad;

        var devolucion = new Devolucion
        {
            Id = Guid.NewGuid(),
            EntradaId = dto.EntradaId,
            MontoReembolsado = Math.Round(montoReembolso, 2),
            FechaSolicitud = DateTime.UtcNow,
            Estado = "Completada"
        };

        await _devolucionRepository.CreateAsync(devolucion);
        await _entradaRepository.UpdateEstadoAsync(dto.EntradaId, EstadoEntrada.Devuelta);
        
        var sector = await _entradaRepository.GetByIdAsync(dto.EntradaId);
        await _sectorRepository.IncrementarVendidasAsync(sector!.SectorId, -1);

        return Result.Success(new DevolucionResponseDTO
        {
            Id = devolucion.Id,
            EntradaId = devolucion.EntradaId,
            CodigoUnicoEntrada = sector!.CodigoUnico,
            MontoReembolsado = devolucion.MontoReembolsado,
            FechaSolicitud = devolucion.FechaSolicitud,
            Estado = devolucion.Estado
        });
    }

    public async Task<PagedResult<DevolucionResumenDTO>> GetMisDevolucionesAsync(Guid usuarioId, int page, int pageSize)
    {
        // Implementación simplificada - en producción usar repositorio dedicado
        return new PagedResult<DevolucionResumenDTO>(new List<DevolucionResumenDTO>(), page, pageSize, 0);
    }
}