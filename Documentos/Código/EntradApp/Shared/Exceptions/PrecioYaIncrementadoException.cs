namespace EntradApp.Shared.Exceptions;

public class PrecioYaIncrementadoException : ConflictException
{
    public PrecioYaIncrementadoException() : base("El precio del sector ya fue incrementado. No se aplica doble incremento (idempotencia).") { }
}