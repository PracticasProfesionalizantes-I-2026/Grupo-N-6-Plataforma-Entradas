using EntradApp.DataAccess.Entities;
using EntradApp.Shared.Common;

namespace EntradApp.DataAccess.Repositories.Interfaces;

public interface IDevolucionRepository
{
    Task<Devolucion?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Devolucion?> GetByEntradaIdAsync(Guid entradaId, CancellationToken ct = default);
    Task<PagedResult<Devolucion>> GetByUsuarioAsync(Guid usuarioId, int page, int pageSize, CancellationToken ct = default);
    Task<Result<Devolucion>> CreateAsync(Devolucion devolucion, CancellationToken ct = default);
    Task<Result> UpdateAsync(Devolucion devolucion, CancellationToken ct = default);
}