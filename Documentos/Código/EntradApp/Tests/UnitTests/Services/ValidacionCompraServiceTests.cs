using EntradApp.BusinessLogic.Services;
using EntradApp.DataAccess.Repositories.Interfaces;
using EntradApp.Shared.Common;
using EntradApp.Shared.Exceptions;
using Moq;
using Xunit;

namespace EntradApp.Tests.UnitTests.Services;

public class ValidacionCompraServiceTests
{
    private readonly Mock<ISectorRepository> _sectorRepoMock;
    private readonly Mock<ICompraRepository> _compraRepoMock;
    private readonly Mock<IEntradaRepository> _entradaRepoMock;
    private readonly ValidacionCompraService _service;

    public ValidacionCompraServiceTests()
    {
        _sectorRepoMock = new Mock<ISectorRepository>();
        _compraRepoMock = new Mock<ICompraRepository>();
        _entradaRepoMock = new Mock<IEntradaRepository>();
        _service = new ValidacionCompraService(
            _sectorRepoMock.Object,
            _compraRepoMock.Object,
            _entradaRepoMock.Object);
    }

    [Fact]
    public async Task ValidarDisponibilidadAsync_StockSuficiente_RetornaExitoso()
    {
        _sectorRepoMock.Setup(r => r.GetStockDisponibleAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(50);

        var result = await _service.ValidarDisponibilidadAsync(Guid.NewGuid(), 10);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ValidarDisponibilidadAsync_StockInsuficiente_RetornaError()
    {
        _sectorRepoMock.Setup(r => r.GetStockDisponibleAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(5);

        var result = await _service.ValidarDisponibilidadAsync(Guid.NewGuid(), 10);

        Assert.False(result.IsSuccess);
        Assert.Contains("Stock insuficiente", result.Error);
    }

    [Fact]
    public async Task ValidarLimiteMaximoAsync_DentroDeLimite_RetornaExitoso()
    {
        _compraRepoMock.Setup(r => r.ContarEntradasUsuarioEventoAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync(1);

        var result = await _service.ValidarLimiteMaximoAsync(Guid.NewGuid(), Guid.NewGuid(), 2);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ValidarLimiteMaximoAsync_ExcedeLimite_RetornaError()
    {
        _compraRepoMock.Setup(r => r.ContarEntradasUsuarioEventoAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync(3);

        var result = await _service.ValidarLimiteMaximoAsync(Guid.NewGuid(), Guid.NewGuid(), 2);

        Assert.False(result.IsSuccess);
        Assert.Contains("Límite de compra excedido", result.Error);
    }

    [Fact]
    public async Task ValidarDnisDuplicadosAsync_DniDuplicadoEnLista_RetornaError()
    {
        var dnis = new List<string> { "12345678", "12345678", "87654321" };

        var result = await _service.ValidarDnisDuplicadosAsync(Guid.NewGuid(), dnis);

        Assert.False(result.IsSuccess);
        Assert.Contains("12345678", result.Error);
    }

    [Fact]
    public async Task ValidarDnisDuplicadosAsync_DniYaEnBD_RetornaError()
    {
        _entradaRepoMock.Setup(r => r.ContarPorDniYEventoAsync("12345678", It.IsAny<Guid>(), default))
            .ReturnsAsync(1);

        var result = await _service.ValidarDnisDuplicadosAsync(Guid.NewGuid(), new List<string> { "12345678" });

        Assert.False(result.IsSuccess);
        Assert.Contains("12345678", result.Error);
    }
}