using EntradApp.DataAccess.Entities;
using EntradApp.Shared.Common;
using EntradApp.Shared.Enums;

namespace EntradApp.DataAccess.Repositories.Interfaces;

public interface IEventoRepository
{
    Task<Evento?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<Evento>> GetAprobadosAsync(int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<Evento>> GetByCreadorAsync(Guid usuarioId, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<Evento>> GetPendientesAsync(int page, int pageSize, CancellationToken ct = default);
    Task<Result<Evento>> CreateAsync(Evento evento, CancellationToken ct = default);
    Task<Result> UpdateAsync(Evento evento, CancellationToken ct = default);
    Task<Result> SoftDeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExisteAsync(Guid id, CancellationToken ct = default);
}