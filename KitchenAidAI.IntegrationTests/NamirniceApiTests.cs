using System.Net;
using System.Net.Http.Json;
using KitchenAidAI.Data;
using KitchenAidAI.IntegrationTests.Infrastructure;
using KitchenAidAI.Models;
using KitchenAidAI.Models.DTOs;
using KitchenAidAI.Models.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace KitchenAidAI.IntegrationTests;

public class NamirniceApiTests : IClassFixture<KitchenAidApiFactory>
{
    private readonly KitchenAidApiFactory _factory;

    public NamirniceApiTests(KitchenAidApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/namirnice");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithOwnerUser_ReturnsCreated_AndValidBody()
    {
        var seed = await SeedOwnerContextAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");
        var payload = BuildValidPayload(seed.OwnerFridgeId, "OwnerCreate");

        var response = await client.PostAsJsonAsync("/api/namirnice", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<NamirnicaDto>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.True(body.data!.id > 0);
        Assert.Equal(seed.OwnerFridgeId, body.data.friziderId);
    }

    [Fact]
    public async Task Create_WithNonOwnerUser_ReturnsForbidden()
    {
        var seed = await SeedOwnerContextAsync();
        using var client = CreateAuthorizedClient(seed.OtherUserId, "User");
        var payload = BuildValidPayload(seed.OwnerFridgeId, "NonOwnerCreate");

        var response = await client.PostAsJsonAsync("/api/namirnice", payload);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(body);
        Assert.False(body!.success);
        Assert.NotNull(body.alert);
    }

    [Fact]
    public async Task Create_WithMissingFridge_ReturnsNotFound()
    {
        var seed = await SeedOwnerContextAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");
        var payload = BuildValidPayload(999999, "MissingFridgeCreate");

        var response = await client.PostAsJsonAsync("/api/namirnice", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(body);
        Assert.False(body!.success);
        Assert.NotNull(body.alert);
    }

    [Fact]
    public async Task Create_WithInvalidPayload_ReturnsBadRequest()
    {
        var seed = await SeedOwnerContextAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");
        var payload = new
        {
            friziderId = seed.OwnerFridgeId,
            naziv = (string?)null,
            kategorija = KategorijaNamirnice.Voce,
            mjera = Mjera.Komad,
            kolicinaUFrizideru = 3.0
        };

        var response = await client.PostAsJsonAsync("/api/namirnice", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithOwnerUser_ReturnsOk()
    {
        var seed = await SeedNamirnicaForOwnerAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");

        var response = await client.GetAsync($"/api/namirnice/{seed.ItemId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<NamirnicaPublicDto>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Equal(seed.ItemId, body.data!.id);
    }

    [Fact]
    public async Task GetById_WithNonOwnerUser_ReturnsForbidden()
    {
        var seed = await SeedNamirnicaForOwnerAsync();
        using var client = CreateAuthorizedClient(seed.OtherUserId, "User");

        var response = await client.GetAsync($"/api/namirnice/{seed.ItemId}");

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

        var response = await client.GetAsync("/api/namirnice/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithOwnerUser_ReturnsOk_AndUpdatedFields()
    {
        var seed = await SeedNamirnicaForOwnerAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");
        var payload = new
        {
            friziderId = seed.OwnerFridgeId,
            naziv = $"Namirnica-OwnerUpdate-{Guid.NewGuid():N}",
            kategorija = KategorijaNamirnice.Meso,
            mjera = Mjera.Kilogram,
            kolicinaUFrizideru = 9.5
        };

        var response = await client.PutAsJsonAsync($"/api/namirnice/{seed.ItemId}", payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<NamirnicaPublicDto>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Equal(payload.naziv, body.data!.naziv);
        Assert.Equal(payload.kategorija, body.data.kategorija);
        Assert.Equal(payload.kolicinaUFrizideru, body.data.kolicinaUFrizideru);
    }

    [Fact]
    public async Task Update_WithNonOwnerUser_ReturnsForbidden()
    {
        var seed = await SeedNamirnicaForOwnerAsync();
        using var client = CreateAuthorizedClient(seed.OtherUserId, "User");
        var payload = BuildValidPayload(seed.OwnerFridgeId, "NonOwnerUpdate");

        var response = await client.PutAsJsonAsync($"/api/namirnice/{seed.ItemId}", payload);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithNonExistingId_ReturnsNotFound()
    {
        var seed = await SeedOwnerContextAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");
        var payload = BuildValidPayload(seed.OwnerFridgeId, "MissingUpdate");

        var response = await client.PutAsJsonAsync("/api/namirnice/999999", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithInvalidPayload_ReturnsBadRequest()
    {
        var seed = await SeedNamirnicaForOwnerAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");
        var payload = new
        {
            friziderId = seed.OwnerFridgeId,
            naziv = (string?)null,
            kategorija = KategorijaNamirnice.Povrce,
            mjera = Mjera.Gram,
            kolicinaUFrizideru = 2.2
        };

        var response = await client.PutAsJsonAsync($"/api/namirnice/{seed.ItemId}", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithOwnerUser_ReturnsOk_AndSoftDeletesItem()
    {
        var seed = await SeedNamirnicaForOwnerAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");

        var response = await client.DeleteAsync($"/api/namirnice/{seed.ItemId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<NamirnicaDto>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.True(body.data!.isDeleted);

        var ownerGet = await client.GetAsync($"/api/namirnice/{seed.ItemId}");
        Assert.Equal(HttpStatusCode.NotFound, ownerGet.StatusCode);
    }

    [Fact]
    public async Task Delete_WithNonOwnerUser_ReturnsForbidden()
    {
        var seed = await SeedNamirnicaForOwnerAsync();
        using var client = CreateAuthorizedClient(seed.OtherUserId, "User");

        var response = await client.DeleteAsync($"/api/namirnice/{seed.ItemId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithNonExistingId_ReturnsNotFound()
    {
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.DeleteAsync("/api/namirnice/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_WithAdminIncludeDeletedFalse_ExcludesDeletedItems()
    {
        var seed = await SeedQueryDataAsync();
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.GetAsync($"/api/namirnice?search={seed.SearchToken}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<NamirnicaDto>>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Contains(body.data!, item => item.id == seed.OwnerVisibleItemId);
        Assert.DoesNotContain(body.data!, item => item.id == seed.OwnerDeletedItemId);
    }

    [Fact]
    public async Task GetAll_WithAdminIncludeDeletedTrue_IncludesDeletedItems()
    {
        var seed = await SeedQueryDataAsync();
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.GetAsync($"/api/namirnice?search={seed.SearchToken}&includeDeleted=true");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<NamirnicaDto>>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Contains(body.data!, item => item.id == seed.OwnerVisibleItemId);
        Assert.Contains(body.data!, item => item.id == seed.OwnerDeletedItemId);
    }

    [Fact]
    public async Task GetAll_WithNonAdmin_ReturnsOnlyOwnVisibleItems()
    {
        var seed = await SeedQueryDataAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");

        var response = await client.GetAsync("/api/namirnice");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<NamirnicaPublicDto>>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Contains(body.data!, item => item.id == seed.OwnerVisibleItemId);
        Assert.DoesNotContain(body.data!, item => item.id == seed.OwnerDeletedItemId);
        Assert.DoesNotContain(body.data!, item => item.id == seed.OtherUserItemId);
    }

    [Fact]
    public async Task GetAll_WithAdminFiltersAndSort_ReturnsExpectedSubset()
    {
        var seed = await SeedQueryDataAsync();
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.GetAsync(
            $"/api/namirnice?search={seed.SearchToken}&includeDeleted=true&friziderId={seed.OwnerFridgeId}&kategorija={(int)KategorijaNamirnice.Povrce}&mjera={(int)Mjera.Komad}&minKolicina=2&maxKolicina=8&minKalorije=10&maxKalorije=120&odDatuma={seed.FromDate:yyyy-MM-dd}&doDatuma={seed.ToDate:yyyy-MM-dd}&sortBy=kalorije&sortDir=desc");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<NamirnicaDto>>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.True(body.data!.Count >= 1);
        Assert.Contains(body.data, item => item.id == seed.OwnerVisibleItemId);
    }

    private HttpClient CreateAuthorizedClient(int userId, string role)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
        return client;
    }

    private static object BuildValidPayload(int fridgeId, string suffix, double kolicina = 4.0, KategorijaNamirnice kategorija = KategorijaNamirnice.Povrce, Mjera mjera = Mjera.Komad)
    {
        return new
        {
            friziderId = fridgeId,
            naziv = $"Namirnica-{suffix}-{Guid.NewGuid():N}",
            kategorija,
            mjera,
            kolicinaUFrizideru = kolicina
        };
    }

    private async Task<OwnerContextSeed> SeedOwnerContextAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KitchenAidDbContext>();

        var owner = new User
        {
            username = $"owner-nam-{Guid.NewGuid():N}",
            email = $"owner-nam-{Guid.NewGuid():N}@example.com",
            preferencijaPrehrane = PreferencijaPrehrane.Omnivorte,
            isAdmin = false,
            isDeleted = false
        };

        var other = new User
        {
            username = $"other-nam-{Guid.NewGuid():N}",
            email = $"other-nam-{Guid.NewGuid():N}@example.com",
            preferencijaPrehrane = PreferencijaPrehrane.Omnivorte,
            isAdmin = false,
            isDeleted = false
        };

        db.Users.AddRange(owner, other);
        await db.SaveChangesAsync();

        var ownerFridge = new Frizider { userId = owner.id, isDeleted = false };
        var otherFridge = new Frizider { userId = other.id, isDeleted = false };

        db.Frizideri.AddRange(ownerFridge, otherFridge);
        await db.SaveChangesAsync();

        return new OwnerContextSeed(owner.id, other.id, ownerFridge.id, otherFridge.id);
    }

    private async Task<OwnerItemSeed> SeedNamirnicaForOwnerAsync()
    {
        var seed = await SeedOwnerContextAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KitchenAidDbContext>();

        var item = new Namirnica
        {
            friziderId = seed.OwnerFridgeId,
            naziv = $"owner-item-{Guid.NewGuid():N}",
            kategorija = KategorijaNamirnice.Voce,
            mjera = Mjera.Komad,
            kolicinaUFrizideru = 3,
            isDeleted = false,
            nutritivnaVrijednost = new NutritivnaVrijednost
            {
                kalorije = 50,
                proteini = 1,
                masti = 0.3,
                ugljikohidrati = 12,
                vlakna = 2,
                sol = 0.1
            }
        };

        db.Namirnice.Add(item);
        await db.SaveChangesAsync();

        return new OwnerItemSeed(seed.OwnerUserId, seed.OtherUserId, seed.OwnerFridgeId, item.id);
    }

    private async Task<QuerySeedResult> SeedQueryDataAsync()
    {
        var seed = await SeedOwnerContextAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KitchenAidDbContext>();

        var token = $"query-nam-{Guid.NewGuid():N}";
        var baseDate = DateTime.UtcNow.Date;

        var ownerVisible = new Namirnica
        {
            friziderId = seed.OwnerFridgeId,
            naziv = $"{token}-visible",
            kategorija = KategorijaNamirnice.Povrce,
            mjera = Mjera.Komad,
            kolicinaUFrizideru = 4,
            isDeleted = false,
            kreirano = baseDate.AddDays(-1),
            nutritivnaVrijednost = new NutritivnaVrijednost { kalorije = 70, proteini = 2, masti = 1, ugljikohidrati = 10, vlakna = 3, sol = 0.2 }
        };

        var ownerDeleted = new Namirnica
        {
            friziderId = seed.OwnerFridgeId,
            naziv = $"{token}-deleted",
            kategorija = KategorijaNamirnice.Povrce,
            mjera = Mjera.Komad,
            kolicinaUFrizideru = 5,
            isDeleted = true,
            kreirano = baseDate.AddDays(-2),
            nutritivnaVrijednost = new NutritivnaVrijednost { kalorije = 90, proteini = 2, masti = 1.5, ugljikohidrati = 12, vlakna = 4, sol = 0.2 }
        };

        var otherUserItem = new Namirnica
        {
            friziderId = seed.OtherFridgeId,
            naziv = $"{token}-other",
            kategorija = KategorijaNamirnice.Voce,
            mjera = Mjera.Komad,
            kolicinaUFrizideru = 3,
            isDeleted = false,
            kreirano = baseDate.AddDays(-3),
            nutritivnaVrijednost = new NutritivnaVrijednost { kalorije = 40, proteini = 1, masti = 0.2, ugljikohidrati = 9, vlakna = 2, sol = 0.1 }
        };

        db.Namirnice.AddRange(ownerVisible, ownerDeleted, otherUserItem);
        await db.SaveChangesAsync();

        return new QuerySeedResult(
            seed.OwnerUserId,
            seed.OwnerFridgeId,
            token,
            ownerVisible.id,
            ownerDeleted.id,
            otherUserItem.id,
            baseDate.AddDays(-10),
            baseDate.AddDays(1));
    }

    private sealed record OwnerContextSeed(int OwnerUserId, int OtherUserId, int OwnerFridgeId, int OtherFridgeId);

    private sealed record OwnerItemSeed(int OwnerUserId, int OtherUserId, int OwnerFridgeId, int ItemId);

    private sealed record QuerySeedResult(
        int OwnerUserId,
        int OwnerFridgeId,
        string SearchToken,
        int OwnerVisibleItemId,
        int OwnerDeletedItemId,
        int OtherUserItemId,
        DateTime FromDate,
        DateTime ToDate);
}
