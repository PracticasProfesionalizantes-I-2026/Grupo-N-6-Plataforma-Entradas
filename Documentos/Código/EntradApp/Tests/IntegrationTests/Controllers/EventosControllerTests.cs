using EntradApp.Tests.IntegrationTests;
using EntradApp.Shared.DTOs.Evento;
using EntradApp.Shared.Enums;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EntradApp.Tests.IntegrationTests.Controllers;

public class EventosControllerTests : TestBase
{
    public EventosControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetEventos_SinAutenticacion_RetornaLista()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/eventos?page=1&pageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetEvento_NoExistente_Retorna404()
    {
        // Act
        var response = await _client.GetAsync($"/api/v1/eventos/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CrearEvento_SinAutenticacion_Retorna401()
    {
        // Arrange
        var dto = new EventoCreateDTO
        {
            Nombre = "Test",
            FechaInicio = DateTime.Today.AddDays(10),
            FechaFin = DateTime.Today.AddDays(12),
            Lugar = "Estadio",
            Direccion = "Av. Test 123",
            Sectores = new() { new() { Nombre = "S1", Capacidad = 100, PrecioBase = 1000 } }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/eventos", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CrearEvento_ConAutenticacionUsuario_Retorna201()
    {
        // Arrange
        var token = await GetTokenAsync("usuario@test.com", "Usuario123!");
        SetAuthHeader(token);

        var dto = new EventoCreateDTO
        {
            Nombre = "Evento Test",
            FechaInicio = DateTime.Today.AddDays(10),
            FechaFin = DateTime.Today.AddDays(12),
            Lugar = "Estadio",
            Direccion = "Av. Test 123",
            Sectores = new() { new() { Nombre = "Platea", Capacidad = 100, PrecioBase = 5000 } }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/eventos", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<EventoResponseDTO>();
        Assert.NotNull(result);
        Assert.Equal("Evento Test", result.Nombre);
        Assert.Equal(EstadoEvento.PendienteAprobacion, result.Estado);
    }

    [Fact]
    public async Task GetMisEventos_ConAutenticacion_RetornaLista()
    {
        // Arrange
        var token = await GetTokenAsync("usuario@test.com", "Usuario123!");
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync("/api/v1/eventos/mis-eventos?page=1&pageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}