using EntradApp.BusinessLogic.Services;
using EntradApp.DataAccess.Entities;
using EntradApp.DataAccess.Repositories.Interfaces;
using EntradApp.Shared.Common;
using EntradApp.Shared.Enums;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace EntradApp.Tests.UnitTests.Services;

public class PrecioDinamicoServiceTests
{
    private readonly Mock<ISectorRepository> _sectorRepoMock;
    private readonly Mock<IEventoRepository> _eventoRepoMock;
    private readonly PrecioDinamicoService _service;

    public PrecioDinamicoServiceTests()
    {
        _sectorRepoMock = new Mock<ISectorRepository>();
        _eventoRepoMock = new Mock<IEventoRepository>();
        
        var options = Options.Create(new PrecioDinamicoOptions { UmbralOcupacion = 80, PorcentajeIncremento = 20 });
        _service = new PrecioDinamicoService(_sectorRepoMock.Object, _eventoRepoMock.Object, options);
    }

    [Fact]
    public async Task EvaluarYAplicarIncrementoAsync_SectorConOcupacionSuperior_IncrementaPrecio()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var sector = new Sector
        {
            Id = Guid.NewGuid(),
            EventoId = eventoId,
            PrecioBase = 5000,
            PrecioActual = 5000,
            EntradasVendidas = 90,
            Capacidad = 100
        };

        var evento = new Evento { Id = eventoId, Estado = EstadoEvento.Aprobado };
        var eventos = new PagedResult<Evento>(new List<Evento> { evento }, 1, 10, 1);

        _eventoRepoMock.Setup(r => r.GetAprobadosAsync(1, It.IsAny<int>(), default))
            .ReturnsAsync(eventos);
        _sectorRepoMock.Setup(r => r.GetByEventoAsync(eventoId, default))
            .ReturnsAsync(new List<Sector> { sector });
        _sectorRepoMock.Setup(r => r.UpdateAsync(It.Is<Sector>(s => s.PrecioActual == 6000), default))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _service.EvaluarYAplicarIncrementoAsync();

        // Assert
        Assert.True(result.IsSuccess);
        _sectorRepoMock.Verify(r => r.UpdateAsync(It.Is<Sector>(s => s.PrecioActual == 6000m), default), Times.Once);
    }

    [Fact]
    public async Task EvaluarYAplicarIncrementoAsync_SectorYaIncrementado_NoDobleIncremento()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var sector = new Sector
        {
            Id = Guid.NewGuid(),
            EventoId = eventoId,
            PrecioBase = 5000,
            PrecioActual = 6000, // Ya incrementado
            EntradasVendidas = 90,
            Capacidad = 100
        };

        var evento = new Evento { Id = eventoId, Estado = EstadoEvento.Aprobado };
        var eventos = new PagedResult<Evento>(new List<Evento> { evento }, 1, 10, 1);

        _eventoRepoMock.Setup(r => r.GetAprobadosAsync(1, It.IsAny<int>(), default))
            .ReturnsAsync(eventos);
        _sectorRepoMock.Setup(r => r.GetByEventoAsync(eventoId, default))
            .ReturnsAsync(new List<Sector> { sector });

        // Act
        var result = await _service.EvaluarYAplicarIncrementoAsync();

        // Assert
        Assert.True(result.IsSuccess);
        _sectorRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Sector>(), default), Times.Never);
    }
}