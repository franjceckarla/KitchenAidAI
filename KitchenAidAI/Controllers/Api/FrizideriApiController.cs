using KitchenAidAI.Data;
using KitchenAidAI.Models;
using KitchenAidAI.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KitchenAidAI.Controllers.Api
{
    [Route("api/frizideri")]
    public class FrizideriApiController : ApiControllerBase
    {
        private readonly KitchenAidDbContext _dbContext;

        public FrizideriApiController(KitchenAidDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult GetAll(
            [FromQuery] int? userId,
            [FromQuery] DateTime? odDatuma,
            [FromQuery] DateTime? doDatuma,
            [FromQuery] bool? includeDeleted,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDir)
        {
            if (!TryGetSession(out var currentUserId, out var isAdmin, out var error))
            {
                return error!;
            }

            var query = _dbContext.Frizideri
                .Include(fridge => fridge.namirnice)
                .AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(fridge => fridge.userId == userId.Value);
            }

            if (!isAdmin)
            {
                query = query.Where(fridge => fridge.userId == currentUserId && !fridge.isDeleted);
            }
            else if (includeDeleted.HasValue && !includeDeleted.Value)
            {
                query = query.Where(fridge => !fridge.isDeleted);
            }

            if (odDatuma.HasValue)
            {
                var fromDate = odDatuma.Value.Date;
                query = query.Where(fridge => fridge.kreirano >= fromDate);
            }

            if (doDatuma.HasValue)
            {
                var toDate = doDatuma.Value.Date;
                query = query.Where(fridge => fridge.kreirano <= toDate);
            }

            var sortDescending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            query = sortBy switch
            {
                "kreirano" => sortDescending
                    ? query.OrderByDescending(fridge => fridge.kreirano)
                    : query.OrderBy(fridge => fridge.kreirano),
                "azurirano" => sortDescending
                    ? query.OrderByDescending(fridge => fridge.azurirano)
                    : query.OrderBy(fridge => fridge.azurirano),
                _ => query.OrderBy(fridge => fridge.id)
            };

            var fridges = query.AsNoTracking().ToList();
            if (isAdmin)
            {
                var adminDtos = fridges.Select(fridge => fridge.ToDto()).ToList();
                return Ok(ApiResponse<List<FriziderDto>>.Ok(adminDtos));
            }

            var publicDtos = fridges.Select(fridge => fridge.ToPublicDto()).ToList();
            return Ok(ApiResponse<List<FriziderPublicDto>>.Ok(publicDtos));
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            if (!TryGetSession(out var currentUserId, out var isAdmin, out var error))
            {
                return error!;
            }

            var fridge = _dbContext.Frizideri
                .Include(currentFridge => currentFridge.namirnice)
                .AsNoTracking()
                .FirstOrDefault(currentFridge => currentFridge.id == id && (isAdmin || !currentFridge.isDeleted));
            if (fridge is null)
            {
                return ApiNotFound("Frizider nije pronadjen.");
            }

            if (!isAdmin && fridge.userId != currentUserId)
            {
                return ApiForbidden("Nemate pravo pristupa ovom frizideru.");
            }

            if (isAdmin)
            {
                return Ok(ApiResponse<FriziderDto>.Ok(fridge.ToDto()));
            }

            return Ok(ApiResponse<FriziderPublicDto>.Ok(fridge.ToPublicDto()));
        }

        [HttpPost]
        public IActionResult Create([FromBody] FriziderCreateDto input)
        {
            if (!TryGetSession(out var currentUserId, out var isAdmin, out var error))
            {
                return error!;
            }

            var targetUserId = input.userId;
            if (!isAdmin && currentUserId != targetUserId)
            {
                return ApiForbidden("Nemate pravo kreirati frizider za drugog korisnika.");
            }

            var user = _dbContext.Users.FirstOrDefault(currentUser => currentUser.id == targetUserId && (isAdmin || !currentUser.isDeleted));
            if (user is null)
            {
                return ApiNotFound("Korisnik nije pronadjen.");
            }

            if (user.frizider is not null)
            {
                return ApiBadRequest("Korisnik vec ima frizider.", "FRIDGE_EXISTS");
            }

            var fridge = new Frizider
            {
                userId = targetUserId,
                kreirano = DateTime.Now,
                azurirano = DateTime.Now,
                isDeleted = false
            };

            _dbContext.Frizideri.Add(fridge);
            _dbContext.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = fridge.id }, ApiResponse<FriziderDto>.Ok(fridge.ToDto()));
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] FriziderUpdateDto input)
        {
            if (!TryGetSession(out var currentUserId, out var isAdmin, out var error))
            {
                return error!;
            }

            var fridge = _dbContext.Frizideri.FirstOrDefault(currentFridge => currentFridge.id == id);
            if (fridge is null)
            {
                return ApiNotFound("Frizider nije pronadjen.");
            }

            if (!isAdmin && fridge.userId != currentUserId)
            {
                return ApiForbidden("Nemate pravo uredjivati ovaj frizider.");
            }

            fridge.isDeleted = input.isDeleted;
            fridge.azurirano = DateTime.Now;
            _dbContext.SaveChanges();

            if (isAdmin)
            {
                return Ok(ApiResponse<FriziderDto>.Ok(fridge.ToDto()));
            }

            return Ok(ApiResponse<FriziderPublicDto>.Ok(fridge.ToPublicDto()));
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            if (!TryGetSession(out var currentUserId, out var isAdmin, out var error))
            {
                return error!;
            }

            var fridge = _dbContext.Frizideri
                .Include(currentFridge => currentFridge.namirnice)
                .FirstOrDefault(currentFridge => currentFridge.id == id);
            if (fridge is null)
            {
                return ApiNotFound("Frizider nije pronadjen.");
            }

            if (!isAdmin && fridge.userId != currentUserId)
            {
                return ApiForbidden("Nemate pravo obrisati ovaj frizider.");
            }

            fridge.isDeleted = true;
            foreach (var item in fridge.namirnice)
            {
                item.isDeleted = true;
            }

            _dbContext.SaveChanges();

            return Ok(ApiResponse<FriziderDto>.Ok(fridge.ToDto()));
        }
    }
}
