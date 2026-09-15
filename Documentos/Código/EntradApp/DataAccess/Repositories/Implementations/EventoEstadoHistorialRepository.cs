using EntradApp.DataAccess.Entities;
using EntradApp.DataAccess.Repositories.Interfaces;
using EntradApp.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace EntradApp.DataAccess.Repositories.Implementations;

public class EventoEstadoHistorialRepository : IEventoEstadoHistorialRepository
{
    private readonly EntradAppDbContext _context;

    public EventoEstadoHistorialRepository(EntradAppDbContext context) => _context = context;

    public async Task<List<EventoEstadoHistorial>> GetByEventoAsync(Guid eventoId, CancellationToken ct = default)
        => await _context.EventosEstadosHistorial
            .Where(h => h.EventoId == eventoId)
            .OrderBy(h => h.FechaCambio)
            .ToListAsync(ct);

    public async Task<Result<EventoEstadoHistorial>> CreateAsync(EventoEstadoHistorial historial, CancellationToken ct = default)
    {
        _context.EventosEstadosHistorial.Add(historial);
        await _context.SaveChangesAsync(ct);
        return Result.Success(historial);
    }
}