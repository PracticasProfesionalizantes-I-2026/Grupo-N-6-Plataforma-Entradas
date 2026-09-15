using EntradApp.Tests.IntegrationTests;
using EntradApp.Shared.DTOs.Entrada;
using System.Net;
using Xunit;

namespace EntradApp.Tests.IntegrationTests.Controllers;

public class EntradasControllerTests : TestBase
{
    public EntradasControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetMisEntradas_SinAutenticacion_Retorna401()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/entradas");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMisEntradas_ConAutenticacion_Retorna200()
    {
        // Arrange
        var token = await GetTokenAsync("usuario@test.com", "Usuario123!");
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync("/api/v1/entradas?page=1&pageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetUtilizadas_ConAutenticacion_Retorna200()
    {
        // Arrange
        var token = await GetTokenAsync("usuario@test.com", "Usuario123!");
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync("/api/v1/entradas/utilizadas?page=1&pageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}