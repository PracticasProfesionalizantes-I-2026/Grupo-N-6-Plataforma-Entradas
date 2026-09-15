using EntradApp.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace EntradApp.DataAccess.Entities;

public class Evento
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Descripcion { get; set; }

    [Required]
    public DateTime FechaInicio { get; set; }

    [Required]
    public DateTime FechaFin { get; set; }

    [Required]
    [MaxLength(200)]
    public string Lugar { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string Direccion { get; set; } = string.Empty;

    [Required]
    public EstadoEvento Estado { get; set; } = EstadoEvento.Borrador;

    [Required]
    public Guid UsuarioCreadorId { get; set; }

    [Required]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }

    // Soft delete
    public bool EsBorrado { get; set; } = false;

    // Navigation
    public virtual Usuario UsuarioCreador { get; set; } = null!;
    public virtual ICollection<Sector> Sectores { get; set; } = new List<Sector>();
    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();
    public virtual ICollection<EventoEstadoHistorial> HistorialEstados { get; set; } = new List<EventoEstadoHistorial>();
}