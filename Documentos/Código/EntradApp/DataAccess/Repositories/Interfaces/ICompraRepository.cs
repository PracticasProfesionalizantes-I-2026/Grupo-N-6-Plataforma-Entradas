using EntradApp.DataAccess.Entities;
using EntradApp.Shared.Common;
using EntradApp.Shared.Enums;

namespace EntradApp.DataAccess.Repositories.Interfaces;

public interface ICompraRepository
{
    Task<Compra?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<Compra>> GetByUsuarioAsync(Guid usuarioId, int page, int pageSize, CancellationToken ct = default);
    Task<Result<Compra>> CreateAsync(Compra compra, CancellationToken ct = default);
    Task<Result> UpdateEstadoAsync(Guid compraId, EstadoCompra nuevoEstado, CancellationToken ct = default);
    Task<int> ContarEntradasUsuarioEventoAsync(Guid usuarioId, Guid eventoId, CancellationToken ct = default);
}