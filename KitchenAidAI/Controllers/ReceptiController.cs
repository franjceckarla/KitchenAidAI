using KitchenAidAI.Data;
using KitchenAidAI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace KitchenAidAI.Controllers
{
    public class ReceptiController : Controller
    {
        private readonly KitchenAidDbContext _dbContext;

        public ReceptiController(KitchenAidDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var recipes = _dbContext.Recepti
                .AsNoTracking()
                .ToList();
            return View(recipes);
        }

        public IActionResult Details(int id)
        {
            var recipe = _dbContext.Recepti
                .AsNoTracking()
                .FirstOrDefault(currentRecipe => currentRecipe.id == id);
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
            if (!ModelState.IsValid)
            {
                return View(recipe);
            }

            recipe.datumKreiranja = DateTime.Now;
            _dbContext.Recepti.Add(recipe);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var recipe = _dbContext.Recepti.FirstOrDefault(currentRecipe => currentRecipe.id == id);
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
            var recipe = _dbContext.Recepti.FirstOrDefault(currentRecipe => currentRecipe.id == id);
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
            var recipe = _dbContext.Recepti.FirstOrDefault(currentRecipe => currentRecipe.id == id);
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
            var recipe = _dbContext.Recepti.FirstOrDefault(currentRecipe => currentRecipe.id == id);
            if (recipe is null)
            {
                return NotFound();
            }

            _dbContext.Recepti.Remove(recipe);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
