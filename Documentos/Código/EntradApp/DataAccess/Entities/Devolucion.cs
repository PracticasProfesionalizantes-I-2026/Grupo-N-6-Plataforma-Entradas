using System.ComponentModel.DataAnnotations;

namespace EntradApp.DataAccess.Entities;

public class Devolucion
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid EntradaId { get; set; }

    [Required]
    public decimal MontoReembolsado { get; set; }

    [Required]
    public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;

    public DateTime? FechaProcesamiento { get; set; }

    [Required]
    [MaxLength(50)]
    public string Estado { get; set; } = "Solicitada"; // Solicitada, Completada, Rechazada

    // Soft delete
    public bool EsBorrado { get; set; } = false;

    // Navigation
    public virtual Entrada Entrada { get; set; } = null!;
}