namespace EntradApp.Shared.Exceptions;

public class SectorNoDisponibleException : Exception
{
    public SectorNoDisponibleException(string message) : base(message) { }
    public SectorNoDisponibleException() : base("Sector no disponible: no existe, no pertenece al evento, o el evento no está aprobado.") { }
}