namespace EntradApp.Shared.Exceptions;

public class ReservaExpiradaException : ConflictException
{
    public ReservaExpiradaException() : base("La reserva temporal ha expirado. Intente nuevamente.") { }
    public ReservaExpiradaException(string message) : base(message) { }
}