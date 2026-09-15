using EntradApp.Shared.Common;
using EntradApp.Shared.DTOs.Evento;
using EntradApp.Shared.Enums;

namespace EntradApp.BusinessLogic.Interfaces;

public interface IEventoService
{
    Task<Result<EventoResponseDTO>> CrearAsync(Guid usuarioId, EventoCreateDTO dto);
    Task<Result<EventoResponseDTO>> GetByIdAsync(Guid id);
    Task<PagedResult<EventoResumenDTO>> GetAprobadosAsync(int page, int pageSize);
    Task<PagedResult<EventoResumenDTO>> GetMisEventosAsync(Guid usuarioId, int page, int pageSize);
    Task<Result<EventoResponseDTO>> ActualizarAsync(Guid usuarioId, Guid eventoId, EventoUpdateDTO dto);
    Task<Result> EliminarAsync(Guid usuarioId, Guid eventoId);
    Task<List<SectorResponseDTO>> GetSectoresAsync(Guid eventoId);
}