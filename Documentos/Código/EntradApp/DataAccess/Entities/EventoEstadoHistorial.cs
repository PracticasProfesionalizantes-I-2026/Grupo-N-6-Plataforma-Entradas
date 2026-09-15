using EntradApp.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace EntradApp.DataAccess.Entities;

public class EventoEstadoHistorial
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid EventoId { get; set; }

    [Required]
    public EstadoEvento EstadoAnterior { get; set; }

    [Required]
    public EstadoEvento EstadoNuevo { get; set; }

    [Required]
    public Guid SuperAdminId { get; set; }

    [MaxLength(500)]
    public string? Observaciones { get; set; }

    [Required]
    public DateTime FechaCambio { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual Evento Evento { get; set; } = null!;
    public virtual Usuario SuperAdmin { get; set; } = null!;
}