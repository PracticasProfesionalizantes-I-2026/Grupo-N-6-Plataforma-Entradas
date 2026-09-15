using EntradApp.DataAccess.Entities;
using EntradApp.DataAccess.Repositories.Interfaces;
using EntradApp.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace EntradApp.DataAccess.Repositories.Implementations;

public class ReservaTemporalRepository : IReservaTemporalRepository
{
    private readonly EntradAppDbContext _context;

    public ReservaTemporalRepository(EntradAppDbContext context) => _context = context;

    public async Task<ReservaTemporal?> GetByCompraIdAsync(Guid compraId, CancellationToken ct = default)
        => await _context.ReservasTemporales
            .FirstOrDefaultAsync(r => r.CompraId == compraId, ct);

    public async Task<List<ReservaTemporal>> GetExpiradasAsync(CancellationToken ct = default)
        => await _context.ReservasTemporales
            .Where(r => !r.Expirada && r.ExpiraEn <= DateTime.UtcNow)
            .ToListAsync(ct);

    public async Task<Result<ReservaTemporal>> CreateAsync(ReservaTemporal reserva, CancellationToken ct = default)
    {
        _context.ReservasTemporales.Add(reserva);
        await _context.SaveChangesAsync(ct);
        return Result.Success(reserva);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var reserva = await _context.ReservasTemporales.FindAsync([id], ct);
        if (reserva == null) return Result.Failure("Reserva no encontrada");
        
        _context.ReservasTemporales.Remove(reserva);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> MarcarExpiradaAsync(Guid id, CancellationToken ct = default)
    {
        var reserva = await _context.ReservasTemporales.FindAsync([id], ct);
        if (reserva == null) return Result.Failure("Reserva no encontrada");
        
        reserva.Expirada = true;
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}