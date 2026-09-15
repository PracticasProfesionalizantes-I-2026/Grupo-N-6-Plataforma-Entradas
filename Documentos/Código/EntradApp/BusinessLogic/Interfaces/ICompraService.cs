using EntradApp.Shared.Common;
using EntradApp.Shared.DTOs.Compra;

namespace EntradApp.BusinessLogic.Interfaces;

public interface ICompraService
{
    Task<Result<CompraResponseDTO>> CrearCompraAsync(Guid usuarioId, CompraCreateDTO dto);
    Task<Result<CompraResponseDTO>> ConfirmarPagoAsync(Guid usuarioId, Guid compraId);
    Task<Result> CancelarCompraAsync(Guid usuarioId, Guid compraId);
    Task<Result<CompraResponseDTO>> GetByIdAsync(Guid compraId);
    Task<PagedResult<CompraResponseDTO>> GetByUsuarioAsync(Guid usuarioId, int page, int pageSize);
}