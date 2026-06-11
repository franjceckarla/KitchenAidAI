using KitchenAidAI.Data;
using KitchenAidAI.Filters;
using KitchenAidAI.Helpers;
using KitchenAidAI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KitchenAidAI.Controllers
{
    [Authorize]
    [RequireSession]
    public class DatotekeController : MvcApiControllerBase
    {
        private readonly KitchenAidDbContext _dbContext;

        public DatotekeController(IHttpClientFactory httpClientFactory, KitchenAidDbContext dbContext)
            : base(httpClientFactory)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index(int? userId, string? search, bool includeDeleted = false)
        {
            var isAdmin = User.IsInRole("Admin");
            var currentUserId = AuthSession.GetUserId(HttpContext);

            int targetUserId;
            if (isAdmin)
            {
                if (!userId.HasValue)
                {
                    return RedirectToAction("Index", "Korisnici");
                }

                targetUserId = userId.Value;
            }
            else if (currentUserId.HasValue)
            {
                targetUserId = currentUserId.Value;
            }
            else
            {
                return RedirectToAction("Login", "Auth");
            }

            var targetUser = _dbContext.Users.AsNoTracking().FirstOrDefault(user => user.id == targetUserId);
            if (targetUser is null)
            {
                return NotFound();
            }

            var query = new List<string>
            {
                $"userId={targetUserId}",
                $"includeDeleted={includeDeleted.ToString().ToLowerInvariant()}"
            };

            if (!string.IsNullOrWhiteSpace(search))
            {
                query.Add($"search={Uri.EscapeDataString(search)}");
            }

            var response = await GetApiResponseAsync<List<DatotekaDto>>($"/api/datoteke?{string.Join("&", query)}");
            var items = response?.success == true && response.data is not null
                ? response.data
                : new List<DatotekaDto>();

            ViewBag.TargetUserId = targetUserId;
            ViewBag.TargetUsername = targetUser.username;
            ViewBag.IsAdmin = isAdmin;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_FileRows", items);
            }

            return View(items);
        }
    }
}
