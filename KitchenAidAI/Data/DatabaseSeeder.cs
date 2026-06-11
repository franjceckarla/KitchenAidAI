using KitchenAidAI.Helpers;
using KitchenAidAI.Helpers.SeedData;
using KitchenAidAI.Models;
using KitchenAidAI.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KitchenAidAI.Data
{
    public class DatabaseSeeder
    {
        private readonly KitchenAidDbContext _dbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IPasswordHasher<AppUser> _appUserPasswordHasher;

        public DatabaseSeeder(KitchenAidDbContext dbContext, UserManager<AppUser> userManager, RoleManager<IdentityRole<int>> roleManager, IPasswordHasher<AppUser> appUserPasswordHasher)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _appUserPasswordHasher = appUserPasswordHasher;
        }

        public async Task SeedAsync()
        {
            await _dbContext.Database.MigrateAsync();

            if (!await _dbContext.Countries.AnyAsync())
            {
                var countries = CountrySeedData.AllCountries
                    .Select(name => new Country { naziv = name })
                    .ToList();
                await _dbContext.Countries.AddRangeAsync(countries);
                await _dbContext.SaveChangesAsync();
            }

            // Ensure roles
            var roles = new[] { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole<int>(role));
                }
            }

            // Seed an admin AppUser if none exists
            var anyAppUsers = await _userManager.Users.AnyAsync();
            if (!anyAppUsers)
            {
                var admin = new AppUser
                {
                    UserName = "admin",
                    Email = "admin@kitchenaid.local",
                    ime = "Admin",
                    prezime = "Administrator",
                    preferencijaPrehrane = PreferencijaPrehrane.Omnivorte,
                    isAdmin = true,
                    kreirano = DateTime.Now
                };

                var result = await _userManager.CreateAsync(admin);
                if (result.Succeeded)
                {
                    admin.PasswordHash = _appUserPasswordHasher.HashPassword(admin, "admin");
                    await _userManager.UpdateAsync(admin);
                    await _userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            // Legacy seed for mock data into legacy Users/Recepti as before
            var passwordHasher = new PasswordHasher<User>();
            if (!await _dbContext.Users.AnyAsync())
            {
                var mockData = new MockDataService();
                var users = mockData.GetUsers().ToList();
                var recipes = mockData.GetRecipes().ToList();

                foreach (var user in users)
                {
                    user.passwordHash = passwordHasher.HashPassword(user, user.username ?? string.Empty);
                    user.isAdmin = false;
                }

                var adminUser = new User
                {
                    username = "admin",
                    email = "admin@kitchenaid.local",
                    preferencijaPrehrane = PreferencijaPrehrane.Omnivorte,
                    isAdmin = true,
                    frizider = new Frizider(),
                    kuharica = new Kuharica { naziv = "Admin kuharica" }
                };
                adminUser.passwordHash = passwordHasher.HashPassword(adminUser, "admin");
                users.Add(adminUser);

                await _dbContext.Users.AddRangeAsync(users);
                await _dbContext.Recepti.AddRangeAsync(recipes);
                await _dbContext.SaveChangesAsync();
                return;
            }

            var existingUsers = await _dbContext.Users.ToListAsync();
            foreach (var user in existingUsers)
            {
                if (string.IsNullOrWhiteSpace(user.passwordHash))
                {
                    user.passwordHash = passwordHasher.HashPassword(user, user.username ?? string.Empty);
                }

                if (string.IsNullOrWhiteSpace(user.ime) || string.IsNullOrWhiteSpace(user.prezime))
                {
                    var (ime, prezime) = GuessName(user.username, user.email);
                    if (string.IsNullOrWhiteSpace(user.ime))
                    {
                        user.ime = ime;
                    }

                    if (string.IsNullOrWhiteSpace(user.prezime))
                    {
                        user.prezime = prezime;
                    }
                }
            }

            var hasAdmin = existingUsers.Any(user => user.isAdmin);
            if (!hasAdmin)
            {
                var adminUser = new User
                {
                    username = "admin",
                    email = "admin@kitchenaid.local",
                    preferencijaPrehrane = PreferencijaPrehrane.Omnivorte,
                    isAdmin = true,
                    frizider = new Frizider(),
                    kuharica = new Kuharica { naziv = "Admin kuharica" }
                };
                adminUser.passwordHash = passwordHasher.HashPassword(adminUser, "admin");
                _dbContext.Users.Add(adminUser);
            }

            await _dbContext.SaveChangesAsync();
        }

        private static (string ime, string prezime) GuessName(string? username, string? email)
        {
            var key = (username ?? string.Empty).ToLower();
            return key switch
            {
                "marko92" => ("Marko", "Horvat"),
                "petra_love" => ("Petra", "Kovac"),
                "ana_fit" => ("Ana", "Ilic"),
                "admin" => ("Admin", "Administrator"),
                _ => (FallbackName(email), "Korisnik")
            };
        }

        private static string FallbackName(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return "Korisnik";
            }

            var handle = email.Split('@', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(handle))
            {
                return "Korisnik";
            }

            return char.ToUpperInvariant(handle[0]) + handle.Substring(1);
        }
    }
}