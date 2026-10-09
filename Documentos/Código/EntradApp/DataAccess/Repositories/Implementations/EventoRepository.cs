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
            .AsNoTracking()
            .Include(e => e.Sectores.Where(s => !s.EsBorrado))
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<PagedResult<Evento>> GetAprobadosAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.Eventos
            .AsNoTracking()
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

    public async Task<PagedResult<Evento>> GetAprobadosConFiltrosAsync(
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        string? lugar,
        decimal? precioMin,
        decimal? precioMax,
        string? sortBy,
        bool sortDesc,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _context.Eventos
            .AsNoTracking()
            .Where(e => e.Estado == EstadoEvento.Aprobado);

        // Filtro fecha desde
        if (fechaDesde.HasValue)
            query = query.Where(e => e.FechaInicio >= fechaDesde.Value);

        // Filtro fecha hasta
        if (fechaHasta.HasValue)
            query = query.Where(e => e.FechaFin <= fechaHasta.Value);

        // Filtro lugar
        if (!string.IsNullOrWhiteSpace(lugar))
            query = query.Where(e => e.Lugar.ToLower().Contains(lugar.Trim().ToLower()));

        // Filtro precio (evento aparece si ALGÚN sector está en rango)
        if (precioMin.HasValue || precioMax.HasValue)
        {
            query = query.Where(e => e.Sectores.Any(s =>
                (!precioMin.HasValue || s.PrecioActual >= precioMin.Value) &&
                (!precioMax.HasValue || s.PrecioActual <= precioMax.Value)));
        }

        // Ordenamiento
        query = AplicarOrdenamiento(query, sortBy, sortDesc);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(e => e.Sectores.Where(s => !s.EsBorrado))
            .ToListAsync(ct);

        return new PagedResult<Evento>(items, page, pageSize, total);
    }

    private static IQueryable<Evento> AplicarOrdenamiento(IQueryable<Evento> query, string? sortBy, bool sortDesc)
    {
        var desc = sortDesc;
        return (sortBy?.ToLower()) switch
        {
            "fecha" => desc ? query.OrderByDescending(e => e.FechaInicio) : query.OrderBy(e => e.FechaInicio),
            "nombre" => desc ? query.OrderByDescending(e => e.Nombre) : query.OrderBy(e => e.Nombre),
            "precio" => desc
                ? query.OrderByDescending(e => e.Sectores.Where(s => !s.EsBorrado).Min(s => s.PrecioActual))
                : query.OrderBy(e => e.Sectores.Where(s => !s.EsBorrado).Min(s => s.PrecioActual)),
            _ => query.OrderByDescending(e => e.FechaInicio) // Default: fecha desc
        };
    }

    public async Task<PagedResult<Evento>> GetByCreadorAsync(Guid usuarioId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.Eventos
            .AsNoTracking()
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
            .AsNoTracking()
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