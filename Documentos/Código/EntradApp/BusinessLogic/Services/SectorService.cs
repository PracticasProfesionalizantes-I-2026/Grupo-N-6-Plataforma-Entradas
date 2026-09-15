using EntradApp.BusinessLogic.Interfaces;
using EntradApp.DataAccess.Entities;
using EntradApp.DataAccess.Repositories.Interfaces;
using EntradApp.Shared.Common;
using EntradApp.Shared.DTOs.Sector;
using EntradApp.Shared.Exceptions;

namespace EntradApp.BusinessLogic.Services;

public class SectorService : ISectorService
{
    private readonly ISectorRepository _sectorRepository;
    private readonly IEventoRepository _eventoRepository;

    public SectorService(ISectorRepository sectorRepository, IEventoRepository eventoRepository)
    {
        _sectorRepository = sectorRepository;
        _eventoRepository = eventoRepository;
    }

    public async Task<Result<SectorResponseDTO>> CrearAsync(Guid usuarioId, Guid eventoId, SectorCreateDTO dto)
    {
        var evento = await _eventoRepository.GetByIdAsync(eventoId);
        if (evento == null) return Result.Failure<SectorResponseDTO>("Evento no encontrado");
        if (evento.UsuarioCreadorId != usuarioId) return Result.Failure<SectorResponseDTO>("No tiene permisos para gestionar sectores de este evento");
        if (evento.Estado == EntradApp.Shared.Enums.EstadoEvento.Finalizado)
            return Result.Failure<SectorResponseDTO>("No se pueden agregar sectores a un evento finalizado");

        var sector = new Sector
        {
            Id = Guid.NewGuid(),
            EventoId = eventoId,
            Nombre = dto.Nombre.Trim(),
            Capacidad = dto.Capacidad,
            PrecioBase = dto.PrecioBase,
            PrecioActual = dto.PrecioBase
        };

        await _sectorRepository.CreateAsync(sector);
        return Result.Success(MapToResponseDTO(sector));
    }

    public async Task<Result<SectorResponseDTO>> ActualizarAsync(Guid usuarioId, Guid sectorId, SectorUpdateDTO dto)
    {
        var sector = await _sectorRepository.GetByIdWithEventoAsync(sectorId);
        if (sector == null) return Result.Failure<SectorResponseDTO>("Sector no encontrado");
        if (sector.Evento.UsuarioCreadorId != usuarioId) return Result.Failure<SectorResponseDTO>("No tiene permisos para modificar este sector");
        if (sector.Evento.Estado == EntradApp.Shared.Enums.EstadoEvento.Finalizado)
            return Result.Failure<SectorResponseDTO>("No se pueden modificar sectores de un evento finalizado");

        if (!string.IsNullOrWhiteSpace(dto.Nombre)) sector.Nombre = dto.Nombre.Trim();
        if (dto.Capacidad.HasValue)
        {
            if (dto.Capacidad < sector.EntradasVendidas)
                return Result.Failure<SectorResponseDTO>("No se puede reducir la capacidad por debajo de las entradas ya vendidas");
            sector.Capacidad = dto.Capacidad.Value;
        }
        if (dto.PrecioBase.HasValue) sector.PrecioBase = dto.PrecioBase.Value;

        await _sectorRepository.UpdateAsync(sector);
        return Result.Success(MapToResponseDTO(sector));
    }

    public async Task<Result> EliminarAsync(Guid usuarioId, Guid sectorId)
    {
        var sector = await _sectorRepository.GetByIdWithEventoAsync(sectorId);
        if (sector == null) return Result.Failure("Sector no encontrado");
        if (sector.Evento.UsuarioCreadorId != usuarioId) return Result.Failure("No tiene permisos para eliminar este sector");
        if (sector.EntradasVendidas > 0)
            return Result.Failure("No se puede eliminar un sector con entradas vendidas");

        await _sectorRepository.DeleteAsync(sectorId);
        return Result.Success();
    }

    private static SectorResponseDTO MapToResponseDTO(Sector s) => new()
    {
        Id = s.Id,
        EventoId = s.EventoId,
        Nombre = s.Nombre,
        Capacidad = s.Capacidad,
        PrecioBase = s.PrecioBase,
        PrecioActual = s.PrecioActual,
        EntradasVendidas = s.EntradasVendidas,
        EntradasReservadas = s.EntradasReservadas,
        EntradasDisponibles = s.StockDisponible,
        PorcentajeOcupacion = s.PorcentajeOcupacion
    };
}