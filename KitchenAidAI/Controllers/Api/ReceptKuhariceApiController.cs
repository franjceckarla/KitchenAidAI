using KitchenAidAI.Data;
using KitchenAidAI.Models;
using KitchenAidAI.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KitchenAidAI.Controllers.Api
{
    [Route("api/recept-kuharice")]
    public class ReceptKuhariceApiController : ApiControllerBase
    {
        private readonly KitchenAidDbContext _dbContext;

        public ReceptKuhariceApiController(KitchenAidDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult GetAll(
            [FromQuery] int? kuharicaId,
            [FromQuery] int? receptId,
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

            var query = _dbContext.ReceptKuharice
                .Include(join => join.kuharica)
                .Include(join => join.recept)
                .AsQueryable();

            if (kuharicaId.HasValue)
            {
                query = query.Where(join => join.kuharicaId == kuharicaId.Value);
            }

            if (receptId.HasValue)
            {
                query = query.Where(join => join.receptId == receptId.Value);
            }

            if (isAdmin)
            {
                if (!includeDeleted)
                {
                    query = query.Where(join => !join.isDeleted);
                }
            }
            else
            {
                query = query.Where(join => join.kuharica != null
                    && join.kuharica.userId == userId
                    && !join.isDeleted
                    && join.recept != null
                    && !join.recept.isDeleted);
            }

            if (odDatuma.HasValue)
            {
                var fromDate = odDatuma.Value.Date;
                query = query.Where(join => join.kreirano >= fromDate);
            }

            if (doDatuma.HasValue)
            {
                var toDate = doDatuma.Value.Date;
                query = query.Where(join => join.kreirano <= toDate);
            }

            var sortDescending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            query = sortBy switch
            {
                "kreirano" => sortDescending
                    ? query.OrderByDescending(join => join.kreirano)
                    : query.OrderBy(join => join.kreirano),
                _ => query.OrderBy(join => join.id)
            };

            var joins = query.AsNoTracking().ToList();
            if (isAdmin)
            {
                var adminDtos = joins.Select(join => join.ToDto()).ToList();
                return Ok(ApiResponse<List<ReceptKuharicaDto>>.Ok(adminDtos));
            }

            var publicDtos = joins.Select(join => join.ToPublicDto()).ToList();
            return Ok(ApiResponse<List<ReceptKuharicaPublicDto>>.Ok(publicDtos));
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            if (!TryGetSession(out var userId, out var isAdmin, out var error))
            {
                return error!;
            }

            var join = _dbContext.ReceptKuharice
                .Include(currentJoin => currentJoin.kuharica)
                .Include(currentJoin => currentJoin.recept)
                .AsNoTracking()
                .FirstOrDefault(currentJoin => currentJoin.id == id && (isAdmin || !currentJoin.isDeleted));
            if (join is null)
            {
                return ApiNotFound("Poveznica nije pronadjena.");
            }

            if (!isAdmin && join.kuharica?.userId != userId)
            {
                return ApiForbidden("Nemate pravo pristupa ovoj kuharici.");
            }

            if (isAdmin)
            {
                return Ok(ApiResponse<ReceptKuharicaDto>.Ok(join.ToDto()));
            }

            return Ok(ApiResponse<ReceptKuharicaPublicDto>.Ok(join.ToPublicDto()));
        }

        [HttpPost]
        public IActionResult Create([FromBody] ReceptKuharicaCreateDto input)
        {
            if (!TryGetSession(out var userId, out var isAdmin, out var error))
            {
                return error!;
            }

            var cookbook = _dbContext.Kuharice.FirstOrDefault(currentCookbook => currentCookbook.id == input.kuharicaId && (isAdmin || !currentCookbook.isDeleted));
            if (cookbook is null)
            {
                return ApiNotFound("Kuharica nije pronadjena.");
            }

            if (!isAdmin && cookbook.userId != userId)
            {
                return ApiForbidden("Nemate pravo dodati recept u ovu kuharicu.");
            }

            var recipe = _dbContext.Recepti.FirstOrDefault(currentRecipe => currentRecipe.id == input.receptId && (isAdmin || !currentRecipe.isDeleted));
            if (recipe is null)
            {
                return ApiNotFound("Recept nije pronadjen.");
            }

            var exists = _dbContext.ReceptKuharice.Any(join => join.receptId == input.receptId && join.kuharicaId == input.kuharicaId);
            if (exists)
            {
                return ApiBadRequest("Recept je vec u kuharici.", "RECIPE_EXISTS");
            }

            var joinEntity = new ReceptKuharica
            {
                receptId = input.receptId,
                kuharicaId = input.kuharicaId,
                isDeleted = false,
                kreirano = DateTime.Now
            };

            _dbContext.ReceptKuharice.Add(joinEntity);
            _dbContext.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = joinEntity.id }, ApiResponse<ReceptKuharicaDto>.Ok(joinEntity.ToDto()));
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] ReceptKuharicaUpdateDto input)
        {
            if (!TryGetSession(out var userId, out var isAdmin, out var error))
            {
                return error!;
            }

            var joinEntity = _dbContext.ReceptKuharice
                .Include(currentJoin => currentJoin.kuharica)
                .FirstOrDefault(currentJoin => currentJoin.id == id);
            if (joinEntity is null)
            {
                return ApiNotFound("Poveznica nije pronadjena.");
            }

            if (!isAdmin && joinEntity.kuharica?.userId != userId)
            {
                return ApiForbidden("Nemate pravo uredjivati ovu poveznicu.");
            }

            joinEntity.isDeleted = input.isDeleted;
            _dbContext.SaveChanges();

            return Ok(ApiResponse<ReceptKuharicaDto>.Ok(joinEntity.ToDto()));
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            if (!TryGetSession(out var userId, out var isAdmin, out var error))
            {
                return error!;
            }

            var joinEntity = _dbContext.ReceptKuharice
                .Include(currentJoin => currentJoin.kuharica)
                .FirstOrDefault(currentJoin => currentJoin.id == id);
            if (joinEntity is null)
            {
                return ApiNotFound("Poveznica nije pronadjena.");
            }

            if (!isAdmin && joinEntity.kuharica?.userId != userId)
            {
                return ApiForbidden("Nemate pravo obrisati ovu poveznicu.");
            }

            joinEntity.isDeleted = true;
            _dbContext.SaveChanges();

            return Ok(ApiResponse<ReceptKuharicaDto>.Ok(joinEntity.ToDto()));
        }
    }
}
