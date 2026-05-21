using KitchenAidAI.Data;
using KitchenAidAI.Filters;
using KitchenAidAI.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace KitchenAidAI.Controllers
{
    [RequireSession]
    public class KuhariceController : Controller
    {
        private readonly KitchenAidDbContext _dbContext;

        public KuhariceController(KitchenAidDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index(string? search)
        {
            var isAdmin = AuthSession.IsAdmin(HttpContext);
            var currentUserId = AuthSession.GetUserId(HttpContext);

            var usersQuery = _dbContext.Users
                .Include(user => user.kuharica)
                .ThenInclude(cookbook => cookbook!.receptKuharice)
                .AsQueryable();

            if (isAdmin)
            {
                usersQuery = usersQuery.Where(user => !user.isAdmin);
                if (!string.IsNullOrWhiteSpace(search))
                {
                    usersQuery = usersQuery.Where(user =>
                        (user.username ?? string.Empty).Contains(search)
                        || (user.kuharica != null && (user.kuharica.naziv ?? string.Empty).Contains(search)));
                }
            }
            else if (currentUserId.HasValue)
            {
                usersQuery = usersQuery.Where(user =>
                    user.id == currentUserId.Value
                    && !user.isDeleted
                    && user.kuharica != null
                    && !user.kuharica.isDeleted);
            }

            var users = usersQuery.AsNoTracking().ToList();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CookbookCards", users);
            }

            return View(users);
        }

        public IActionResult Details(int userId)
        {
            var isAdmin = AuthSession.IsAdmin(HttpContext);
            var currentUserId = AuthSession.GetUserId(HttpContext);
            if (!isAdmin && currentUserId != userId)
            {
                return NotFound();
            }

            var user = _dbContext.Users
                .Include(currentUser => currentUser.kuharica)
                .ThenInclude(cookbook => cookbook!.receptKuharice)
                .ThenInclude(join => join.recept)
                .AsNoTracking()
                .FirstOrDefault(currentUser => currentUser.id == userId && (isAdmin || !currentUser.isDeleted));
            if (user is null)
            {
                return NotFound();
            }

            var cookbook = user.kuharica;
            if (cookbook is null || (!isAdmin && cookbook.isDeleted))
            {
                return NotFound();
            }

            if (!isAdmin)
            {
                cookbook.receptKuharice = cookbook.receptKuharice
                    .Where(join => !join.isDeleted && join.recept is not null && !join.recept.isDeleted)
                    .ToList();
            }

            ViewBag.UserName = user.username;
            ViewBag.UserId = user.id;
            return View(cookbook);
        }

        public IActionResult Search(int userId, string? search)
        {
            var isAdmin = AuthSession.IsAdmin(HttpContext);
            var currentUserId = AuthSession.GetUserId(HttpContext);
            if (!isAdmin && currentUserId != userId)
            {
                return NotFound();
            }

            var cookbook = _dbContext.Kuharice
                .Include(currentCookbook => currentCookbook.receptKuharice)
                .ThenInclude(join => join.recept)
                .AsNoTracking()
                .FirstOrDefault(currentCookbook => currentCookbook.userId == userId && (isAdmin || !currentCookbook.isDeleted));
            if (cookbook is null)
            {
                return NotFound();
            }

            var items = cookbook.receptKuharice.AsQueryable();
            if (!isAdmin)
            {
                items = items.Where(join => !join.isDeleted && join.recept != null && !join.recept.isDeleted);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                items = items.Where(join => join.recept != null
                    && ((join.recept.naziv ?? string.Empty).Contains(search)
                        || (join.recept.opis ?? string.Empty).Contains(search)));
            }

            ViewBag.IsAdmin = isAdmin;
            return PartialView("_CookbookRecipes", items.ToList());
        }
    }
}
