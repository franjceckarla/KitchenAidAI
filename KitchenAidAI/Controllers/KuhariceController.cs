using KitchenAidAI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace KitchenAidAI.Controllers
{
    public class KuhariceController : Controller
    {
        private readonly MockDataService _mockData;

        public KuhariceController(MockDataService mockData)
        {
            _mockData = mockData;
        }

        public IActionResult Index()
        {
            var users = _mockData.GetUsers();
            return View(users);
        }

        public IActionResult Details(int userId)
        {
            var user = _mockData.GetUserById(userId);
            if (user is null)
            {
                return NotFound();
            }

            var cookbook = _mockData.GetCookbookByUserId(userId);
            if (cookbook is null)
            {
                return NotFound();
            }

            ViewBag.UserName = user.username;
            return View(cookbook);
        }
    }
}
