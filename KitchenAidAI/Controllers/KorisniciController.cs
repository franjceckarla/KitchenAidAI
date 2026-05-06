using KitchenAidAI.Data;
using KitchenAidAI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace KitchenAidAI.Controllers
{
    public class KorisniciController : Controller
    {
        private readonly KitchenAidDbContext _dbContext;

        public KorisniciController(KitchenAidDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var users = _dbContext.Users
                .AsNoTracking()
                .ToList();
            return View(users);
        }

        public IActionResult Details(int id)
        {
            var user = _dbContext.Users
                .Include(currentUser => currentUser.frizider)
                .Include(currentUser => currentUser.kuharica)
                .AsNoTracking()
                .FirstOrDefault(currentUser => currentUser.id == id);
            if (user is null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(User user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            user.kreirano = DateTime.Now;
            user.frizider = new Frizider();
            user.kuharica = new Kuharica();

            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var user = _dbContext.Users.FirstOrDefault(currentUser => currentUser.id == id);
            if (user is null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, User input)
        {
            var user = _dbContext.Users.FirstOrDefault(currentUser => currentUser.id == id);
            if (user is null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(input);
            }

            user.username = input.username;
            user.email = input.email;
            user.preferencijaPrehrane = input.preferencijaPrehrane;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var user = _dbContext.Users.FirstOrDefault(currentUser => currentUser.id == id);
            if (user is null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var user = _dbContext.Users.FirstOrDefault(currentUser => currentUser.id == id);
            if (user is null)
            {
                return NotFound();
            }

            _dbContext.Users.Remove(user);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
