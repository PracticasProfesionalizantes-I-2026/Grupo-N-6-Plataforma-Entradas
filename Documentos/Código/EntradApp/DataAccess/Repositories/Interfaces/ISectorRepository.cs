using EntradApp.DataAccess.Entities;
using EntradApp.Shared.Common;

namespace EntradApp.DataAccess.Repositories.Interfaces;

public interface ISectorRepository
{
    Task<Sector?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Sector>> GetByEventoAsync(Guid eventoId, CancellationToken ct = default);
    Task<Sector?> GetByIdWithEventoAsync(Guid id, CancellationToken ct = default);
    Task<Result<Sector>> CreateAsync(Sector sector, CancellationToken ct = default);
    Task<Result> UpdateAsync(Sector sector, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<Result> IncrementarVendidasAsync(Guid sectorId, int cantidad, CancellationToken ct = default);
    Task<Result> IncrementarReservadasAsync(Guid sectorId, int cantidad, CancellationToken ct = default);
    Task<Result> DecrementarReservadasAsync(Guid sectorId, int cantidad, CancellationToken ct = default);
    Task<int> GetStockDisponibleAsync(Guid sectorId, CancellationToken ct = default);
}