using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using KitchenAidAI.Data;
using KitchenAidAI.IntegrationTests.Infrastructure;
using KitchenAidAI.Models;
using KitchenAidAI.Models.DTOs;
using KitchenAidAI.Models.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace KitchenAidAI.IntegrationTests;

public class DatotekeApiTests : IClassFixture<KitchenAidApiFactory>
{
    private readonly KitchenAidApiFactory _factory;

    public DatotekeApiTests(KitchenAidApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/datoteke");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_WithAdminWithoutUserId_ReturnsBadRequest_AndCode()
    {
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.GetAsync("/api/datoteke");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(body);
        Assert.False(body!.success);
        Assert.NotNull(body.alert);
        Assert.Equal("USER_ID_REQUIRED", body.alert!.code);
    }

    [Fact]
    public async Task GetAll_WithAdminAndUserFilter_RespectsIncludeDeletedAndSearch()
    {
        var seed = await SeedQueryDataAsync();
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.GetAsync($"/api/datoteke?userId={seed.OwnerUserId}&search={seed.SearchToken}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<DatotekaDto>>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);

        Assert.Contains(body.data!, file => file.id == seed.OwnerVisibleFileId);
        Assert.DoesNotContain(body.data!, file => file.id == seed.OwnerDeletedFileId);
        Assert.DoesNotContain(body.data!, file => file.id == seed.OtherUserFileId);
    }

    [Fact]
    public async Task GetAll_WithAdminIncludeDeletedTrue_ReturnsDeletedFiles()
    {
        var seed = await SeedQueryDataAsync();
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.GetAsync($"/api/datoteke?userId={seed.OwnerUserId}&includeDeleted=true&search={seed.SearchToken}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<DatotekaDto>>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Contains(body.data!, file => file.id == seed.OwnerVisibleFileId);
        Assert.Contains(body.data!, file => file.id == seed.OwnerDeletedFileId && file.isDeleted);
    }

    [Fact]
    public async Task GetAll_WithNonAdmin_ReturnsOnlyOwnFiles_EvenWhenUserIdIsProvided()
    {
        var seed = await SeedQueryDataAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");

        var response = await client.GetAsync($"/api/datoteke?userId={seed.OtherUserId}&includeDeleted=true");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<DatotekaDto>>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);

        Assert.Contains(body.data!, file => file.id == seed.OwnerVisibleFileId);
        Assert.DoesNotContain(body.data!, file => file.id == seed.OtherUserFileId);
    }

    [Fact]
    public async Task GetById_WithOwnerUser_ReturnsOk_AndBody()
    {
        var seed = await SeedSingleOwnerFileAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");

        var response = await client.GetAsync($"/api/datoteke/{seed.OwnerFileId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<DatotekaDto>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Equal(seed.OwnerFileId, body.data!.id);
        Assert.Equal(seed.OwnerUserId, body.data.userId);
    }

    [Fact]
    public async Task GetById_WithAdmin_ReturnsForbidden()
    {
        var seed = await SeedSingleOwnerFileAsync();
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.GetAsync($"/api/datoteke/{seed.OwnerFileId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(body);
        Assert.False(body!.success);
        Assert.NotNull(body.alert);
        Assert.Equal("FORBIDDEN", body.alert!.code);
    }

    [Fact]
    public async Task GetById_WithOtherUser_ReturnsNotFound()
    {
        var seed = await SeedSingleOwnerFileAsync();
        using var client = CreateAuthorizedClient(seed.OtherUserId, "User");

        var response = await client.GetAsync($"/api/datoteke/{seed.OwnerFileId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithDeletedFile_ReturnsNotFound()
    {
        var seed = await SeedSingleOwnerFileAsync(isDeleted: true);
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");

        var response = await client.GetAsync($"/api/datoteke/{seed.OwnerFileId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Autocomplete_WithUser_ReturnsSuggestions()
    {
        var seed = await SeedUsersOnlyAsync();
        var term = $"auto-{Guid.NewGuid():N}";
        var relativePath = await CreateDocumentFileAsync($"fixtures/{term}.txt", "autocomplete-content");
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");

        var response = await client.GetAsync($"/api/datoteke/autocomplete?term={term}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<string>>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Contains(relativePath, body.data!);
    }

    [Fact]
    public async Task Autocomplete_WithAdmin_ReturnsForbidden()
    {
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.GetAsync("/api/datoteke/autocomplete?term=test");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Upload_WithoutFile_ReturnsBadRequest_AndCode()
    {
        var seed = await SeedUsersOnlyAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");
        using var content = new MultipartFormDataContent();

        var response = await client.PostAsync("/api/datoteke/upload", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Upload_WithAdmin_ReturnsForbidden()
    {
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");
        using var content = BuildUploadContent("admin.txt", "hello", "text/plain", "admin-opis");

        var response = await client.PostAsync("/api/datoteke/upload", content);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Upload_WithUser_ReturnsOk_AndPersistsEntity()
    {
        var seed = await SeedUsersOnlyAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");
        using var content = BuildUploadContent("sample.txt", "hello upload", "text/plain", "opis upload");

        var response = await client.PostAsync("/api/datoteke/upload", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<DatotekaDto>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Equal(seed.OwnerUserId, body.data!.userId);
        Assert.Equal("sample.txt", body.data.naziv);
        Assert.False(body.data.isDeleted);
        Assert.Contains($"uploads/{seed.OwnerUserId}/", body.data.putanja ?? string.Empty);
    }

    [Fact]
    public async Task UploadFromDocument_WithAdmin_ReturnsForbidden()
    {
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.PostAsJsonAsync("/api/datoteke/upload-from-document", new { relativePath = "test.txt" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UploadFromDocument_WithMissingPath_ReturnsBadRequest_AndCode()
    {
        var seed = await SeedUsersOnlyAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");

        var response = await client.PostAsJsonAsync("/api/datoteke/upload-from-document", new { relativePath = " ", opis = "x" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(body);
        Assert.False(body!.success);
        Assert.NotNull(body.alert);
        Assert.Equal("PATH_REQUIRED", body.alert!.code);
    }

    [Fact]
    public async Task UploadFromDocument_WithInvalidPathTraversal_ReturnsBadRequest_AndCode()
    {
        var seed = await SeedUsersOnlyAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");

        var response = await client.PostAsJsonAsync("/api/datoteke/upload-from-document", new { relativePath = "../secret.txt", opis = "x" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(body);
        Assert.False(body!.success);
        Assert.NotNull(body.alert);
        Assert.Equal("INVALID_PATH", body.alert!.code);
    }

    [Fact]
    public async Task UploadFromDocument_WithMissingSourceFile_ReturnsNotFound()
    {
        var seed = await SeedUsersOnlyAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");

        var response = await client.PostAsJsonAsync("/api/datoteke/upload-from-document", new { relativePath = $"missing-{Guid.NewGuid():N}.txt", opis = "x" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UploadFromDocument_WithUser_ReturnsOk_AndPersistsEntity()
    {
        var seed = await SeedUsersOnlyAsync();
        var sourceRelativePath = await CreateDocumentFileAsync($"imports/source-{Guid.NewGuid():N}.txt", "document-source");
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");

        var response = await client.PostAsJsonAsync("/api/datoteke/upload-from-document", new
        {
            relativePath = sourceRelativePath,
            opis = "copied-from-document"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<DatotekaDto>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.Equal(seed.OwnerUserId, body.data!.userId);
        Assert.Equal("copied-from-document", body.data.opis);
        Assert.Contains($"uploads/{seed.OwnerUserId}/", body.data.putanja ?? string.Empty);
    }

    [Fact]
    public async Task Delete_WithOwnerUser_ReturnsOk_AndMarksDeleted()
    {
        var seed = await SeedSingleOwnerFileAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");

        var response = await client.DeleteAsync($"/api/datoteke/{seed.OwnerFileId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<DatotekaDto>>();
        Assert.NotNull(body);
        Assert.True(body!.success);
        Assert.NotNull(body.data);
        Assert.True(body.data!.isDeleted);
        Assert.NotNull(body.data.deletedAt);
    }

    [Fact]
    public async Task Delete_OnAlreadyDeletedFile_ReturnsConflict_AndCode()
    {
        var seed = await SeedSingleOwnerFileAsync(isDeleted: true);
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");

        var response = await client.DeleteAsync($"/api/datoteke/{seed.OwnerFileId}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(body);
        Assert.False(body!.success);
        Assert.NotNull(body.alert);
        Assert.Equal("ALREADY_DELETED", body.alert!.code);
    }

    [Fact]
    public async Task Delete_WithAdmin_ReturnsForbidden()
    {
        var seed = await SeedSingleOwnerFileAsync();
        using var client = CreateAuthorizedClient(userId: 1, role: "Admin");

        var response = await client.DeleteAsync($"/api/datoteke/{seed.OwnerFileId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithMissingId_ReturnsNotFound()
    {
        var seed = await SeedUsersOnlyAsync();
        using var client = CreateAuthorizedClient(seed.OwnerUserId, "User");

        var response = await client.DeleteAsync("/api/datoteke/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private HttpClient CreateAuthorizedClient(int userId, string role)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
        return client;
    }

    private static MultipartFormDataContent BuildUploadContent(string fileName, string fileText, string contentType, string opis)
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(fileText));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file", fileName);
        content.Add(new StringContent(opis), "opis");
        return content;
    }

    private async Task<string> CreateDocumentFileAsync(string relativePath, string content)
    {
        using var scope = _factory.Services.CreateScope();
        var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        var normalizedRelative = relativePath.Replace('\\', '/').TrimStart('/');
        var documentRoot = Path.Combine(env.ContentRootPath, "document");
        var absolutePath = Path.Combine(documentRoot, normalizedRelative);

        var dir = Path.GetDirectoryName(absolutePath);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }

        await File.WriteAllTextAsync(absolutePath, content, Encoding.UTF8);
        return normalizedRelative;
    }

    private async Task<UsersSeedResult> SeedUsersOnlyAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KitchenAidDbContext>();

        var owner = new User
        {
            username = $"file-owner-{Guid.NewGuid():N}",
            email = $"file-owner-{Guid.NewGuid():N}@example.com",
            preferencijaPrehrane = PreferencijaPrehrane.Omnivorte,
            isAdmin = false,
            isDeleted = false
        };

        var other = new User
        {
            username = $"file-other-{Guid.NewGuid():N}",
            email = $"file-other-{Guid.NewGuid():N}@example.com",
            preferencijaPrehrane = PreferencijaPrehrane.Omnivorte,
            isAdmin = false,
            isDeleted = false
        };

        db.Users.AddRange(owner, other);
        await db.SaveChangesAsync();

        return new UsersSeedResult(owner.id, other.id);
    }

    private async Task<SingleFileSeedResult> SeedSingleOwnerFileAsync(bool isDeleted = false)
    {
        var users = await SeedUsersOnlyAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KitchenAidDbContext>();

        var file = new Datoteka
        {
            userId = users.OwnerUserId,
            naziv = $"owner-file-{Guid.NewGuid():N}.txt",
            opis = "owner file",
            contentType = "text/plain",
            velicina = 10,
            putanja = $"uploads/{users.OwnerUserId}/seed.txt",
            isDeleted = isDeleted,
            deletedAt = isDeleted ? DateTime.UtcNow : null,
            kreirano = DateTime.UtcNow
        };

        db.Datoteke.Add(file);
        await db.SaveChangesAsync();

        return new SingleFileSeedResult(users.OwnerUserId, users.OtherUserId, file.id);
    }

    private async Task<QuerySeedResult> SeedQueryDataAsync()
    {
        var users = await SeedUsersOnlyAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KitchenAidDbContext>();

        var token = $"file-query-{Guid.NewGuid():N}";

        var ownerVisible = new Datoteka
        {
            userId = users.OwnerUserId,
            naziv = $"{token}-visible.txt",
            opis = "visible file",
            contentType = "text/plain",
            velicina = 11,
            putanja = $"uploads/{users.OwnerUserId}/visible.txt",
            isDeleted = false,
            deletedAt = null,
            kreirano = DateTime.UtcNow.AddMinutes(-2)
        };

        var ownerDeleted = new Datoteka
        {
            userId = users.OwnerUserId,
            naziv = $"{token}-deleted.txt",
            opis = "deleted file",
            contentType = "text/plain",
            velicina = 22,
            putanja = $"uploads/{users.OwnerUserId}/deleted.txt",
            isDeleted = true,
            deletedAt = DateTime.UtcNow,
            kreirano = DateTime.UtcNow.AddMinutes(-1)
        };

        var otherVisible = new Datoteka
        {
            userId = users.OtherUserId,
            naziv = $"{token}-other.txt",
            opis = "other file",
            contentType = "text/plain",
            velicina = 33,
            putanja = $"uploads/{users.OtherUserId}/other.txt",
            isDeleted = false,
            deletedAt = null,
            kreirano = DateTime.UtcNow
        };

        db.Datoteke.AddRange(ownerVisible, ownerDeleted, otherVisible);
        await db.SaveChangesAsync();

        return new QuerySeedResult(
            OwnerUserId: users.OwnerUserId,
            OtherUserId: users.OtherUserId,
            SearchToken: token,
            OwnerVisibleFileId: ownerVisible.id,
            OwnerDeletedFileId: ownerDeleted.id,
            OtherUserFileId: otherVisible.id);
    }

    private sealed record UsersSeedResult(int OwnerUserId, int OtherUserId);

    private sealed record SingleFileSeedResult(int OwnerUserId, int OtherUserId, int OwnerFileId);

    private sealed record QuerySeedResult(
        int OwnerUserId,
        int OtherUserId,
        string SearchToken,
        int OwnerVisibleFileId,
        int OwnerDeletedFileId,
        int OtherUserFileId);
}
