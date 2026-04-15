using System.Diagnostics;
using KitchenAidAI.Helpers;
using KitchenAidAI.Models;
using KitchenAidAI.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace KitchenAidAI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MockDataService _mockData;

        public HomeController(ILogger<HomeController> logger, MockDataService mockData)
        {
            _logger = logger;
            _mockData = mockData;
        }

        public IActionResult Index()
        {
            var vm = new HomeDashboardViewModel
            {
                Users = _mockData.GetUsers()
            };

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Chat()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
