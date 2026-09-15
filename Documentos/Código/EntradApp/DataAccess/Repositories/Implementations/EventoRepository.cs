using EntradApp.DataAccess.Entities;
using EntradApp.DataAccess.Repositories.Interfaces;
using EntradApp.Shared.Common;
using EntradApp.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace EntradApp.DataAccess.Repositories.Implementations;

public class EventoRepository : IEventoRepository
{
    private readonly EntradAppDbContext _context;

    public EventoRepository(EntradAppDbContext context) => _context = context;

    public async Task<Evento?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Eventos
            .Include(e => e.Sectores.Where(s => !s.EsBorrado))
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<PagedResult<Evento>> GetAprobadosAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.Eventos
            .Where(e => e.Estado == EstadoEvento.Aprobado)
            .OrderByDescending(e => e.FechaInicio);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(e => e.Sectores.Where(s => !s.EsBorrado))
            .ToListAsync(ct);

        return new PagedResult<Evento>(items, page, pageSize, total);
    }

    public async Task<PagedResult<Evento>> GetByCreadorAsync(Guid usuarioId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.Eventos
            .Where(e => e.UsuarioCreadorId == usuarioId)
            .OrderByDescending(e => e.FechaCreacion);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(e => e.Sectores.Where(s => !s.EsBorrado))
            .ToListAsync(ct);

        return new PagedResult<Evento>(items, page, pageSize, total);
    }

    public async Task<PagedResult<Evento>> GetPendientesAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.Eventos
            .Where(e => e.Estado == EstadoEvento.PendienteAprobacion)
            .OrderBy(e => e.FechaCreacion);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(e => e.UsuarioCreador)
            .Include(e => e.Sectores.Where(s => !s.EsBorrado))
            .ToListAsync(ct);

        return new PagedResult<Evento>(items, page, pageSize, total);
    }

    public async Task<Result<Evento>> CreateAsync(Evento evento, CancellationToken ct = default)
    {
        _context.Eventos.Add(evento);
        await _context.SaveChangesAsync(ct);
        return Result.Success(evento);
    }

    public async Task<Result> UpdateAsync(Evento evento, CancellationToken ct = default)
    {
        _context.Eventos.Update(evento);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var evento = await _context.Eventos.FindAsync([id], ct);
        if (evento == null) return Result.Failure("Evento no encontrado");
        
        evento.EsBorrado = true;
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<bool> ExisteAsync(Guid id, CancellationToken ct = default)
        => await _context.Eventos.AnyAsync(e => e.Id == id, ct);
}