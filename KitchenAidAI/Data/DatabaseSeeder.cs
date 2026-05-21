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

        public DatabaseSeeder(KitchenAidDbContext dbContext)
        {
            _dbContext = dbContext;
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