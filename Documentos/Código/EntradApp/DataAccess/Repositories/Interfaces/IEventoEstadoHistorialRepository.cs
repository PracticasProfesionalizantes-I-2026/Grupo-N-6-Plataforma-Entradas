using EntradApp.DataAccess.Entities;
using EntradApp.Shared.Common;

namespace EntradApp.DataAccess.Repositories.Interfaces;

public interface IEventoEstadoHistorialRepository
{
    Task<List<EventoEstadoHistorial>> GetByEventoAsync(Guid eventoId, CancellationToken ct = default);
    Task<Result<EventoEstadoHistorial>> CreateAsync(EventoEstadoHistorial historial, CancellationToken ct = default);
}