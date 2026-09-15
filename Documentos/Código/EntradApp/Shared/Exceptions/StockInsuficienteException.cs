namespace EntradApp.Shared.Exceptions;

public class StockInsuficienteException : ConflictException
{
    public int StockDisponible { get; }
    public int StockSolicitado { get; }

    public StockInsuficienteException(int stockDisponible, int stockSolicitado) 
        : base($"Stock insuficiente. Disponible: {stockDisponible}, Solicitado: {stockSolicitado}.")
    {
        StockDisponible = stockDisponible;
        StockSolicitado = stockSolicitado;
    }
}