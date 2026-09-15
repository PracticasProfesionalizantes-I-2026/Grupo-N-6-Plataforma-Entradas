using System.ComponentModel.DataAnnotations;

namespace EntradApp.DataAccess.Entities;

public class Sector
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid EventoId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public int Capacidad { get; set; }

    [Required]
    public decimal PrecioBase { get; set; }

    [Required]
    public decimal PrecioActual { get; set; }

    [Required]
    public int EntradasVendidas { get; set; } = 0;

    [Required]
    public int EntradasReservadas { get; set; } = 0;

    // Soft delete
    public bool EsBorrado { get; set; } = false;

    // Navigation
    public virtual Evento Evento { get; set; } = null!;
    public virtual ICollection<Entrada> Entradas { get; set; } = new List<Entrada>();

    public int StockDisponible => Capacidad - EntradasVendidas - EntradasReservadas;
    public double PorcentajeOcupacion => Capacidad > 0 ? (double)EntradasVendidas / Capacidad * 100 : 0;
}