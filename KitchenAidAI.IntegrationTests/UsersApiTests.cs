using System.Net;
using System.Net.Http.Json;
using KitchenAidAI.IntegrationTests.Infrastructure;
using KitchenAidAI.Models.DTOs;

namespace KitchenAidAI.IntegrationTests;

public class UsersApiTests : IClassFixture<KitchenAidApiFactory>
{
    private readonly KitchenAidApiFactory _factory;

    public UsersApiTests(KitchenAidApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithAuthenticatedNonAdminForAnotherUser_ReturnsForbidden()
    {
        using var client = CreateAuthorizedClient(userId: 5, role: "User");

        var response = await client.GetAsync("/api/users/1");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithAdminAuthentication_ReturnsCreated()
    {
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");
        var payload = new
        {
            username = $"user-{Guid.NewGuid():N}",
            ime = "Test",
            prezime = "User",
            email = $"test-{Guid.NewGuid():N}@example.com",
            password = "test123",
            preferencijaPrehrane = 0
        };

        var response = await client.PostAsJsonAsync("/api/users", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithAdminForMissingUser_ReturnsNotFound()
    {
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.GetAsync("/api/users/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithAdminAndInvalidPayload_ReturnsBadRequest()
    {
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");
        var payload = new
        {
            ime = "Invalid",
            prezime = "Payload"
        };

        var response = await client.PostAsJsonAsync("/api/users", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithAuthenticatedUserForOwnId_ReturnsOk()
    {
        var createdUserId = await CreateUserAsAdminAsync();
        using var client = CreateAuthorizedClient(userId: createdUserId, role: "User");

        var response = await client.GetAsync($"/api/users/{createdUserId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithAdminAuthentication_ReturnsOk()
    {
        var createdUserId = await CreateUserAsAdminAsync();
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");
        var payload = new
        {
            username = $"admin-updated-{Guid.NewGuid():N}",
            email = $"admin-updated-{Guid.NewGuid():N}@example.com",
            preferencijaPrehrane = 1,
            isAdmin = false,
            newPassword = "newpass123"
        };

        var response = await client.PutAsJsonAsync($"/api/users/{createdUserId}", payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithAuthenticatedUserForOwnId_ReturnsOk()
    {
        var createdUserId = await CreateUserAsAdminAsync();
        using var client = CreateAuthorizedClient(userId: createdUserId, role: "User");
        var payload = new
        {
            username = $"user-updated-{Guid.NewGuid():N}",
            email = $"user-updated-{Guid.NewGuid():N}@example.com",
            preferencijaPrehrane = 2,
            isAdmin = true,
            newPassword = "should-be-ignored-for-user"
        };

        var response = await client.PutAsJsonAsync($"/api/users/{createdUserId}", payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithAdminAuthentication_ReturnsOk()
    {
        var createdUserId = await CreateUserAsAdminAsync();
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.DeleteAsync($"/api/users/{createdUserId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Restore_WithAdminAuthentication_ReturnsOk()
    {
        var createdUserId = await CreateUserAsAdminAsync();
        using var adminClient = CreateAuthorizedClient(userId: 1, role: "Admin");

        var deleteResponse = await adminClient.DeleteAsync($"/api/users/{createdUserId}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var restoreResponse = await adminClient.PostAsync($"/api/users/{createdUserId}/restore", content: null);

        Assert.Equal(HttpStatusCode.OK, restoreResponse.StatusCode);
    }

    [Fact]
    public async Task Restore_WithAuthenticatedNonAdmin_ReturnsForbidden()
    {
        var createdUserId = await CreateUserAsAdminAsync();
        using var userClient = CreateAuthorizedClient(userId: createdUserId, role: "User");

        var response = await userClient.PostAsync($"/api/users/{createdUserId}/restore", content: null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<int> CreateUserAsAdminAsync()
    {
        using var adminClient = CreateAuthorizedClient(userId: 1, role: "Admin");
        var payload = new
        {
            username = $"user-{Guid.NewGuid():N}",
            ime = "Integration",
            prezime = "Test",
            email = $"user-{Guid.NewGuid():N}@example.com",
            password = "test123",
            preferencijaPrehrane = 0
        };

        var createResponse = await adminClient.PostAsJsonAsync("/api/users", payload);
        createResponse.EnsureSuccessStatusCode();

        var apiResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
        Assert.NotNull(apiResponse);
        Assert.NotNull(apiResponse!.data);

        return apiResponse.data!.id;
    }

    private HttpClient CreateAuthorizedClient(int userId, string role)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
        return client;
    }
}