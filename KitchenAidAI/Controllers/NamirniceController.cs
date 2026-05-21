using KitchenAidAI.Data;
using KitchenAidAI.Filters;
using KitchenAidAI.Helpers;
using KitchenAidAI.Models;
using KitchenAidAI.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KitchenAidAI.Controllers
{
    [RequireSession]
    public class NamirniceController : Controller
    {
        private readonly KitchenAidDbContext _dbContext;

        public NamirniceController(KitchenAidDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult Create(int friziderId)
        {
            var isAdmin = AuthSession.IsAdmin(HttpContext);
            var currentUserId = AuthSession.GetUserId(HttpContext);

            var fridge = _dbContext.Frizideri.AsNoTracking().FirstOrDefault(currentFridge => currentFridge.id == friziderId && (isAdmin || !currentFridge.isDeleted));
            if (fridge is null)
            {
                return NotFound();
            }

            if (!isAdmin && fridge.userId != currentUserId)
            {
                return NotFound();
            }

            ViewBag.FriziderId = friziderId;
            ViewBag.UserId = fridge.userId;
            PopulateDropdowns();
            return View(new Namirnica { friziderId = friziderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Namirnica namirnica)
        {
            var isAdmin = AuthSession.IsAdmin(HttpContext);
            var currentUserId = AuthSession.GetUserId(HttpContext);

            if (!ModelState.IsValid)
            {
                ViewBag.FriziderId = namirnica.friziderId;
                ViewBag.UserId = _dbContext.Frizideri.Where(currentFridge => currentFridge.id == namirnica.friziderId).Select(currentFridge => currentFridge.userId).FirstOrDefault();
                PopulateDropdowns();
                return View(namirnica);
            }

            var fridge = _dbContext.Frizideri.FirstOrDefault(currentFridge => currentFridge.id == namirnica.friziderId && (isAdmin || !currentFridge.isDeleted));
            if (fridge is null)
            {
                return NotFound();
            }

            if (!isAdmin && fridge.userId != currentUserId)
            {
                return NotFound();
            }

            _dbContext.Namirnice.Add(namirnica);
            _dbContext.SaveChanges();

            return RedirectToAction("Index", "Frizider", new { userId = _dbContext.Frizideri.Where(currentFridge => currentFridge.id == namirnica.friziderId).Select(currentFridge => currentFridge.userId).FirstOrDefault() });
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var isAdmin = AuthSession.IsAdmin(HttpContext);
            var currentUserId = AuthSession.GetUserId(HttpContext);

            var namirnica = _dbContext.Namirnice
                .Include(currentItem => currentItem.frizider)
                .FirstOrDefault(currentItem => currentItem.id == id && (isAdmin || !currentItem.isDeleted));
            if (namirnica is null)
            {
                return NotFound();
            }

            if (!isAdmin && namirnica.frizider?.userId != currentUserId)
            {
                return NotFound();
            }

            ViewBag.FriziderId = namirnica.friziderId;
            ViewBag.UserId = _dbContext.Frizideri.Where(currentFridge => currentFridge.id == namirnica.friziderId).Select(currentFridge => currentFridge.userId).FirstOrDefault();
            PopulateDropdowns();
            return View(namirnica);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Namirnica input)
        {
            var isAdmin = AuthSession.IsAdmin(HttpContext);
            var currentUserId = AuthSession.GetUserId(HttpContext);

            var namirnica = _dbContext.Namirnice
                .Include(currentItem => currentItem.frizider)
                .FirstOrDefault(currentItem => currentItem.id == id && (isAdmin || !currentItem.isDeleted));
            if (namirnica is null)
            {
                return NotFound();
            }

            if (!isAdmin && namirnica.frizider?.userId != currentUserId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.FriziderId = input.friziderId;
                ViewBag.UserId = _dbContext.Frizideri.Where(currentFridge => currentFridge.id == input.friziderId).Select(currentFridge => currentFridge.userId).FirstOrDefault();
                PopulateDropdowns();
                return View(input);
            }

            namirnica.naziv = input.naziv;
            namirnica.kategorija = input.kategorija;
            namirnica.mjera = input.mjera;
            namirnica.kolicinaUFrizideru = input.kolicinaUFrizideru;
            namirnica.friziderId = input.friziderId;

            _dbContext.SaveChanges();

            return RedirectToAction("Index", "Frizider", new { userId = _dbContext.Frizideri.Where(currentFridge => currentFridge.id == input.friziderId).Select(currentFridge => currentFridge.userId).FirstOrDefault() });
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var isAdmin = AuthSession.IsAdmin(HttpContext);
            var currentUserId = AuthSession.GetUserId(HttpContext);

            var namirnica = _dbContext.Namirnice
                .Include(currentItem => currentItem.frizider)
                .FirstOrDefault(currentItem => currentItem.id == id && (isAdmin || !currentItem.isDeleted));
            if (namirnica is null)
            {
                return NotFound();
            }

            if (!isAdmin && namirnica.frizider?.userId != currentUserId)
            {
                return NotFound();
            }
            
            ViewBag.UserId = _dbContext.Frizideri.Where(currentFridge => currentFridge.id == namirnica.friziderId).Select(currentFridge => currentFridge.userId).FirstOrDefault();
            return View(namirnica);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var isAdmin = AuthSession.IsAdmin(HttpContext);
            var currentUserId = AuthSession.GetUserId(HttpContext);

            var namirnica = _dbContext.Namirnice
                .Include(currentItem => currentItem.frizider)
                .FirstOrDefault(currentItem => currentItem.id == id && (isAdmin || !currentItem.isDeleted));
            if (namirnica is null)
            {
                return NotFound();
            }

            if (!isAdmin && namirnica.frizider?.userId != currentUserId)
            {
                return NotFound();
            }

            var userId = _dbContext.Frizideri.Where(currentFridge => currentFridge.id == namirnica.friziderId).Select(currentFridge => currentFridge.userId).FirstOrDefault();
            namirnica.isDeleted = true;
            _dbContext.SaveChanges();

            return RedirectToAction("Index", "Frizider", new { userId = userId });
        }

        private void PopulateDropdowns()
        {
            ViewBag.Kategorije = new SelectList(Enum.GetValues<KategorijaNamirnice>().Select(value => new { Value = value, Text = value.ToString() }), "Value", "Text");
            ViewBag.Mjere = new SelectList(Enum.GetValues<Mjera>().Select(value => new { Value = value, Text = value.ToString() }), "Value", "Text");
        }
    }
}