namespace EntradApp.Shared.Exceptions;

public class DniDuplicadoException : ConflictException
{
    public string Dni { get; }

    public DniDuplicadoException(string dni) 
        : base($"El DNI '{dni}' ya posee una entrada para este evento.")
    {
        Dni = dni;
    }
}