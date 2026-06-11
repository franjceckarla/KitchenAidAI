using KitchenAidAI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KitchenAidAI.Controllers.Api
{
    [Authorize]
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        private const string LegacyUserIdClaimType = "legacy_user_id";

        protected bool TryGetSession(out int userId, out bool isAdmin, out IActionResult? error)
        {
            var identity = HttpContext.User;
            if (identity?.Identity?.IsAuthenticated != true)
            {
                userId = 0;
                isAdmin = false;
                error = Unauthorized(ApiResponse<object>.Fail("Neautorizirano", "Prijavite se za pristup API-ju.", "AUTH_401"));
                return false;
            }

            var identityUserId = identity.FindFirstValue(LegacyUserIdClaimType)
                ?? identity.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(identityUserId, out var parsedUserId))
            {
                userId = parsedUserId;
                isAdmin = identity.IsInRole("Admin");
                error = null;
                return true;
            }

            userId = 0;
            isAdmin = false;
            error = Unauthorized(ApiResponse<object>.Fail("Neautorizirano", "Prijavite se za pristup API-ju.", "AUTH_401"));
            return false;
        }

        protected IActionResult ApiNotFound(string message)
        {
            return NotFound(ApiResponse<object>.Fail("Nije pronadjeno", message, "NOT_FOUND"));
        }

        protected IActionResult ApiForbidden(string message)
        {
            return StatusCode(403, ApiResponse<object>.Fail("Zabranjeno", message, "FORBIDDEN"));
        }

        protected IActionResult ApiBadRequest(string message, string? code = null)
        {
            return BadRequest(ApiResponse<object>.Fail("Neispravan zahtjev", message, code ?? "BAD_REQUEST"));
        }
    }
}
