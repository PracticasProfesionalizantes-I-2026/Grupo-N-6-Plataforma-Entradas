using EntradApp.Shared.Common;
using EntradApp.Shared.DTOs.Entrada;

namespace EntradApp.BusinessLogic.Interfaces;

public interface IEntradaService
{
    Task<PagedResult<EntradaResponseDTO>> GetMisEntradasAsync(Guid usuarioId, int page, int pageSize);
    Task<PagedResult<EntradaResponseDTO>> GetUtilizadasAsync(Guid usuarioId, int page, int pageSize);
    Task<PagedResult<EntradaResponseDTO>> GetDevueltasAsync(Guid usuarioId, int page, int pageSize);
    Task<byte[]> GenerarComprobanteAsync(Guid usuarioId, Guid entradaId);
    Task<Result<EntradaResponseDTO>> ValidarEntradaAsync(string codigoUnico);
}