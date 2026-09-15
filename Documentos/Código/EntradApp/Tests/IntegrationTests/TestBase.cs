using EntradApp.Tests.IntegrationTests;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;

namespace EntradApp.Tests.IntegrationTests;

public class TestBase : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly HttpClient _client;
    protected readonly CustomWebApplicationFactory _factory;

    public TestBase(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    protected async Task<string> GetTokenAsync(string email = "admin@entradapp.com", string password = "Admin123!")
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return result!.Token;
    }

    protected void SetAuthHeader(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    protected void ClearAuthHeader()
    {
        _client.DefaultRequestHeaders.Authorization = null;
    }
}

public record LoginResponse(string Token, object Usuario);