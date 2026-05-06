using KitchenAidAI.Helpers;
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

            if (await _dbContext.Users.AnyAsync())
            {
                return;
            }

            var mockData = new MockDataService();
            await _dbContext.Users.AddRangeAsync(mockData.GetUsers());
            await _dbContext.Recepti.AddRangeAsync(mockData.GetRecipes());
            await _dbContext.SaveChangesAsync();
        }
    }
}