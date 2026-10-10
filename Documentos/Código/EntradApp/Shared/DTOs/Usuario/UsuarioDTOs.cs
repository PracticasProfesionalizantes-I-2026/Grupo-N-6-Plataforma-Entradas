using System.ComponentModel.DataAnnotations;
using EntradApp.Shared.Enums;

namespace EntradApp.Shared.DTOs.Usuario;

public class UsuarioCreateDTO
{
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Apellido { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{7,8}$", ErrorMessage = "El DNI debe tener 7 u 8 dígitos numéricos.")]
    public string Dni { get; set; } = string.Empty;

    [Required]
    public DateTime FechaNacimiento { get; set; }
}

public class UsuarioUpdateDTO
{
    [MaxLength(100)]
    public string? Nombre { get; set; }

    [MaxLength(100)]
    public string? Apellido { get; set; }

    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; set; }

    [RegularExpression(@"^\d{7,8}$", ErrorMessage = "El DNI debe tener 7 u 8 dígitos numéricos.")]
    public string? Dni { get; set; }

    public DateTime? FechaNacimiento { get; set; }
}

public class UsuarioResponseDTO
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public RolUsuario Rol { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class LoginResponseDTO
{
    public string Token { get; set; } = string.Empty;
    public UsuarioResponseDTO Usuario { get; set; } = new();
}

public class LoginRequestDTO
{
    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato de email no es válido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    public string Password { get; set; } = string.Empty;
}

public class ActualizarPasswordDTO
{
    [Required(ErrorMessage = "La contraseña actual es obligatoria")]
    public string PasswordActual { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
    [MinLength(6, ErrorMessage = "La nueva contraseña debe tener al menos 6 caracteres")]
    [MaxLength(100)]
    public string PasswordNueva { get; set; } = string.Empty;
}