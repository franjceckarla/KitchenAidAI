using System.Net;
using System.Net.Http.Json;
using KitchenAidAI.Data;
using KitchenAidAI.IntegrationTests.Infrastructure;
using KitchenAidAI.Models;
using KitchenAidAI.Models.DTOs;
using KitchenAidAI.Models.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace KitchenAidAI.IntegrationTests;

public class KoraciReceptaApiTests : IClassFixture<KitchenAidApiFactory>
{
    private readonly KitchenAidApiFactory _factory;

    public KoraciReceptaApiTests(KitchenAidApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/koraci");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithOwnerUser_ReturnsCreated_AndValidBody()
    {
        var seed = await SeedOwnerRecipeAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");
        var payload = BuildValidStepPayload(seed.RecipeId, redniBroj: 1, opis: "Owner create", trajanje: 12);

        var response = await client.PostAsJsonAsync("/api/koraci", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<KorakReceptaDto>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.True(body.data!.id > 0);
        Assert.Equal(seed.RecipeId, body.data.receptId);
        Assert.False(body.data.isDeleted);
    }

    [Fact]
    public async Task Create_WithNonOwnerNonAdmin_ReturnsForbidden_AndErrorBody()
    {
        var seed = await SeedOwnerRecipeAsync();
        using var client = CreateAuthorizedClient(seed.OtherUserId, "User");
        var payload = BuildValidStepPayload(seed.RecipeId, redniBroj: 2, opis: "Forbidden", trajanje: 7);

        var response = await client.PostAsJsonAsync("/api/koraci", payload);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(body);
        Assert.False(body!.success);
        Assert.NotNull(body.alert);
        Assert.Equal("FORBIDDEN", body.alert!.code);
    }

    [Fact]
    public async Task Create_WithMissingRecipe_ReturnsNotFound()
    {
        var seed = await SeedOwnerRecipeAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");
        var payload = BuildValidStepPayload(999999, redniBroj: 3, opis: "Missing recipe", trajanje: 5);

        var response = await client.PostAsJsonAsync("/api/koraci", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(body);
        Assert.False(body!.success);
        Assert.NotNull(body.alert);
        Assert.Equal("NOT_FOUND", body.alert!.code);
    }

    [Fact]
    public async Task Create_WithMalformedJson_ReturnsBadRequest()
    {
        var seed = await SeedOwnerRecipeAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");
        using var content = new StringContent("{ invalid-json", System.Text.Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/koraci", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithOwnerUser_ReturnsOk_AndPublicBody()
    {
        var seed = await SeedStepAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");

        var response = await client.GetAsync($"/api/koraci/{seed.StepId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<KorakReceptaPublicDto>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Equal(seed.StepId, body.data!.id);
        Assert.Equal(seed.RecipeId, body.data.receptId);
    }

    [Fact]
    public async Task GetById_WithNonOwnerUser_ReturnsForbidden()
    {
        var seed = await SeedStepAsync();
        using var client = CreateAuthorizedClient(seed.OtherUserId, "User");

        var response = await client.GetAsync($"/api/koraci/{seed.StepId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(body);
        Assert.False(body!.success);
        Assert.NotNull(body.alert);
        Assert.Equal("FORBIDDEN", body.alert!.code);
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ReturnsNotFound()
    {
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.GetAsync("/api/koraci/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithOwnerUser_ReturnsOk_AndUpdatedBody()
    {
        var seed = await SeedStepAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");
        var payload = BuildValidStepPayload(seed.RecipeId, redniBroj: 9, opis: "Updated owner step", trajanje: 42);

        var response = await client.PutAsJsonAsync($"/api/koraci/{seed.StepId}", payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<KorakReceptaDto>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Equal(9, body.data!.redniBroj);
        Assert.Equal("Updated owner step", body.data.opis);
        Assert.Equal(42, body.data.trajanje);
    }

    [Fact]
    public async Task Update_WithNonOwnerUser_ReturnsForbidden()
    {
        var seed = await SeedStepAsync();
        using var client = CreateAuthorizedClient(seed.OtherUserId, "User");
        var payload = BuildValidStepPayload(seed.RecipeId, redniBroj: 7, opis: "No access", trajanje: 13);

        var response = await client.PutAsJsonAsync($"/api/koraci/{seed.StepId}", payload);

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
        var seed = await SeedOwnerRecipeAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");
        var payload = BuildValidStepPayload(seed.RecipeId, redniBroj: 1, opis: "Missing", trajanje: 11);

        var response = await client.PutAsJsonAsync("/api/koraci/999999", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithOwnerUser_ReturnsOk_AndSoftDeleteHidesFromOwnerGetById()
    {
        var seed = await SeedStepAsync();
        using var ownerClient = CreateAuthorizedClient(seed.OwnerUserId, "User");
        using var adminClient = CreateAuthorizedClient(userId: 1, role: "Admin");

        var deleteResponse = await ownerClient.DeleteAsync($"/api/koraci/{seed.StepId}");

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        var deleteBody = await deleteResponse.Content.ReadFromJsonAsync<ApiResponse<KorakReceptaDto>>();
        Assert.NotNull(deleteBody);
        Assert.True(deleteBody!.success);
        Assert.NotNull(deleteBody.data);
        Assert.True(deleteBody.data!.isDeleted);

        var ownerGetAfterDelete = await ownerClient.GetAsync($"/api/koraci/{seed.StepId}");
        Assert.Equal(HttpStatusCode.NotFound, ownerGetAfterDelete.StatusCode);

        var adminGetAfterDelete = await adminClient.GetAsync($"/api/koraci/{seed.StepId}");
        Assert.Equal(HttpStatusCode.OK, adminGetAfterDelete.StatusCode);
        var adminBody = await adminGetAfterDelete.Content.ReadFromJsonAsync<ApiResponse<KorakReceptaDto>>();
        Assert.NotNull(adminBody);
        Assert.True(adminBody!.success);
        Assert.NotNull(adminBody.data);
        Assert.True(adminBody.data!.isDeleted);
    }

    [Fact]
    public async Task Delete_WithNonOwnerUser_ReturnsForbidden()
    {
        var seed = await SeedStepAsync();
        using var client = CreateAuthorizedClient(seed.OtherUserId, "User");

        var response = await client.DeleteAsync($"/api/koraci/{seed.StepId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(body);
        Assert.False(body!.success);
        Assert.NotNull(body.alert);
        Assert.Equal("FORBIDDEN", body.alert!.code);
    }

    [Fact]
    public async Task Delete_WithNonExistingId_ReturnsNotFound()
    {
        var seed = await SeedOwnerRecipeAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");

        var response = await client.DeleteAsync("/api/koraci/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_WithOwnerUser_ReturnsOnlyOwnedAndVisibleSteps()
    {
        var seed = await SeedQueryDataAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");

        var response = await client.GetAsync("/api/koraci");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<KorakReceptaPublicDto>>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);

        Assert.Contains(body.data!, step => step.id == seed.OwnerVisibleOldStepId);
        Assert.Contains(body.data!, step => step.id == seed.OwnerVisibleNewStepId);
        Assert.DoesNotContain(body.data!, step => step.id == seed.OwnerSoftDeletedStepId);
        Assert.DoesNotContain(body.data!, step => step.id == seed.OtherUserStepId);
        Assert.DoesNotContain(body.data!, step => step.id == seed.OwnerStepOnDeletedRecipeId);
    }

    [Fact]
    public async Task GetAll_WithAdminIncludeDeletedFalse_ExcludesDeletedStep()
    {
        var seed = await SeedQueryDataAsync();
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.GetAsync($"/api/koraci?receptId={seed.OwnerVisibleRecipeId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<KorakReceptaDto>>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);

        Assert.DoesNotContain(body.data!, step => step.id == seed.OwnerSoftDeletedStepId);
    }

    [Fact]
    public async Task GetAll_WithAdminIncludeDeletedTrue_IncludesDeletedStep()
    {
        var seed = await SeedQueryDataAsync();
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.GetAsync($"/api/koraci?receptId={seed.OwnerVisibleRecipeId}&includeDeleted=true");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<KorakReceptaDto>>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Contains(body.data!, step => step.id == seed.OwnerSoftDeletedStepId && step.isDeleted);
    }

    [Fact]
    public async Task GetAll_WithAdminFiltersAndSortDesc_ReturnsExpectedSubsetAndOrder()
    {
        var seed = await SeedQueryDataAsync();
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.GetAsync(
            $"/api/koraci?receptId={seed.OwnerVisibleRecipeId}&includeDeleted=true&search={seed.SearchToken}&minRedniBroj=2&maxRedniBroj=9&minTrajanje=10&maxTrajanje=40&odDatuma={seed.FromDate:yyyy-MM-dd}&doDatuma={seed.ToDate:yyyy-MM-dd}&sortBy=redniBroj&sortDir=desc");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<KorakReceptaDto>>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.True(body.data!.Count >= 2);

        for (var i = 0; i < body.data.Count - 1; i++)
        {
            Assert.True(body.data[i].redniBroj >= body.data[i + 1].redniBroj);
        }

        Assert.Contains(body.data, step => step.id == seed.OwnerVisibleNewStepId);
        Assert.Contains(body.data, step => step.id == seed.OwnerSoftDeletedStepId);
        Assert.DoesNotContain(body.data, step => step.id == seed.OwnerVisibleOldStepId);
    }

    private HttpClient CreateAuthorizedClient(int userId, string role)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
        return client;
    }

    private static object BuildValidStepPayload(int receptId, int redniBroj, string opis, double trajanje)
    {
        return new
        {
            receptId,
            redniBroj,
            opis,
            trajanje
        };
    }

    private async Task<SeedResult> SeedOwnerRecipeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KitchenAidDbContext>();

        var ownerUser = new User
        {
            username = $"owner-{Guid.NewGuid():N}",
            email = $"owner-{Guid.NewGuid():N}@example.com",
            preferencijaPrehrane = PreferencijaPrehrane.Omnivorte,
            isAdmin = false,
            isDeleted = false
        };

        var otherUser = new User
        {
            username = $"other-{Guid.NewGuid():N}",
            email = $"other-{Guid.NewGuid():N}@example.com",
            preferencijaPrehrane = PreferencijaPrehrane.Omnivorte,
            isAdmin = false,
            isDeleted = false
        };

        db.Users.AddRange(ownerUser, otherUser);
        await db.SaveChangesAsync();

        var cookbook = new Kuharica
        {
            naziv = $"Kuharica-{Guid.NewGuid():N}",
            userId = ownerUser.id,
            isDeleted = false
        };

        var recipe = new Recept
        {
            naziv = $"Recept-{Guid.NewGuid():N}",
            opis = "Test recipe",
            brojPorcija = 2,
            vrijemeKuhanja = 20,
            tezina = TezinaRecepta.Srednje,
            isDeleted = false
        };

        db.Kuharice.Add(cookbook);
        db.Recepti.Add(recipe);
        await db.SaveChangesAsync();

        var join = new ReceptKuharica
        {
            kuharicaId = cookbook.id,
            receptId = recipe.id,
            isDeleted = false
        };

        db.ReceptKuharice.Add(join);
        await db.SaveChangesAsync();

        return new SeedResult(ownerUser.id, otherUser.id, cookbook.id, recipe.id, 0, join.id);
    }

    private async Task<SeedResult> SeedStepAsync()
    {
        var baseData = await SeedOwnerRecipeAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KitchenAidDbContext>();

        var step = new KorakRecepta
        {
            receptId = baseData.RecipeId,
            redniBroj = 1,
            opis = "Initial step",
            trajanje = 10,
            isDeleted = false
        };

        db.KoraciRecepta.Add(step);
        await db.SaveChangesAsync();

        return baseData with { StepId = step.id };
    }

    private async Task<QuerySeedResult> SeedQueryDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KitchenAidDbContext>();

        var ownerUser = new User
        {
            username = $"owner-query-{Guid.NewGuid():N}",
            email = $"owner-query-{Guid.NewGuid():N}@example.com",
            preferencijaPrehrane = PreferencijaPrehrane.Omnivorte,
            isAdmin = false,
            isDeleted = false
        };

        var otherUser = new User
        {
            username = $"other-query-{Guid.NewGuid():N}",
            email = $"other-query-{Guid.NewGuid():N}@example.com",
            preferencijaPrehrane = PreferencijaPrehrane.Omnivorte,
            isAdmin = false,
            isDeleted = false
        };

        db.Users.AddRange(ownerUser, otherUser);
        await db.SaveChangesAsync();

        var ownerCookbook = new Kuharica
        {
            naziv = $"Owner-Cookbook-{Guid.NewGuid():N}",
            userId = ownerUser.id,
            isDeleted = false
        };

        var otherCookbook = new Kuharica
        {
            naziv = $"Other-Cookbook-{Guid.NewGuid():N}",
            userId = otherUser.id,
            isDeleted = false
        };

        var ownerVisibleRecipe = new Recept
        {
            naziv = $"Owner-Visible-{Guid.NewGuid():N}",
            opis = "Owner visible recipe",
            brojPorcija = 2,
            vrijemeKuhanja = 25,
            tezina = TezinaRecepta.Srednje,
            isDeleted = false
        };

        var ownerDeletedRecipe = new Recept
        {
            naziv = $"Owner-Deleted-{Guid.NewGuid():N}",
            opis = "Owner deleted recipe",
            brojPorcija = 3,
            vrijemeKuhanja = 35,
            tezina = TezinaRecepta.Tesko,
            isDeleted = true
        };

        var otherRecipe = new Recept
        {
            naziv = $"Other-Recipe-{Guid.NewGuid():N}",
            opis = "Other user recipe",
            brojPorcija = 4,
            vrijemeKuhanja = 30,
            tezina = TezinaRecepta.Lako,
            isDeleted = false
        };

        db.Kuharice.AddRange(ownerCookbook, otherCookbook);
        db.Recepti.AddRange(ownerVisibleRecipe, ownerDeletedRecipe, otherRecipe);
        await db.SaveChangesAsync();

        var ownerVisibleJoin = new ReceptKuharica
        {
            kuharicaId = ownerCookbook.id,
            receptId = ownerVisibleRecipe.id,
            isDeleted = false
        };

        var ownerDeletedRecipeJoin = new ReceptKuharica
        {
            kuharicaId = ownerCookbook.id,
            receptId = ownerDeletedRecipe.id,
            isDeleted = false
        };

        var otherJoin = new ReceptKuharica
        {
            kuharicaId = otherCookbook.id,
            receptId = otherRecipe.id,
            isDeleted = false
        };

        db.ReceptKuharice.AddRange(ownerVisibleJoin, ownerDeletedRecipeJoin, otherJoin);
        await db.SaveChangesAsync();

        var searchToken = $"token-{Guid.NewGuid():N}";

        var oldStep = new KorakRecepta
        {
            receptId = ownerVisibleRecipe.id,
            redniBroj = 1,
            opis = $"{searchToken}-old",
            trajanje = 5,
            kreirano = new DateTime(2026, 1, 1),
            isDeleted = false
        };

        var newStep = new KorakRecepta
        {
            receptId = ownerVisibleRecipe.id,
            redniBroj = 8,
            opis = $"{searchToken}-new",
            trajanje = 30,
            kreirano = new DateTime(2026, 1, 3),
            isDeleted = false
        };

        var softDeletedStep = new KorakRecepta
        {
            receptId = ownerVisibleRecipe.id,
            redniBroj = 4,
            opis = $"{searchToken}-deleted",
            trajanje = 15,
            kreirano = new DateTime(2026, 1, 2),
            isDeleted = true
        };

        var stepOnDeletedRecipe = new KorakRecepta
        {
            receptId = ownerDeletedRecipe.id,
            redniBroj = 6,
            opis = $"{searchToken}-recipe-deleted",
            trajanje = 22,
            kreirano = new DateTime(2026, 1, 2),
            isDeleted = false
        };

        var otherUserStep = new KorakRecepta
        {
            receptId = otherRecipe.id,
            redniBroj = 5,
            opis = $"{searchToken}-other",
            trajanje = 19,
            kreirano = new DateTime(2026, 1, 2),
            isDeleted = false
        };

        db.KoraciRecepta.AddRange(oldStep, newStep, softDeletedStep, stepOnDeletedRecipe, otherUserStep);
        await db.SaveChangesAsync();

        return new QuerySeedResult(
            OwnerUserId: ownerUser.id,
            OtherUserId: otherUser.id,
            OwnerVisibleRecipeId: ownerVisibleRecipe.id,
            OwnerVisibleOldStepId: oldStep.id,
            OwnerVisibleNewStepId: newStep.id,
            OwnerSoftDeletedStepId: softDeletedStep.id,
            OwnerStepOnDeletedRecipeId: stepOnDeletedRecipe.id,
            OtherUserStepId: otherUserStep.id,
            SearchToken: searchToken,
            FromDate: new DateTime(2026, 1, 2),
            ToDate: new DateTime(2026, 1, 3));
    }

    private sealed record SeedResult(
        int OwnerUserId,
        int OtherUserId,
        int CookbookId,
        int RecipeId,
        int StepId,
        int JoinId);

    private sealed record QuerySeedResult(
        int OwnerUserId,
        int OtherUserId,
        int OwnerVisibleRecipeId,
        int OwnerVisibleOldStepId,
        int OwnerVisibleNewStepId,
        int OwnerSoftDeletedStepId,
        int OwnerStepOnDeletedRecipeId,
        int OtherUserStepId,
        string SearchToken,
        DateTime FromDate,
        DateTime ToDate);
}
