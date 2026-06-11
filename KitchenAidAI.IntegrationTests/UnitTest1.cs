using System.Net;
using System.Net.Http.Json;
using KitchenAidAI.IntegrationTests.Infrastructure;

namespace KitchenAidAI.IntegrationTests;

public class CountriesApiTests : IClassFixture<KitchenAidApiFactory>
{
    private readonly KitchenAidApiFactory _factory;

    public CountriesApiTests(KitchenAidApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/countries");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithAuthenticatedNonAdmin_ReturnsForbidden()
    {
        using var client = CreateAuthorizedClient(userId: 5, role: "User");
        var payload = new { naziv = $"NoAdmin-{Guid.NewGuid():N}" };

        var response = await client.PostAsJsonAsync("/api/countries", payload);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithAdminAuthentication_ReturnsCreated()
    {
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");
        var payload = new { naziv = $"Admin-{Guid.NewGuid():N}" };

        var response = await client.PostAsJsonAsync("/api/countries", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private HttpClient CreateAuthorizedClient(int userId, string role)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
        return client;
    }
}
