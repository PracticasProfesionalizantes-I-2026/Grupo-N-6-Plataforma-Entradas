
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
        => await _context.Sectores
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<List<Sector>> GetByEventoAsync(
        Guid eventoId, CancellationToken ct = default)
        => await _context.Sectores
            .AsNoTracking()
            .Where(s => s.EventoId == eventoId)
            .ToListAsync(ct);

    public async Task<Sector?> GetByIdWithEventoAsync(
        Guid id, CancellationToken ct = default)
        => await _context.Sectores
            .AsNoTracking()
            .Include(s => s.Evento)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<Result<Sector>> CreateAsync(
        Sector sector, CancellationToken ct = default)
    {
        _context.Sectores.Add(sector);
        await _context.SaveChangesAsync(ct);
        return Result.Success(sector);
    }

    public async Task<Result> UpdateAsync(
        Sector sector, CancellationToken ct = default)
    {
        _context.Sectores.Update(sector);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(
        Guid id, CancellationToken ct = default)
    {
        var sector = await _context.Sectores.FindAsync([id], ct);

        if (sector == null)
            return Result.Failure("Sector no encontrado");

        sector.EsBorrado = true;

        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result> IncrementarVendidasAsync(
        Guid sectorId, int cantidad, CancellationToken ct = default)
    {
        var filasAfectadas = await _context.Sectores
            .Where(s => s.Id == sectorId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    s => s.EntradasVendidas,
                    s => s.EntradasVendidas + cantidad),
                ct);

        if (filasAfectadas == 0)
            return Result.Failure("Sector no encontrado");

        return Result.Success();
    }

    public async Task<Result> IncrementarReservadasAsync(
        Guid sectorId, int cantidad, CancellationToken ct = default)
    {
        if (cantidad <= 0)
            return Result.Failure("La cantidad debe ser mayor a cero");

        var filasAfectadas = await _context.Sectores
            .Where(s =>
                s.Id == sectorId &&
                !s.EsBorrado &&
                s.Capacidad - s.EntradasVendidas - s.EntradasReservadas >= cantidad)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    s => s.EntradasReservadas,
                    s => s.EntradasReservadas + cantidad),
                ct);

        if (filasAfectadas == 0)
            return Result.Failure(
                "Stock insuficiente para realizar la reserva");

        return Result.Success();
    }

    public async Task<Result> DecrementarReservadasAsync(
        Guid sectorId, int cantidad, CancellationToken ct = default)
    {
        var filasAfectadas = await _context.Sectores
            .Where(s => s.Id == sectorId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    s => s.EntradasReservadas,
                    s => s.EntradasReservadas >= cantidad
                        ? s.EntradasReservadas - cantidad
                        : 0),
                ct);

        if (filasAfectadas == 0)
            return Result.Failure("Sector no encontrado");

        return Result.Success();
    }

    public async Task<int> GetStockDisponibleAsync(
        Guid sectorId, CancellationToken ct = default)
    {
        var sector = await _context.Sectores
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sectorId, ct);

        return sector?.StockDisponible ?? 0;
    }
}
