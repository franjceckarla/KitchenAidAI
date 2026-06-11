using KitchenAidAI.Filters;
using KitchenAidAI.Helpers;
using KitchenAidAI.Models.DTOs;
using KitchenAidAI.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KitchenAidAI.Controllers
{
    [Authorize]
    [RequireSession]
    public class KuhariceController : MvcApiControllerBase
    {
        public KuhariceController(IHttpClientFactory httpClientFactory)
            : base(httpClientFactory)
        {
        }

        public async Task<IActionResult> Index(
            string? search,
            string? username,
            string? email,
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
                    return PartialView("_CookbookCards", data);
                }

                return View(data);
            }

            var publicResponse = await GetApiResponseAsync<List<UserPublicDto>>(query);
            var items = publicResponse?.success == true && publicResponse.data is not null
                ? publicResponse.data.Select(user => user.ToDto()).ToList()
                : new List<UserDto>();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CookbookCards", items);
            }

            return View(items);
        }

        public async Task<IActionResult> Details(int userId)
        {
            var isAdmin = User.IsInRole("Admin");
            var currentUserId = AuthSession.GetUserId(HttpContext);
            if (!isAdmin && currentUserId != userId)
            {
                return NotFound();
            }

            if (isAdmin)
            {
                var response = await GetApiResponseAsync<UserDto>($"/api/users/{userId}");
                if (response?.success != true || response.data is null)
                {
                    return NotFound();
                }

                var cookbook = response.data.kuharica;
                if (cookbook is null || cookbook.isDeleted)
                {
                    return NotFound();
                }

                ViewBag.UserName = response.data.username;
                ViewBag.UserId = response.data.id;
                return View(cookbook);
            }

            var publicResponse = await GetApiResponseAsync<UserPublicDto>($"/api/users/{userId}");
            if (publicResponse?.success != true || publicResponse.data is null)
            {
                return NotFound();
            }

            var publicUser = publicResponse.data.ToDto();
            var publicCookbook = publicUser.kuharica;
            if (publicCookbook is null)
            {
                return NotFound();
            }

            ViewBag.UserName = publicUser.username;
            ViewBag.UserId = publicUser.id;
            return View(publicCookbook);
        }

        public async Task<IActionResult> Search(
            int userId,
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
            var currentUserId = AuthSession.GetUserId(HttpContext);
            if (!isAdmin && currentUserId != userId)
            {
                return NotFound();
            }

            var cookbookId = await GetCookbookIdAsync(userId, isAdmin);
            if (!cookbookId.HasValue)
            {
                return NotFound();
            }

            var apiPath = $"/api/recept-kuharice?kuharicaId={cookbookId.Value}";
            if (isAdmin)
            {
                var response = await GetApiResponseAsync<List<ReceptKuharicaDto>>(apiPath);
                var items = response?.success == true && response.data is not null
                    ? response.data
                    : new List<ReceptKuharicaDto>();

                items = ApplyRecipeFilters(items, search, tezina, minVrijeme, maxVrijeme, minPorcija, maxPorcija, odDatuma, doDatuma);
                items = ApplyRecipeSorting(items, sortBy, sortDir);

                ViewBag.IsAdmin = true;
                return PartialView("_CookbookRecipes", items);
            }

            var publicResponse = await GetApiResponseAsync<List<ReceptKuharicaPublicDto>>(apiPath);
            var publicItems = publicResponse?.success == true && publicResponse.data is not null
                ? publicResponse.data.Select(join => join.ToDto()).ToList()
                : new List<ReceptKuharicaDto>();

            publicItems = ApplyRecipeFilters(publicItems, search, tezina, minVrijeme, maxVrijeme, minPorcija, maxPorcija, odDatuma, doDatuma);
            publicItems = ApplyRecipeSorting(publicItems, sortBy, sortDir);

            ViewBag.IsAdmin = false;
            return PartialView("_CookbookRecipes", publicItems);
        }

        private static List<ReceptKuharicaDto> ApplyRecipeFilters(
            List<ReceptKuharicaDto> items,
            string? search,
            TezinaRecepta? tezina,
            double? minVrijeme,
            double? maxVrijeme,
            int? minPorcija,
            int? maxPorcija,
            DateTime? odDatuma,
            DateTime? doDatuma)
        {
            var filtered = items.Where(join => join.recept != null);

            if (!string.IsNullOrWhiteSpace(search))
            {
                filtered = filtered.Where(join => join.recept != null
                    && (((join.recept.naziv ?? string.Empty).Contains(search)
                        || (join.recept.opis ?? string.Empty).Contains(search))));
            }

            if (tezina.HasValue)
            {
                filtered = filtered.Where(join => join.recept != null && join.recept.tezina == tezina.Value);
            }

            if (minVrijeme.HasValue)
            {
                filtered = filtered.Where(join => join.recept != null && join.recept.vrijemeKuhanja >= minVrijeme.Value);
            }

            if (maxVrijeme.HasValue)
            {
                filtered = filtered.Where(join => join.recept != null && join.recept.vrijemeKuhanja <= maxVrijeme.Value);
            }

            if (minPorcija.HasValue)
            {
                filtered = filtered.Where(join => join.recept != null && join.recept.brojPorcija >= minPorcija.Value);
            }

            if (maxPorcija.HasValue)
            {
                filtered = filtered.Where(join => join.recept != null && join.recept.brojPorcija <= maxPorcija.Value);
            }

            if (odDatuma.HasValue)
            {
                var fromDate = odDatuma.Value.Date;
                filtered = filtered.Where(join => join.kreirano >= fromDate);
            }

            if (doDatuma.HasValue)
            {
                var toDate = doDatuma.Value.Date;
                filtered = filtered.Where(join => join.kreirano <= toDate);
            }

            return filtered.ToList();
        }

        private static List<ReceptKuharicaDto> ApplyRecipeSorting(
            List<ReceptKuharicaDto> items,
            string? sortBy,
            string? sortDir)
        {
            var sortDescending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            return sortBy switch
            {
                "naziv" => sortDescending
                    ? items.OrderByDescending(join => join.recept?.naziv).ToList()
                    : items.OrderBy(join => join.recept?.naziv).ToList(),
                "vrijemeKuhanja" => sortDescending
                    ? items.OrderByDescending(join => join.recept?.vrijemeKuhanja ?? 0).ToList()
                    : items.OrderBy(join => join.recept?.vrijemeKuhanja ?? 0).ToList(),
                "brojPorcija" => sortDescending
                    ? items.OrderByDescending(join => join.recept?.brojPorcija ?? 0).ToList()
                    : items.OrderBy(join => join.recept?.brojPorcija ?? 0).ToList(),
                "kreirano" => sortDescending
                    ? items.OrderByDescending(join => join.kreirano).ToList()
                    : items.OrderBy(join => join.kreirano).ToList(),
                _ => items.OrderBy(join => join.id).ToList()
            };
        }

        private async Task<int?> GetCookbookIdAsync(int userId, bool isAdmin)
        {
            if (isAdmin)
            {
                var response = await GetApiResponseAsync<UserDto>($"/api/users/{userId}");
                return response?.success == true ? response.data?.kuharica?.id : null;
            }

            var publicResponse = await GetApiResponseAsync<UserPublicDto>($"/api/users/{userId}");
            return publicResponse?.success == true ? publicResponse.data?.kuharica?.id : null;
        }
    }
}
