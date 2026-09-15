using EntradApp.Tests.IntegrationTests;
using EntradApp.Shared.DTOs.Admin;
using EntradApp.Shared.Enums;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EntradApp.Tests.IntegrationTests.Controllers;

public class AdminControllerTests : TestBase
{
    public AdminControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetEventosPendientes_SinAutenticacion_Retorna401()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/admin/eventos/pendientes");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetEventosPendientes_ConUsuarioNormal_Retorna403()
    {
        // Arrange
        var token = await GetTokenAsync("usuario@test.com", "Usuario123!");
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync("/api/v1/admin/eventos/pendientes");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetEventosPendientes_ConSuperAdmin_Retorna200()
    {
        // Arrange
        var token = await GetTokenAsync("admin@entradapp.com", "Admin123!");
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync("/api/v1/admin/eventos/pendientes?page=1&pageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AprobarEvento_EventoPendiente_Retorna204()
    {
        // Arrange
        var adminToken = await GetTokenAsync("admin@entradapp.com", "Admin123!");
        
        // Crear evento pendiente como usuario
        var userToken = await GetTokenAsync("usuario@test.com", "Usuario123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        
        var eventoDto = new EntradApp.Shared.DTOs.Evento.EventoCreateDTO
        {
            Nombre = "Evento Para Aprobar",
            FechaInicio = DateTime.Today.AddDays(10),
            FechaFin = DateTime.Today.AddDays(12),
            Lugar = "Estadio",
            Direccion = "Av. Test",
            Sectores = new() { new() { Nombre = "S1", Capacidad = 100, PrecioBase = 5000 } }
        };
        var eventoResp = await _client.PostAsJsonAsync("/api/v1/eventos", eventoDto);
        var evento = await eventoResp.Content.ReadFromJsonAsync<EntradApp.Shared.DTOs.Evento.EventoResponseDTO>();

        // Act - Aprobar como admin
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var dto = new EventoEstadoUpdateDTO { NuevoEstado = EstadoEvento.Aprobado, Observaciones = "Aprobado" };
        var response = await _client.PatchAsJsonAsync($"/api/v1/admin/eventos/{evento!.Id}/estado", dto);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task RechazarEvento_EventoPendiente_Retorna204()
    {
        // Arrange
        var adminToken = await GetTokenAsync("admin@entradapp.com", "Admin123!");
        
        // Crear evento pendiente
        var userToken = await GetTokenAsync("usuario@test.com", "Usuario123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        
        var eventoDto = new EntradApp.Shared.DTOs.Evento.EventoCreateDTO
        {
            Nombre = "Evento Para Rechazar",
            FechaInicio = DateTime.Today.AddDays(10),
            FechaFin = DateTime.Today.AddDays(12),
            Lugar = "Estadio",
            Direccion = "Av. Test",
            Sectores = new() { new() { Nombre = "S1", Capacidad = 100, PrecioBase = 5000 } }
        };
        var eventoResp = await _client.PostAsJsonAsync("/api/v1/eventos", eventoDto);
        var evento = await eventoResp.Content.ReadFromJsonAsync<EntradApp.Shared.DTOs.Evento.EventoResponseDTO>();

        // Act - Rechazar
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var dto = new EventoEstadoUpdateDTO { NuevoEstado = EstadoEvento.Rechazado, Observaciones = "No cumple requisitos" };
        var response = await _client.PatchAsJsonAsync($"/api/v1/admin/eventos/{evento!.Id}/estado", dto);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}