
using EntradApp.BusinessLogic.Interfaces;
using EntradApp.BusinessLogic.Services;
using EntradApp.DataAccess;
using EntradApp.DataAccess.Entities;
using EntradApp.DataAccess.Repositories.Interfaces;
using EntradApp.Shared.Common;
using EntradApp.Shared.DTOs.Compra;
using EntradApp.Shared.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace EntradApp.Tests.UnitTests.Services;

public class CompraServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly EntradAppDbContext _context;

    private readonly Mock<ICompraRepository> _compraRepoMock;
    private readonly Mock<IEntradaRepository> _entradaRepoMock;
    private readonly Mock<ISectorRepository> _sectorRepoMock;
    private readonly Mock<IEventoRepository> _eventoRepoMock;
    private readonly Mock<IValidacionCompraService> _validacionMock;
    private readonly Mock<IReservaTemporalService> _reservaMock;

    private readonly CompraService _service;

    public CompraServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<EntradAppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new EntradAppDbContext(options);

        _compraRepoMock = new Mock<ICompraRepository>();
        _entradaRepoMock = new Mock<IEntradaRepository>();
        _sectorRepoMock = new Mock<ISectorRepository>();
        _eventoRepoMock = new Mock<IEventoRepository>();
        _validacionMock = new Mock<IValidacionCompraService>();
        _reservaMock = new Mock<IReservaTemporalService>();

        _service = new CompraService(
            _context,
            _compraRepoMock.Object,
            _entradaRepoMock.Object,
            _sectorRepoMock.Object,
            _eventoRepoMock.Object,
            _validacionMock.Object,
            _reservaMock.Object);
    }

    [Fact]
    public async Task CrearCompraAsync_ConDatosValidos_RetornaCompraCreada()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var eventoId = Guid.NewGuid();
        var sectorId = Guid.NewGuid();

        var dto = new CompraCreateDTO
        {
            EventoId = eventoId,
            SectorId = sectorId,
            Cantidad = 2,
            Dnis = new List<string> { "12345678", "87654321" }
        };

        var evento = new Evento
        {
            Id = eventoId,
            Estado = EstadoEvento.Aprobado
        };

        var sector = new Sector
        {
            Id = sectorId,
            EventoId = eventoId,
            PrecioActual = 5000,
            PrecioBase = 5000
        };

        _eventoRepoMock
            .Setup(r => r.GetByIdAsync(eventoId, default))
            .ReturnsAsync(evento);

        _sectorRepoMock
            .Setup(r => r.GetByIdAsync(sectorId, default))
            .ReturnsAsync(sector);

        _validacionMock
            .Setup(v => v.ValidarDisponibilidadAsync(sectorId, 2))
            .ReturnsAsync(Result.Success());

        _validacionMock
            .Setup(v => v.ValidarLimiteMaximoAsync(It.IsAny<Guid>(), eventoId, 2))
            .ReturnsAsync(Result.Success());

        _validacionMock
            .Setup(v => v.ValidarDnisDuplicadosAsync(eventoId, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(Result.Success());

        _reservaMock
            .Setup(r => r.CrearReservaAsync(It.IsAny<Guid>(), sectorId, 2, 10))
            .ReturnsAsync(Result.Success());

        _compraRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<Compra>(), default))
            .ReturnsAsync(Result.Success<Compra>(null!));

        _entradaRepoMock
            .Setup(r => r.CreateRangeAsync(It.IsAny<IEnumerable<Entrada>>(), default))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _service.CrearCompraAsync(usuarioId, dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(EstadoCompra.PendientePago, result.Value.Estado);
        Assert.Equal(2, result.Value.Entradas.Count);
    }

    [Fact]
    public async Task CrearCompraAsync_SinDisponibilidad_RetornaError()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var sectorId = Guid.NewGuid();

        var dto = new CompraCreateDTO
        {
            EventoId = eventoId,
            SectorId = sectorId,
            Cantidad = 2,
            Dnis = new List<string> { "12345678", "87654321" }
        };

        var evento = new Evento
        {
            Id = eventoId,
            Estado = EstadoEvento.Aprobado
        };

        var sector = new Sector
        {
            Id = sectorId,
            EventoId = eventoId,
            PrecioActual = 5000,
            PrecioBase = 5000
        };

        _eventoRepoMock
            .Setup(r => r.GetByIdAsync(eventoId, default))
            .ReturnsAsync(evento);

        _sectorRepoMock
            .Setup(r => r.GetByIdAsync(sectorId, default))
            .ReturnsAsync(sector);

        _validacionMock
            .Setup(v => v.ValidarDisponibilidadAsync(sectorId, 2))
            .ReturnsAsync(Result.Failure("Stock insuficiente"));

        // Act
        var result = await _service.CrearCompraAsync(Guid.NewGuid(), dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Stock insuficiente", result.Error);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
