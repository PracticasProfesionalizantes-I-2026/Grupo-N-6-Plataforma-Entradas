namespace EntradApp.Shared.Exceptions;

public class InvalidStateTransitionException : ConflictException
{
    public InvalidStateTransitionException(string message) : base(message) { }
    public InvalidStateTransitionException(string estadoActual, string estadoIntentado) 
        : base($"Transición de estado no permitida: de '{estadoActual}' a '{estadoIntentado}'.") { }
}