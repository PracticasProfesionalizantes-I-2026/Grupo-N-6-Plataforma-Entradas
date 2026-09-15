using EntradApp.BusinessLogic.Services;
using EntradApp.DataAccess.Entities;
using EntradApp.DataAccess.Repositories.Interfaces;
using EntradApp.Shared.Common;
using EntradApp.Shared.DTOs.Evento;
using EntradApp.Shared.Enums;
using EntradApp.Shared.Exceptions;
using Moq;
using Xunit;

namespace EntradApp.Tests.UnitTests.Services;

public class EventoServiceTests
{
    private readonly Mock<IEventoRepository> _eventoRepoMock;
    private readonly Mock<ISectorRepository> _sectorRepoMock;
    private readonly EventoService _service;

    public EventoServiceTests()
    {
        _eventoRepoMock = new Mock<IEventoRepository>();
        _sectorRepoMock = new Mock<ISectorRepository>();
        _service = new EventoService(_eventoRepoMock.Object, _sectorRepoMock.Object);
    }

    [Fact]
    public async Task CrearAsync_ConDatosValidos_RetornaEventoCreado()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var dto = new EventoCreateDTO
        {
            Nombre = "Evento Test",
            Descripcion = "Descripción",
            FechaInicio = DateTime.Today.AddDays(10),
            FechaFin = DateTime.Today.AddDays(12),
            Lugar = "Estadio",
            Direccion = "Av. Principal 123",
            Sectores = new List<SectorCreateDTO>
            {
                new() { Nombre = "Platea", Capacidad = 100, PrecioBase = 5000 },
                new() { Nombre = "Popular", Capacidad = 200, PrecioBase = 3000 }
            }
        };

        _eventoRepoMock.Setup(r => r.CreateAsync(It.IsAny<Evento>(), default))
            .ReturnsAsync(Result.Success<Evento>(null!));

        // Act
        var result = await _service.CrearAsync(usuarioId, dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(dto.Nombre, result.Value.Nombre);
        Assert.Equal(EstadoEvento.PendienteAprobacion, result.Value.Estado);
        Assert.Equal(2, result.Value.Sectores.Count);
    }

    [Fact]
    public async Task CrearAsync_FechaPasada_LanzaValidationException()
    {
        // Arrange
        var dto = new EventoCreateDTO
        {
            Nombre = "Evento",
            FechaInicio = DateTime.Today.AddDays(-1), // Pasada
            FechaFin = DateTime.Today.AddDays(1),
            Lugar = "Lugar",
            Direccion = "Dirección",
            Sectores = new() { new SectorCreateDTO { Nombre = "S1", Capacidad = 10, PrecioBase = 100 } }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _service.CrearAsync(Guid.NewGuid(), dto));
    }
}