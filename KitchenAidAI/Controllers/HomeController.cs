using System.Diagnostics;
using KitchenAidAI.Data;
using KitchenAidAI.Filters;
using KitchenAidAI.Helpers;
using KitchenAidAI.Models;
using KitchenAidAI.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace KitchenAidAI.Controllers
{
    [RequireSession]
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
            var isAdmin = AuthSession.IsAdmin(HttpContext);
            var currentUserId = AuthSession.GetUserId(HttpContext);

            var usersQuery = _dbContext.Users
                .Include(user => user.frizider)
                .ThenInclude(fridge => fridge!.namirnice)
                .Include(user => user.kuharica)
                .AsQueryable();

            if (isAdmin)
            {
                usersQuery = usersQuery.Where(user => !user.isAdmin);
            }
            else if (currentUserId.HasValue)
            {
                usersQuery = usersQuery.Where(user => user.id == currentUserId.Value && !user.isDeleted);
            }

            var users = usersQuery.AsNoTracking().ToList();
            if (!isAdmin)
            {
                foreach (var user in users)
                {
                    if (user.frizider?.namirnice is not null)
                    {
                        user.frizider.namirnice = user.frizider.namirnice
                            .Where(item => !item.isDeleted)
                            .ToList();
                    }
                }
            }

            var vm = new HomeDashboardViewModel
            {
                Users = users
            };

            ViewBag.CurrentUserId = currentUserId;

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
