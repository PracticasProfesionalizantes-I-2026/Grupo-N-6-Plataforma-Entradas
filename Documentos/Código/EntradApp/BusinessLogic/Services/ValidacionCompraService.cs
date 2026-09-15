using EntradApp.BusinessLogic.Interfaces;
using EntradApp.DataAccess.Repositories.Interfaces;
using EntradApp.Shared.Common;
using EntradApp.Shared.Exceptions;

namespace EntradApp.BusinessLogic.Services;

public class ValidacionCompraService : IValidacionCompraService
{
    private readonly ISectorRepository _sectorRepository;
    private readonly ICompraRepository _compraRepository;
    private readonly IEntradaRepository _entradaRepository;

    public ValidacionCompraService(
        ISectorRepository sectorRepository,
        ICompraRepository compraRepository,
        IEntradaRepository entradaRepository)
    {
        _sectorRepository = sectorRepository;
        _compraRepository = compraRepository;
        _entradaRepository = entradaRepository;
    }

    public async Task<Result> ValidarDisponibilidadAsync(Guid sectorId, int cantidad)
    {
        var stock = await _sectorRepository.GetStockDisponibleAsync(sectorId);
        if (stock < cantidad)
            return Result.Failure($"Stock insuficiente. Disponible: {stock}, Solicitado: {cantidad}");
        return Result.Success();
    }

    public async Task<Result> ValidarLimiteMaximoAsync(Guid usuarioId, Guid eventoId, int cantidad)
    {
        const int LIMITE_MAXIMO = 4;
        var actuales = await _compraRepository.ContarEntradasUsuarioEventoAsync(usuarioId, eventoId);
        if (actuales + cantidad > LIMITE_MAXIMO)
            return Result.Failure($"Límite de compra excedido. Máximo: {LIMITE_MAXIMO}, Actual: {actuales}, Solicitado: {cantidad}");
        return Result.Success();
    }

    public async Task<Result> ValidarDnisDuplicadosAsync(Guid eventoId, IEnumerable<string> dnis)
    {
        var listaDnis = dnis.Select(d => d.Trim()).ToList();
        
        // Duplicados en la lista actual
        var duplicadosEnLista = listaDnis.GroupBy(d => d).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicadosEnLista.Count > 0)
            return Result.Failure($"DNI duplicados en la lista: {string.Join(", ", duplicadosEnLista)}");

        // Duplicados en BD
        foreach (var dni in listaDnis)
        {
            var existe = await _entradaRepository.ContarPorDniYEventoAsync(dni, eventoId);
            if (existe > 0)
                return Result.Failure($"El DNI '{dni}' ya posee una entrada para este evento");
        }

        return Result.Success();
    }
}