using System.Diagnostics;
using KitchenAidAI.Data;
using KitchenAidAI.Models;
using KitchenAidAI.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace KitchenAidAI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly KitchenAidDbContext _dbContext;

        public HomeController(ILogger<HomeController> logger, KitchenAidDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var vm = new HomeDashboardViewModel
            {
                Users = _dbContext.Users
                    .Include(user => user.frizider)
                    .ThenInclude(fridge => fridge!.namirnice)
                    .Include(user => user.kuharica)
                    .AsNoTracking()
                    .ToList()
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
