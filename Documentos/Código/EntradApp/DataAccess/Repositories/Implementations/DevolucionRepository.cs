using EntradApp.DataAccess.Entities;
using EntradApp.DataAccess.Repositories.Interfaces;
using EntradApp.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace EntradApp.DataAccess.Repositories.Implementations;

public class DevolucionRepository : IDevolucionRepository
{
    private readonly EntradAppDbContext _context;

    public DevolucionRepository(EntradAppDbContext context) => _context = context;

    public async Task<Devolucion?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Devoluciones
            .AsNoTracking()
            .Include(d => d.Entrada)
            .ThenInclude(e => e!.Compra)
            .ThenInclude(c => c!.Evento)
            .FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<Devolucion?> GetByEntradaIdAsync(Guid entradaId, CancellationToken ct = default)
        => await _context.Devoluciones
            .AsNoTracking()
            .Include(d => d.Entrada)
            .ThenInclude(e => e!.Compra)
            .FirstOrDefaultAsync(d => d.EntradaId == entradaId, ct);

    public async Task<PagedResult<Devolucion>> GetByUsuarioAsync(Guid usuarioId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.Devoluciones
            .AsNoTracking()
            .Where(d => d.Entrada!.Compra!.UsuarioId == usuarioId)
            .Include(d => d.Entrada)
            .ThenInclude(e => e!.Compra)
            .ThenInclude(c => c!.Evento)
            .OrderByDescending(d => d.FechaSolicitud);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Devolucion>(items, page, pageSize, total);
    }

    public async Task<Result<Devolucion>> CreateAsync(Devolucion devolucion, CancellationToken ct = default)
    {
        _context.Devoluciones.Add(devolucion);
        await _context.SaveChangesAsync(ct);
        return Result.Success(devolucion);
    }

    public async Task<Result> UpdateAsync(Devolucion devolucion, CancellationToken ct = default)
    {
        _context.Devoluciones.Update(devolucion);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}