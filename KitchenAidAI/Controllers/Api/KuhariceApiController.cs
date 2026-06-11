using KitchenAidAI.Data;
using KitchenAidAI.Models;
using KitchenAidAI.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KitchenAidAI.Controllers.Api
{
    [Route("api/kuharice")]
    public class KuhariceApiController : ApiControllerBase
    {
        private readonly KitchenAidDbContext _dbContext;

        public KuhariceApiController(KitchenAidDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult GetAll(
            [FromQuery] string? search,
            [FromQuery] int? filterUserId,
            [FromQuery] DateTime? odDatuma,
            [FromQuery] DateTime? doDatuma,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDir,
            [FromQuery] bool includeDeleted = false)
        {
            if (!TryGetSession(out var userId, out var isAdmin, out var error))
            {
                return error!;
            }

            var query = _dbContext.Kuharice
                .Include(cookbook => cookbook.receptKuharice)
                .ThenInclude(join => join.recept)
                .AsQueryable();

            if (isAdmin)
            {
                if (!includeDeleted)
                {
                    query = query.Where(cookbook => !cookbook.isDeleted);
                }

                if (filterUserId.HasValue)
                {
                    query = query.Where(cookbook => cookbook.userId == filterUserId.Value);
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(cookbook => (cookbook.naziv ?? string.Empty).Contains(search));
                }
            }
            else
            {
                query = query.Where(cookbook => cookbook.userId == userId && !cookbook.isDeleted);
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(cookbook => (cookbook.naziv ?? string.Empty).Contains(search));
                }
            }

            if (odDatuma.HasValue)
            {
                var fromDate = odDatuma.Value.Date;
                query = query.Where(cookbook => cookbook.kreirano >= fromDate);
            }

            if (doDatuma.HasValue)
            {
                var toDate = doDatuma.Value.Date;
                query = query.Where(cookbook => cookbook.kreirano <= toDate);
            }

            var sortDescending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            query = sortBy switch
            {
                "naziv" => sortDescending
                    ? query.OrderByDescending(cookbook => cookbook.naziv)
                    : query.OrderBy(cookbook => cookbook.naziv),
                "kreirano" => sortDescending
                    ? query.OrderByDescending(cookbook => cookbook.kreirano)
                    : query.OrderBy(cookbook => cookbook.kreirano),
                _ => query.OrderBy(cookbook => cookbook.id)
            };

            var cookbooks = query.AsNoTracking().ToList();
            if (isAdmin)
            {
                var adminDtos = cookbooks.Select(cookbook => cookbook.ToDto()).ToList();
                return Ok(ApiResponse<List<KuharicaDto>>.Ok(adminDtos));
            }

            var publicDtos = cookbooks.Select(cookbook => cookbook.ToPublicDto()).ToList();
            return Ok(ApiResponse<List<KuharicaPublicDto>>.Ok(publicDtos));
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            if (!TryGetSession(out var userId, out var isAdmin, out var error))
            {
                return error!;
            }

            var cookbook = _dbContext.Kuharice
                .Include(currentCookbook => currentCookbook.receptKuharice)
                .ThenInclude(join => join.recept)
                .AsNoTracking()
                .FirstOrDefault(currentCookbook => currentCookbook.id == id && (isAdmin || !currentCookbook.isDeleted));
            if (cookbook is null)
            {
                return ApiNotFound("Kuharica nije pronadjena.");
            }

            if (!isAdmin && cookbook.userId != userId)
            {
                return ApiForbidden("Nemate pravo pristupa ovoj kuharici.");
            }

            if (isAdmin)
            {
                return Ok(ApiResponse<KuharicaDto>.Ok(cookbook.ToDto()));
            }

            return Ok(ApiResponse<KuharicaPublicDto>.Ok(cookbook.ToPublicDto()));
        }

        [HttpPost]
        public IActionResult Create([FromBody] KuharicaCreateDto input)
        {
            if (!TryGetSession(out var userId, out var isAdmin, out var error))
            {
                return error!;
            }

            if (!isAdmin && userId != input.userId)
            {
                return ApiForbidden("Nemate pravo kreirati kuharicu za drugog korisnika.");
            }

            var user = _dbContext.Users
                .Include(currentUser => currentUser.kuharica)
                .FirstOrDefault(currentUser => currentUser.id == input.userId && (isAdmin || !currentUser.isDeleted));
            if (user is null)
            {
                return ApiNotFound("Korisnik nije pronadjen.");
            }

            if (user.kuharica is not null)
            {
                return ApiBadRequest("Korisnik vec ima kuharicu.", "COOKBOOK_EXISTS");
            }

            var cookbook = new Kuharica
            {
                userId = input.userId,
                naziv = string.IsNullOrWhiteSpace(input.naziv) ? "Kuharica" : input.naziv,
                isDeleted = false,
                kreirano = DateTime.Now
            };

            _dbContext.Kuharice.Add(cookbook);
            _dbContext.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = cookbook.id }, ApiResponse<KuharicaDto>.Ok(cookbook.ToDto()));
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] KuharicaUpdateDto input)
        {
            if (!TryGetSession(out var userId, out var isAdmin, out var error))
            {
                return error!;
            }

            var cookbook = _dbContext.Kuharice.FirstOrDefault(currentCookbook => currentCookbook.id == id);
            if (cookbook is null)
            {
                return ApiNotFound("Kuharica nije pronadjena.");
            }

            if (!isAdmin && cookbook.userId != userId)
            {
                return ApiForbidden("Nemate pravo uredjivati ovu kuharicu.");
            }

            cookbook.naziv = input.naziv;
            _dbContext.SaveChanges();

            if (isAdmin)
            {
                return Ok(ApiResponse<KuharicaDto>.Ok(cookbook.ToDto()));
            }

            return Ok(ApiResponse<KuharicaPublicDto>.Ok(cookbook.ToPublicDto()));
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            if (!TryGetSession(out var userId, out var isAdmin, out var error))
            {
                return error!;
            }

            var cookbook = _dbContext.Kuharice
                .Include(currentCookbook => currentCookbook.receptKuharice)
                .FirstOrDefault(currentCookbook => currentCookbook.id == id);
            if (cookbook is null)
            {
                return ApiNotFound("Kuharica nije pronadjena.");
            }

            if (!isAdmin && cookbook.userId != userId)
            {
                return ApiForbidden("Nemate pravo obrisati ovu kuharicu.");
            }

            cookbook.isDeleted = true;
            foreach (var join in cookbook.receptKuharice)
            {
                join.isDeleted = true;
            }

            _dbContext.SaveChanges();

            return Ok(ApiResponse<KuharicaDto>.Ok(cookbook.ToDto()));
        }
    }
}
