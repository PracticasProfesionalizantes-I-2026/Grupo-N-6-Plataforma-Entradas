using EntradApp.DataAccess.Entities;
using EntradApp.Shared.Common;

namespace EntradApp.DataAccess.Repositories.Interfaces;

public interface IDevolucionRepository
{
    Task<Devolucion?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Devolucion?> GetByEntradaIdAsync(Guid entradaId, CancellationToken ct = default);
    Task<Result<Devolucion>> CreateAsync(Devolucion devolucion, CancellationToken ct = default);
}