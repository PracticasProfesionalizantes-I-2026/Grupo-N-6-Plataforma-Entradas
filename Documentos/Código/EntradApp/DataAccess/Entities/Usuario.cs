using EntradApp.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace EntradApp.DataAccess.Entities;

public class Usuario
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Apellido { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Dni { get; set; } = string.Empty;

    [Required]
    public DateTime FechaNacimiento { get; set; }

    [Required]
    public RolUsuario Rol { get; set; } = RolUsuario.Usuario;

    [Required]
    public bool Activo { get; set; } = true;

    [Required]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaUltimoLogin { get; set; }

    // Soft delete
    public bool EsBorrado { get; set; } = false;

    // Navigation
    public virtual ICollection<Evento> EventosCreados { get; set; } = new List<Evento>();
    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();
    public virtual ICollection<EventoEstadoHistorial> HistorialEstados { get; set; } = new List<EventoEstadoHistorial>();
}