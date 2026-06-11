using KitchenAidAI.Data;
using KitchenAidAI.Models;
using KitchenAidAI.Models.DTOs;
using KitchenAidAI.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KitchenAidAI.Controllers.Api
{
    [Route("api/recepti")]
    public class ReceptiApiController : ApiControllerBase
    {
        private readonly KitchenAidDbContext _dbContext;

        public ReceptiApiController(KitchenAidDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult GetAll(
            [FromQuery] string? search,
            [FromQuery] TezinaRecepta? tezina,
            [FromQuery] double? minVrijeme,
            [FromQuery] double? maxVrijeme,
            [FromQuery] int? minPorcija,
            [FromQuery] int? maxPorcija,
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

            IQueryable<Recept> recipesQuery = _dbContext.Recepti;
            if (isAdmin)
            {
                if (!includeDeleted)
                {
                    recipesQuery = recipesQuery.Where(recipe => !recipe.isDeleted);
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    recipesQuery = recipesQuery.Where(recipe =>
                        (recipe.naziv ?? string.Empty).Contains(search)
                        || (recipe.opis ?? string.Empty).Contains(search));
                }
            }
            else
            {
                recipesQuery = recipesQuery.Where(recipe =>
                    !recipe.isDeleted
                    && recipe.receptKuharice.Any(join =>
                        !join.isDeleted
                        && join.kuharica != null
                        && join.kuharica.userId == userId
                        && !join.kuharica.isDeleted));
            }

            if (tezina.HasValue)
            {
                recipesQuery = recipesQuery.Where(recipe => recipe.tezina == tezina.Value);
            }

            if (minVrijeme.HasValue)
            {
                recipesQuery = recipesQuery.Where(recipe => recipe.vrijemeKuhanja >= minVrijeme.Value);
            }

            if (maxVrijeme.HasValue)
            {
                recipesQuery = recipesQuery.Where(recipe => recipe.vrijemeKuhanja <= maxVrijeme.Value);
            }

            if (minPorcija.HasValue)
            {
                recipesQuery = recipesQuery.Where(recipe => recipe.brojPorcija >= minPorcija.Value);
            }

            if (maxPorcija.HasValue)
            {
                recipesQuery = recipesQuery.Where(recipe => recipe.brojPorcija <= maxPorcija.Value);
            }

            if (odDatuma.HasValue)
            {
                var fromDate = odDatuma.Value.Date;
                recipesQuery = recipesQuery.Where(recipe => recipe.kreirano >= fromDate);
            }

            if (doDatuma.HasValue)
            {
                var toDate = doDatuma.Value.Date;
                recipesQuery = recipesQuery.Where(recipe => recipe.kreirano <= toDate);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                recipesQuery = recipesQuery.Where(recipe =>
                    (recipe.naziv ?? string.Empty).Contains(search)
                    || (recipe.opis ?? string.Empty).Contains(search));
            }

            var sortDescending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            recipesQuery = sortBy switch
            {
                "naziv" => sortDescending
                    ? recipesQuery.OrderByDescending(recipe => recipe.naziv)
                    : recipesQuery.OrderBy(recipe => recipe.naziv),
                "vrijemeKuhanja" => sortDescending
                    ? recipesQuery.OrderByDescending(recipe => recipe.vrijemeKuhanja)
                    : recipesQuery.OrderBy(recipe => recipe.vrijemeKuhanja),
                "brojPorcija" => sortDescending
                    ? recipesQuery.OrderByDescending(recipe => recipe.brojPorcija)
                    : recipesQuery.OrderBy(recipe => recipe.brojPorcija),
                "kreirano" => sortDescending
                    ? recipesQuery.OrderByDescending(recipe => recipe.kreirano)
                    : recipesQuery.OrderBy(recipe => recipe.kreirano),
                _ => recipesQuery.OrderBy(recipe => recipe.id)
            };

            var recipes = recipesQuery.AsNoTracking().ToList();
            if (isAdmin)
            {
                var adminDtos = recipes.Select(recipe => recipe.ToDto()).ToList();
                return Ok(ApiResponse<List<ReceptDto>>.Ok(adminDtos));
            }

            var publicDtos = recipes.Select(recipe => recipe.ToPublicDto()).ToList();
            return Ok(ApiResponse<List<ReceptPublicDto>>.Ok(publicDtos));
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            if (!TryGetSession(out var userId, out var isAdmin, out var error))
            {
                return error!;
            }

            IQueryable<Recept> recipeQuery = _dbContext.Recepti.AsNoTracking();
            if (isAdmin)
            {
                recipeQuery = recipeQuery.Where(currentRecipe => currentRecipe.id == id);
            }
            else
            {
                recipeQuery = recipeQuery.Where(currentRecipe => currentRecipe.id == id
                    && !currentRecipe.isDeleted
                    && currentRecipe.receptKuharice.Any(join =>
                        !join.isDeleted
                        && join.kuharica != null
                        && join.kuharica.userId == userId
                        && !join.kuharica.isDeleted));
            }

            var recipe = recipeQuery.FirstOrDefault();
            if (recipe is null)
            {
                return ApiNotFound("Recept nije pronadjen.");
            }

            if (isAdmin)
            {
                return Ok(ApiResponse<ReceptDto>.Ok(recipe.ToDto()));
            }

            return Ok(ApiResponse<ReceptPublicDto>.Ok(recipe.ToPublicDto()));
        }

        [HttpPost]
        public IActionResult Create([FromBody] ReceptFormDto recipe)
        {
            if (!TryGetSession(out var userId, out var isAdmin, out var error))
            {
                return error!;
            }

            if (!ModelState.IsValid)
            {
                return ApiBadRequest("Provjerite unesene podatke.");
            }

            var newRecipe = new Recept
            {
                naziv = recipe.naziv,
                opis = recipe.opis,
                vrijemeKuhanja = recipe.vrijemeKuhanja,
                tezina = recipe.tezina,
                brojPorcija = recipe.brojPorcija,
                kreirano = DateTime.Now,
                isDeleted = false
            };

            _dbContext.Recepti.Add(newRecipe);

            if (!isAdmin)
            {
                var cookbookId = _dbContext.Kuharice
                    .Where(cookbook => cookbook.userId == userId && !cookbook.isDeleted)
                    .Select(cookbook => cookbook.id)
                    .FirstOrDefault();
                if (cookbookId == 0)
                {
                    return ApiBadRequest("Kuharica nije pronadjena.", "COOKBOOK_NOT_FOUND");
                }

                _dbContext.ReceptKuharice.Add(new ReceptKuharica
                {
                    recept = newRecipe,
                    kuharicaId = cookbookId
                });
            }

            _dbContext.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = newRecipe.id }, ApiResponse<ReceptDto>.Ok(newRecipe.ToDto()));
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] ReceptFormDto input)
        {
            if (!TryGetSession(out var userId, out var isAdmin, out var error))
            {
                return error!;
            }

            if (!ModelState.IsValid)
            {
                return ApiBadRequest("Provjerite unesene podatke.");
            }

            var recipeQuery = _dbContext.Recepti.AsQueryable();
            if (!isAdmin)
            {
                recipeQuery = recipeQuery.Where(currentRecipe => currentRecipe.id == id
                    && !currentRecipe.isDeleted
                    && currentRecipe.receptKuharice.Any(join =>
                        !join.isDeleted
                        && join.kuharica != null
                        && join.kuharica.userId == userId
                        && !join.kuharica.isDeleted));
            }
            else
            {
                recipeQuery = recipeQuery.Where(currentRecipe => currentRecipe.id == id);
            }

            var recipe = recipeQuery.FirstOrDefault();
            if (recipe is null)
            {
                return ApiNotFound("Recept nije pronadjen.");
            }

            recipe.naziv = input.naziv;
            recipe.opis = input.opis;
            recipe.vrijemeKuhanja = input.vrijemeKuhanja;
            recipe.tezina = input.tezina;
            recipe.brojPorcija = input.brojPorcija;

            _dbContext.SaveChanges();

            if (isAdmin)
            {
                return Ok(ApiResponse<ReceptDto>.Ok(recipe.ToDto()));
            }

            return Ok(ApiResponse<ReceptPublicDto>.Ok(recipe.ToPublicDto()));
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            if (!TryGetSession(out var userId, out var isAdmin, out var error))
            {
                return error!;
            }

            var recipeQuery = _dbContext.Recepti
                .Include(currentRecipe => currentRecipe.koraci)
                .Include(currentRecipe => currentRecipe.receptKuharice)
                .AsQueryable();
            if (!isAdmin)
            {
                recipeQuery = recipeQuery.Where(currentRecipe => currentRecipe.id == id
                    && !currentRecipe.isDeleted
                    && currentRecipe.receptKuharice.Any(join =>
                        !join.isDeleted
                        && join.kuharica != null
                        && join.kuharica.userId == userId
                        && !join.kuharica.isDeleted));
            }
            else
            {
                recipeQuery = recipeQuery.Where(currentRecipe => currentRecipe.id == id);
            }

            var recipe = recipeQuery.FirstOrDefault();
            if (recipe is null)
            {
                return ApiNotFound("Recept nije pronadjen.");
            }

            recipe.isDeleted = true;
            foreach (var step in recipe.koraci)
            {
                step.isDeleted = true;
            }

            foreach (var join in recipe.receptKuharice)
            {
                join.isDeleted = true;
            }

            _dbContext.SaveChanges();

            return Ok(ApiResponse<ReceptDto>.Ok(recipe.ToDto()));
        }

        [HttpPost("{id:int}/restore")]
        public IActionResult Restore(int id)
        {
            if (!TryGetSession(out _, out var isAdmin, out var error))
            {
                return error!;
            }

            if (!isAdmin)
            {
                return ApiForbidden("Samo admin moze vratiti recept.");
            }

            var recipe = _dbContext.Recepti.FirstOrDefault(currentRecipe => currentRecipe.id == id);
            if (recipe is null)
            {
                return ApiNotFound("Recept nije pronadjen.");
            }

            recipe.isDeleted = false;
            _dbContext.SaveChanges();

            return Ok(ApiResponse<ReceptDto>.Ok(recipe.ToDto()));
        }
    }
}
