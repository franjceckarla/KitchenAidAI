using KitchenAidAI.Data;
using KitchenAidAI.Helpers;
using KitchenAidAI.Models.Enums;
using KitchenAidAI.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KitchenAidAI.Controllers
{
    public class AuthController : Controller
    {
        private readonly KitchenAidDbContext _dbContext;
        private readonly PasswordHasher<Models.User> _passwordHasher = new();

        public AuthController(KitchenAidDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            if (AuthSession.GetUserId(HttpContext).HasValue)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new LoginViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel input)
        {
            if (!ModelState.IsValid)
            {
                return View(input);
            }

            var user = _dbContext.Users
                .AsNoTracking()
                .FirstOrDefault(currentUser => currentUser.username != null
                    && input.Username != null
                    && currentUser.username.ToLower() == input.Username.ToLower());
            if (user is null || user.isDeleted)
            {
                ModelState.AddModelError(string.Empty, "Neispravno korisničko ime ili lozinka.");
                return View(input);
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.passwordHash ?? string.Empty, input.Password ?? string.Empty);
            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Neispravno korisničko ime ili lozinka.");
                return View(input);
            }

            AuthSession.SignIn(HttpContext, user);
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            if (AuthSession.GetUserId(HttpContext).HasValue)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegisterViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel input)
        {
            if (input.SelectedPreferences is null || input.SelectedPreferences.Count == 0)
            {
                ModelState.AddModelError(nameof(RegisterViewModel.SelectedPreferences), "Odaberite barem jednu preferenciju prehrane.");
            }
            else if (input.SelectedPreferences.Count > 1)
            {
                ModelState.AddModelError(nameof(RegisterViewModel.SelectedPreferences), "Odaberite samo jednu preferenciju prehrane.");
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
                ModelState.AddModelError(nameof(RegisterViewModel.Username), "Korisnicko ime je vec zauzeto.");
                return View(input);
            }

            var emailExists = _dbContext.Users.Any(user => user.email != null
                && input.Email != null
                && user.email.ToLower() == input.Email.ToLower());
            if (emailExists)
            {
                ModelState.AddModelError(nameof(RegisterViewModel.Email), "Email je vec registriran.");
                return View(input);
            }

            var newUser = new Models.User
            {
                username = input.Username,
                ime = input.Ime,
                prezime = input.Prezime,
                datumRodenja = input.DatumRodenja?.Date,
                zemlja = input.Zemlja,
                email = input.Email,
                preferencijaPrehrane = input.SelectedPreferences?.FirstOrDefault() ?? PreferencijaPrehrane.Omnivorte,
                isAdmin = false,
                frizider = new Models.Frizider(),
                kuharica = new Models.Kuharica { naziv = $"{input.Ime} kuharica" }
            };

            newUser.passwordHash = _passwordHasher.HashPassword(newUser, input.Password ?? string.Empty);
            _dbContext.Users.Add(newUser);
            _dbContext.SaveChanges();

            AuthSession.SignIn(HttpContext, newUser);
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult CountrySearch(string? term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return Json(Array.Empty<string>());
            }

            var query = term.Trim().ToLower();
            var matches = _dbContext.Countries
                .AsNoTracking()
                .Where(country => country.naziv != null && country.naziv.ToLower().Contains(query))
                .OrderBy(country => country.naziv)
                .Take(10)
                .Select(country => country.naziv ?? string.Empty)
                .ToList();

            return Json(matches);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult UserFieldSearch(string? term, string? field)
        {
            if (string.IsNullOrWhiteSpace(term) || string.IsNullOrWhiteSpace(field))
            {
                return Json(Array.Empty<string>());
            }

            var query = term.Trim().ToLower();
            var normalizedField = field.Trim().ToLower();
            var users = _dbContext.Users.AsNoTracking().Where(user => !user.isDeleted && !user.isAdmin);

            IQueryable<string?> values = normalizedField switch
            {
                "ime" => users.Select(user => user.ime),
                "prezime" => users.Select(user => user.prezime),
                "email" => users.Select(user => user.email),
                _ => Enumerable.Empty<string?>().AsQueryable()
            };

            var matches = values
                .Where(value => value != null && value.ToLower().Contains(query))
                .Select(value => value ?? string.Empty)
                .Distinct()
                .OrderBy(value => value)
                .Take(10)
                .ToList();

            return Json(matches);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            AuthSession.SignOut(HttpContext);
            return RedirectToAction("Login", "Auth");
        }
    }
}
