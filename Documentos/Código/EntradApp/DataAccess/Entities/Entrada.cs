using EntradApp.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace EntradApp.DataAccess.Entities;

public class Entrada
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid CompraId { get; set; }

    [Required]
    public Guid SectorId { get; set; }

    [Required]
    [MaxLength(20)]
    public string Dni { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string CodigoUnico { get; set; } = string.Empty;

    [Required]
    public EstadoEntrada Estado { get; set; } = EstadoEntrada.Activa;

    public DateTime? FechaValidacion { get; set; }

    [Required]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Soft delete
    public bool EsBorrado { get; set; } = false;

    // Navigation
    public virtual Compra Compra { get; set; } = null!;
    public virtual Sector Sector { get; set; } = null!;
    public virtual ICollection<Devolucion> Devoluciones { get; set; } = new List<Devolucion>();
}