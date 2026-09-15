using EntradApp.Shared.Common;

namespace EntradApp.BusinessLogic.Interfaces;

public interface IValidacionCompraService
{
    Task<Result> ValidarDisponibilidadAsync(Guid sectorId, int cantidad);
    Task<Result> ValidarLimiteMaximoAsync(Guid usuarioId, Guid eventoId, int cantidad);
    Task<Result> ValidarDnisDuplicadosAsync(Guid eventoId, IEnumerable<string> dnis);
}