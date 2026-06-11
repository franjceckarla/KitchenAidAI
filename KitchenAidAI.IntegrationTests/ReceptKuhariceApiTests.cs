using System.Net;
using System.Net.Http.Json;
using KitchenAidAI.Data;
using KitchenAidAI.IntegrationTests.Infrastructure;
using KitchenAidAI.Models;
using KitchenAidAI.Models.DTOs;
using KitchenAidAI.Models.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace KitchenAidAI.IntegrationTests;

public class ReceptKuhariceApiTests : IClassFixture<KitchenAidApiFactory>
{
    private readonly KitchenAidApiFactory _factory;

    public ReceptKuhariceApiTests(KitchenAidApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/recept-kuharice");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithOwnerUser_ReturnsCreated_AndValidBody()
    {
        var data = await SeedOwnerAndRecipeAsync();
        using var client = CreateAuthorizedClient(data.OwnerUserId, "User");
        var payload = new { receptId = data.RecipeId, kuharicaId = data.OwnerCookbookId };

        var response = await client.PostAsJsonAsync("/api/recept-kuharice", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ReceptKuharicaDto>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Equal(data.RecipeId, body.data!.receptId);
        Assert.Equal(data.OwnerCookbookId, body.data.kuharicaId);
    }

    [Fact]
    public async Task Create_WithNonOwnerNonAdmin_ReturnsForbidden_AndErrorBody()
    {
        var data = await SeedOwnerAndRecipeAsync();
        using var client = CreateAuthorizedClient(data.OtherUserId, "User");
        var payload = new { receptId = data.RecipeId, kuharicaId = data.OwnerCookbookId };

        var response = await client.PostAsJsonAsync("/api/recept-kuharice", payload);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(body);
        Assert.False(body!.success);
        Assert.NotNull(body.alert);
    }

    [Fact]
    public async Task Create_WithMissingCookbook_ReturnsNotFound()
    {
        var data = await SeedOwnerAndRecipeAsync();
        using var client = CreateAuthorizedClient(data.OwnerUserId, "User");
        var payload = new { receptId = data.RecipeId, kuharicaId = 999999 };

        var response = await client.PostAsJsonAsync("/api/recept-kuharice", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(body);
        Assert.False(body!.success);
        Assert.NotNull(body.alert);
    }

    [Fact]
    public async Task Create_WithMissingRecipe_ReturnsNotFound()
    {
        var data = await SeedOwnerAndRecipeAsync();
        using var client = CreateAuthorizedClient(data.OwnerUserId, "User");
        var payload = new { receptId = 999999, kuharicaId = data.OwnerCookbookId };

        var response = await client.PostAsJsonAsync("/api/recept-kuharice", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(body);
        Assert.False(body!.success);
        Assert.NotNull(body.alert);
    }

    [Fact]
    public async Task Create_WithDuplicatePayload_ReturnsBadRequest_AndCode()
    {
        var data = await SeedOwnerAndRecipeAsync();
        using var client = CreateAuthorizedClient(data.OwnerUserId, "User");
        var payload = new { receptId = data.RecipeId, kuharicaId = data.OwnerCookbookId };

        var first = await client.PostAsJsonAsync("/api/recept-kuharice", payload);
        first.EnsureSuccessStatusCode();

        var duplicate = await client.PostAsJsonAsync("/api/recept-kuharice", payload);

        Assert.Equal(HttpStatusCode.BadRequest, duplicate.StatusCode);
        var body = await duplicate.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(body);
        Assert.False(body!.success);
        Assert.NotNull(body.alert);
        Assert.Equal("RECIPE_EXISTS", body.alert!.code);
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ReturnsNotFound()
    {
        var data = await SeedOwnerAndRecipeAsync();
        using var client = CreateAuthorizedClient(data.OwnerUserId, "User");

        var response = await client.GetAsync("/api/recept-kuharice/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(body);
        Assert.False(body!.success);
        Assert.NotNull(body.alert);
    }

    [Fact]
    public async Task GetById_WithNonOwnerUser_ReturnsForbidden()
    {
        var data = await SeedJoinAsync();
        using var client = CreateAuthorizedClient(data.OtherUserId, "User");

        var response = await client.GetAsync($"/api/recept-kuharice/{data.JoinId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(body);
        Assert.False(body!.success);
        Assert.NotNull(body.alert);
    }

    [Fact]
    public async Task GetById_WithOwnerUser_ReturnsOk_AndPublicBody()
    {
        var data = await SeedJoinAsync();
        using var client = CreateAuthorizedClient(data.OwnerUserId, "User");

        var response = await client.GetAsync($"/api/recept-kuharice/{data.JoinId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ReceptKuharicaPublicDto>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Equal(data.JoinId, body.data!.id);
    }

    [Fact]
    public async Task Update_WithOwnerUser_ReturnsOk_AndUpdatesIsDeleted()
    {
        var data = await SeedJoinAsync();
        using var client = CreateAuthorizedClient(data.OwnerUserId, "User");
        var payload = new { isDeleted = true };

        var response = await client.PutAsJsonAsync($"/api/recept-kuharice/{data.JoinId}", payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ReceptKuharicaDto>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.True(body.data!.isDeleted);
    }

    [Fact]
    public async Task Update_WithNonOwnerUser_ReturnsForbidden()
    {
        var data = await SeedJoinAsync();
        using var client = CreateAuthorizedClient(data.OtherUserId, "User");
        var payload = new { isDeleted = true };

        var response = await client.PutAsJsonAsync($"/api/recept-kuharice/{data.JoinId}", payload);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(body);
        Assert.False(body!.success);
        Assert.NotNull(body.alert);
    }

    [Fact]
    public async Task Update_WithNonExistingId_ReturnsNotFound()
    {
        var data = await SeedOwnerAndRecipeAsync();
        using var client = CreateAuthorizedClient(data.OwnerUserId, "User");
        var payload = new { isDeleted = true };

        var response = await client.PutAsJsonAsync("/api/recept-kuharice/999999", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(body);
        Assert.False(body!.success);
        Assert.NotNull(body.alert);
    }

    [Fact]
    public async Task Delete_WithOwnerUser_ReturnsOk_AndSoftDeleteHidesFromOwnerGetById()
    {
        var data = await SeedJoinAsync();
        using var ownerClient = CreateAuthorizedClient(data.OwnerUserId, "User");

        var deleteResponse = await ownerClient.DeleteAsync($"/api/recept-kuharice/{data.JoinId}");

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        var deleteBody = await deleteResponse.Content.ReadFromJsonAsync<ApiResponse<ReceptKuharicaDto>>();
        Assert.NotNull(deleteBody);
        Assert.True(deleteBody!.success);
        Assert.NotNull(deleteBody.data);
        Assert.True(deleteBody.data!.isDeleted);

        var ownerGetAfterDelete = await ownerClient.GetAsync($"/api/recept-kuharice/{data.JoinId}");
        Assert.Equal(HttpStatusCode.NotFound, ownerGetAfterDelete.StatusCode);
    }

    [Fact]
    public async Task Delete_WithNonOwnerUser_ReturnsForbidden()
    {
        var data = await SeedJoinAsync();
        using var client = CreateAuthorizedClient(data.OtherUserId, "User");

        var response = await client.DeleteAsync($"/api/recept-kuharice/{data.JoinId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(body);
        Assert.False(body!.success);
        Assert.NotNull(body.alert);
    }

    [Fact]
    public async Task Delete_WithNonExistingId_ReturnsNotFound()
    {
        var data = await SeedOwnerAndRecipeAsync();
        using var client = CreateAuthorizedClient(data.OwnerUserId, "User");

        var response = await client.DeleteAsync("/api/recept-kuharice/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(body);
        Assert.False(body!.success);
        Assert.NotNull(body.alert);
    }

    [Fact]
    public async Task GetAll_WithAdminIncludeDeletedTrue_ReturnsSoftDeletedJoin()
    {
        var data = await SeedJoinAsync();
        using var ownerClient = CreateAuthorizedClient(data.OwnerUserId, "User");
        using var adminClient = CreateAuthorizedClient(userId: 1, role: "Admin");

        var deleteResponse = await ownerClient.DeleteAsync($"/api/recept-kuharice/{data.JoinId}");
        deleteResponse.EnsureSuccessStatusCode();

        var response = await adminClient.GetAsync($"/api/recept-kuharice?kuharicaId={data.OwnerCookbookId}&includeDeleted=true");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<ReceptKuharicaDto>>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Contains(body.data!, join => join.id == data.JoinId && join.isDeleted);
    }

    [Fact]
    public async Task GetAll_WithOwnerUser_ReturnsOnlyOwnedAndVisibleJoins()
    {
        var data = await SeedQueryDataAsync();
        using var client = CreateAuthorizedClient(data.OwnerUserId, "User");

        var response = await client.GetAsync("/api/recept-kuharice");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<ReceptKuharicaPublicDto>>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);

        Assert.Contains(body.data!, join => join.id == data.OwnerVisibleOldJoinId);
        Assert.Contains(body.data!, join => join.id == data.OwnerVisibleNewJoinId);
        Assert.DoesNotContain(body.data!, join => join.id == data.OwnerSoftDeletedJoinId);
        Assert.DoesNotContain(body.data!, join => join.id == data.OtherUserJoinId);
        Assert.DoesNotContain(body.data!, join => join.id == data.OwnerJoinWithDeletedRecipeId);
    }

    [Fact]
    public async Task GetAll_WithAdminIncludeDeletedFalse_ExcludesDeletedJoin()
    {
        var data = await SeedQueryDataAsync();
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.GetAsync($"/api/recept-kuharice?kuharicaId={data.OwnerCookbookId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<ReceptKuharicaDto>>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);

        Assert.DoesNotContain(body.data!, join => join.id == data.OwnerSoftDeletedJoinId);
    }

    [Fact]
    public async Task GetAll_WithAdminFiltersByKuharicaAndRecept_ReturnsSingleExpectedJoin()
    {
        var data = await SeedQueryDataAsync();
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.GetAsync(
            $"/api/recept-kuharice?kuharicaId={data.OwnerCookbookId}&receptId={data.OwnerNewRecipeId}&includeDeleted=true");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<ReceptKuharicaDto>>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Single(body.data!);
        Assert.Equal(data.OwnerVisibleNewJoinId, body.data![0].id);
    }

    [Fact]
    public async Task GetAll_WithAdminDateRangeAndSortDesc_ReturnsOrderedSubset()
    {
        var data = await SeedQueryDataAsync();
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.GetAsync(
            $"/api/recept-kuharice?kuharicaId={data.OwnerCookbookId}&includeDeleted=true&odDatuma={data.FromDate:yyyy-MM-dd}&doDatuma={data.ToDate:yyyy-MM-dd}&sortBy=kreirano&sortDir=desc");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<ReceptKuharicaDto>>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.True(body.data!.Count >= 2);

        for (var i = 0; i < body.data.Count - 1; i++)
        {
            Assert.True(body.data[i].kreirano >= body.data[i + 1].kreirano);
        }

        Assert.Contains(body.data, join => join.id == data.OwnerVisibleOldJoinId);
        Assert.Contains(body.data, join => join.id == data.OwnerVisibleNewJoinId);
    }

    private HttpClient CreateAuthorizedClient(int userId, string role)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
        return client;
    }

    private async Task<SeedResult> SeedOwnerAndRecipeAsync()
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

        var ownerCookbook = new Kuharica
        {
            naziv = $"Kuharica-{Guid.NewGuid():N}",
            userId = ownerUser.id,
            isDeleted = false
        };

        var recipe = new Recept
        {
            naziv = $"Recept-{Guid.NewGuid():N}",
            opis = "Test recept",
            brojPorcija = 2,
            vrijemeKuhanja = 20,
            tezina = TezinaRecepta.Srednje,
            isDeleted = false
        };

        db.Kuharice.Add(ownerCookbook);
        db.Recepti.Add(recipe);
        await db.SaveChangesAsync();

        return new SeedResult(
            OwnerUserId: ownerUser.id,
            OtherUserId: otherUser.id,
            OwnerCookbookId: ownerCookbook.id,
            RecipeId: recipe.id,
            JoinId: 0);
    }

    private async Task<SeedResult> SeedJoinAsync()
    {
        var baseData = await SeedOwnerAndRecipeAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KitchenAidDbContext>();

        var join = new ReceptKuharica
        {
            receptId = baseData.RecipeId,
            kuharicaId = baseData.OwnerCookbookId,
            isDeleted = false
        };

        db.ReceptKuharice.Add(join);
        await db.SaveChangesAsync();

        return baseData with { JoinId = join.id };
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
            naziv = $"Owner-Kuharica-{Guid.NewGuid():N}",
            userId = ownerUser.id,
            isDeleted = false
        };

        var otherCookbook = new Kuharica
        {
            naziv = $"Other-Kuharica-{Guid.NewGuid():N}",
            userId = otherUser.id,
            isDeleted = false
        };

        var ownerOldRecipe = new Recept
        {
            naziv = $"Owner-Old-{Guid.NewGuid():N}",
            opis = "Owner old recipe",
            brojPorcija = 2,
            vrijemeKuhanja = 15,
            tezina = TezinaRecepta.Lako,
            isDeleted = false
        };

        var ownerNewRecipe = new Recept
        {
            naziv = $"Owner-New-{Guid.NewGuid():N}",
            opis = "Owner new recipe",
            brojPorcija = 3,
            vrijemeKuhanja = 25,
            tezina = TezinaRecepta.Srednje,
            isDeleted = false
        };

        var ownerSoftDeletedRecipe = new Recept
        {
            naziv = $"Owner-DeletedJoin-{Guid.NewGuid():N}",
            opis = "Owner soft-deleted join recipe",
            brojPorcija = 4,
            vrijemeKuhanja = 30,
            tezina = TezinaRecepta.Tesko,
            isDeleted = false
        };

        var ownerJoinWithDeletedRecipe = new Recept
        {
            naziv = $"Owner-RecipeDeleted-{Guid.NewGuid():N}",
            opis = "Recipe marked as deleted",
            brojPorcija = 1,
            vrijemeKuhanja = 10,
            tezina = TezinaRecepta.Lako,
            isDeleted = true
        };

        var otherUserRecipe = new Recept
        {
            naziv = $"Other-Recipe-{Guid.NewGuid():N}",
            opis = "Other user recipe",
            brojPorcija = 2,
            vrijemeKuhanja = 12,
            tezina = TezinaRecepta.Lako,
            isDeleted = false
        };

        db.Kuharice.AddRange(ownerCookbook, otherCookbook);
        db.Recepti.AddRange(ownerOldRecipe, ownerNewRecipe, ownerSoftDeletedRecipe, ownerJoinWithDeletedRecipe, otherUserRecipe);
        await db.SaveChangesAsync();

        var baseDate = DateTime.UtcNow.Date;

        var ownerVisibleOldJoin = new ReceptKuharica
        {
            kuharicaId = ownerCookbook.id,
            receptId = ownerOldRecipe.id,
            isDeleted = false,
            kreirano = baseDate.AddDays(-5)
        };

        var ownerVisibleNewJoin = new ReceptKuharica
        {
            kuharicaId = ownerCookbook.id,
            receptId = ownerNewRecipe.id,
            isDeleted = false,
            kreirano = baseDate.AddDays(-1)
        };

        var ownerSoftDeletedJoin = new ReceptKuharica
        {
            kuharicaId = ownerCookbook.id,
            receptId = ownerSoftDeletedRecipe.id,
            isDeleted = true,
            kreirano = baseDate.AddDays(-2)
        };

        var ownerJoinForDeletedRecipe = new ReceptKuharica
        {
            kuharicaId = ownerCookbook.id,
            receptId = ownerJoinWithDeletedRecipe.id,
            isDeleted = false,
            kreirano = baseDate.AddDays(-3)
        };

        var otherUserJoin = new ReceptKuharica
        {
            kuharicaId = otherCookbook.id,
            receptId = otherUserRecipe.id,
            isDeleted = false,
            kreirano = baseDate.AddDays(-4)
        };

        db.ReceptKuharice.AddRange(
            ownerVisibleOldJoin,
            ownerVisibleNewJoin,
            ownerSoftDeletedJoin,
            ownerJoinForDeletedRecipe,
            otherUserJoin);

        await db.SaveChangesAsync();

        return new QuerySeedResult(
            OwnerUserId: ownerUser.id,
            OtherUserId: otherUser.id,
            OwnerCookbookId: ownerCookbook.id,
            OwnerNewRecipeId: ownerNewRecipe.id,
            OwnerVisibleOldJoinId: ownerVisibleOldJoin.id,
            OwnerVisibleNewJoinId: ownerVisibleNewJoin.id,
            OwnerSoftDeletedJoinId: ownerSoftDeletedJoin.id,
            OwnerJoinWithDeletedRecipeId: ownerJoinForDeletedRecipe.id,
            OtherUserJoinId: otherUserJoin.id,
            FromDate: baseDate.AddDays(-6),
            ToDate: baseDate.AddDays(0));
    }

    private sealed record SeedResult(
        int OwnerUserId,
        int OtherUserId,
        int OwnerCookbookId,
        int RecipeId,
        int JoinId);

    private sealed record QuerySeedResult(
        int OwnerUserId,
        int OtherUserId,
        int OwnerCookbookId,
        int OwnerNewRecipeId,
        int OwnerVisibleOldJoinId,
        int OwnerVisibleNewJoinId,
        int OwnerSoftDeletedJoinId,
        int OwnerJoinWithDeletedRecipeId,
        int OtherUserJoinId,
        DateTime FromDate,
        DateTime ToDate);
}
