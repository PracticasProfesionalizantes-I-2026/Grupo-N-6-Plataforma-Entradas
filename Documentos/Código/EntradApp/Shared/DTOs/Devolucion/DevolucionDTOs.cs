using System.ComponentModel.DataAnnotations;
using EntradApp.Shared.Enums;

namespace EntradApp.Shared.DTOs.Devolucion;

public class DevolucionCreateDTO
{
    [Required]
    public Guid EntradaId { get; set; }
}

public class DevolucionResponseDTO
{
    public Guid Id { get; set; }
    public Guid EntradaId { get; set; }
    public string CodigoUnicoEntrada { get; set; } = string.Empty;
    public decimal MontoReembolsado { get; set; }
    public DateTime FechaSolicitud { get; set; }
    public DateTime? FechaProcesamiento { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public class DevolucionResumenDTO
{
    public Guid Id { get; set; }
    public string EventoNombre { get; set; } = string.Empty;
    public string CodigoUnicoEntrada { get; set; } = string.Empty;
    public decimal MontoReembolsado { get; set; }
    public DateTime FechaSolicitud { get; set; }
}