using EntradApp.BusinessLogic.Interfaces;
using EntradApp.DataAccess.Entities;
using EntradApp.DataAccess.Repositories.Interfaces;
using EntradApp.Shared.Common;
using EntradApp.Shared.DTOs.Admin;
using EntradApp.Shared.Enums;
using EntradApp.Shared.Exceptions;

namespace EntradApp.BusinessLogic.Services;

public class AdminService : IAdminService
{
    private readonly IEventoRepository _eventoRepository;
    private readonly IEventoEstadoHistorialRepository _historialRepository;

    public AdminService(IEventoRepository eventoRepository, IEventoEstadoHistorialRepository historialRepository)
    {
        _eventoRepository = eventoRepository;
        _historialRepository = historialRepository;
    }

    public async Task<Result<AdminEventosPendientesResponseDTO>> GetEventosPendientesAsync(int page, int pageSize)
    {
        var result = await _eventoRepository.GetPendientesAsync(page, pageSize);
        var pendientes = result.Items.Select(e => new EventoPendienteResponseDTO
        {
            Id = e.Id,
            Nombre = e.Nombre,
            CreadorEmail = e.UsuarioCreador?.Email ?? "",
            CreadorNombre = $"{e.UsuarioCreador?.Nombre} {e.UsuarioCreador?.Apellido}",
            FechaEvento = e.FechaInicio,
            FechaSolicitud = e.FechaCreacion,
            TotalSectores = e.Sectores.Count(s => !s.EsBorrado),
            TotalCapacidad = e.Sectores.Sum(s => s.Capacidad)
        }).ToList();

        var historial = await _historialRepository.GetByEventoAsync(Guid.Empty);
        // En producción, obtener historial paginado por separado

        return Result.Success(new AdminEventosPendientesResponseDTO
        {
            Pendientes = pendientes,
            Historial = new List<EventoHistorialRevisionDTO>()
        });
    }

    public async Task<Result> AprobarEventoAsync(Guid adminId, Guid eventoId, string? observaciones)
    {
        var evento = await _eventoRepository.GetByIdAsync(eventoId);
        if (evento == null) return Result.Failure("Evento no encontrado");
        if (evento.Estado != EstadoEvento.PendienteAprobacion)
            return Result.Failure("El evento no está pendiente de aprobación");
        if (!evento.Sectores.Any(s => !s.EsBorrado))
            return Result.Failure("El evento debe tener al menos un sector para ser aprobado");

        var estadoAnterior = evento.Estado;
        evento.Estado = EstadoEvento.Aprobado;
        evento.FechaActualizacion = DateTime.UtcNow;

        await _eventoRepository.UpdateAsync(evento);

        var historial = new EventoEstadoHistorial
        {
            Id = Guid.NewGuid(),
            EventoId = eventoId,
            EstadoAnterior = estadoAnterior,
            EstadoNuevo = EstadoEvento.Aprobado,
            SuperAdminId = adminId,
            Observaciones = observaciones,
            FechaCambio = DateTime.UtcNow
        };

        await _historialRepository.CreateAsync(historial);
        return Result.Success();
    }

    public async Task<Result> RechazarEventoAsync(Guid adminId, Guid eventoId, string motivo)
    {
        var evento = await _eventoRepository.GetByIdAsync(eventoId);
        if (evento == null) return Result.Failure("Evento no encontrado");
        if (evento.Estado != EstadoEvento.PendienteAprobacion)
            return Result.Failure("El evento no está pendiente de aprobación");

        var estadoAnterior = evento.Estado;
        evento.Estado = EstadoEvento.Rechazado;
        evento.FechaActualizacion = DateTime.UtcNow;

        await _eventoRepository.UpdateAsync(evento);

        var historial = new EventoEstadoHistorial
        {
            Id = Guid.NewGuid(),
            EventoId = eventoId,
            EstadoAnterior = estadoAnterior,
            EstadoNuevo = EstadoEvento.Rechazado,
            SuperAdminId = adminId,
            Observaciones = motivo,
            FechaCambio = DateTime.UtcNow
        };

        await _historialRepository.CreateAsync(historial);
        return Result.Success();
    }
}