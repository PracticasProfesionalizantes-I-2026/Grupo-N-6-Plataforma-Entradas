using System.ComponentModel.DataAnnotations;

namespace EntradApp.DataAccess.Entities;

public class ReservaTemporal
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid CompraId { get; set; }

    [Required]
    public Guid SectorId { get; set; }

    [Required]
    public int Cantidad { get; set; }

    [Required]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime ExpiraEn { get; set; }

    [Required]
    public bool Expirada { get; set; } = false;

    // Navigation
    public virtual Compra Compra { get; set; } = null!;
    public virtual Sector Sector { get; set; } = null!;
}