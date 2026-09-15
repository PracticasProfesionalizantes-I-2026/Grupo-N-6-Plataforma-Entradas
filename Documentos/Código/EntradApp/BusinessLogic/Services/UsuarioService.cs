using BCrypt.Net;
using EntradApp.BusinessLogic.Interfaces;
using EntradApp.DataAccess.Entities;
using EntradApp.DataAccess.Repositories.Interfaces;
using EntradApp.Shared.Common;
using EntradApp.Shared.DTOs.Usuario;
using EntradApp.Shared.Enums;
using EntradApp.Shared.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EntradApp.BusinessLogic.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IConfiguration _configuration;

    public UsuarioService(IUsuarioRepository usuarioRepository, IConfiguration configuration)
    {
        _usuarioRepository = usuarioRepository;
        _configuration = configuration;
    }

    public async Task<Result<UsuarioResponseDTO>> RegistrarAsync(UsuarioCreateDTO dto)
    {
        if (await _usuarioRepository.GetByEmailAsync(dto.Email) != null)
            return Result.Failure<UsuarioResponseDTO>("El email ya está registrado");

        if (await _usuarioRepository.GetByDniAsync(dto.Dni) != null)
            return Result.Failure<UsuarioResponseDTO>("El DNI ya está registrado");

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Nombre = dto.Nombre.Trim(),
            Apellido = dto.Apellido.Trim(),
            Dni = dto.Dni.Trim(),
            FechaNacimiento = dto.FechaNacimiento,
            Rol = RolUsuario.Usuario,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        await _usuarioRepository.CreateAsync(usuario);
        return Result.Success(MapToResponseDTO(usuario));
    }

    public async Task<Result<UsuarioResponseDTO>> GetByIdAsync(Guid id)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario == null) return Result.Failure<UsuarioResponseDTO>("Usuario no encontrado");
        return Result.Success(MapToResponseDTO(usuario));
    }

    public async Task<Result<UsuarioResponseDTO>> GetByEmailAsync(string email)
    {
        var usuario = await _usuarioRepository.GetByEmailAsync(email);
        if (usuario == null) return Result.Failure<UsuarioResponseDTO>("Usuario no encontrado");
        return Result.Success(MapToResponseDTO(usuario));
    }

    public async Task<Result> ActualizarDatosPersonalesAsync(Guid id, UsuarioUpdateDTO dto)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario == null) return Result.Failure("Usuario no encontrado");

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            if (await _usuarioRepository.GetByEmailAsync(dto.Email) is Usuario u && u.Id != id)
                return Result.Failure("El email ya está en uso");
            usuario.Email = dto.Email.Trim();
        }
        if (!string.IsNullOrWhiteSpace(dto.Nombre)) usuario.Nombre = dto.Nombre.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Apellido)) usuario.Apellido = dto.Apellido.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Dni))
        {
            if (await _usuarioRepository.GetByDniAsync(dto.Dni) is Usuario u && u.Id != id)
                return Result.Failure("El DNI ya está en uso");
            usuario.Dni = dto.Dni.Trim();
        }
        if (dto.FechaNacimiento.HasValue) usuario.FechaNacimiento = dto.FechaNacimiento.Value;

        await _usuarioRepository.UpdateAsync(usuario);
        return Result.Success();
    }

    public async Task<Result> ActualizarPasswordAsync(Guid id, string passwordActual, string passwordNueva)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario == null) return Result.Failure("Usuario no encontrado");

        if (!BCrypt.Net.BCrypt.Verify(passwordActual, usuario.PasswordHash))
            return Result.Failure("La contraseña actual es incorrecta");

        if (BCrypt.Net.BCrypt.Verify(passwordNueva, usuario.PasswordHash))
            return Result.Failure("La nueva contraseña no puede ser igual a la actual");

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordNueva);
        await _usuarioRepository.UpdateAsync(usuario);
        return Result.Success();
    }

    public async Task<Result> LoginAsync(string email, string password)
    {
        var usuario = await _usuarioRepository.GetByEmailAsync(email);
        if (usuario == null || !usuario.Activo)
            return Result.Failure("Credenciales inválidas");

        if (!BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash))
            return Result.Failure("Credenciales inválidas");

        var token = GenerarTokenJwt(usuario);
        usuario.FechaUltimoLogin = DateTime.UtcNow;
        await _usuarioRepository.UpdateAsync(usuario);

        return Result.Success(new { Token = token, Usuario = MapToResponseDTO(usuario) });
    }

    private string GenerarTokenJwt(Usuario usuario)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            "SuperSecretKeyForDevelopmentOnlyChangeInProduction12345!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Rol.ToString()),
            new Claim("nombre", usuario.Nombre),
            new Claim("apellido", usuario.Apellido)
        };

        var token = new JwtSecurityToken(
            issuer: "EntradApp",
            audience: "EntradAppUsers",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UsuarioResponseDTO MapToResponseDTO(Usuario u) => new()
    {
        Id = u.Id,
        Email = u.Email,
        Nombre = u.Nombre,
        Apellido = u.Apellido,
        Dni = u.Dni,
        FechaNacimiento = u.FechaNacimiento,
        Rol = u.Rol,
        Activo = u.Activo,
        FechaCreacion = u.FechaCreacion
    };
}