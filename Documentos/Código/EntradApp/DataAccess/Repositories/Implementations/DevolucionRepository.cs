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
            .Include(d => d.Entrada)
            .ThenInclude(e => e!.Compra)
            .FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<Devolucion?> GetByEntradaIdAsync(Guid entradaId, CancellationToken ct = default)
        => await _context.Devoluciones
            .Include(d => d.Entrada)
            .ThenInclude(e => e!.Compra)
            .FirstOrDefaultAsync(d => d.EntradaId == entradaId, ct);

    public async Task<Result<Devolucion>> CreateAsync(Devolucion devolucion, CancellationToken ct = default)
    {
        _context.Devoluciones.Add(devolucion);
        await _context.SaveChangesAsync(ct);
        return Result.Success(devolucion);
    }
}