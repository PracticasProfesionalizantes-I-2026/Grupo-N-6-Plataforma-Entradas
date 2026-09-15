using EntradApp.DataAccess.Entities;
using EntradApp.DataAccess.Repositories.Interfaces;
using EntradApp.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace EntradApp.DataAccess.Repositories.Implementations;

public class SectorRepository : ISectorRepository
{
    private readonly EntradAppDbContext _context;

    public SectorRepository(EntradAppDbContext context) => _context = context;

    public async Task<Sector?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Sectores.FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<List<Sector>> GetByEventoAsync(Guid eventoId, CancellationToken ct = default)
        => await _context.Sectores.Where(s => s.EventoId == eventoId).ToListAsync(ct);

    public async Task<Sector?> GetByIdWithEventoAsync(Guid id, CancellationToken ct = default)
        => await _context.Sectores
            .Include(s => s.Evento)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<Result<Sector>> CreateAsync(Sector sector, CancellationToken ct = default)
    {
        _context.Sectores.Add(sector);
        await _context.SaveChangesAsync(ct);
        return Result.Success(sector);
    }

    public async Task<Result> UpdateAsync(Sector sector, CancellationToken ct = default)
    {
        _context.Sectores.Update(sector);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var sector = await _context.Sectores.FindAsync([id], ct);
        if (sector == null) return Result.Failure("Sector no encontrado");
        
        sector.EsBorrado = true;
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> IncrementarVendidasAsync(Guid sectorId, int cantidad, CancellationToken ct = default)
    {
        var sector = await _context.Sectores.FindAsync([sectorId], ct);
        if (sector == null) return Result.Failure("Sector no encontrado");
        
        sector.EntradasVendidas += cantidad;
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> IncrementarReservadasAsync(Guid sectorId, int cantidad, CancellationToken ct = default)
    {
        var sector = await _context.Sectores.FindAsync([sectorId], ct);
        if (sector == null) return Result.Failure("Sector no encontrado");
        
        sector.EntradasReservadas += cantidad;
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> DecrementarReservadasAsync(Guid sectorId, int cantidad, CancellationToken ct = default)
    {
        var sector = await _context.Sectores.FindAsync([sectorId], ct);
        if (sector == null) return Result.Failure("Sector no encontrado");
        
        sector.EntradasReservadas = Math.Max(0, sector.EntradasReservadas - cantidad);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<int> GetStockDisponibleAsync(Guid sectorId, CancellationToken ct = default)
    {
        var sector = await _context.Sectores.FindAsync([sectorId], ct);
        return sector?.StockDisponible ?? 0;
    }
}