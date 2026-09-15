namespace EntradApp.Shared.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
    public ForbiddenException() : base("Acceso denegado. Se requiere rol SuperAdmin.") { }
}