using EntradApp.BusinessLogic.Services;
using EntradApp.DataAccess.Entities;
using EntradApp.DataAccess.Repositories.Interfaces;
using EntradApp.Shared.Common;
using EntradApp.Shared.DTOs.Usuario;
using EntradApp.Shared.Enums;
using EntradApp.Shared.Exceptions;
using Moq;
using Xunit;

namespace EntradApp.Tests.UnitTests.Services;

public class UsuarioServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepoMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly UsuarioService _service;

    public UsuarioServiceTests()
    {
        _usuarioRepoMock = new Mock<IUsuarioRepository>();
        _configMock = new Mock<IConfiguration>();
        _configMock.Setup(c => c["Jwt:Key"]).Returns("SuperSecretKeyForDevelopmentOnlyChangeInProduction12345!");
        
        _service = new UsuarioService(_usuarioRepoMock.Object, _configMock.Object);
    }

    [Fact]
    public async Task RegistrarAsync_ConDatosValidos_RetornaUsuarioCreado()
    {
        // Arrange
        var dto = new UsuarioCreateDTO
        {
            Email = "test@test.com",
            Password = "Password123!",
            Nombre = "Test",
            Apellido = "User",
            Dni = "12345678",
            FechaNacimiento = new DateTime(1990, 1, 1)
        };

        _usuarioRepoMock.Setup(r => r.GetByEmailAsync(dto.Email, default))
            .ReturnsAsync((Usuario?)null);
        _usuarioRepoMock.Setup(r => r.GetByDniAsync(dto.Dni, default))
            .ReturnsAsync((Usuario?)null);

        // Act
        var result = await _service.RegistrarAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(dto.Email, result.Value.Email);
        Assert.Equal(dto.Nombre, result.Value.Nombre);
        _usuarioRepoMock.Verify(r => r.CreateAsync(It.IsAny<Usuario>(), default), Times.Once);
    }

    [Fact]
    public async Task RegistrarAsync_EmailDuplicado_RetornaError()
    {
        // Arrange
        var dto = new UsuarioCreateDTO { Email = "existing@test.com", Password = "Pass123!", Nombre = "T", Apellido = "U", Dni = "87654321", FechaNacimiento = DateTime.Now.AddYears(-20) };
        
        _usuarioRepoMock.Setup(r => r.GetByEmailAsync(dto.Email, default))
            .ReturnsAsync(new Usuario { Email = dto.Email });

        // Act
        var result = await _service.RegistrarAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("El email ya está registrado", result.Error);
    }

    [Fact]
    public async Task LoginAsync_CredencialesValidas_RetornaToken()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Nombre = "Test",
            Apellido = "User",
            Dni = "12345678",
            FechaNacimiento = DateTime.Now.AddYears(-20),
            Rol = RolUsuario.Usuario,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        _usuarioRepoMock.Setup(r => r.GetByEmailAsync(usuario.Email, default))
            .ReturnsAsync(usuario);

        // Act
        var result = await _service.LoginAsync(usuario.Email, "Password123!");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }
}