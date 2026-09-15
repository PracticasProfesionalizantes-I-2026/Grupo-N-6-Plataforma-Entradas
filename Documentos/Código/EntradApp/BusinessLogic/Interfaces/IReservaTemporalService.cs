using EntradApp.Shared.Common;

namespace EntradApp.BusinessLogic.Interfaces;

public interface IReservaTemporalService
{
    Task<Result> CrearReservaAsync(Guid compraId, Guid sectorId, int cantidad, int duracionMinutos);
    Task<Result> LiberarReservaAsync(Guid compraId);
    Task<Result> LimpiarReservasExpiradasAsync();
}