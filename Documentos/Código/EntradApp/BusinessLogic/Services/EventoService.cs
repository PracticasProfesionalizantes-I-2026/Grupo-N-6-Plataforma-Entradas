using EntradApp.BusinessLogic.Interfaces;
using EntradApp.DataAccess.Entities;
using EntradApp.DataAccess.Repositories.Interfaces;
using EntradApp.Shared.Common;
using EntradApp.Shared.DTOs.Evento;
using EntradApp.Shared.Enums;
using EntradApp.Shared.Exceptions;

namespace EntradApp.BusinessLogic.Services;

public class EventoService : IEventoService
{
    private readonly IEventoRepository _eventoRepository;
    private readonly ISectorRepository _sectorRepository;

    public EventoService(IEventoRepository eventoRepository, ISectorRepository sectorRepository)
    {
        _eventoRepository = eventoRepository;
        _sectorRepository = sectorRepository;
    }

    public async Task<Result<EventoResponseDTO>> CrearAsync(Guid usuarioId, EventoCreateDTO dto)
    {
        ValidarEvento(dto);

        var evento = new Evento
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre.Trim(),
            Descripcion = dto.Descripcion?.Trim(),
            FechaInicio = dto.FechaInicio,
            FechaFin = dto.FechaFin,
            Lugar = dto.Lugar.Trim(),
            Direccion = dto.Direccion.Trim(),
            Estado = EstadoEvento.PendienteAprobacion,
            UsuarioCreadorId = usuarioId,
            FechaCreacion = DateTime.UtcNow
        };

        foreach (var sectorDto in dto.Sectores)
        {
            evento.Sectores.Add(new Sector
            {
                Id = Guid.NewGuid(),
                Nombre = sectorDto.Nombre.Trim(),
                Capacidad = sectorDto.Capacidad,
                PrecioBase = sectorDto.PrecioBase,
                PrecioActual = sectorDto.PrecioBase
            });
        }

