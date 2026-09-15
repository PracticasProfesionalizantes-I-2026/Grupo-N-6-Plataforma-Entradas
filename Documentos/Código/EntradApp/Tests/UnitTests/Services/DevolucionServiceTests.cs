using EntradApp.BusinessLogic.Services;
using EntradApp.DataAccess.Entities;
using EntradApp.DataAccess.Repositories.Interfaces;
using EntradApp.Shared.Common;
using EntradApp.Shared.DTOs.Devolucion;
using EntradApp.Shared.Enums;
using EntradApp.Shared.Exceptions;
using Moq;
using Xunit;

namespace EntradApp.Tests.UnitTests.Services;

public class DevolucionServiceTests
{
    private readonly Mock<IDevolucionRepository> _devolucionRepoMock;
    private readonly Mock<IEntradaRepository> _entradaRepoMock;
    private readonly Mock<ISectorRepository> _sectorRepoMock;
    private readonly Mock<IEventoRepository> _eventoRepoMock;
    private readonly DevolucionService _service;

    public DevolucionServiceTests()
    {
        _devolucionRepoMock = new Mock<IDevolucionRepository>();
        _entradaRepoMock = new Mock<IEntradaRepository>();
        _sectorRepoMock = new Mock<ISectorRepository>();
        _eventoRepoMock = new Mock<IEventoRepository>();
        _service = new DevolucionService(
            _devolucionRepoMock.Object,
            _entradaRepoMock.Object,
            _sectorRepoMock.Object,
            _eventoRepoMock.Object);
    }

    [Fact]
    public async Task SolicitarDevolucionAsync_EntradaValida_RetornaDevolucion()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var entradaId = Guid.NewGuid();
        var compraId = Guid.NewGuid();
        var sectorId = Guid.NewGuid();
        var eventoId = Guid.NewGuid();

        var dto = new DevolucionCreateDTO { EntradaId = entradaId };

        var entrada = new Entrada
        {
            Id = entradaId,
            CompraId = compraId,
            SectorId = sectorId,
            Estado = EstadoEntrada.Activa
        };

        var compra = new Compra
        {
            Id = compraId,
            UsuarioId = usuarioId,
            EventoId = eventoId,
            Total = 10000,
            Cantidad = 1
        };

        var evento = new Evento { Id = eventoId, FechaInicio = DateTime.UtcNow.AddDays(5) };
        var sector = new Sector { Id = sectorId };

        _entradaRepoMock.Setup(r => r.GetByIdAsync(entradaId, default)).ReturnsAsync(entrada);
        _entradaRepoMock.Setup(r => r.GetByIdAsync(compraId, default)).ReturnsAsync(compra);
        _eventoRepoMock.Setup(r => r.GetByIdAsync(eventoId, default)).ReturnsAsync(evento);
        _sectorRepoMock.Setup(r => r.GetByIdAsync(sectorId)).ReturnsAsync(sector);
        _devolucionRepoMock.Setup(r => r.CreateAsync(It.IsAny<Devolucion>(), default)).ReturnsAsync(Result.Success<Devolucion>(null!));
        _entradaRepoMock.Setup(r => r.UpdateEstadoAsync(entradaId, EstadoEntrada.Devuelta, default)).ReturnsAsync(Result.Success());
        _sectorRepoMock.Setup(r => r.IncrementarVendidasAsync(sectorId, -1, default)).ReturnsAsync(Result.Success());

        // Act
        var result = await _service.SolicitarDevolucionAsync(usuarioId, dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(8000, result.Value.MontoReembolsado); // 80% de 10000
        _devolucionRepoMock.Verify(r => r.CreateAsync(It.IsAny<Devolucion>(), default), Times.Once);
        _entradaRepoMock.Verify(r => r.UpdateEstadoAsync(entradaId, EstadoEntrada.Devuelta, default), Times.Once);
    }
}