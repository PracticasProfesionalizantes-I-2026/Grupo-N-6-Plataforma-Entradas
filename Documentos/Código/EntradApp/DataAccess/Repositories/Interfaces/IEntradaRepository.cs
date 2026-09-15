using EntradApp.DataAccess.Entities;
using EntradApp.Shared.Common;
using EntradApp.Shared.Enums;

namespace EntradApp.DataAccess.Repositories.Interfaces;

public interface IEntradaRepository
{
    Task<Entrada?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Entrada?> GetByCodigoUnicoAsync(string codigoUnico, CancellationToken ct = default);
    Task<PagedResult<Entrada>> GetByUsuarioAsync(Guid usuarioId, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<Entrada>> GetByCompraAsync(Guid compraId, CancellationToken ct = default);
    Task<Result> CreateRangeAsync(IEnumerable<Entrada> entradas, CancellationToken ct = default);
    Task<Result> UpdateEstadoAsync(Guid entradaId, EstadoEntrada nuevoEstado, CancellationToken ct = default);
    Task<int> ContarPorDniYEventoAsync(string dni, Guid eventoId, CancellationToken ct = default);
}