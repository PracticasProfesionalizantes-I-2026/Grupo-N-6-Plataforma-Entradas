using EntradApp.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace EntradApp.Shared.DTOs.Entrada;

public class EntradaResponseDTO
{
    public Guid Id { get; set; }
    public Guid CompraId { get; set; }
    public Guid SectorId { get; set; }
    public string SectorNombre { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public string CodigoUnico { get; set; } = string.Empty;
    public EstadoEntrada Estado { get; set; }
    public DateTime? FechaValidacion { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string? EventoNombre { get; set; }
    public DateTime? EventoFechaInicio { get; set; }
    public string? EventoLugar { get; set; }
}

public class ComprobanteDTO
{
    public Guid CompraId { get; set; }
    public string EventoNombre { get; set; } = string.Empty;
    public DateTime EventoFecha { get; set; }
    public string EventoLugar { get; set; } = string.Empty;
    public string SectorNombre { get; set; } = string.Empty;
    public List<EntradaEnComprobanteDTO> Entradas { get; set; } = new();
    public decimal Total { get; set; }
    public DateTime FechaCompra { get; set; }
    public string CodigoQR { get; set; } = string.Empty;
}

public class EntradaEnComprobanteDTO
{
    public string Dni { get; set; } = string.Empty;
    public string CodigoUnico { get; set; } = string.Empty;
    public decimal Precio { get; set; }
}

public class ValidarEntradaDTO
{
    public Guid EntradaId { get; set; }
    public string CodigoUnico { get; set; } = string.Empty;
}

public class ValidarControlAccesoRequestDTO
{
    [Required]
    public string CodigoUnico { get; set; } = string.Empty;
}

public class ValidarControlAccesoResponseDTO
{
    public bool Valido { get; set; }
    public string? Error { get; set; }
    public Guid? EntradaId { get; set; }
    public string? CodigoUnico { get; set; }
    public string? Dni { get; set; }
    public string? SectorNombre { get; set; }
    public string? EventoNombre { get; set; }
    public DateTime? EventoFechaInicio { get; set; }
    public string? EventoLugar { get; set; }
    public EstadoEntrada? EstadoAnterior { get; set; }
    public DateTime? FechaValidacion { get; set; }
}