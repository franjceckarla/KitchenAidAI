using KitchenAidAI.Data;
using KitchenAidAI.Filters;
using KitchenAidAI.Helpers;
using KitchenAidAI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace KitchenAidAI.Controllers
{
    [RequireSession]
    public class KorisniciController : Controller
    {
        private readonly KitchenAidDbContext _dbContext;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public KorisniciController(KitchenAidDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index(string? search)
        {
            var isAdmin = AuthSession.IsAdmin(HttpContext);
            var currentUserId = AuthSession.GetUserId(HttpContext);

            var usersQuery = _dbContext.Users.AsQueryable();
            if (isAdmin)
            {
                usersQuery = usersQuery.Where(user => !user.isAdmin);
                if (!string.IsNullOrWhiteSpace(search))
                {
                    usersQuery = usersQuery.Where(user =>
                        (user.username ?? string.Empty).Contains(search)
                        || (user.email ?? string.Empty).Contains(search));
                }
            }
            else if (currentUserId.HasValue)
            {
                usersQuery = usersQuery.Where(user => user.id == currentUserId.Value && !user.isDeleted);
            }

            var users = usersQuery.AsNoTracking().ToList();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_UserCards", users);
            }

            return View(users);
        }

        public IActionResult Details(int id)
        {
            var isAdmin = AuthSession.IsAdmin(HttpContext);
            var currentUserId = AuthSession.GetUserId(HttpContext);
            if (!isAdmin && currentUserId != id)
            {
                return NotFound();
            }

            var user = _dbContext.Users
                .Include(currentUser => currentUser.frizider)
                .Include(currentUser => currentUser.kuharica)
                .AsNoTracking()
                .FirstOrDefault(currentUser => currentUser.id == id && (isAdmin || !currentUser.isDeleted));
            if (user is null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!AuthSession.IsAdmin(HttpContext))
            {
                return Forbid();
            }

            return View(new KitchenAidAI.Models.ViewModels.RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(KitchenAidAI.Models.ViewModels.RegisterViewModel input)
        {
            if (!AuthSession.IsAdmin(HttpContext))
            {
                return Forbid();
            }

            if (input.SelectedPreferences is null || input.SelectedPreferences.Count == 0)
            {
                ModelState.AddModelError(nameof(KitchenAidAI.Models.ViewModels.RegisterViewModel.SelectedPreferences), "Odaberite barem jednu preferenciju prehrane.");
            }
            else if (input.SelectedPreferences.Count > 1)
            {
                ModelState.AddModelError(nameof(KitchenAidAI.Models.ViewModels.RegisterViewModel.SelectedPreferences), "Odaberite samo jednu preferenciju prehrane.");
            }

            if (!ModelState.IsValid)
            {
                return View(input);
            }

            var usernameExists = _dbContext.Users.Any(user => user.username != null
                && input.Username != null
                && user.username.ToLower() == input.Username.ToLower());
            if (usernameExists)
            {
                ModelState.AddModelError(nameof(KitchenAidAI.Models.ViewModels.RegisterViewModel.Username), "Korisnicko ime je vec zauzeto.");
                return View(input);
            }

            var emailExists = _dbContext.Users.Any(user => user.email != null
                && input.Email != null
                && user.email.ToLower() == input.Email.ToLower());
            if (emailExists)
            {
                ModelState.AddModelError(nameof(KitchenAidAI.Models.ViewModels.RegisterViewModel.Email), "Email je vec registriran.");
                return View(input);
            }

            var newUser = new User
            {
                username = input.Username,
                ime = input.Ime,
                prezime = input.Prezime,
                datumRodenja = input.DatumRodenja?.Date,
                zemlja = input.Zemlja,
                email = input.Email,
                preferencijaPrehrane = input.SelectedPreferences?.FirstOrDefault() ?? KitchenAidAI.Models.Enums.PreferencijaPrehrane.Omnivorte,
                isAdmin = false,
                frizider = new Frizider(),
                kuharica = new Kuharica { naziv = $"{input.Ime} kuharica" }
            };

            newUser.passwordHash = _passwordHasher.HashPassword(newUser, input.Password ?? string.Empty);
            _dbContext.Users.Add(newUser);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var isAdmin = AuthSession.IsAdmin(HttpContext);
            var currentUserId = AuthSession.GetUserId(HttpContext);
            if (!isAdmin && currentUserId != id)
            {
                return NotFound();
            }

            var user = _dbContext.Users.FirstOrDefault(currentUser => currentUser.id == id);
            if (user is null)
            {
                return NotFound();
            }

            if (!isAdmin && user.isDeleted)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, User input, string? newPassword, bool isAdminCheckbox = false)
        {
            var isAdmin = AuthSession.IsAdmin(HttpContext);
            var currentUserId = AuthSession.GetUserId(HttpContext);
            if (!isAdmin && currentUserId != id)
            {
                return NotFound();
            }

            var user = _dbContext.Users.FirstOrDefault(currentUser => currentUser.id == id);
            if (user is null)
            {
                return NotFound();
            }

            if (!isAdmin && user.isDeleted)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(input);
            }

            var usernameChanged = !string.Equals(user.username, input.username, StringComparison.Ordinal);
            user.username = input.username;
            user.email = input.email;
            user.preferencijaPrehrane = input.preferencijaPrehrane;

            if (isAdmin)
            {
                user.isAdmin = isAdminCheckbox;
                if (!string.IsNullOrWhiteSpace(newPassword))
                {
                    user.passwordHash = _passwordHasher.HashPassword(user, newPassword);
                }
            }

            if (usernameChanged && string.IsNullOrWhiteSpace(newPassword))
            {
                user.passwordHash = _passwordHasher.HashPassword(user, user.username ?? string.Empty);
            }

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (!AuthSession.IsAdmin(HttpContext))
            {
                return Forbid();
            }

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
            if (!AuthSession.IsAdmin(HttpContext))
            {
                return Forbid();
            }

            var user = _dbContext.Users.FirstOrDefault(currentUser => currentUser.id == id);
            if (user is null)
            {
                return NotFound();
            }

            var fullUser = _dbContext.Users
                .Include(currentUser => currentUser.frizider)
                .ThenInclude(fridge => fridge!.namirnice)
                .Include(currentUser => currentUser.kuharica)
                .ThenInclude(cookbook => cookbook!.receptKuharice)
                .Include(currentUser => currentUser.chatPoruke)
                .FirstOrDefault(currentUser => currentUser.id == id);
            if (fullUser is null)
            {
                return NotFound();
            }

            fullUser.isDeleted = true;
            if (fullUser.frizider is not null)
            {
                fullUser.frizider.isDeleted = true;
                foreach (var item in fullUser.frizider.namirnice)
                {
                    item.isDeleted = true;
                }
            }

            if (fullUser.kuharica is not null)
            {
                fullUser.kuharica.isDeleted = true;
                foreach (var join in fullUser.kuharica.receptKuharice)
                {
                    join.isDeleted = true;
                }
            }

            foreach (var message in fullUser.chatPoruke)
            {
                message.isDeleted = true;
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

            var user = _dbContext.Users
                .Include(currentUser => currentUser.frizider)
                .ThenInclude(fridge => fridge!.namirnice)
                .Include(currentUser => currentUser.kuharica)
                .ThenInclude(cookbook => cookbook!.receptKuharice)
                .Include(currentUser => currentUser.chatPoruke)
                .FirstOrDefault(currentUser => currentUser.id == id);
                
            if (user is null)
            {
                return NotFound();
            }

            user.isDeleted = false;
            
            if (user.frizider is not null)
            {
                user.frizider.isDeleted = false;
                foreach (var item in user.frizider.namirnice)
                {
                    item.isDeleted = false;
                }
            }

            if (user.kuharica is not null)
            {
                user.kuharica.isDeleted = false;
                foreach (var join in user.kuharica.receptKuharice)
                {
                    join.isDeleted = false;
                }
            }

            foreach (var message in user.chatPoruke)
            {
                message.isDeleted = false;
            }

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
