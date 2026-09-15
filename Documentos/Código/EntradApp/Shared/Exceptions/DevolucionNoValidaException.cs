namespace EntradApp.Shared.Exceptions;

public class DevolucionNoValidaException : ConflictException
{
    public DevolucionNoValidaException(string message) : base(message) { }
    public DevolucionNoValidaException(string motivo, bool isMotivo) : base($"La devolución no puede realizarse: {motivo}") { }
}