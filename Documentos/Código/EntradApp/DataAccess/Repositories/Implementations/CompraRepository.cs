using EntradApp.DataAccess.Entities;
using EntradApp.DataAccess.Repositories.Interfaces;
using EntradApp.Shared.Common;
using EntradApp.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace EntradApp.DataAccess.Repositories.Implementations;

public class CompraRepository : ICompraRepository
{
    private readonly EntradAppDbContext _context;

    public CompraRepository(EntradAppDbContext context) => _context = context;

    public async Task<Compra?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Compras
            .Include(c => c.Entradas.Where(e => !e.EsBorrado))
            .Include(c => c.Sector)
            .Include(c => c.Evento)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<PagedResult<Compra>> GetByUsuarioAsync(Guid usuarioId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.Compras
            .Where(c => c.UsuarioId == usuarioId)
            .OrderByDescending(c => c.FechaCreacion);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(c => c.Entradas.Where(e => !e.EsBorrado))
            .Include(c => c.Sector)
            .Include(c => c.Evento)
            .ToListAsync(ct);

        return new PagedResult<Compra>(items, page, pageSize, total);
    }

    public async Task<Result<Compra>> CreateAsync(Compra compra, CancellationToken ct = default)
    {
        _context.Compras.Add(compra);
        await _context.SaveChangesAsync(ct);
        return Result.Success(compra);
    }

    public async Task<Result> UpdateEstadoAsync(Guid compraId, EstadoCompra nuevoEstado, CancellationToken ct = default)
    {
        var compra = await _context.Compras.FindAsync([compraId], ct);
        if (compra == null) return Result.Failure("Compra no encontrada");
        
        compra.Estado = nuevoEstado;
        if (nuevoEstado == EntradApp.Shared.Enums.EstadoCompra.Aprobado)
            compra.FechaPago = DateTime.UtcNow;
        
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<int> ContarEntradasUsuarioEventoAsync(Guid usuarioId, Guid eventoId, CancellationToken ct = default)
    {
        return await _context.Entradas
            .Where(e => e.Compra.UsuarioId == usuarioId 
                     && e.Compra.EventoId == eventoId
                     && (e.Estado == EntradApp.Shared.Enums.EstadoEntrada.Activa 
                      || e.Estado == EntradApp.Shared.Enums.EstadoEntrada.Utilizada)
                     && !e.EsBorrado)
            .CountAsync(ct);
    }
}