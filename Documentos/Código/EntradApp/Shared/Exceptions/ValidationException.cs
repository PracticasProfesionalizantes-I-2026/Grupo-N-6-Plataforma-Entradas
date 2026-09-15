namespace EntradApp.Shared.Exceptions;

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
    public ValidationException(string field, string message) : base($"Campo '{field}': {message}") { }
}