        await _eventoRepository.CreateAsync(evento);
        return Result.Success(MapToResponseDTO(evento));
    }

    public async Task<Result<EventoResponseDTO>> GetByIdAsync(Guid id)
    {
        var evento = await _eventoRepository.GetByIdAsync(id);
        if (evento == null) return Result.Failure<EventoResponseDTO>("Evento no encontrado");
        return Result.Success(MapToResponseDTO(evento));
    }

    public async Task<PagedResult<EventoResumenDTO>> GetAprobadosAsync(int page, int pageSize)
    {
        var result = await _eventoRepository.GetAprobadosAsync(page, 10);
        var items = result.Items.Select(MapToResumenDTO).ToList();
        return new PagedResult<EventoResumenDTO>(items, result.PageNumber, result.PageSize, result.TotalCount);
    }

    public async Task<PagedResult<EventoResumenDTO>> GetMisEventosAsync(Guid usuarioId, int page, int pageSize)
    {
        var result = await _eventoRepository.GetByCreadorAsync(usuarioId, page, 10);
        var items = result.Items.Select(MapToResumenDTO).ToList();
        return new PagedResult<EventoResumenDTO>(items, result.PageNumber, result.PageSize, result.TotalCount);
    }

    public async Task<Result<EventoResponseDTO>> ActualizarAsync(Guid usuarioId, Guid eventoId, EventoUpdateDTO dto)
    {
        var evento = await _eventoRepository.GetByIdAsync(eventoId);
        if (evento == null) return Result.Failure<EventoResponseDTO>("Evento no encontrado");
        if (evento.UsuarioCreadorId != usuarioId) return Result.Failure<EventoResponseDTO>("No tiene permisos para modificar este evento");
        if (evento.Estado == EstadoEvento.Finalizado || evento.Estado == EstadoEvento.Cancelado)
            return Result.Failure<EventoResponseDTO>("No se puede modificar un evento finalizado o cancelado");

        if (!string.IsNullOrWhiteSpace(dto.Nombre)) evento.Nombre = dto.Nombre.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Descripcion)) evento.Descripcion = dto.Descripcion?.Trim();
        if (dto.FechaInicio.HasValue) evento.FechaInicio = dto.FechaInicio.Value;
        if (dto.FechaFin.HasValue) evento.FechaFin = dto.FechaFin.Value;
        if (!string.IsNullOrWhiteSpace(dto.Lugar)) evento.Lugar = dto.Lugar.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Direccion)) evento.Direccion = dto.Direccion.Trim();
        evento.FechaActualizacion = DateTime.UtcNow;

        if (evento.FechaInicio >= evento.FechaFin)
            return Result.Failure<EventoResponseDTO>("La fecha de fin debe ser posterior a la de inicio");
        if (evento.FechaInicio < DateTime.Today)
            return Result.Failure<EventoResponseDTO>("La fecha de inicio debe ser futura");

        await _eventoRepository.UpdateAsync(evento);
        return Result.Success(MapToResponseDTO(evento));
    }

    public async Task<Result> EliminarAsync(Guid usuarioId, Guid eventoId)
    {
        var evento = await _eventoRepository.GetByIdAsync(eventoId);
        if (evento == null) return Result.Failure("Evento no encontrado");
        if (evento.UsuarioCreadorId != usuarioId) return Result.Failure("No tiene permisos para eliminar este evento");

        await _eventoRepository.SoftDeleteAsync(eventoId);
        return Result.Success();
    }

    public async Task<List<SectorResponseDTO>> GetSectoresAsync(Guid eventoId)
    {
        var sectores = await _sectorRepository.GetByEventoAsync(eventoId);
        return sectores.Select(MapToSectorResponseDTO).ToList();
    }

    private static void ValidarEvento(EventoCreateDTO dto)
    {
        if (dto.FechaInicio < DateTime.Today)
            throw new ValidationException("fechaInicio", "La fecha de inicio debe ser futura");
        if (dto.FechaFin <= dto.FechaInicio)
            throw new ValidationException("fechaFin", "La fecha de fin debe ser posterior a la de inicio");
        if (dto.Sectores.Count == 0)
            throw new ValidationException("sectores", "Debe haber al menos un sector");
        foreach (var s in dto.Sectores)
        {
            if (s.Capacidad <= 0) throw new ValidationException("capacidad", "La capacidad debe ser mayor a 0");
            if (s.PrecioBase < 0) throw new ValidationException("precioBase", "El precio base no puede ser negativo");
        }
    }

    private static EventoResponseDTO MapToResponseDTO(Evento e) => new()
    {
        Id = e.Id,
        Nombre = e.Nombre,
        Descripcion = e.Descripcion,
        FechaInicio = e.FechaInicio,
        FechaFin = e.FechaFin,
        Lugar = e.Lugar,
        Direccion = e.Direccion,
        Estado = e.Estado,
        UsuarioCreadorId = e.UsuarioCreadorId,
        FechaCreacion = e.FechaCreacion,
        FechaActualizacion = e.FechaActualizacion,
        Sectores = e.Sectores.Where(s => !s.EsBorrado).Select(MapToSectorResponseDTO).ToList()
    };

    private static EventoResumenDTO MapToResumenDTO(Evento e) => new()
    {
        Id = e.Id,
        Nombre = e.Nombre,
        FechaInicio = e.FechaInicio,
        FechaFin = e.FechaFin,
        Lugar = e.Lugar,
        Estado = e.Estado,
        TotalSectores = e.Sectores.Count(s => !s.EsBorrado),
        TotalEntradasVendidas = e.Sectores.Sum(s => s.EntradasVendidas),
        IngresosTotales = e.Sectores.Sum(s => s.EntradasVendidas * s.PrecioActual)
    };

    private static SectorResponseDTO MapToSectorResponseDTO(Sector s) => new()
    {
        Id = s.Id,
        Nombre = s.Nombre,
        Capacidad = s.Capacidad,
        PrecioBase = s.PrecioBase,
        PrecioActual = s.PrecioActual,
        EntradasVendidas = s.EntradasVendidas,
        EntradasDisponibles = s.StockDisponible,
        PorcentajeOcupacion = s.PorcentajeOcupacion
    };
}