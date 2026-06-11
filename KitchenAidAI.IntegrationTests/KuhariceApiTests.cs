using System.Net;
using System.Net.Http.Json;
using KitchenAidAI.Data;
using KitchenAidAI.IntegrationTests.Infrastructure;
using KitchenAidAI.Models;
using KitchenAidAI.Models.DTOs;
using KitchenAidAI.Models.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace KitchenAidAI.IntegrationTests;

public class KuhariceApiTests : IClassFixture<KitchenAidApiFactory>
{
    private readonly KitchenAidApiFactory _factory;

    public KuhariceApiTests(KitchenAidApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/kuharice");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithOwnerUser_ReturnsCreated_AndValidBody()
    {
        var seed = await SeedUsersOnlyAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");
        var payload = new { userId = seed.OwnerUserId, naziv = $"Kuharica-{Guid.NewGuid():N}" };

        var response = await client.PostAsJsonAsync("/api/kuharice", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<KuharicaDto>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Equal(seed.OwnerUserId, body.data!.userId);
        Assert.False(body.data.isDeleted);
    }

    [Fact]
    public async Task Create_WithNonOwnerUser_ReturnsForbidden()
    {
        var seed = await SeedUsersOnlyAsync();
        using var client = CreateAuthorizedClient(seed.OtherUserId, "User");
        var payload = new { userId = seed.OwnerUserId, naziv = "Nije dozvoljeno" };

        var response = await client.PostAsJsonAsync("/api/kuharice", payload);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(body);
        Assert.False(body!.success);
        Assert.NotNull(body.alert);
    }

    [Fact]
    public async Task Create_WithMissingUser_ReturnsNotFound()
    {
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");
        var payload = new { userId = 999999, naziv = "Nepostojeci" };

        var response = await client.PostAsJsonAsync("/api/kuharice", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ForUserWithExistingCookbook_ReturnsBadRequestAndCode()
    {
        var seed = await SeedUserWithCookbookAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");
        var payload = new { userId = seed.OwnerUserId, naziv = "Duplicate cookbook" };

        var response = await client.PostAsJsonAsync("/api/kuharice", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(body);
        Assert.False(body!.success);
        Assert.NotNull(body.alert);
        Assert.Equal("COOKBOOK_EXISTS", body.alert!.code);
    }

    [Fact]
    public async Task Create_WithInvalidPayload_ReturnsBadRequest()
    {
        var seed = await SeedUsersOnlyAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");
        var payload = new
        {
            userId = seed.OwnerUserId,
            naziv = new string('a', 250)
        };

        var response = await client.PostAsJsonAsync("/api/kuharice", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithOwnerUser_ReturnsOk()
    {
        var seed = await SeedCookbookWithJoinAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");

        var response = await client.GetAsync($"/api/kuharice/{seed.CookbookId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<KuharicaPublicDto>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Equal(seed.CookbookId, body.data!.id);
    }

    [Fact]
    public async Task GetById_WithNonOwnerUser_ReturnsForbidden()
    {
        var seed = await SeedCookbookWithJoinAsync();
        using var client = CreateAuthorizedClient(seed.OtherUserId, "User");

        var response = await client.GetAsync($"/api/kuharice/{seed.CookbookId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ReturnsNotFound()
    {
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.GetAsync("/api/kuharice/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithOwnerUser_ReturnsOk_AndUpdatedName()
    {
        var seed = await SeedCookbookWithJoinAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");
        var payload = new { naziv = $"Updated-{Guid.NewGuid():N}" };

        var response = await client.PutAsJsonAsync($"/api/kuharice/{seed.CookbookId}", payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<KuharicaPublicDto>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Equal(payload.naziv, body.data!.naziv);
    }

    [Fact]
    public async Task Update_WithNonOwnerUser_ReturnsForbidden()
    {
        var seed = await SeedCookbookWithJoinAsync();
        using var client = CreateAuthorizedClient(seed.OtherUserId, "User");
        var payload = new { naziv = "Unauthorized update" };

        var response = await client.PutAsJsonAsync($"/api/kuharice/{seed.CookbookId}", payload);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithNonExistingId_ReturnsNotFound()
    {
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");
        var payload = new { naziv = "Nope" };

        var response = await client.PutAsJsonAsync("/api/kuharice/999999", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithOwnerUser_ReturnsOk_AndSoftDeletesCookbookAndJoins()
    {
        var seed = await SeedCookbookWithJoinAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");

        var response = await client.DeleteAsync($"/api/kuharice/{seed.CookbookId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<KuharicaDto>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.True(body.data!.isDeleted);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KitchenAidDbContext>();
        var join = db.ReceptKuharice.First(current => current.id == seed.JoinId);
        Assert.True(join.isDeleted);
    }

    [Fact]
    public async Task Delete_WithNonOwnerUser_ReturnsForbidden()
    {
        var seed = await SeedCookbookWithJoinAsync();
        using var client = CreateAuthorizedClient(seed.OtherUserId, "User");

        var response = await client.DeleteAsync($"/api/kuharice/{seed.CookbookId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithNonExistingId_ReturnsNotFound()
    {
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.DeleteAsync("/api/kuharice/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_WithAdminIncludeDeletedFalse_ExcludesDeletedCookbooks()
    {
        var seed = await SeedQueryDataAsync();
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.GetAsync($"/api/kuharice?search={seed.SearchToken}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<KuharicaDto>>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Contains(body.data!, cookbook => cookbook.id == seed.OwnerVisibleCookbookId);
        Assert.DoesNotContain(body.data!, cookbook => cookbook.id == seed.OwnerDeletedCookbookId);
    }

    [Fact]
    public async Task GetAll_WithAdminIncludeDeletedTrueAndUserFilter_ReturnsFilteredSet()
    {
        var seed = await SeedQueryDataAsync();
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.GetAsync($"/api/kuharice?search={seed.SearchToken}&includeDeleted=true&filterUserId={seed.OwnerUserId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<KuharicaDto>>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Contains(body.data!, cookbook => cookbook.id == seed.OwnerVisibleCookbookId);
        Assert.Contains(body.data!, cookbook => cookbook.id == seed.OwnerDeletedCookbookId);
        Assert.DoesNotContain(body.data!, cookbook => cookbook.id == seed.OtherUserCookbookId);
    }

    [Fact]
    public async Task GetAll_WithOwnerUser_ReturnsOnlyOwnVisibleCookbooks()
    {
        var seed = await SeedQueryDataAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");

        var response = await client.GetAsync("/api/kuharice");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<KuharicaPublicDto>>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Contains(body.data!, cookbook => cookbook.id == seed.OwnerVisibleCookbookId);
        Assert.DoesNotContain(body.data!, cookbook => cookbook.id == seed.OwnerDeletedCookbookId);
        Assert.DoesNotContain(body.data!, cookbook => cookbook.id == seed.OtherUserCookbookId);
    }

    [Fact]
    public async Task GetAll_WithAdminDateRangeAndSortByNazivDesc_ReturnsExpectedOrder()
    {
        var seed = await SeedQueryDataAsync();
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.GetAsync(
            $"/api/kuharice?search={seed.SearchToken}&includeDeleted=true&odDatuma={seed.FromDate:yyyy-MM-dd}&doDatuma={seed.ToDate:yyyy-MM-dd}&sortBy=naziv&sortDir=desc");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<KuharicaDto>>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.True(body.data!.Count >= 2);

        for (var i = 0; i < body.data.Count - 1; i++)
        {
            var current = body.data[i].naziv ?? string.Empty;
            var next = body.data[i + 1].naziv ?? string.Empty;
            Assert.True(string.Compare(current, next, StringComparison.Ordinal) >= 0);
        }
    }

    private HttpClient CreateAuthorizedClient(int userId, string role)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
        return client;
    }

    private async Task<UsersSeed> SeedUsersOnlyAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KitchenAidDbContext>();

        var owner = new User
        {
            username = $"owner-kuh-{Guid.NewGuid():N}",
            email = $"owner-kuh-{Guid.NewGuid():N}@example.com",
            preferencijaPrehrane = PreferencijaPrehrane.Omnivorte,
            isAdmin = false,
            isDeleted = false
        };

        var other = new User
        {
            username = $"other-kuh-{Guid.NewGuid():N}",
            email = $"other-kuh-{Guid.NewGuid():N}@example.com",
            preferencijaPrehrane = PreferencijaPrehrane.Omnivorte,
            isAdmin = false,
            isDeleted = false
        };

        db.Users.AddRange(owner, other);
        await db.SaveChangesAsync();

        return new UsersSeed(owner.id, other.id);
    }

    private async Task<CookbookSeed> SeedUserWithCookbookAsync()
    {
        var users = await SeedUsersOnlyAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KitchenAidDbContext>();

        var cookbook = new Kuharica
        {
            userId = users.OwnerUserId,
            naziv = $"Kuharica-{Guid.NewGuid():N}",
            isDeleted = false
        };

        db.Kuharice.Add(cookbook);
        await db.SaveChangesAsync();

        return new CookbookSeed(users.OwnerUserId, users.OtherUserId, cookbook.id, 0);
    }

    private async Task<CookbookSeed> SeedCookbookWithJoinAsync()
    {
        var seed = await SeedUserWithCookbookAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KitchenAidDbContext>();

        var recipe = new Recept
        {
            naziv = $"Recept-{Guid.NewGuid():N}",
            opis = "Test recept",
            vrijemeKuhanja = 20,
            brojPorcija = 2,
            tezina = TezinaRecepta.Lako,
            isDeleted = false
        };

        db.Recepti.Add(recipe);
        await db.SaveChangesAsync();

        var join = new ReceptKuharica
        {
            receptId = recipe.id,
            kuharicaId = seed.CookbookId,
            isDeleted = false
        };

        db.ReceptKuharice.Add(join);
        await db.SaveChangesAsync();

        return seed with { JoinId = join.id };
    }

    private async Task<QuerySeed> SeedQueryDataAsync()
    {
        var users = await SeedUsersOnlyAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KitchenAidDbContext>();

        var token = $"query-kuh-{Guid.NewGuid():N}";
        var baseDate = DateTime.UtcNow.Date;

        var ownerVisible = new Kuharica
        {
            userId = users.OwnerUserId,
            naziv = $"{token}-A-visible",
            isDeleted = false,
            kreirano = baseDate.AddDays(-1)
        };

        var ownerDeleted = new Kuharica
        {
            userId = users.OwnerUserId,
            naziv = $"{token}-B-deleted",
            isDeleted = true,
            kreirano = baseDate.AddDays(-2)
        };

        var otherVisible = new Kuharica
        {
            userId = users.OtherUserId,
            naziv = $"{token}-C-other",
            isDeleted = false,
            kreirano = baseDate.AddDays(-3)
        };

        db.Kuharice.AddRange(ownerVisible, ownerDeleted, otherVisible);
        await db.SaveChangesAsync();

        return new QuerySeed(
            users.OwnerUserId,
            users.OtherUserId,
            token,
            ownerVisible.id,
            ownerDeleted.id,
            otherVisible.id,
            baseDate.AddDays(-10),
            baseDate.AddDays(1));
    }

    private sealed record UsersSeed(int OwnerUserId, int OtherUserId);

    private sealed record CookbookSeed(int OwnerUserId, int OtherUserId, int CookbookId, int JoinId);

    private sealed record QuerySeed(
        int OwnerUserId,
        int OtherUserId,
        string SearchToken,
        int OwnerVisibleCookbookId,
        int OwnerDeletedCookbookId,
        int OtherUserCookbookId,
        DateTime FromDate,
        DateTime ToDate);
}
