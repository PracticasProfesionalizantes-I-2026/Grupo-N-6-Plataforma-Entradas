using EntradApp.Shared.Common;
using EntradApp.Shared.DTOs.Admin;
using EntradApp.Shared.Enums;

namespace EntradApp.BusinessLogic.Interfaces;

public interface IAdminService
{
    Task<Result<AdminEventosPendientesResponseDTO>> GetEventosPendientesAsync(int page, int pageSize);
    Task<Result> AprobarEventoAsync(Guid adminId, Guid eventoId, string? observaciones);
    Task<Result> RechazarEventoAsync(Guid adminId, Guid eventoId, string motivo);
}