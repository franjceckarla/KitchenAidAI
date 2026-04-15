using KitchenAidAI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace KitchenAidAI.Controllers
{
    public class FriziderController : Controller
    {
        private readonly MockDataService _mockData;

        public FriziderController(MockDataService mockData)
        {
            _mockData = mockData;
        }

        public IActionResult Index(int? userId)
        {
            if (!userId.HasValue)
            {
                TempData["Warning"] = "Za otvaranje frižidera prvo odaberite korisnika.";
                return RedirectToAction("Index", "Korisnici");
            }

            var targetUserId = userId.Value;
            var user = _mockData.GetUserById(targetUserId);
            if (user is null)
            {
                return NotFound();
            }

            ViewBag.UserName = user.username;
            ViewBag.UserId = user.id;
            return View(user.frizider);
        }
    }
}
