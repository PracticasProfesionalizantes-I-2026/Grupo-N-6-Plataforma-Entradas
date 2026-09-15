using EntradApp.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace EntradApp.DataAccess.Entities;

public class Compra
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UsuarioId { get; set; }

    [Required]
    public Guid EventoId { get; set; }

    [Required]
    public Guid SectorId { get; set; }

    [Required]
    public int Cantidad { get; set; }

    [Required]
    public decimal Total { get; set; }

    [Required]
    public EstadoCompra Estado { get; set; } = EstadoCompra.PendientePago;

    [Required]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaPago { get; set; }

    // Soft delete
    public bool EsBorrado { get; set; } = false;

    // Navigation
    public virtual Usuario Usuario { get; set; } = null!;
    public virtual Evento Evento { get; set; } = null!;
    public virtual Sector Sector { get; set; } = null!;
    public virtual ICollection<Entrada> Entradas { get; set; } = new List<Entrada>();
    public virtual ICollection<ReservaTemporal> ReservasTemporales { get; set; } = new List<ReservaTemporal>();
}