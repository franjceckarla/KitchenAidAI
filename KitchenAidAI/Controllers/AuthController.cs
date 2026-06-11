using KitchenAidAI.Data;
using KitchenAidAI.Models;
using KitchenAidAI.Models.Enums;
using KitchenAidAI.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KitchenAidAI.Controllers
{
    public class AuthController : Controller
    {
        private const string LegacyUserIdClaimType = "legacy_user_id";

        private readonly KitchenAidDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IPasswordHasher<AppUser> _appUserPasswordHasher;

        public AuthController(KitchenAidDbContext dbContext, IConfiguration configuration,
            UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<IdentityRole<int>> roleManager,
            IPasswordHasher<AppUser> appUserPasswordHasher)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _appUserPasswordHasher = appUserPasswordHasher;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.GoogleLoginEnabled = IsConfigured("Authentication:Google", "ClientId", "ClientSecret");
            ViewBag.FacebookLoginEnabled = IsConfigured("Authentication:Facebook", "AppId", "AppSecret");
            ViewBag.MicrosoftLoginEnabled = IsConfigured("Authentication:Microsoft", "ClientId", "ClientSecret");

            return View(new LoginViewModel());
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrWhiteSpace(provider))
            {
                return RedirectToAction(nameof(Login));
            }

            if (!IsProviderEnabled(provider))
            {
                TempData["Warning"] = "Odabrani vanjski provider nije konfiguriran.";
                return RedirectToAction(nameof(Login));
            }

            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(ExternalLoginCallback), new { returnUrl })
            };
            return Challenge(properties, provider);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null)
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["Warning"] = "Vanjska prijava nije uspjela.";
                return RedirectToAction(nameof(Login));
            }

            // Try to sign in existing linked user
            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (signInResult.Succeeded)
            {
                var linkedUser = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return LocalRedirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            // If no linked user, create one
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var usernameClaim = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email ?? $"user_{Guid.NewGuid():N}";
            var baseUsername = BuildUsername(usernameClaim, email, info.LoginProvider);
            var uniqueUsername = EnsureUniqueUsername(baseUsername);

            var appUser = new AppUser
            {
                UserName = uniqueUsername,
                Email = email,
                ime = info.Principal.FindFirstValue(ClaimTypes.GivenName),
                prezime = info.Principal.FindFirstValue(ClaimTypes.Surname),
                preferencijaPrehrane = PreferencijaPrehrane.Omnivorte,
                isAdmin = false,
                kreirano = DateTime.Now
            };

            var createResult = await _userManager.CreateAsync(appUser);
            if (createResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(appUser, "User");
                await _userManager.AddLoginAsync(appUser, info);
                await _signInManager.SignInAsync(appUser, isPersistent: false);
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return LocalRedirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            TempData["Warning"] = "Nije moguće dovršiti vanjsku prijavu.";
            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel input)
        {
            if (!ModelState.IsValid)
            {
                return View(input);
            }

            var result = await _signInManager.PasswordSignInAsync(input.Username ?? string.Empty, input.Password ?? string.Empty, isPersistent: false, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                var legacySignInSucceeded = await TryLegacyLoginAsync(input.Username, input.Password);
                if (!legacySignInSucceeded)
                {
                    ModelState.AddModelError(string.Empty, "Neispravno korisničko ime ili lozinka.");
                    return View(input);
                }
            }
            else
            {
                await EnsureLegacyUserClaimByUsernameAsync(input.Username);
            }

            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegisterViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel input)
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

            var usernameExists = await _userManager.FindByNameAsync(input.Username ?? string.Empty) != null;
            if (usernameExists)
            {
                ModelState.AddModelError(nameof(RegisterViewModel.Username), "Korisnicko ime je vec zauzeto.");
                return View(input);
            }

            var emailExists = await _userManager.FindByEmailAsync(input.Email ?? string.Empty) != null;
            if (emailExists)
            {
                ModelState.AddModelError(nameof(RegisterViewModel.Email), "Email je vec registriran.");
                return View(input);
            }

            var appUser = new AppUser
            {
                UserName = input.Username,
                Email = input.Email,
                ime = input.Ime,
                prezime = input.Prezime,
                datumRodenja = input.DatumRodenja?.Date,
                zemlja = input.Zemlja,
                preferencijaPrehrane = input.SelectedPreferences?.FirstOrDefault() ?? PreferencijaPrehrane.Omnivorte,
                isAdmin = false,
                frizider = new Models.Frizider(),
                kuharica = new Models.Kuharica { naziv = $"{input.Ime} kuharica" }
            };

            var result = await _userManager.CreateAsync(appUser, input.Password ?? string.Empty);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(input);
            }

            await _userManager.AddToRoleAsync(appUser, "User");
            await _signInManager.SignInAsync(appUser, isPersistent: false);

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
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Auth");
        }

        private string BuildUsername(string? username, string? email, string provider)
        {
            var candidate = !string.IsNullOrWhiteSpace(username)
                ? username.Trim()
                : !string.IsNullOrWhiteSpace(email)
                    ? email.Split('@')[0]
                    : provider;

            return SanitizeUsername(candidate);
        }

        private string SanitizeUsername(string value)
        {
            var filtered = new string(value.Where(character => char.IsLetterOrDigit(character) || character == '_' || character == '.').ToArray());
            if (string.IsNullOrWhiteSpace(filtered))
            {
                filtered = $"user_{Guid.NewGuid():N}";
            }

            return filtered.Length > 100 ? filtered[..100] : filtered;
        }

        private string EnsureUniqueUsername(string baseUsername)
        {
            var candidate = baseUsername;
            var suffix = 1;

            while (_userManager.Users.Any(user => user.UserName != null && user.UserName.ToLower() == candidate.ToLower()))
            {
                candidate = $"{baseUsername}{suffix}";
                suffix += 1;
            }

            return candidate;
        }

        private static string EnsureExternalEmail(string? email, string provider, string providerKey)
        {
            if (!string.IsNullOrWhiteSpace(email))
            {
                return email;
            }

            var safeProvider = new string(provider.Where(character => char.IsLetterOrDigit(character)).ToArray()).ToLowerInvariant();
            var safeKey = new string(providerKey.Where(character => char.IsLetterOrDigit(character)).ToArray()).ToLowerInvariant();
            return $"{safeProvider}-{safeKey}@external.kitchenaidai";
        }

        private bool IsProviderEnabled(string provider)
        {
            return provider switch
            {
                "Google" => IsConfigured("Authentication:Google", "ClientId", "ClientSecret"),
                "Facebook" => IsConfigured("Authentication:Facebook", "AppId", "AppSecret"),
                "Microsoft" => IsConfigured("Authentication:Microsoft", "ClientId", "ClientSecret"),
                _ => false
            };
        }

        private bool IsConfigured(string sectionPath, string key1, string key2)
        {
            var section = _configuration.GetSection(sectionPath);
            return !string.IsNullOrWhiteSpace(section[key1]) && !string.IsNullOrWhiteSpace(section[key2]);
        }

        private async Task<bool> TryLegacyLoginAsync(string? username, string? password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            var normalizedUsername = username.Trim();
            var legacyUser = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(user =>
                    !user.isDeleted
                    && user.username != null
                    && user.username.ToLower() == normalizedUsername.ToLower());

            if (legacyUser is null || string.IsNullOrWhiteSpace(legacyUser.passwordHash))
            {
                return false;
            }

            var legacyPasswordHasher = new PasswordHasher<User>();
            var verificationResult = legacyPasswordHasher.VerifyHashedPassword(legacyUser, legacyUser.passwordHash, password);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return false;
            }

            var appUser = await _userManager.FindByNameAsync(normalizedUsername);
            if (appUser is null)
            {
                appUser = new AppUser
                {
                    UserName = legacyUser.username,
                    Email = legacyUser.email,
                    ime = legacyUser.ime,
                    prezime = legacyUser.prezime,
                    datumRodenja = legacyUser.datumRodenja,
                    zemlja = legacyUser.zemlja,
                    preferencijaPrehrane = legacyUser.preferencijaPrehrane,
                    isAdmin = legacyUser.isAdmin,
                    isDeleted = legacyUser.isDeleted,
                    kreirano = legacyUser.kreirano,
                    authProvider = legacyUser.authProvider,
                    authProviderKey = legacyUser.authProviderKey
                };

                var createResult = await _userManager.CreateAsync(appUser);
                if (!createResult.Succeeded)
                {
                    return false;
                }
            }

            appUser.PasswordHash = _appUserPasswordHasher.HashPassword(appUser, password);
            var updateResult = await _userManager.UpdateAsync(appUser);
            if (!updateResult.Succeeded)
            {
                return false;
            }

            var roleName = legacyUser.isAdmin ? "Admin" : "User";
            if (!await _userManager.IsInRoleAsync(appUser, roleName))
            {
                await _userManager.AddToRoleAsync(appUser, roleName);
            }

            await EnsureLegacyUserClaimAsync(appUser, legacyUser);
            await _signInManager.SignInAsync(appUser, isPersistent: false);
            return true;
        }

        private async Task EnsureLegacyUserClaimByUsernameAsync(string? username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return;
            }

            var normalizedUsername = username.Trim();
            var appUser = await _userManager.FindByNameAsync(normalizedUsername);
            if (appUser is null)
            {
                return;
            }

            var legacyUser = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(user =>
                    !user.isDeleted
                    && user.username != null
                    && user.username.ToLower() == normalizedUsername.ToLower());

            if (legacyUser is null)
            {
                return;
            }

            var legacyClaimChanged = await EnsureLegacyUserClaimAsync(appUser, legacyUser);
            if (legacyClaimChanged)
            {
                await _signInManager.RefreshSignInAsync(appUser);
            }
        }

        private async Task<bool> EnsureLegacyUserClaimAsync(AppUser appUser, User legacyUser)
        {
            var expectedLegacyUserId = legacyUser.id.ToString();
            var claims = await _userManager.GetClaimsAsync(appUser);
            var existingClaim = claims.FirstOrDefault(claim => claim.Type == LegacyUserIdClaimType);

            if (existingClaim is null)
            {
                await _userManager.AddClaimAsync(appUser, new Claim(LegacyUserIdClaimType, expectedLegacyUserId));
                return true;
            }

            if (existingClaim.Value != expectedLegacyUserId)
            {
                await _userManager.RemoveClaimAsync(appUser, existingClaim);
                await _userManager.AddClaimAsync(appUser, new Claim(LegacyUserIdClaimType, expectedLegacyUserId));
                return true;
            }

            return false;
        }
    }
}
