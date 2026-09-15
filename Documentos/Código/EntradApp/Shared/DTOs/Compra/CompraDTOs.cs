using System.ComponentModel.DataAnnotations;
using EntradApp.Shared.Enums;

namespace EntradApp.Shared.DTOs.Compra;

public class CompraCreateDTO
{
    [Required]
    public Guid EventoId { get; set; }

    [Required]
    public Guid SectorId { get; set; }

    [Required]
    [Range(1, 4, ErrorMessage = "La cantidad debe ser entre 1 y 4 entradas.")]
    public int Cantidad { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Debe proporcionar un DNI por cada entrada.")]
    public List<string> Dnis { get; set; } = new();
}

public class CompraConfirmarPagoDTO
{
    // En MVP solo efectivo, no se requieren datos adicionales
}

public class EntradaEnComprobanteDTO
{
    public Guid Id { get; set; }
    public string Dni { get; set; } = string.Empty;
    public string CodigoUnico { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;
    public decimal Precio { get; set; }
}

public class CompraResponseDTO
{
    public Guid Id { get; set; }
    public Guid EventoId { get; set; }
    public string EventoNombre { get; set; } = string.Empty;
    public Guid SectorId { get; set; }
    public string SectorNombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Total { get; set; }
    public EstadoCompra Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaPago { get; set; }
    public List<EntradaEnComprobanteDTO> Entradas { get; set; } = new();
    public string CodigoQR { get; set; } = string.Empty;
}

public class CompraResumenDTO
{
    public Guid Id { get; set; }
    public string EventoNombre { get; set; } = string.Empty;
    public string SectorNombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Total { get; set; }
    public EstadoCompra Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
}