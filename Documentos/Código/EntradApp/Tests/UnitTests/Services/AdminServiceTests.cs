using EntradApp.BusinessLogic.Services;
using EntradApp.DataAccess.Entities;
using EntradApp.DataAccess.Repositories.Interfaces;
using EntradApp.Shared.Common;
using EntradApp.Shared.Enums;
using EntradApp.Shared.Exceptions;
using Moq;
using Xunit;

namespace EntradApp.Tests.UnitTests.Services;

public class AdminServiceTests
{
    private readonly Mock<IEventoRepository> _eventoRepoMock;
    private readonly Mock<IEventoEstadoHistorialRepository> _historialRepoMock;
    private readonly AdminService _service;

    public AdminServiceTests()
    {
        _eventoRepoMock = new Mock<IEventoRepository>();
        _historialRepoMock = new Mock<IEventoEstadoHistorialRepository>();
        _service = new AdminService(_eventoRepoMock.Object, _historialRepoMock.Object);
    }

    [Fact]
    public async Task AprobarEventoAsync_EventoPendiente_ApruebaYRegistraHistorial()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var eventoId = Guid.NewGuid();

        var evento = new Evento
        {
            Id = eventoId,
            Estado = EstadoEvento.PendienteAprobacion,
            Sectores = new List<Sector> { new() { Id = Guid.NewGuid() } }
        };

        _eventoRepoMock.Setup(r => r.GetByIdAsync(eventoId, default)).ReturnsAsync(evento);
        _eventoRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Evento>(), default)).ReturnsAsync(Result.Success());
        _historialRepoMock.Setup(r => r.CreateAsync(It.IsAny<EventoEstadoHistorial>(), default)).ReturnsAsync(Result.Success<EventoEstadoHistorial>(null!));

        // Act
        var result = await _service.AprobarEventoAsync(Guid.NewGuid(), eventoId, "Aprobado por admin");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(EstadoEvento.Aprobado, evento.Estado);
        _historialRepoMock.Verify(r => r.CreateAsync(
            It.Is<EventoEstadoHistorial>(h => 
                h.EventoId == eventoId && 
                h.EstadoNuevo == EstadoEvento.Aprobado && 
                h.Observaciones == "Aprobado por admin"), default), Times.Once);
    }

    [Fact]
    public async Task RechazarEventoAsync_EventoPendiente_RechazaYRegistraHistorial()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var eventoId = Guid.NewGuid();
        var motivo = "No cumple requisitos";

        var evento = new Evento
        {
            Id = eventoId,
            Estado = EstadoEvento.PendienteAprobacion,
            Sectores = new List<Sector> { new() { Id = Guid.NewGuid() } }
        };

        _eventoRepoMock.Setup(r => r.GetByIdAsync(eventoId, default)).ReturnsAsync(evento);
        _eventoRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Evento>(), default)).ReturnsAsync(Result.Success());
        _historialRepoMock.Setup(r => r.CreateAsync(It.IsAny<EventoEstadoHistorial>(), default)).ReturnsAsync(Result.Success<EventoEstadoHistorial>(null!));

        // Act
        var result = await _service.RechazarEventoAsync(adminId, eventoId, motivo);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(EstadoEvento.Rechazado, evento.Estado);
        _historialRepoMock.Verify(r => r.CreateAsync(
            It.Is<EventoEstadoHistorial>(h => 
                h.EventoId == eventoId && 
                h.EstadoNuevo == EstadoEvento.Rechazado && 
                h.Observaciones == motivo), default), Times.Once);
    }

    [Fact]
    public async Task RechazarEventoAsync_SinMotivo_RetornaError()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var eventoId = Guid.NewGuid();

        var evento = new Evento
        {
            Id = eventoId,
            Estado = EstadoEvento.PendienteAprobacion,
            Sectores = new List<Sector> { new() { Id = Guid.NewGuid() } }
        };

        _eventoRepoMock.Setup(r => r.GetByIdAsync(eventoId, default)).ReturnsAsync(evento);

        // Act
        var result = await _service.RechazarEventoAsync(adminId, eventoId, "");

        // Assert
        Assert.False(result.IsSuccess);
    }
}