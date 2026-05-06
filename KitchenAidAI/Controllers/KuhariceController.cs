using KitchenAidAI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace KitchenAidAI.Controllers
{
    public class KuhariceController : Controller
    {
        private readonly KitchenAidDbContext _dbContext;

        public KuhariceController(KitchenAidDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var users = _dbContext.Users
                .Include(user => user.kuharica)
                .ThenInclude(cookbook => cookbook!.receptKuharice)
                .AsNoTracking()
                .ToList();
            return View(users);
        }

        public IActionResult Details(int userId)
        {
            var user = _dbContext.Users
                .Include(currentUser => currentUser.kuharica)
                .ThenInclude(cookbook => cookbook!.receptKuharice)
                .ThenInclude(join => join.recept)
                .AsNoTracking()
                .FirstOrDefault(currentUser => currentUser.id == userId);
            if (user is null)
            {
                return NotFound();
            }

            var cookbook = user.kuharica;
            if (cookbook is null)
            {
                return NotFound();
            }

            ViewBag.UserName = user.username;
            return View(cookbook);
        }
    }
}
