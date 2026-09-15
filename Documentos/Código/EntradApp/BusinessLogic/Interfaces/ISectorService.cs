using EntradApp.Shared.Common;
using EntradApp.Shared.DTOs.Sector;

namespace EntradApp.BusinessLogic.Interfaces;

public interface ISectorService
{
    Task<Result<SectorResponseDTO>> CrearAsync(Guid usuarioId, Guid eventoId, SectorCreateDTO dto);
    Task<Result<SectorResponseDTO>> ActualizarAsync(Guid usuarioId, Guid sectorId, SectorUpdateDTO dto);
    Task<Result> EliminarAsync(Guid usuarioId, Guid sectorId);
}