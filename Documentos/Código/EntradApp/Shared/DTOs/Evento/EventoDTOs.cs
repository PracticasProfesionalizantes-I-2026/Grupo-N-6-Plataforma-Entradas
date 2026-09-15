using System.ComponentModel.DataAnnotations;
using EntradApp.Shared.Enums;

namespace EntradApp.Shared.DTOs.Evento;

public class SectorCreateDTO
{
    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "La capacidad debe ser mayor a 0.")]
    public int Capacidad { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "El precio base debe ser mayor o igual a 0.")]
    public decimal PrecioBase { get; set; }
}

public class EventoCreateDTO
{
    [Required]
    [MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Descripcion { get; set; }

    [Required]
    public DateTime FechaInicio { get; set; }

    [Required]
    public DateTime FechaFin { get; set; }

    [Required]
    [MaxLength(200)]
    public string Lugar { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string Direccion { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "Debe haber al menos un sector.")]
    public List<SectorCreateDTO> Sectores { get; set; } = new();
}

public class EventoUpdateDTO
{
    [MaxLength(200)]
    public string? Nombre { get; set; }

    [MaxLength(2000)]
    public string? Descripcion { get; set; }

    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    [MaxLength(200)]
    public string? Lugar { get; set; }

    [MaxLength(300)]
    public string? Direccion { get; set; }
}

public class SectorResponseDTO
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Capacidad { get; set; }
    public decimal PrecioBase { get; set; }
    public decimal PrecioActual { get; set; }
    public int EntradasVendidas { get; set; }
    public int EntradasDisponibles { get; set; }
    public double PorcentajeOcupacion { get; set; }
}

public class EventoResponseDTO
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DateTime FechaInicio { get; set; }
public DateTime FechaFin { get; set; }
    public string Lugar { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public EstadoEvento Estado { get; set; }
    public Guid UsuarioCreadorId { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public List<SectorResponseDTO> Sectores { get; set; } = new();
}

public class EventoResumenDTO
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string Lugar { get; set; } = string.Empty;
    public EstadoEvento Estado { get; set; }
    public int TotalSectores { get; set; }
    public int TotalEntradasVendidas { get; set; }
    public decimal IngresosTotales { get; set; }
}