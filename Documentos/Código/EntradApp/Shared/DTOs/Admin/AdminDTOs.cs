using System.ComponentModel.DataAnnotations;
using EntradApp.Shared.Enums;

namespace EntradApp.Shared.DTOs.Admin;

public class EventoEstadoUpdateDTO
{
    [Required]
    public EstadoEvento NuevoEstado { get; set; }

    [MaxLength(500)]
    public string? Observaciones { get; set; }
}

public class EventoPendienteResponseDTO
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string CreadorEmail { get; set; } = string.Empty;
    public string CreadorNombre { get; set; } = string.Empty;
    public DateTime FechaEvento { get; set; }
    public DateTime FechaSolicitud { get; set; }
    public int TotalSectores { get; set; }
    public int TotalCapacidad { get; set; }
}

public class EventoHistorialRevisionDTO
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string CreadorEmail { get; set; } = string.Empty;
    public EstadoEvento Estado { get; set; }
    public string AdminEmail { get; set; } = string.Empty;
    public DateTime FechaRevision { get; set; }
    public string? Observaciones { get; set; }
}

public class AdminEventosPendientesResponseDTO
{
    public List<EventoPendienteResponseDTO> Pendientes { get; set; } = new();
    public List<EventoHistorialRevisionDTO> Historial { get; set; } = new();
}