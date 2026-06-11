using KitchenAidAI.Filters;
using System.Globalization;
using KitchenAidAI.Helpers;
using KitchenAidAI.Models.DTOs;
using KitchenAidAI.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KitchenAidAI.Controllers
{
    [Authorize]
    [RequireSession]
    public class FriziderController : MvcApiControllerBase
    {
        public FriziderController(IHttpClientFactory httpClientFactory)
            : base(httpClientFactory)
        {
        }

        public async Task<IActionResult> Index(int? userId)
        {
            if (!userId.HasValue)
            {
                TempData["Warning"] = "Za otvaranje frižidera prvo odaberite korisnika.";
                return RedirectToAction("Index", "Korisnici");
            }

            var isAdmin = User.IsInRole("Admin");
            var currentUserId = AuthSession.GetUserId(HttpContext);

            var targetUserId = userId.Value;
            if (!isAdmin && currentUserId != targetUserId)
            {
                return NotFound();
            }

            var userDto = await GetUserAsync(targetUserId, isAdmin);
            if (userDto is null)
            {
                return NotFound();
            }

            var fridgeDto = await GetFridgeByUserAsync(targetUserId, isAdmin);
            if (fridgeDto is null || (!isAdmin && fridgeDto.isDeleted))
            {
                TempData["Warning"] = "Korisnik još nema kreiran frižider.";
                return RedirectToAction("Index", "Korisnici");
            }

            ViewBag.UserName = userDto.username;
            ViewBag.UserId = userDto.id;
            ViewBag.IsAdmin = isAdmin;
            return View(fridgeDto);
        }

        public async Task<IActionResult> Search(
            int userId,
            string? search,
            KategorijaNamirnice? kategorija,
            Mjera? mjera,
            double? minKolicina,
            double? maxKolicina,
            double? minKalorije,
            double? maxKalorije,
            double? minProteini,
            double? maxProteini,
            double? minMasti,
            double? maxMasti,
            double? minUgljikohidrati,
            double? maxUgljikohidrati,
            double? minVlakna,
            double? maxVlakna,
            double? minSol,
            double? maxSol,
            DateTime? odDatuma,
            DateTime? doDatuma,
            string? sortBy,
            string? sortDir)
        {
            var isAdmin = User.IsInRole("Admin");
            var currentUserId = AuthSession.GetUserId(HttpContext);
            if (!isAdmin && currentUserId != userId)
            {
                return NotFound();
            }

            var fridge = await GetFridgeByUserAsync(userId, isAdmin);
            if (fridge is null)
            {
                return NotFound();
            }

            var includeDeleted = isAdmin ? "true" : "false";
            var parameters = new List<string>
            {
                $"friziderId={fridge.id}",
                $"includeDeleted={includeDeleted}"
            };

            void AddParam(string name, string? value)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    parameters.Add($"{name}={Uri.EscapeDataString(value)}");
                }
            }

            AddParam("search", search);
            AddParam("kategorija", kategorija?.ToString());
            AddParam("mjera", mjera?.ToString());
            AddParam("minKolicina", minKolicina?.ToString(CultureInfo.InvariantCulture));
            AddParam("maxKolicina", maxKolicina?.ToString(CultureInfo.InvariantCulture));
            AddParam("minKalorije", minKalorije?.ToString(CultureInfo.InvariantCulture));
            AddParam("maxKalorije", maxKalorije?.ToString(CultureInfo.InvariantCulture));
            AddParam("minProteini", minProteini?.ToString(CultureInfo.InvariantCulture));
            AddParam("maxProteini", maxProteini?.ToString(CultureInfo.InvariantCulture));
            AddParam("minMasti", minMasti?.ToString(CultureInfo.InvariantCulture));
            AddParam("maxMasti", maxMasti?.ToString(CultureInfo.InvariantCulture));
            AddParam("minUgljikohidrati", minUgljikohidrati?.ToString(CultureInfo.InvariantCulture));
            AddParam("maxUgljikohidrati", maxUgljikohidrati?.ToString(CultureInfo.InvariantCulture));
            AddParam("minVlakna", minVlakna?.ToString(CultureInfo.InvariantCulture));
            AddParam("maxVlakna", maxVlakna?.ToString(CultureInfo.InvariantCulture));
            AddParam("minSol", minSol?.ToString(CultureInfo.InvariantCulture));
            AddParam("maxSol", maxSol?.ToString(CultureInfo.InvariantCulture));
            AddParam("odDatuma", odDatuma?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            AddParam("doDatuma", doDatuma?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            AddParam("sortBy", sortBy);
            AddParam("sortDir", sortDir);

            var query = "/api/namirnice" + (parameters.Count > 0 ? $"?{string.Join("&", parameters)}" : string.Empty);

            if (isAdmin)
            {
                var response = await GetApiResponseAsync<List<NamirnicaDto>>(query);
                if (response?.success != true || response.data is null)
                {
                    return PartialView("_FridgeItems", new List<NamirnicaDto>());
                }

                ViewBag.IsAdmin = true;
                return PartialView("_FridgeItems", response.data);
            }

            var publicResponse = await GetApiResponseAsync<List<NamirnicaPublicDto>>(query);
            if (publicResponse?.success != true || publicResponse.data is null)
            {
                return PartialView("_FridgeItems", new List<NamirnicaDto>());
            }

            var items = publicResponse.data.Select(item => item.ToDto()).ToList();
            ViewBag.IsAdmin = false;
            return PartialView("_FridgeItems", items);
        }

        private async Task<UserDto?> GetUserAsync(int userId, bool isAdmin)
        {
            if (isAdmin)
            {
                var response = await GetApiResponseAsync<UserDto>($"/api/users/{userId}");
                return response?.success == true ? response.data : null;
            }

            var publicResponse = await GetApiResponseAsync<UserPublicDto>($"/api/users/{userId}");
            return publicResponse?.success == true && publicResponse.data is not null
                ? publicResponse.data.ToDto()
                : null;
        }

        private async Task<FriziderDto?> GetFridgeByUserAsync(int userId, bool isAdmin)
        {
            var response = isAdmin
                ? await GetApiResponseAsync<List<FriziderDto>>($"/api/frizideri?userId={userId}")
                : null;

            if (isAdmin && response?.success == true && response.data is not null)
            {
                return response.data.FirstOrDefault();
            }

            var publicResponse = await GetApiResponseAsync<List<FriziderPublicDto>>($"/api/frizideri?userId={userId}");
            if (publicResponse?.success == true && publicResponse.data is not null)
            {
                var first = publicResponse.data.FirstOrDefault();
                return first is null ? null : first.ToDto();
            }

            return null;
        }
    }
}
