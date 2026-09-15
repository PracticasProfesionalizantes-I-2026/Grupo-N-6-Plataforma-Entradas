using EntradApp.DataAccess.Entities;
using EntradApp.DataAccess.Repositories.Interfaces;
using EntradApp.Shared.Common;
using EntradApp.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace EntradApp.DataAccess.Repositories.Implementations;

public class EntradaRepository : IEntradaRepository
{
    private readonly EntradAppDbContext _context;

    public EntradaRepository(EntradAppDbContext context) => _context = context;

    public async Task<Entrada?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Entradas
            .Include(e => e.Compra)
            .Include(e => e.Sector)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<Entrada?> GetByCodigoUnicoAsync(string codigoUnico, CancellationToken ct = default)
        => await _context.Entradas
            .Include(e => e.Compra)
            .Include(e => e.Sector)
            .FirstOrDefaultAsync(e => e.CodigoUnico == codigoUnico, ct);

    public async Task<PagedResult<Entrada>> GetByUsuarioAsync(Guid usuarioId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.Entradas
            .Where(e => e.Compra.UsuarioId == usuarioId)
            .OrderByDescending(e => e.FechaCreacion);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(e => e.Compra)
            .Include(e => e.Sector)
            .ToListAsync(ct);

        return new PagedResult<Entrada>(items, page, pageSize, total);
    }

    public async Task<PagedResult<Entrada>> GetByCompraAsync(Guid compraId, CancellationToken ct = default)
    {
        var query = _context.Entradas.Where(e => e.CompraId == compraId);
        var total = await query.CountAsync(ct);
        var items = await query
            .Include(e => e.Compra)
            .Include(e => e.Sector)
            .ToListAsync(ct);

        return new PagedResult<Entrada>(items, 1, total, total);
    }

    public async Task<Result> CreateRangeAsync(IEnumerable<Entrada> entradas, CancellationToken ct = default)
    {
        await _context.Entradas.AddRangeAsync(entradas, ct);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> UpdateEstadoAsync(Guid entradaId, EstadoEntrada nuevoEstado, CancellationToken ct = default)
    {
        var entrada = await _context.Entradas.FindAsync([entradaId], ct);
        if (entrada == null) return Result.Failure("Entrada no encontrada");
        
        entrada.Estado = nuevoEstado;
        if (nuevoEstado == EstadoEntrada.Utilizada)
            entrada.FechaValidacion = DateTime.UtcNow;
        
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<int> ContarPorDniYEventoAsync(string dni, Guid eventoId, CancellationToken ct = default)
    {
        return await _context.Entradas
            .Where(e => e.Dni == dni 
                     && e.Compra.EventoId == eventoId
                     && (e.Estado == EstadoEntrada.Activa || e.Estado == EstadoEntrada.Utilizada)
                     && !e.EsBorrado)
            .CountAsync(ct);
    }
}