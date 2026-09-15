namespace EntradApp.Shared.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
    public UnauthorizedException() : base("No autorizado. Token JWT inválido, expirado o ausente.") { }
}