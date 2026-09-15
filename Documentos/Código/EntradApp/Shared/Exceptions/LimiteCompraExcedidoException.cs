namespace EntradApp.Shared.Exceptions;

public class LimiteCompraExcedidoException : ConflictException
{
    public int LimiteMaximo { get; }
    public int Actual { get; }
    public int Solicitado { get; }

    public LimiteCompraExcedidoException(int limiteMaximo, int actual, int solicitado) 
        : base($"Límite de compra excedido. Máximo: {limiteMaximo}, Actual: {actual}, Solicitado: {solicitado}.")
    {
        LimiteMaximo = limiteMaximo;
        Actual = actual;
        Solicitado = solicitado;
    }
}