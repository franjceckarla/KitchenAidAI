using KitchenAidAI.Filters;
using KitchenAidAI.Helpers;
using KitchenAidAI.Models.DTOs;
using KitchenAidAI.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KitchenAidAI.Controllers
{
    [Authorize]
    [RequireSession]
    public class NamirniceController : MvcApiControllerBase
    {
        public NamirniceController(IHttpClientFactory httpClientFactory)
            : base(httpClientFactory)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Create(int friziderId)
        {
            var isAdmin = User.IsInRole("Admin");
            var currentUserId = AuthSession.GetUserId(HttpContext);

            var fridge = await GetFridgeAsync(friziderId, isAdmin);
            if (fridge is null)
            {
                return NotFound();
            }

            if (!isAdmin && fridge.userId != currentUserId)
            {
                return NotFound();
            }

            ViewBag.FriziderId = friziderId;
            ViewBag.UserId = fridge.userId;
            PopulateDropdowns();
            return View(new NamirnicaFormDto { friziderId = friziderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NamirnicaFormDto namirnica)
        {
            var isAdmin = User.IsInRole("Admin");
            var currentUserId = AuthSession.GetUserId(HttpContext);

            if (!ModelState.IsValid)
            {
                ViewBag.FriziderId = namirnica.friziderId;
                var fridgeForInvalid = await GetFridgeAsync(namirnica.friziderId, isAdmin);
                ViewBag.UserId = fridgeForInvalid?.userId;
                PopulateDropdowns();
                return View(namirnica);
            }

            var fridge = await GetFridgeAsync(namirnica.friziderId, isAdmin);
            if (fridge is null)
            {
                return NotFound();
            }

            if (!isAdmin && fridge.userId != currentUserId)
            {
                return NotFound();
            }

            var response = await PostApiResponseAsync<NamirnicaDto>("/api/namirnice", namirnica);
            if (response?.success != true)
            {
                TempData["Warning"] = GetAlertMessage(response, "Neuspjelo spremanje namirnice.");
                ViewBag.FriziderId = namirnica.friziderId;
                ViewBag.UserId = fridge.userId;
                PopulateDropdowns();
                return View(namirnica);
            }

            return RedirectToAction("Index", "Frizider", new { userId = fridge.userId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var isAdmin = User.IsInRole("Admin");
            var currentUserId = AuthSession.GetUserId(HttpContext);

            var item = await GetNamirnicaAsync(id, isAdmin);
            if (item is null)
            {
                return NotFound();
            }

            var fridge = await GetFridgeAsync(item.friziderId, isAdmin);
            if (!isAdmin && fridge?.userId != currentUserId)
            {
                return NotFound();
            }

            ViewBag.FriziderId = item.friziderId;
            ViewBag.UserId = fridge?.userId;
            PopulateDropdowns();
            return View(new NamirnicaFormDto
            {
                id = item.id,
                friziderId = item.friziderId,
                naziv = item.naziv,
                kategorija = item.kategorija,
                mjera = item.mjera,
                kolicinaUFrizideru = item.kolicinaUFrizideru
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NamirnicaFormDto input)
        {
            var isAdmin = User.IsInRole("Admin");
            var currentUserId = AuthSession.GetUserId(HttpContext);

            var existing = await GetNamirnicaAsync(id, isAdmin);
            if (existing is null)
            {
                return NotFound();
            }

            var fridge = await GetFridgeAsync(existing.friziderId, isAdmin);
            if (!isAdmin && fridge?.userId != currentUserId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.FriziderId = input.friziderId;
                ViewBag.UserId = fridge?.userId;
                PopulateDropdowns();
                return View(input);
            }

            var response = await PutApiResponseAsync<NamirnicaDto>($"/api/namirnice/{id}", input);
            if (response?.success != true)
            {
                TempData["Warning"] = GetAlertMessage(response, "Neuspjelo spremanje namirnice.");
                ViewBag.FriziderId = input.friziderId;
                ViewBag.UserId = fridge?.userId;
                PopulateDropdowns();
                return View(input);
            }

            return RedirectToAction("Index", "Frizider", new { userId = fridge?.userId });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var isAdmin = User.IsInRole("Admin");
            var currentUserId = AuthSession.GetUserId(HttpContext);

            var item = await GetNamirnicaAsync(id, isAdmin);
            if (item is null)
            {
                return NotFound();
            }

            var fridge = await GetFridgeAsync(item.friziderId, isAdmin);
            if (!isAdmin && fridge?.userId != currentUserId)
            {
                return NotFound();
            }

            ViewBag.UserId = fridge?.userId;
            return View(item);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var isAdmin = User.IsInRole("Admin");
            var currentUserId = AuthSession.GetUserId(HttpContext);

            var item = await GetNamirnicaAsync(id, isAdmin);
            if (item is null)
            {
                return NotFound();
            }

            var fridge = await GetFridgeAsync(item.friziderId, isAdmin);
            if (!isAdmin && fridge?.userId != currentUserId)
            {
                return NotFound();
            }

            var response = await DeleteApiResponseAsync<NamirnicaDto>($"/api/namirnice/{id}");
            if (response?.success != true)
            {
                TempData["Warning"] = GetAlertMessage(response, "Neuspjelo brisanje namirnice.");
            }

            return RedirectToAction("Index", "Frizider", new { userId = fridge?.userId });
        }

        private async Task<FriziderDto?> GetFridgeAsync(int friziderId, bool isAdmin)
        {
            if (isAdmin)
            {
                var response = await GetApiResponseAsync<FriziderDto>($"/api/frizideri/{friziderId}");
                return response?.success == true ? response.data : null;
            }

            var publicResponse = await GetApiResponseAsync<FriziderPublicDto>($"/api/frizideri/{friziderId}");
            return publicResponse?.success == true && publicResponse.data is not null
                ? publicResponse.data.ToDto()
                : null;
        }

        private async Task<NamirnicaDto?> GetNamirnicaAsync(int id, bool isAdmin)
        {
            if (isAdmin)
            {
                var response = await GetApiResponseAsync<NamirnicaDto>($"/api/namirnice/{id}");
                return response?.success == true ? response.data : null;
            }

            var publicResponse = await GetApiResponseAsync<NamirnicaPublicDto>($"/api/namirnice/{id}");
            return publicResponse?.success == true && publicResponse.data is not null
                ? publicResponse.data.ToDto()
                : null;
        }

        private void PopulateDropdowns()
        {
            ViewBag.Kategorije = new SelectList(Enum.GetValues<KategorijaNamirnice>().Select(value => new { Value = value, Text = value.ToString() }), "Value", "Text");
            ViewBag.Mjere = new SelectList(Enum.GetValues<Mjera>().Select(value => new { Value = value, Text = value.ToString() }), "Value", "Text");
        }
    }
}