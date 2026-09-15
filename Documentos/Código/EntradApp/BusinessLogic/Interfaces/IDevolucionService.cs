using EntradApp.Shared.Common;
using EntradApp.Shared.DTOs.Devolucion;

namespace EntradApp.BusinessLogic.Interfaces;

public interface IDevolucionService
{
    Task<Result<DevolucionResponseDTO>> SolicitarDevolucionAsync(Guid usuarioId, DevolucionCreateDTO dto);
    Task<PagedResult<DevolucionResumenDTO>> GetMisDevolucionesAsync(Guid usuarioId, int page, int pageSize);
}