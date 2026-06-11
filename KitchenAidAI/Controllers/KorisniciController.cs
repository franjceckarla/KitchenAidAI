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
    public class KorisniciController : MvcApiControllerBase
    {
        public KorisniciController(IHttpClientFactory httpClientFactory)
            : base(httpClientFactory)
        {
        }

        public async Task<IActionResult> Index(
            string? search,
            string? username,
            string? email,
            string? zemlja,
            PreferencijaPrehrane? preferencijaPrehrane,
            bool? isDeleted,
            bool? hasUploadedFiles,
            DateTime? odDatuma,
            DateTime? doDatuma,
            string? sortBy,
            string? sortDir,
            bool includeAdmins = false,
            bool? includeDeleted = null)
        {
            var isAdmin = User.IsInRole("Admin");

            var parameters = new List<string>();
            void AddParam(string name, string? value)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    parameters.Add($"{name}={Uri.EscapeDataString(value)}");
                }
            }

            AddParam("search", search);
            if (isAdmin)
            {
                AddParam("username", username);
                AddParam("email", email);
                AddParam("zemlja", zemlja);
                AddParam("preferencijaPrehrane", preferencijaPrehrane?.ToString());
                AddParam("isDeleted", isDeleted?.ToString().ToLowerInvariant());
                AddParam("hasUploadedFiles", hasUploadedFiles?.ToString().ToLowerInvariant());
                AddParam("odDatuma", odDatuma?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                AddParam("doDatuma", doDatuma?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                AddParam("sortBy", sortBy);
                AddParam("sortDir", sortDir);
                AddParam("includeAdmins", includeAdmins ? "true" : null);
                AddParam("includeDeleted", includeDeleted.HasValue ? includeDeleted.Value.ToString().ToLowerInvariant() : null);
            }

            var query = "/api/users" + (parameters.Count > 0 ? $"?{string.Join("&", parameters)}" : string.Empty);

            if (isAdmin)
            {
                var response = await GetApiResponseAsync<List<UserDto>>(query);
                var data = response?.success == true && response.data is not null
                    ? response.data
                    : new List<UserDto>();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_UserCards", data);
                }

                return View(data);
            }

            var publicResponse = await GetApiResponseAsync<List<UserPublicDto>>(query);
            var items = publicResponse?.success == true && publicResponse.data is not null
                ? publicResponse.data.Select(user => user.ToDto()).ToList()
                : new List<UserDto>();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_UserCards", items);
            }

            return View(items);
        }

        public async Task<IActionResult> Details(int id)
        {
            var isAdmin = User.IsInRole("Admin");
            var currentUserId = AuthSession.GetUserId(HttpContext);
            if (!isAdmin && currentUserId != id)
            {
                return NotFound();
            }

            if (isAdmin)
            {
                var response = await GetApiResponseAsync<UserDto>($"/api/users/{id}");
                if (response?.success != true || response.data is null)
                {
                    return NotFound();
                }

                return View(response.data);
            }

            var publicResponse = await GetApiResponseAsync<UserPublicDto>($"/api/users/{id}");
            if (publicResponse?.success != true || publicResponse.data is null)
            {
                return NotFound();
            }

            return View(publicResponse.data.ToDto());
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View(new KitchenAidAI.Models.ViewModels.RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(KitchenAidAI.Models.ViewModels.RegisterViewModel input)
        {
            if (input.SelectedPreferences is null || input.SelectedPreferences.Count == 0)
            {
                ModelState.AddModelError(nameof(KitchenAidAI.Models.ViewModels.RegisterViewModel.SelectedPreferences), "Odaberite barem jednu preferenciju prehrane.");
            }
            else if (input.SelectedPreferences.Count > 1)
            {
                ModelState.AddModelError(nameof(KitchenAidAI.Models.ViewModels.RegisterViewModel.SelectedPreferences), "Odaberite samo jednu preferenciju prehrane.");
            }

            if (!ModelState.IsValid)
            {
                return View(input);
            }

            var payload = new UserCreateDto
            {
                username = input.Username,
                ime = input.Ime,
                prezime = input.Prezime,
                datumRodenja = input.DatumRodenja?.Date,
                zemlja = input.Zemlja,
                email = input.Email,
                password = input.Password,
                preferencijaPrehrane = input.SelectedPreferences?.FirstOrDefault() ?? KitchenAidAI.Models.Enums.PreferencijaPrehrane.Omnivorte
            };

            var response = await PostApiResponseAsync<UserDto>("/api/users", payload);
            if (response?.success != true)
            {
                ModelState.AddModelError(string.Empty, GetAlertMessage(response, "Neuspjelo spremanje korisnika."));
                return View(input);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var isAdmin = User.IsInRole("Admin");
            var currentUserId = AuthSession.GetUserId(HttpContext);
            if (!isAdmin && currentUserId != id)
            {
                return NotFound();
            }

            if (isAdmin)
            {
                var response = await GetApiResponseAsync<UserDto>($"/api/users/{id}");
                if (response?.success != true || response.data is null)
                {
                    return NotFound();
                }

                return View(response.data.ToEditDto());
            }

            var publicResponse = await GetApiResponseAsync<UserPublicDto>($"/api/users/{id}");
            if (publicResponse?.success != true || publicResponse.data is null)
            {
                return NotFound();
            }

            var dto = publicResponse.data.ToDto().ToEditDto();
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserEditDto input, string? newPassword, bool isAdminCheckbox = false)
        {
            var isAdmin = User.IsInRole("Admin");
            var currentUserId = AuthSession.GetUserId(HttpContext);
            if (!isAdmin && currentUserId != id)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                return View(input);
            }

            var payload = new UserUpdateDto
            {
                username = input.username,
                email = input.email,
                preferencijaPrehrane = input.preferencijaPrehrane,
                isAdmin = isAdmin ? isAdminCheckbox : false,
                newPassword = newPassword
            };

            var response = await PutApiResponseAsync<UserDto>($"/api/users/{id}", payload);
            if (response?.success != true)
            {
                TempData["Warning"] = GetAlertMessage(response, "Neuspjelo spremanje korisnika.");
                return View(input);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await GetApiResponseAsync<UserDto>($"/api/users/{id}");
            if (response?.success != true || response.data is null)
            {
                return NotFound();
            }

            return View(response.data);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await DeleteApiResponseAsync<UserDto>($"/api/users/{id}");
            if (response?.success != true)
            {
                TempData["Warning"] = GetAlertMessage(response, "Neuspjelo brisanje korisnika.");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Restore(int id)
        {
            var response = await PostApiResponseAsync<UserDto>($"/api/users/{id}/restore", new { });
            if (response?.success != true)
            {
                TempData["Warning"] = GetAlertMessage(response, "Neuspjelo vracanje korisnika.");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
