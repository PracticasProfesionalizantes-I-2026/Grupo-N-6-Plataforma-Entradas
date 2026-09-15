namespace EntradApp.Shared.Exceptions;

public class ConcurrencyException : ConflictException
{
    public ConcurrencyException(string message) : base(message) { }
    public ConcurrencyException() : base("Conflicto de concurrencia: el recurso fue modificado por otra operación. Intente nuevamente.") { }
}