using System.Net;
using System.Net.Http.Json;
using KitchenAidAI.Data;
using KitchenAidAI.IntegrationTests.Infrastructure;
using KitchenAidAI.Models;
using KitchenAidAI.Models.DTOs;
using KitchenAidAI.Models.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace KitchenAidAI.IntegrationTests;

public class ReceptiApiTests : IClassFixture<KitchenAidApiFactory>
{
    private readonly KitchenAidApiFactory _factory;

    public ReceptiApiTests(KitchenAidApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/recepti");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithNonAdminWithoutCookbook_ReturnsBadRequest()
    {
        var ownerUserId = await SeedUserWithoutCookbookAsync();
        using var client = CreateAuthorizedClient(ownerUserId, "User");
        var payload = BuildValidRecipePayload("no-cookbook");

        var response = await client.PostAsJsonAsync("/api/recepti", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(body);
        Assert.False(body!.success);
        Assert.NotNull(body.alert);
        Assert.Equal("COOKBOOK_NOT_FOUND", body.alert!.code);
    }

    [Fact]
    public async Task Create_WithOwnerCookbook_ReturnsCreated_AndPersistsJoin()
    {
        var seed = await SeedOwnerContextAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");
        var payload = BuildValidRecipePayload("owner-create");

        var response = await client.PostAsJsonAsync("/api/recepti", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ReceptDto>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.True(body.data!.id > 0);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KitchenAidDbContext>();
        var joinExists = db.ReceptKuharice.Any(join => join.receptId == body.data.id && join.kuharicaId == seed.OwnerCookbookId && !join.isDeleted);
        Assert.True(joinExists);
    }

    [Fact]
    public async Task Create_WithAdmin_ReturnsCreated_AndValidBody()
    {
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");
        var payload = BuildValidRecipePayload("admin-create");

        var response = await client.PostAsJsonAsync("/api/recepti", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ReceptDto>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.True(body.data!.id > 0);
        Assert.False(body.data.isDeleted);
    }

    [Fact]
    public async Task Create_WithInvalidPayload_ReturnsBadRequest()
    {
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");
        var invalidPayload = new
        {
            opis = "missing naziv",
            vrijemeKuhanja = 12,
            tezina = TezinaRecepta.Lako,
            brojPorcija = 2
        };

        var response = await client.PostAsJsonAsync("/api/recepti", invalidPayload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithAdminAndNonExistingId_ReturnsNotFound()
    {
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.GetAsync("/api/recepti/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithOwnerUser_ReturnsOk()
    {
        var seed = await SeedOwnerRecipeAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");

        var response = await client.GetAsync($"/api/recepti/{seed.RecipeId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ReceptPublicDto>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Equal(seed.RecipeId, body.data!.id);
    }

    [Fact]
    public async Task GetById_WithNonOwnerUser_ReturnsNotFound()
    {
        var seed = await SeedOwnerRecipeAsync();
        using var client = CreateAuthorizedClient(seed.OtherUserId, "User");

        var response = await client.GetAsync($"/api/recepti/{seed.RecipeId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithOwnerUser_ReturnsOk_AndUpdatedBody()
    {
        var seed = await SeedOwnerRecipeAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");
        var payload = BuildValidRecipePayload("owner-update", porcije: 8, vrijeme: 45, tezina: TezinaRecepta.Tesko);

        var response = await client.PutAsJsonAsync($"/api/recepti/{seed.RecipeId}", payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ReceptPublicDto>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Equal(8, body.data!.brojPorcija);
        Assert.Equal(45, body.data.vrijemeKuhanja);
    }

    [Fact]
    public async Task Update_WithNonOwnerUser_ReturnsNotFound()
    {
        var seed = await SeedOwnerRecipeAsync();
        using var client = CreateAuthorizedClient(seed.OtherUserId, "User");
        var payload = BuildValidRecipePayload("non-owner-update");

        var response = await client.PutAsJsonAsync($"/api/recepti/{seed.RecipeId}", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithNonExistingId_ReturnsNotFound()
    {
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");
        var payload = BuildValidRecipePayload("missing-update");

        var response = await client.PutAsJsonAsync("/api/recepti/999999", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithOwnerUser_ReturnsOk_AndCascadesSoftDeleteToStepsAndJoins()
    {
        var seed = await SeedOwnerRecipeWithStepAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");

        var response = await client.DeleteAsync($"/api/recepti/{seed.RecipeId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ReceptDto>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.True(body.data!.isDeleted);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KitchenAidDbContext>();
        var step = db.KoraciRecepta.First(current => current.id == seed.StepId);
        var join = db.ReceptKuharice.First(current => current.id == seed.JoinId);
        Assert.True(step.isDeleted);
        Assert.True(join.isDeleted);
    }

    [Fact]
    public async Task Delete_WithNonOwnerUser_ReturnsNotFound()
    {
        var seed = await SeedOwnerRecipeAsync();
        using var client = CreateAuthorizedClient(seed.OtherUserId, "User");

        var response = await client.DeleteAsync($"/api/recepti/{seed.RecipeId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithNonExistingId_ReturnsNotFound()
    {
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.DeleteAsync("/api/recepti/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Restore_WithNonAdminUser_ReturnsForbidden()
    {
        var ownerUserId = await SeedUserWithoutCookbookAsync();
        using var client = CreateAuthorizedClient(ownerUserId, "User");

        var response = await client.PostAsync("/api/recepti/1/restore", content: null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Restore_WithAdminOnDeletedRecipe_ReturnsOk()
    {
        var deletedRecipeId = await SeedDeletedRecipeAsync();
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.PostAsync($"/api/recepti/{deletedRecipeId}/restore", content: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ReceptDto>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.False(body.data!.isDeleted);
    }

    [Fact]
    public async Task Restore_WithAdminOnNonExistingRecipe_ReturnsNotFound()
    {
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.PostAsync("/api/recepti/999999/restore", content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_WithAdminIncludeDeletedFalse_ExcludesDeletedRecipes()
    {
        var queryData = await SeedRecipeQueryDataAsync();
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.GetAsync($"/api/recepti?search={queryData.SearchToken}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<ReceptDto>>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Contains(body.data!, recipe => recipe.id == queryData.ActiveRecipeId);
        Assert.DoesNotContain(body.data!, recipe => recipe.id == queryData.DeletedRecipeId);
    }

    [Fact]
    public async Task GetAll_WithAdminIncludeDeletedTrue_IncludesDeletedRecipes()
    {
        var queryData = await SeedRecipeQueryDataAsync();
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.GetAsync($"/api/recepti?search={queryData.SearchToken}&includeDeleted=true");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<ReceptDto>>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Contains(body.data!, recipe => recipe.id == queryData.ActiveRecipeId);
        Assert.Contains(body.data!, recipe => recipe.id == queryData.DeletedRecipeId);
    }

    [Fact]
    public async Task GetAll_WithAdminSortAndRangeFilters_ReturnsExpectedSubsetInOrder()
    {
        var queryData = await SeedRecipeQueryDataAsync();
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.GetAsync(
            $"/api/recepti?search={queryData.SearchToken}&includeDeleted=true&minPorcija=2&maxPorcija=6&minVrijeme=15&maxVrijeme=60&odDatuma={queryData.FromDate:yyyy-MM-dd}&doDatuma={queryData.ToDate:yyyy-MM-dd}&sortBy=kreirano&sortDir=desc");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<ReceptDto>>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.True(body.data!.Count >= 2);

        for (var i = 0; i < body.data.Count - 1; i++)
        {
            Assert.True(body.data[i].id != 0);
        }

        Assert.Contains(body.data, recipe => recipe.id == queryData.ActiveRecipeId);
        Assert.Contains(body.data, recipe => recipe.id == queryData.DeletedRecipeId);
    }

    [Fact]
    public async Task GetAll_WithOwnerUser_ReturnsOnlyOwnedVisibleRecipes()
    {
        var queryData = await SeedRecipeQueryDataAsync();
        using var client = CreateAuthorizedClient(queryData.OwnerUserId, "User");

        var response = await client.GetAsync("/api/recepti");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<ReceptPublicDto>>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Contains(body.data!, recipe => recipe.id == queryData.ActiveRecipeId);
        Assert.DoesNotContain(body.data!, recipe => recipe.id == queryData.DeletedRecipeId);
        Assert.DoesNotContain(body.data!, recipe => recipe.id == queryData.OtherUserRecipeId);
    }

    private HttpClient CreateAuthorizedClient(int userId, string role)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
        return client;
    }

    private static object BuildValidRecipePayload(string suffix, int porcije = 4, double vrijeme = 30, TezinaRecepta tezina = TezinaRecepta.Srednje)
    {
        return new
        {
            naziv = $"Recept-{suffix}-{Guid.NewGuid():N}",
            opis = "Test recept",
            vrijemeKuhanja = vrijeme,
            tezina,
            brojPorcija = porcije
        };
    }

    private async Task<int> SeedUserWithoutCookbookAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KitchenAidDbContext>();

        var user = new User
        {
            username = $"user-no-cookbook-{Guid.NewGuid():N}",
            email = $"user-no-cookbook-{Guid.NewGuid():N}@example.com",
            preferencijaPrehrane = PreferencijaPrehrane.Omnivorte,
            isAdmin = false,
            isDeleted = false
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return user.id;
    }

    private async Task<OwnerContextSeed> SeedOwnerContextAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KitchenAidDbContext>();

        var owner = new User
        {
            username = $"owner-{Guid.NewGuid():N}",
            email = $"owner-{Guid.NewGuid():N}@example.com",
            preferencijaPrehrane = PreferencijaPrehrane.Omnivorte,
            isAdmin = false,
            isDeleted = false
        };

        var other = new User
        {
            username = $"other-{Guid.NewGuid():N}",
            email = $"other-{Guid.NewGuid():N}@example.com",
            preferencijaPrehrane = PreferencijaPrehrane.Omnivorte,
            isAdmin = false,
            isDeleted = false
        };

        db.Users.AddRange(owner, other);
        await db.SaveChangesAsync();

        var ownerCookbook = new Kuharica
        {
            naziv = $"owner-cookbook-{Guid.NewGuid():N}",
            userId = owner.id,
            isDeleted = false
        };

        var otherCookbook = new Kuharica
        {
            naziv = $"other-cookbook-{Guid.NewGuid():N}",
            userId = other.id,
            isDeleted = false
        };

        db.Kuharice.AddRange(ownerCookbook, otherCookbook);
        await db.SaveChangesAsync();

        return new OwnerContextSeed(owner.id, other.id, ownerCookbook.id, otherCookbook.id);
    }

    private async Task<OwnerRecipeSeed> SeedOwnerRecipeAsync()
    {
        var context = await SeedOwnerContextAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KitchenAidDbContext>();

        var recipe = new Recept
        {
            naziv = $"owner-recipe-{Guid.NewGuid():N}",
            opis = "Owner recipe",
            vrijemeKuhanja = 25,
            brojPorcija = 3,
            tezina = TezinaRecepta.Lako,
            isDeleted = false
        };

        db.Recepti.Add(recipe);
        await db.SaveChangesAsync();

        var join = new ReceptKuharica
        {
            receptId = recipe.id,
            kuharicaId = context.OwnerCookbookId,
            isDeleted = false
        };

        db.ReceptKuharice.Add(join);
        await db.SaveChangesAsync();

        return new OwnerRecipeSeed(context.OwnerUserId, context.OtherUserId, context.OwnerCookbookId, recipe.id, join.id);
    }

    private async Task<OwnerRecipeWithStepSeed> SeedOwnerRecipeWithStepAsync()
    {
        var baseSeed = await SeedOwnerRecipeAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KitchenAidDbContext>();

        var step = new KorakRecepta
        {
            receptId = baseSeed.RecipeId,
            redniBroj = 1,
            opis = "Prvi korak",
            trajanje = 5,
            isDeleted = false
        };

        db.KoraciRecepta.Add(step);
        await db.SaveChangesAsync();

        return new OwnerRecipeWithStepSeed(baseSeed.OwnerUserId, baseSeed.OtherUserId, baseSeed.RecipeId, baseSeed.JoinId, step.id);
    }

    private async Task<int> SeedDeletedRecipeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KitchenAidDbContext>();

        var recipe = new Recept
        {
            naziv = $"deleted-recipe-{Guid.NewGuid():N}",
            opis = "Deleted recipe",
            vrijemeKuhanja = 20,
            brojPorcija = 2,
            tezina = TezinaRecepta.Srednje,
            isDeleted = true
        };

        db.Recepti.Add(recipe);
        await db.SaveChangesAsync();

        return recipe.id;
    }

    private async Task<RecipeQuerySeed> SeedRecipeQueryDataAsync()
    {
        var context = await SeedOwnerContextAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KitchenAidDbContext>();

        var token = $"query-{Guid.NewGuid():N}";
        var baseDate = DateTime.UtcNow.Date;

        var ownerActiveRecipe = new Recept
        {
            naziv = $"{token}-active",
            opis = token,
            vrijemeKuhanja = 20,
            brojPorcija = 4,
            tezina = TezinaRecepta.Srednje,
            kreirano = baseDate.AddDays(-1),
            isDeleted = false
        };

        var ownerDeletedRecipe = new Recept
        {
            naziv = $"{token}-deleted",
            opis = token,
            vrijemeKuhanja = 35,
            brojPorcija = 5,
            tezina = TezinaRecepta.Tesko,
            kreirano = baseDate.AddDays(-2),
            isDeleted = true
        };

        var otherUserRecipe = new Recept
        {
            naziv = $"{token}-other",
            opis = token,
            vrijemeKuhanja = 15,
            brojPorcija = 2,
            tezina = TezinaRecepta.Lako,
            kreirano = baseDate.AddDays(-3),
            isDeleted = false
        };

        db.Recepti.AddRange(ownerActiveRecipe, ownerDeletedRecipe, otherUserRecipe);
        await db.SaveChangesAsync();

        db.ReceptKuharice.AddRange(
            new ReceptKuharica
            {
                receptId = ownerActiveRecipe.id,
                kuharicaId = context.OwnerCookbookId,
                isDeleted = false
            },
            new ReceptKuharica
            {
                receptId = ownerDeletedRecipe.id,
                kuharicaId = context.OwnerCookbookId,
                isDeleted = false
            },
            new ReceptKuharica
            {
                receptId = otherUserRecipe.id,
                kuharicaId = context.OtherCookbookId,
                isDeleted = false
            });

        await db.SaveChangesAsync();

        return new RecipeQuerySeed(
            context.OwnerUserId,
            token,
            ownerActiveRecipe.id,
            ownerDeletedRecipe.id,
            otherUserRecipe.id,
            baseDate.AddDays(-10),
            baseDate.AddDays(1));
    }

    private sealed record OwnerContextSeed(int OwnerUserId, int OtherUserId, int OwnerCookbookId, int OtherCookbookId);

    private sealed record OwnerRecipeSeed(int OwnerUserId, int OtherUserId, int OwnerCookbookId, int RecipeId, int JoinId);

    private sealed record OwnerRecipeWithStepSeed(int OwnerUserId, int OtherUserId, int RecipeId, int JoinId, int StepId);

    private sealed record RecipeQuerySeed(int OwnerUserId, string SearchToken, int ActiveRecipeId, int DeletedRecipeId, int OtherUserRecipeId, DateTime FromDate, DateTime ToDate);
}
