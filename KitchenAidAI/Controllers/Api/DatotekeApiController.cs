using System.IO;
using KitchenAidAI.Data;
using KitchenAidAI.Models;
using KitchenAidAI.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KitchenAidAI.Controllers.Api
{
    [Route("api/datoteke")]
    public class DatotekeApiController : ApiControllerBase
    {
        private readonly KitchenAidDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DatotekeApiController(KitchenAidDbContext dbContext, IWebHostEnvironment webHostEnvironment)
        {
            _dbContext = dbContext;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public IActionResult GetAll(
            [FromQuery] int? userId,
            [FromQuery] string? search,
            [FromQuery] bool includeDeleted = false)
        {
            if (!TryGetSession(out var currentUserId, out var isAdmin, out var error))
            {
                return error!;
            }

            var targetUserId = isAdmin
                ? userId ?? currentUserId
                : currentUserId;

            if (isAdmin && !userId.HasValue)
            {
                return ApiBadRequest("Za pregled datoteka drugog korisnika potrebno je odabrati korisnika.", "USER_ID_REQUIRED");
            }

            var query = _dbContext.Datoteke
                .Where(file => file.userId == targetUserId)
                .AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(file => !file.isDeleted);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(file => (file.naziv ?? string.Empty).Contains(search)
                    || (file.opis ?? string.Empty).Contains(search));
            }

            var files = query
                .OrderByDescending(file => file.kreirano)
                .AsNoTracking()
                .ToList()
                .Select(file => file.ToDto())
                .ToList();

            return Ok(ApiResponse<List<DatotekaDto>>.Ok(files));
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            if (!TryGetSession(out var currentUserId, out var isAdmin, out var error))
            {
                return error!;
            }

            if (isAdmin)
            {
                return ApiForbidden("Administratori ne koriste pojedinacni pregled bez odabranog korisnika.");
            }

            var file = _dbContext.Datoteke
                .AsNoTracking()
                .FirstOrDefault(item => item.id == id && item.userId == currentUserId && !item.isDeleted);

            if (file is null)
            {
                return ApiNotFound("Datoteka nije pronadjena.");
            }

            return Ok(ApiResponse<DatotekaDto>.Ok(file.ToDto()));
        }

        [HttpGet("autocomplete")]
        public IActionResult Autocomplete([FromQuery] string? term)
        {
            if (!TryGetSession(out _, out var isAdmin, out var error))
            {
                return error!;
            }

            if (isAdmin)
            {
                return ApiForbidden("Administratori nemaju pristup ovoj akciji.");
            }

            var rootPath = EnsureDocumentRoot();
            var normalizedTerm = (term ?? string.Empty).Trim();

            var suggestions = Directory
                .EnumerateFiles(rootPath, "*", SearchOption.AllDirectories)
                .Select(path => Path.GetRelativePath(rootPath, path).Replace('\\', '/'))
                .Where(path => path.IndexOf(normalizedTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(path => path)
                .Take(20)
                .ToList();

            return Ok(ApiResponse<List<string>>.Ok(suggestions));
        }

        [HttpPost("upload")]
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> Upload([FromForm] IFormFile? file, [FromForm] string? opis)
        {
            if (!TryGetSession(out var currentUserId, out var isAdmin, out var error))
            {
                return error!;
            }

            if (isAdmin)
            {
                return ApiForbidden("Administratori nemaju pristup upravljanju korisnickim datotekama.");
            }

            if (file is null || file.Length == 0)
            {
                return ApiBadRequest("Odaberite datoteku za upload.", "FILE_REQUIRED");
            }

            var storedRelativePath = await SaveUploadedFileAsync(file, currentUserId);
            var entity = new Datoteka
            {
                userId = currentUserId,
                naziv = Path.GetFileName(file.FileName),
                opis = string.IsNullOrWhiteSpace(opis) ? null : opis.Trim(),
                contentType = file.ContentType,
                velicina = file.Length,
                putanja = storedRelativePath,
                isDeleted = false,
                deletedAt = null,
                kreirano = DateTime.Now
            };

            _dbContext.Datoteke.Add(entity);
            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<DatotekaDto>.Ok(entity.ToDto()));
        }

        [HttpPost("upload-from-document")]
        public async Task<IActionResult> UploadFromDocument([FromBody] DatotekaUploadFromDocumentDto input)
        {
            if (!TryGetSession(out var currentUserId, out var isAdmin, out var error))
            {
                return error!;
            }

            if (isAdmin)
            {
                return ApiForbidden("Administratori nemaju pristup upravljanju korisnickim datotekama.");
            }

            if (string.IsNullOrWhiteSpace(input.relativePath))
            {
                return ApiBadRequest("Unesite putanju datoteke iz /document mape.", "PATH_REQUIRED");
            }

            var documentRoot = EnsureDocumentRoot();
            var sanitizedRelativePath = input.relativePath.Trim().Replace('\\', '/').TrimStart('/');
            if (sanitizedRelativePath.Contains("..", StringComparison.Ordinal))
            {
                return ApiBadRequest("Putanja nije dozvoljena.", "INVALID_PATH");
            }

            var sourceAbsolutePath = Path.GetFullPath(Path.Combine(documentRoot, sanitizedRelativePath));
            if (!sourceAbsolutePath.StartsWith(documentRoot, StringComparison.OrdinalIgnoreCase))
            {
                return ApiBadRequest("Putanja mora biti unutar /document mape.", "INVALID_PATH");
            }

            if (!System.IO.File.Exists(sourceAbsolutePath))
            {
                return ApiNotFound("Datoteka nije pronadjena u /document mapi.");
            }

            var storedRelativePath = await CopyFromDocumentAsync(sourceAbsolutePath, currentUserId);
            var fileInfo = new FileInfo(sourceAbsolutePath);

            var entity = new Datoteka
            {
                userId = currentUserId,
                naziv = Path.GetFileName(sourceAbsolutePath),
                opis = string.IsNullOrWhiteSpace(input.opis) ? null : input.opis.Trim(),
                contentType = GetContentTypeByExtension(Path.GetExtension(sourceAbsolutePath)),
                velicina = fileInfo.Length,
                putanja = storedRelativePath,
                isDeleted = false,
                deletedAt = null,
                kreirano = DateTime.Now
            };

            _dbContext.Datoteke.Add(entity);
            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<DatotekaDto>.Ok(entity.ToDto()));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            if (!TryGetSession(out var currentUserId, out var isAdmin, out var error))
            {
                return error!;
            }

            if (isAdmin)
            {
                return ApiForbidden("Administratori nemaju pristup upravljanju korisnickim datotekama.");
            }

            var file = await _dbContext.Datoteke.FirstOrDefaultAsync(item => item.id == id && item.userId == currentUserId);
            if (file is null)
            {
                return ApiNotFound("Datoteka nije pronadjena.");
            }

            if (file.isDeleted)
            {
                return StatusCode(409, ApiResponse<object>.Fail(
                    "Konflikt",
                    "Datoteka je vec oznacena kao obrisana.",
                    "ALREADY_DELETED"));
            }

            file.isDeleted = true;
            file.deletedAt = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<DatotekaDto>.Ok(file.ToDto()));
        }

        private string EnsureDocumentRoot()
        {
            var path = Path.Combine(_webHostEnvironment.ContentRootPath, "document");
            Directory.CreateDirectory(path);
            return path;
        }

        private async Task<string> SaveUploadedFileAsync(IFormFile file, int userId)
        {
            var documentRoot = EnsureDocumentRoot();
            var uploadFolder = Path.Combine(documentRoot, "uploads", userId.ToString());
            Directory.CreateDirectory(uploadFolder);

            var safeFileName = Path.GetFileName(file.FileName);
            var storedName = $"{Guid.NewGuid():N}_{safeFileName}";
            var destinationPath = Path.Combine(uploadFolder, storedName);

            await using var stream = new FileStream(destinationPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return Path.GetRelativePath(documentRoot, destinationPath).Replace('\\', '/');
        }

        private async Task<string> CopyFromDocumentAsync(string sourceAbsolutePath, int userId)
        {
            var documentRoot = EnsureDocumentRoot();
            var uploadFolder = Path.Combine(documentRoot, "uploads", userId.ToString());
            Directory.CreateDirectory(uploadFolder);

            var storedName = $"{Guid.NewGuid():N}_{Path.GetFileName(sourceAbsolutePath)}";
            var destinationPath = Path.Combine(uploadFolder, storedName);

            await using var source = new FileStream(sourceAbsolutePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            await using var destination = new FileStream(destinationPath, FileMode.Create);
            await source.CopyToAsync(destination);

            return Path.GetRelativePath(documentRoot, destinationPath).Replace('\\', '/');
        }

        private static string GetContentTypeByExtension(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".txt" => "text/plain",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }
    }
}
