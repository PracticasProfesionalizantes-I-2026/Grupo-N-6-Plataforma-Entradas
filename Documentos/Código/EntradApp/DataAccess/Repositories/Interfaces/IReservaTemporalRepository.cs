using EntradApp.DataAccess.Entities;
using EntradApp.Shared.Common;

namespace EntradApp.DataAccess.Repositories.Interfaces;

public interface IReservaTemporalRepository
{
    Task<ReservaTemporal?> GetByCompraIdAsync(Guid compraId, CancellationToken ct = default);
    Task<List<ReservaTemporal>> GetExpiradasAsync(CancellationToken ct = default);
    Task<Result<ReservaTemporal>> CreateAsync(ReservaTemporal reserva, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<Result> MarcarExpiradaAsync(Guid id, CancellationToken ct = default);
}