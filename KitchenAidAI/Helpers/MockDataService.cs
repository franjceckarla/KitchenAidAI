using KitchenAidAI.Models;
using KitchenAidAI.Helpers.MockData;

namespace KitchenAidAI.Helpers
{
    public class MockDataService
    {
        private readonly List<User> _users;
        private readonly List<Recept> _recipes;

        public MockDataService()
        {
            _recipes = MockRecipeBuilder.BuildRecipes();
            var fridges = MockFridgeBuilder.BuildFridges();
            var cookbooks = MockCookbookBuilder.BuildCookbooks(_recipes);
            _users = MockUserBuilder.BuildUsers(fridges, cookbooks);
        }

        public IReadOnlyList<User> GetUsers() => _users;

        public User? GetUserById(int id) => _users.FirstOrDefault(u => u.id == id);

        public Frizider? GetFriziderByUserId(int userId) => GetUserById(userId)?.frizider;

        public IReadOnlyList<Recept> GetRecipes() => _recipes;

        public Recept? GetRecipeById(int id) => _recipes.FirstOrDefault(r => r.id == id);

        public Kuharica? GetCookbookByUserId(int userId) => GetUserById(userId)?.kuharica;
    }
}
