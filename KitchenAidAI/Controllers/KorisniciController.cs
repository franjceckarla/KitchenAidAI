using KitchenAidAI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace KitchenAidAI.Controllers
{
    public class KorisniciController : Controller
    {
        private readonly MockDataService _mockData;

        public KorisniciController(MockDataService mockData)
        {
            _mockData = mockData;
        }

        public IActionResult Index()
        {
            var users = _mockData.GetUsers();
            return View(users);
        }

        public IActionResult Details(int id)
        {
            var user = _mockData.GetUserById(id);
            if (user is null)
            {
                return NotFound();
            }

            return View(user);
        }
    }
}
