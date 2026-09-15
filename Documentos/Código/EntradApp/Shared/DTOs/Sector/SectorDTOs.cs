using System.ComponentModel.DataAnnotations;

namespace EntradApp.Shared.DTOs.Sector;

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

public class SectorUpdateDTO
{
    [MaxLength(100)]
    public string? Nombre { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La capacidad debe ser mayor a 0.")]
    public int? Capacidad { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El precio base debe ser mayor o igual a 0.")]
    public decimal? PrecioBase { get; set; }
}

public class SectorResponseDTO
{
    public Guid Id { get; set; }
    public Guid EventoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Capacidad { get; set; }
    public decimal PrecioBase { get; set; }
    public decimal PrecioActual { get; set; }
    public int EntradasVendidas { get; set; }
    public int EntradasReservadas { get; set; }
    public int EntradasDisponibles { get; set; }
    public double PorcentajeOcupacion { get; set; }
}

public class DisponibilidadSectorDTO
{
    public Guid SectorId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Capacidad { get; set; }
    public int EntradasVendidas { get; set; }
    public int EntradasReservadas { get; set; }
    public int StockDisponible { get; set; }
    public decimal PrecioActual { get; set; }
    public double PorcentajeOcupacion { get; set; }
}