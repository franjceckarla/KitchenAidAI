using System.Net;
using System.Net.Http.Json;
using KitchenAidAI.Data;
using KitchenAidAI.IntegrationTests.Infrastructure;
using KitchenAidAI.Models;
using KitchenAidAI.Models.DTOs;
using KitchenAidAI.Models.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace KitchenAidAI.IntegrationTests;

public class FrizideriApiTests : IClassFixture<KitchenAidApiFactory>
{
    private readonly KitchenAidApiFactory _factory;

    public FrizideriApiTests(KitchenAidApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/frizideri");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithOwnerUser_ReturnsCreated_AndValidBody()
    {
        var seed = await SeedUsersOnlyAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");
        var payload = new { userId = seed.OwnerUserId };

        var response = await client.PostAsJsonAsync("/api/frizideri", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<FriziderDto>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.True(body.data!.id > 0);
        Assert.Equal(seed.OwnerUserId, body.data.userId);
        Assert.False(body.data.isDeleted);
    }

    [Fact]
    public async Task Create_WithNonOwnerUser_ReturnsForbidden()
    {
        var seed = await SeedUsersOnlyAsync();
        using var client = CreateAuthorizedClient(seed.OtherUserId, "User");
        var payload = new { userId = seed.OwnerUserId };

        var response = await client.PostAsJsonAsync("/api/frizideri", payload);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(body);
        Assert.False(body!.success);
        Assert.NotNull(body.alert);
        Assert.Equal("FORBIDDEN", body.alert!.code);
    }

    [Fact]
    public async Task Create_WithMissingUser_ReturnsNotFound()
    {
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");
        var payload = new { userId = 999999 };

        var response = await client.PostAsJsonAsync("/api/frizideri", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(body);
        Assert.False(body!.success);
        Assert.NotNull(body.alert);
        Assert.Equal("NOT_FOUND", body.alert!.code);
    }

    [Fact]
    public async Task Create_WhenFridgeAlreadyExists_CurrentlyCreatesAnotherFridge()
    {
        var seed = await SeedUserWithFridgeAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");
        var payload = new { userId = seed.OwnerUserId };

        var response = await client.PostAsJsonAsync("/api/frizideri", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<FriziderDto>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.True(body.data!.id > 0);
    }

    [Fact]
    public async Task Create_WithInvalidPayload_ReturnsBadRequest()
    {
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");
        using var content = new StringContent("{\"userId\":\"not-an-int\"}", System.Text.Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/frizideri", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithOwnerUser_ReturnsOk_AndPublicBody()
    {
        var seed = await SeedFridgeWithItemsAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");

        var response = await client.GetAsync($"/api/frizideri/{seed.FridgeId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<FriziderPublicDto>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Equal(seed.FridgeId, body.data!.id);
        Assert.Equal(seed.OwnerUserId, body.data.userId);
    }

    [Fact]
    public async Task GetById_WithNonOwnerUser_ReturnsForbidden()
    {
        var seed = await SeedFridgeWithItemsAsync();
        using var client = CreateAuthorizedClient(seed.OtherUserId, "User");

        var response = await client.GetAsync($"/api/frizideri/{seed.FridgeId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(body);
        Assert.False(body!.success);
        Assert.NotNull(body.alert);
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ReturnsNotFound()
    {
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.GetAsync("/api/frizideri/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithOwnerUser_ReturnsOk_AndUpdatedBody()
    {
        var seed = await SeedFridgeWithItemsAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");
        var payload = new { isDeleted = true };

        var response = await client.PutAsJsonAsync($"/api/frizideri/{seed.FridgeId}", payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<FriziderPublicDto>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Equal(seed.FridgeId, body.data!.id);
    }

    [Fact]
    public async Task Update_WithNonOwnerUser_ReturnsForbidden()
    {
        var seed = await SeedFridgeWithItemsAsync();
        using var client = CreateAuthorizedClient(seed.OtherUserId, "User");
        var payload = new { isDeleted = true };

        var response = await client.PutAsJsonAsync($"/api/frizideri/{seed.FridgeId}", payload);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(body);
        Assert.False(body!.success);
        Assert.NotNull(body.alert);
        Assert.Equal("FORBIDDEN", body.alert!.code);
    }

    [Fact]
    public async Task Update_WithNonExistingId_ReturnsNotFound()
    {
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");
        var payload = new { isDeleted = true };

        var response = await client.PutAsJsonAsync("/api/frizideri/999999", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithOwnerUser_ReturnsOk_AndSoftDeletesFridgeAndItems()
    {
        var seed = await SeedFridgeWithItemsAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");

        var response = await client.DeleteAsync($"/api/frizideri/{seed.FridgeId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<FriziderDto>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.True(body.data!.isDeleted);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KitchenAidDbContext>();
        var fridge = db.Frizideri.First(current => current.id == seed.FridgeId);
        var items = db.Namirnice.Where(current => current.friziderId == seed.FridgeId).ToList();

        Assert.True(fridge.isDeleted);
        Assert.NotEmpty(items);
        Assert.All(items, item => Assert.True(item.isDeleted));
    }

    [Fact]
    public async Task Delete_WithNonOwnerUser_ReturnsForbidden()
    {
        var seed = await SeedFridgeWithItemsAsync();
        using var client = CreateAuthorizedClient(seed.OtherUserId, "User");

        var response = await client.DeleteAsync($"/api/frizideri/{seed.FridgeId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(body);
        Assert.False(body!.success);
        Assert.NotNull(body.alert);
    }

    [Fact]
    public async Task Delete_WithNonExistingId_ReturnsNotFound()
    {
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.DeleteAsync("/api/frizideri/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_WithOwnerUser_ReturnsOnlyOwnVisibleFridge()
    {
        var seed = await SeedQueryDataAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");

        var response = await client.GetAsync("/api/frizideri");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<FriziderPublicDto>>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Contains(body.data!, fridge => fridge.id == seed.OwnerVisibleFridgeId);
        Assert.DoesNotContain(body.data!, fridge => fridge.id == seed.OwnerDeletedFridgeId);
        Assert.DoesNotContain(body.data!, fridge => fridge.id == seed.OtherUserFridgeId);
    }

    [Fact]
    public async Task GetAll_WithAdminIncludeDeletedFalse_ExcludesDeletedFridges()
    {
        var seed = await SeedQueryDataAsync();
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.GetAsync("/api/frizideri?includeDeleted=false");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<FriziderDto>>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Contains(body.data!, fridge => fridge.id == seed.OwnerVisibleFridgeId);
        Assert.DoesNotContain(body.data!, fridge => fridge.id == seed.OwnerDeletedFridgeId);
    }

    [Fact]
    public async Task GetAll_WithAdminIncludeDeletedTrueAndUserFilter_ReturnsFilteredSet()
    {
        var seed = await SeedQueryDataAsync();
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.GetAsync($"/api/frizideri?includeDeleted=true&userId={seed.OwnerUserId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<FriziderDto>>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Contains(body.data!, fridge => fridge.id == seed.OwnerVisibleFridgeId);
        Assert.Contains(body.data!, fridge => fridge.id == seed.OwnerDeletedFridgeId);
        Assert.DoesNotContain(body.data!, fridge => fridge.id == seed.OtherUserFridgeId);
    }

    [Fact]
    public async Task GetAll_WithAdminDateRangeAndSortByKreiranoDesc_ReturnsExpectedOrder()
    {
        var seed = await SeedQueryDataAsync();
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.GetAsync(
            $"/api/frizideri?includeDeleted=true&odDatuma={seed.FromDate:yyyy-MM-dd}&doDatuma={seed.ToDate:yyyy-MM-dd}&sortBy=kreirano&sortDir=desc");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<FriziderDto>>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.True(body.data!.Count >= 2);

        for (var i = 0; i < body.data.Count - 1; i++)
        {
            var currentId = body.data[i].id;
            var nextId = body.data[i + 1].id;
            Assert.NotEqual(nextId, currentId);
        }
    }

    private HttpClient CreateAuthorizedClient(int userId, string role)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
        return client;
    }

    private async Task<UsersSeedResult> SeedUsersOnlyAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KitchenAidDbContext>();

        var owner = new User
        {
            username = $"fridge-owner-{Guid.NewGuid():N}",
            email = $"fridge-owner-{Guid.NewGuid():N}@example.com",
            preferencijaPrehrane = PreferencijaPrehrane.Omnivorte,
            isAdmin = false,
            isDeleted = false
        };

        var other = new User
        {
            username = $"fridge-other-{Guid.NewGuid():N}",
            email = $"fridge-other-{Guid.NewGuid():N}@example.com",
            preferencijaPrehrane = PreferencijaPrehrane.Omnivorte,
            isAdmin = false,
            isDeleted = false
        };

        db.Users.AddRange(owner, other);
        await db.SaveChangesAsync();

        return new UsersSeedResult(owner.id, other.id);
    }

    private async Task<FridgeSeedResult> SeedUserWithFridgeAsync(bool fridgeDeleted = false)
    {
        var users = await SeedUsersOnlyAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KitchenAidDbContext>();

        var fridge = new Frizider
        {
            userId = users.OwnerUserId,
            isDeleted = fridgeDeleted,
            kreirano = DateTime.UtcNow,
            azurirano = DateTime.UtcNow
        };

        db.Frizideri.Add(fridge);
        await db.SaveChangesAsync();

        return new FridgeSeedResult(users.OwnerUserId, users.OtherUserId, fridge.id, 0, 0);
    }

    private async Task<FridgeSeedResult> SeedFridgeWithItemsAsync()
    {
        var seed = await SeedUserWithFridgeAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KitchenAidDbContext>();

        var item1 = new Namirnica
        {
            friziderId = seed.FridgeId,
            naziv = $"Namirnica-1-{Guid.NewGuid():N}",
            kategorija = KategorijaNamirnice.Povrce,
            mjera = Mjera.Komad,
            kolicinaUFrizideru = 2,
            isDeleted = false
        };

        var item2 = new Namirnica
        {
            friziderId = seed.FridgeId,
            naziv = $"Namirnica-2-{Guid.NewGuid():N}",
            kategorija = KategorijaNamirnice.Meso,
            mjera = Mjera.Gram,
            kolicinaUFrizideru = 500,
            isDeleted = false
        };

        db.Namirnice.AddRange(item1, item2);
        await db.SaveChangesAsync();

        return seed with { ItemOneId = item1.id, ItemTwoId = item2.id };
    }

    private async Task<QuerySeedResult> SeedQueryDataAsync()
    {
        var users = await SeedUsersOnlyAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KitchenAidDbContext>();

        var now = DateTime.UtcNow.Date;

        var ownerVisible = new Frizider
        {
            userId = users.OwnerUserId,
            isDeleted = false,
            kreirano = now.AddDays(-3),
            azurirano = now.AddDays(-1)
        };

        var ownerDeleted = new Frizider
        {
            userId = users.OwnerUserId,
            isDeleted = true,
            kreirano = now.AddDays(-2),
            azurirano = now
        };

        var otherVisible = new Frizider
        {
            userId = users.OtherUserId,
            isDeleted = false,
            kreirano = now.AddDays(-1),
            azurirano = now
        };

        db.Frizideri.AddRange(ownerVisible, ownerDeleted, otherVisible);
        await db.SaveChangesAsync();

        return new QuerySeedResult(
            OwnerUserId: users.OwnerUserId,
            OtherUserId: users.OtherUserId,
            OwnerVisibleFridgeId: ownerVisible.id,
            OwnerDeletedFridgeId: ownerDeleted.id,
            OtherUserFridgeId: otherVisible.id,
            FromDate: now.AddDays(-10),
            ToDate: now.AddDays(1));
    }

    private sealed record UsersSeedResult(int OwnerUserId, int OtherUserId);

    private sealed record FridgeSeedResult(int OwnerUserId, int OtherUserId, int FridgeId, int ItemOneId, int ItemTwoId);

    private sealed record QuerySeedResult(
        int OwnerUserId,
        int OtherUserId,
        int OwnerVisibleFridgeId,
        int OwnerDeletedFridgeId,
        int OtherUserFridgeId,
        DateTime FromDate,
        DateTime ToDate);
}
