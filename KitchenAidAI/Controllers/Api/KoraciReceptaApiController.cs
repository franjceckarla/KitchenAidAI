using KitchenAidAI.Data;
using KitchenAidAI.Models;
using KitchenAidAI.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KitchenAidAI.Controllers.Api
{
    [Route("api/koraci")]
    public class KoraciReceptaApiController : ApiControllerBase
    {
        private readonly KitchenAidDbContext _dbContext;

        public KoraciReceptaApiController(KitchenAidDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult GetAll(
            [FromQuery] int? receptId,
            [FromQuery] string? search,
            [FromQuery] int? minRedniBroj,
            [FromQuery] int? maxRedniBroj,
            [FromQuery] double? minTrajanje,
            [FromQuery] double? maxTrajanje,
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

            var query = _dbContext.KoraciRecepta
                .Include(step => step.recept)
                .ThenInclude(recipe => recipe!.receptKuharice)
                .ThenInclude(join => join.kuharica)
                .AsQueryable();

            if (receptId.HasValue)
            {
                query = query.Where(step => step.receptId == receptId.Value);
            }

            if (isAdmin)
            {
                if (!includeDeleted)
                {
                    query = query.Where(step => !step.isDeleted);
                }
            }
            else
            {
                query = query.Where(step => !step.isDeleted
                    && step.recept != null
                    && !step.recept.isDeleted
                    && step.recept.receptKuharice.Any(join =>
                        !join.isDeleted
                        && join.kuharica != null
                        && join.kuharica.userId == userId
                        && !join.kuharica.isDeleted));
            }

            if (minRedniBroj.HasValue)
            {
                query = query.Where(step => step.redniBroj >= minRedniBroj.Value);
            }

            if (maxRedniBroj.HasValue)
            {
                query = query.Where(step => step.redniBroj <= maxRedniBroj.Value);
            }

            if (minTrajanje.HasValue)
            {
                query = query.Where(step => step.trajanje >= minTrajanje.Value);
            }

            if (maxTrajanje.HasValue)
            {
                query = query.Where(step => step.trajanje <= maxTrajanje.Value);
            }

            if (odDatuma.HasValue)
            {
                var fromDate = odDatuma.Value.Date;
                query = query.Where(step => step.kreirano >= fromDate);
            }

            if (doDatuma.HasValue)
            {
                var toDate = doDatuma.Value.Date;
                query = query.Where(step => step.kreirano <= toDate);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(step => (step.opis ?? string.Empty).Contains(search));
            }

            var sortDescending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            query = sortBy switch
            {
                "redniBroj" => sortDescending
                    ? query.OrderByDescending(step => step.redniBroj)
                    : query.OrderBy(step => step.redniBroj),
                "trajanje" => sortDescending
                    ? query.OrderByDescending(step => step.trajanje)
                    : query.OrderBy(step => step.trajanje),
                "kreirano" => sortDescending
                    ? query.OrderByDescending(step => step.kreirano)
                    : query.OrderBy(step => step.kreirano),
                _ => query.OrderBy(step => step.id)
            };

            var steps = query.AsNoTracking().ToList();
            if (isAdmin)
            {
                var adminDtos = steps.Select(step => new KorakReceptaDto
                {
                    id = step.id,
                    receptId = step.receptId,
                    redniBroj = step.redniBroj,
                    opis = step.opis,
                    trajanje = step.trajanje,
                    isDeleted = step.isDeleted
                }).ToList();
                return Ok(ApiResponse<List<KorakReceptaDto>>.Ok(adminDtos));
            }

            var publicDtos = steps.Select(step => new KorakReceptaPublicDto
            {
                id = step.id,
                receptId = step.receptId,
                redniBroj = step.redniBroj,
                opis = step.opis,
                trajanje = step.trajanje
            }).ToList();
            return Ok(ApiResponse<List<KorakReceptaPublicDto>>.Ok(publicDtos));
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            if (!TryGetSession(out var userId, out var isAdmin, out var error))
            {
                return error!;
            }

            var step = _dbContext.KoraciRecepta
                .Include(currentStep => currentStep.recept)
                .ThenInclude(recipe => recipe!.receptKuharice)
                .ThenInclude(join => join.kuharica)
                .AsNoTracking()
                .FirstOrDefault(currentStep => currentStep.id == id && (isAdmin || !currentStep.isDeleted));
            if (step is null)
            {
                return ApiNotFound("Korak nije pronadjen.");
            }

            if (!isAdmin && step.recept?.receptKuharice.All(join => join.kuharica?.userId != userId) == true)
            {
                return ApiForbidden("Nemate pravo pristupa ovom koraku.");
            }

            if (isAdmin)
            {
                return Ok(ApiResponse<KorakReceptaDto>.Ok(new KorakReceptaDto
                {
                    id = step.id,
                    receptId = step.receptId,
                    redniBroj = step.redniBroj,
                    opis = step.opis,
                    trajanje = step.trajanje,
                    isDeleted = step.isDeleted
                }));
            }

            return Ok(ApiResponse<KorakReceptaPublicDto>.Ok(new KorakReceptaPublicDto
            {
                id = step.id,
                receptId = step.receptId,
                redniBroj = step.redniBroj,
                opis = step.opis,
                trajanje = step.trajanje
            }));
        }

        [HttpPost]
        public IActionResult Create([FromBody] KorakReceptaFormDto input)
        {
            if (!TryGetSession(out var userId, out var isAdmin, out var error))
            {
                return error!;
            }

            var recipe = _dbContext.Recepti
                .Include(currentRecipe => currentRecipe.receptKuharice)
                .ThenInclude(join => join.kuharica)
                .FirstOrDefault(currentRecipe => currentRecipe.id == input.receptId && (isAdmin || !currentRecipe.isDeleted));
            if (recipe is null)
            {
                return ApiNotFound("Recept nije pronadjen.");
            }

            if (!isAdmin && recipe.receptKuharice.All(join => join.kuharica?.userId != userId))
            {
                return ApiForbidden("Nemate pravo dodavati korake ovom receptu.");
            }

            var step = new KorakRecepta
            {
                receptId = input.receptId,
                redniBroj = input.redniBroj,
                opis = input.opis,
                trajanje = input.trajanje,
                isDeleted = false
            };

            _dbContext.KoraciRecepta.Add(step);
            _dbContext.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = step.id }, ApiResponse<KorakReceptaDto>.Ok(new KorakReceptaDto
            {
                id = step.id,
                receptId = step.receptId,
                redniBroj = step.redniBroj,
                opis = step.opis,
                trajanje = step.trajanje,
                isDeleted = step.isDeleted
            }));
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] KorakReceptaFormDto input)
        {
            if (!TryGetSession(out var userId, out var isAdmin, out var error))
            {
                return error!;
            }

            var step = _dbContext.KoraciRecepta
                .Include(currentStep => currentStep.recept)
                .ThenInclude(recipe => recipe!.receptKuharice)
                .ThenInclude(join => join.kuharica)
                .FirstOrDefault(currentStep => currentStep.id == id && (isAdmin || !currentStep.isDeleted));
            if (step is null)
            {
                return ApiNotFound("Korak nije pronadjen.");
            }

            if (!isAdmin && step.recept?.receptKuharice.All(join => join.kuharica?.userId != userId) == true)
            {
                return ApiForbidden("Nemate pravo uredjivati ovaj korak.");
            }

            step.redniBroj = input.redniBroj;
            step.opis = input.opis;
            step.trajanje = input.trajanje;
            _dbContext.SaveChanges();

            return Ok(ApiResponse<KorakReceptaDto>.Ok(new KorakReceptaDto
            {
                id = step.id,
                receptId = step.receptId,
                redniBroj = step.redniBroj,
                opis = step.opis,
                trajanje = step.trajanje,
                isDeleted = step.isDeleted
            }));
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            if (!TryGetSession(out var userId, out var isAdmin, out var error))
            {
                return error!;
            }

            var step = _dbContext.KoraciRecepta
                .Include(currentStep => currentStep.recept)
                .ThenInclude(recipe => recipe!.receptKuharice)
                .ThenInclude(join => join.kuharica)
                .FirstOrDefault(currentStep => currentStep.id == id && (isAdmin || !currentStep.isDeleted));
            if (step is null)
            {
                return ApiNotFound("Korak nije pronadjen.");
            }

            if (!isAdmin && step.recept?.receptKuharice.All(join => join.kuharica?.userId != userId) == true)
            {
                return ApiForbidden("Nemate pravo obrisati ovaj korak.");
            }

            step.isDeleted = true;
            _dbContext.SaveChanges();

            return Ok(ApiResponse<KorakReceptaDto>.Ok(new KorakReceptaDto
            {
                id = step.id,
                receptId = step.receptId,
                redniBroj = step.redniBroj,
                opis = step.opis,
                trajanje = step.trajanje,
                isDeleted = step.isDeleted
            }));
        }
    }
}
