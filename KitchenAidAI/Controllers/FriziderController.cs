using KitchenAidAI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace KitchenAidAI.Controllers
{
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

            var targetUserId = userId.Value;
            var user = _dbContext.Users
                .Include(currentUser => currentUser.frizider)
                .ThenInclude(fridge => fridge!.namirnice)
                .AsNoTracking()
                .FirstOrDefault(currentUser => currentUser.id == targetUserId);
            if (user is null)
            {
                return NotFound();
            }

            if (user.frizider is null)
            {
                TempData["Warning"] = "Korisnik još nema kreiran frižider.";
                return RedirectToAction("Index", "Korisnici");
            }

            ViewBag.UserName = user.username;
            ViewBag.UserId = user.id;
            return View(user.frizider);
        }
    }
}
