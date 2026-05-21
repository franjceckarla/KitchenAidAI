using KitchenAidAI.Data;
using KitchenAidAI.Filters;
using KitchenAidAI.Helpers;
using KitchenAidAI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace KitchenAidAI.Controllers
{
    [RequireSession]
    public class ReceptiController : Controller
    {
        private readonly KitchenAidDbContext _dbContext;

        public ReceptiController(KitchenAidDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index(string? search)
        {
            var isAdmin = AuthSession.IsAdmin(HttpContext);
            var currentUserId = AuthSession.GetUserId(HttpContext);

            IQueryable<Recept> recipesQuery = _dbContext.Recepti;
            if (isAdmin)
            {
                if (!string.IsNullOrWhiteSpace(search))
                {
                    recipesQuery = recipesQuery.Where(recipe =>
                        (recipe.naziv ?? string.Empty).Contains(search)
                        || (recipe.opis ?? string.Empty).Contains(search));
                }
            }
            else if (currentUserId.HasValue)
            {
                recipesQuery = recipesQuery.Where(recipe =>
                    !recipe.isDeleted
                    && recipe.receptKuharice.Any(join =>
                        !join.isDeleted
                        && join.kuharica != null
                        && join.kuharica.userId == currentUserId.Value
                        && !join.kuharica.isDeleted));

                if (!string.IsNullOrWhiteSpace(search))
                {
                    recipesQuery = recipesQuery.Where(recipe =>
                        (recipe.naziv ?? string.Empty).Contains(search)
                        || (recipe.opis ?? string.Empty).Contains(search));
                }
            }

            var recipes = recipesQuery.AsNoTracking().ToList();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_RecipeCards", recipes);
            }

            return View(recipes);
        }

        public IActionResult Details(int id)
        {
            var isAdmin = AuthSession.IsAdmin(HttpContext);
            var currentUserId = AuthSession.GetUserId(HttpContext);

            IQueryable<Recept> recipeQuery = _dbContext.Recepti.AsNoTracking();
            if (isAdmin)
            {
                recipeQuery = recipeQuery.Where(currentRecipe => currentRecipe.id == id);
            }
            else if (currentUserId.HasValue)
            {
                recipeQuery = recipeQuery.Where(currentRecipe => currentRecipe.id == id
                    && !currentRecipe.isDeleted
                    && currentRecipe.receptKuharice.Any(join =>
                        !join.isDeleted
                        && join.kuharica != null
                        && join.kuharica.userId == currentUserId.Value
                        && !join.kuharica.isDeleted));
            }

            var recipe = recipeQuery.FirstOrDefault();
            if (recipe is null)
            {
                return NotFound();
            }

            return View(recipe);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Recept());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Recept recipe)
        {
            var isAdmin = AuthSession.IsAdmin(HttpContext);
            var currentUserId = AuthSession.GetUserId(HttpContext);

            if (!ModelState.IsValid)
            {
                return View(recipe);
            }

            recipe.kreirano = DateTime.Now;
            recipe.isDeleted = false;
            _dbContext.Recepti.Add(recipe);

            if (!isAdmin && currentUserId.HasValue)
            {
                var cookbookId = _dbContext.Kuharice
                    .Where(cookbook => cookbook.userId == currentUserId.Value && !cookbook.isDeleted)
                    .Select(cookbook => cookbook.id)
                    .FirstOrDefault();
                if (cookbookId == 0)
                {
                    return NotFound();
                }

                _dbContext.ReceptKuharice.Add(new ReceptKuharica
                {
                    recept = recipe,
                    kuharicaId = cookbookId
                });
            }

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var isAdmin = AuthSession.IsAdmin(HttpContext);
            var currentUserId = AuthSession.GetUserId(HttpContext);

            var recipeQuery = _dbContext.Recepti.AsQueryable();
            if (!isAdmin && currentUserId.HasValue)
            {
                recipeQuery = recipeQuery.Where(currentRecipe => currentRecipe.id == id
                    && !currentRecipe.isDeleted
                    && currentRecipe.receptKuharice.Any(join =>
                        !join.isDeleted
                        && join.kuharica != null
                        && join.kuharica.userId == currentUserId.Value
                        && !join.kuharica.isDeleted));
            }
            else
            {
                recipeQuery = recipeQuery.Where(currentRecipe => currentRecipe.id == id);
            }

            var recipe = recipeQuery.FirstOrDefault();
            if (recipe is null)
            {
                return NotFound();
            }

            return View(recipe);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Recept input)
        {
            var isAdmin = AuthSession.IsAdmin(HttpContext);
            var currentUserId = AuthSession.GetUserId(HttpContext);

            var recipeQuery = _dbContext.Recepti.AsQueryable();
            if (!isAdmin && currentUserId.HasValue)
            {
                recipeQuery = recipeQuery.Where(currentRecipe => currentRecipe.id == id
                    && !currentRecipe.isDeleted
                    && currentRecipe.receptKuharice.Any(join =>
                        !join.isDeleted
                        && join.kuharica != null
                        && join.kuharica.userId == currentUserId.Value
                        && !join.kuharica.isDeleted));
            }
            else
            {
                recipeQuery = recipeQuery.Where(currentRecipe => currentRecipe.id == id);
            }

            var recipe = recipeQuery.FirstOrDefault();
            if (recipe is null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(input);
            }

            recipe.naziv = input.naziv;
            recipe.opis = input.opis;
            recipe.vrijemeKuhanja = input.vrijemeKuhanja;
            recipe.tezina = input.tezina;
            recipe.brojPorcija = input.brojPorcija;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var isAdmin = AuthSession.IsAdmin(HttpContext);
            var currentUserId = AuthSession.GetUserId(HttpContext);

            var recipeQuery = _dbContext.Recepti.AsQueryable();
            if (!isAdmin && currentUserId.HasValue)
            {
                recipeQuery = recipeQuery.Where(currentRecipe => currentRecipe.id == id
                    && !currentRecipe.isDeleted
                    && currentRecipe.receptKuharice.Any(join =>
                        !join.isDeleted
                        && join.kuharica != null
                        && join.kuharica.userId == currentUserId.Value
                        && !join.kuharica.isDeleted));
            }
            else
            {
                recipeQuery = recipeQuery.Where(currentRecipe => currentRecipe.id == id);
            }

            var recipe = recipeQuery.FirstOrDefault();
            if (recipe is null)
            {
                return NotFound();
            }

            return View(recipe);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var isAdmin = AuthSession.IsAdmin(HttpContext);
            var currentUserId = AuthSession.GetUserId(HttpContext);

            var recipeQuery = _dbContext.Recepti
                .Include(currentRecipe => currentRecipe.koraci)
                .Include(currentRecipe => currentRecipe.receptKuharice)
                .AsQueryable();
            if (!isAdmin && currentUserId.HasValue)
            {
                recipeQuery = recipeQuery.Where(currentRecipe => currentRecipe.id == id
                    && !currentRecipe.isDeleted
                    && currentRecipe.receptKuharice.Any(join =>
                        !join.isDeleted
                        && join.kuharica != null
                        && join.kuharica.userId == currentUserId.Value
                        && !join.kuharica.isDeleted));
            }
            else
            {
                recipeQuery = recipeQuery.Where(currentRecipe => currentRecipe.id == id);
            }

            var recipe = recipeQuery.FirstOrDefault();
            if (recipe is null)
            {
                return NotFound();
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

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Restore(int id)
        {
            if (!AuthSession.IsAdmin(HttpContext))
            {
                return Forbid();
            }

            var recipe = _dbContext.Recepti.FirstOrDefault(currentRecipe => currentRecipe.id == id);
            if (recipe is null)
            {
                return NotFound();
            }

            recipe.isDeleted = false;
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
