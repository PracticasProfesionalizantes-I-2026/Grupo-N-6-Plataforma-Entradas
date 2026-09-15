namespace EntradApp.Shared.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string entityName, object id) : base($"{entityName} con ID '{id}' no encontrado.") { }
}