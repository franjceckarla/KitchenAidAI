using KitchenAidAI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace KitchenAidAI.Controllers
{
    public class ReceptiController : Controller
    {
        private readonly MockDataService _mockData;

        public ReceptiController(MockDataService mockData)
        {
            _mockData = mockData;
        }

        public IActionResult Index()
        {
            var recipes = _mockData.GetRecipes();
            return View(recipes);
        }

        public IActionResult Details(int id)
        {
            var recipe = _mockData.GetRecipeById(id);
            if (recipe is null)
            {
                return NotFound();
            }

            return View(recipe);
        }
    }
}
