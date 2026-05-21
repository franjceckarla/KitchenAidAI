using KitchenAidAI.Data;
using KitchenAidAI.Filters;
using KitchenAidAI.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace KitchenAidAI.Controllers
{
    [RequireSession]
    public class FriziderController : Controller
    {
        private readonly KitchenAidDbContext _dbContext;

        public FriziderController(KitchenAidDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index(int? userId)
        {
            if (!userId.HasValue)
            {
                TempData["Warning"] = "Za otvaranje frižidera prvo odaberite korisnika.";
                return RedirectToAction("Index", "Korisnici");
            }

            var isAdmin = AuthSession.IsAdmin(HttpContext);
            var currentUserId = AuthSession.GetUserId(HttpContext);

            var targetUserId = userId.Value;
            if (!isAdmin && currentUserId != targetUserId)
            {
                return NotFound();
            }

            var user = _dbContext.Users
                .Include(currentUser => currentUser.frizider)
                .ThenInclude(fridge => fridge!.namirnice)
                .AsNoTracking()
                .FirstOrDefault(currentUser => currentUser.id == targetUserId && (isAdmin || !currentUser.isDeleted));
            if (user is null)
            {
                return NotFound();
            }

            if (user.frizider is null || (!isAdmin && user.frizider.isDeleted))
            {
                TempData["Warning"] = "Korisnik još nema kreiran frižider.";
                return RedirectToAction("Index", "Korisnici");
            }

            if (!isAdmin && user.frizider?.namirnice is not null)
            {
                user.frizider.namirnice = user.frizider.namirnice
                    .Where(item => !item.isDeleted)
                    .ToList();
            }

            ViewBag.UserName = user.username;
            ViewBag.UserId = user.id;
            ViewBag.IsAdmin = isAdmin;
            return View(user.frizider);
        }

        public IActionResult Search(int userId, string? search)
        {
            var isAdmin = AuthSession.IsAdmin(HttpContext);
            var currentUserId = AuthSession.GetUserId(HttpContext);
            if (!isAdmin && currentUserId != userId)
            {
                return NotFound();
            }

            var fridge = _dbContext.Frizideri
                .Include(currentFridge => currentFridge.namirnice)
                .AsNoTracking()
                .FirstOrDefault(currentFridge => currentFridge.userId == userId && (isAdmin || !currentFridge.isDeleted));
            if (fridge is null)
            {
                return NotFound();
            }

            var items = fridge.namirnice.AsQueryable();
            if (!isAdmin)
            {
                items = items.Where(item => !item.isDeleted);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                items = items.Where(item => (item.naziv ?? string.Empty).Contains(search));
            }

            ViewBag.IsAdmin = isAdmin;
            return PartialView("_FridgeItems", items.ToList());
        }
    }
}
