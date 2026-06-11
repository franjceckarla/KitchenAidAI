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
    public class ReceptiController : MvcApiControllerBase
    {
        public ReceptiController(IHttpClientFactory httpClientFactory)
            : base(httpClientFactory)
        {
        }

        public async Task<IActionResult> Index(
            string? search,
            TezinaRecepta? tezina,
            double? minVrijeme,
            double? maxVrijeme,
            int? minPorcija,
            int? maxPorcija,
            DateTime? odDatuma,
            DateTime? doDatuma,
            string? sortBy,
            string? sortDir)
        {
            var isAdmin = User.IsInRole("Admin");
            var includeDeleted = isAdmin ? "true" : "false";
            var parameters = new List<string> { $"includeDeleted={includeDeleted}" };
            void AddParam(string name, string? value)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    parameters.Add($"{name}={Uri.EscapeDataString(value)}");
                }
            }

            AddParam("search", search);
            AddParam("tezina", tezina?.ToString());
            AddParam("minVrijeme", minVrijeme?.ToString(CultureInfo.InvariantCulture));
            AddParam("maxVrijeme", maxVrijeme?.ToString(CultureInfo.InvariantCulture));
            AddParam("minPorcija", minPorcija?.ToString(CultureInfo.InvariantCulture));
            AddParam("maxPorcija", maxPorcija?.ToString(CultureInfo.InvariantCulture));
            AddParam("odDatuma", odDatuma?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            AddParam("doDatuma", doDatuma?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            AddParam("sortBy", sortBy);
            AddParam("sortDir", sortDir);

            var query = "/api/recepti" + (parameters.Count > 0 ? $"?{string.Join("&", parameters)}" : string.Empty);

            if (isAdmin)
            {
                var response = await GetApiResponseAsync<List<ReceptDto>>(query);
                var data = response?.success == true && response.data is not null
                    ? response.data
                    : new List<ReceptDto>();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_RecipeCards", data);
                }

                return View(data);
            }

            var publicResponse = await GetApiResponseAsync<List<ReceptPublicDto>>(query);
            var items = publicResponse?.success == true && publicResponse.data is not null
                ? publicResponse.data.Select(recipe => recipe.ToDto()).ToList()
                : new List<ReceptDto>();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_RecipeCards", items);
            }

            return View(items);
        }

        public async Task<IActionResult> Details(int id)
        {
            var isAdmin = User.IsInRole("Admin");

            if (isAdmin)
            {
                var response = await GetApiResponseAsync<ReceptDto>($"/api/recepti/{id}");
                if (response?.success != true || response.data is null)
                {
                    return NotFound();
                }

                return View(response.data);
            }

            var publicResponse = await GetApiResponseAsync<ReceptPublicDto>($"/api/recepti/{id}");
            if (publicResponse?.success != true || publicResponse.data is null)
            {
                return NotFound();
            }

            return View(publicResponse.data.ToDto());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new ReceptFormDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReceptFormDto recipe)
        {
            if (!ModelState.IsValid)
            {
                return View(recipe);
            }

            var response = await PostApiResponseAsync<ReceptDto>("/api/recepti", recipe);
            if (response?.success != true)
            {
                TempData["Warning"] = GetAlertMessage(response, "Neuspjelo spremanje recepta.");
                return View(recipe);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var isAdmin = User.IsInRole("Admin");

            if (isAdmin)
            {
                var response = await GetApiResponseAsync<ReceptDto>($"/api/recepti/{id}");
                if (response?.success != true || response.data is null)
                {
                    return NotFound();
                }

                return View(ToFormDto(response.data));
            }

            var publicResponse = await GetApiResponseAsync<ReceptPublicDto>($"/api/recepti/{id}");
            if (publicResponse?.success != true || publicResponse.data is null)
            {
                return NotFound();
            }

            return View(ToFormDto(publicResponse.data.ToDto()));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ReceptFormDto input)
        {
            if (!ModelState.IsValid)
            {
                return View(input);
            }

            var response = await PutApiResponseAsync<ReceptDto>($"/api/recepti/{id}", input);
            if (response?.success != true)
            {
                TempData["Warning"] = GetAlertMessage(response, "Neuspjelo spremanje recepta.");
                return View(input);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var isAdmin = User.IsInRole("Admin");

            if (isAdmin)
            {
                var response = await GetApiResponseAsync<ReceptDto>($"/api/recepti/{id}");
                if (response?.success != true || response.data is null)
                {
                    return NotFound();
                }

                return View(response.data);
            }

            var publicResponse = await GetApiResponseAsync<ReceptPublicDto>($"/api/recepti/{id}");
            if (publicResponse?.success != true || publicResponse.data is null)
            {
                return NotFound();
            }

            return View(publicResponse.data.ToDto());
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await DeleteApiResponseAsync<ReceptDto>($"/api/recepti/{id}");
            if (response?.success != true)
            {
                TempData["Warning"] = GetAlertMessage(response, "Neuspjelo brisanje recepta.");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Restore(int id)
        {
            var response = await PostApiResponseAsync<ReceptDto>($"/api/recepti/{id}/restore", new { });
            if (response?.success != true)
            {
                TempData["Warning"] = GetAlertMessage(response, "Neuspjelo vracanje recepta.");
            }

            return RedirectToAction(nameof(Index));
        }

        private static ReceptFormDto ToFormDto(ReceptDto recipe)
        {
            return new ReceptFormDto
            {
                id = recipe.id,
                naziv = recipe.naziv,
                opis = recipe.opis,
                vrijemeKuhanja = recipe.vrijemeKuhanja,
                tezina = recipe.tezina,
                brojPorcija = recipe.brojPorcija
            };
        }
    }
}
