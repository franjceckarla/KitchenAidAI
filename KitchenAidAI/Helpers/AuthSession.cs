using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace KitchenAidAI.Helpers
{
    public static class AuthSession
    {
        private const string LegacyUserIdClaimType = "legacy_user_id";

        public static int? GetUserId(HttpContext httpContext)
        {
            if (httpContext.User?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var legacyUserId = httpContext.User.FindFirstValue(LegacyUserIdClaimType);
            if (int.TryParse(legacyUserId, out var parsedLegacyUserId))
            {
                return parsedLegacyUserId;
            }

            var identityUserId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(identityUserId, out var parsedUserId) ? parsedUserId : null;
        }

        public static string? GetUsername(HttpContext httpContext)
        {
            return httpContext.User.Identity?.Name;
        }

        public static bool IsAdmin(HttpContext httpContext)
        {
            if (httpContext.User?.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            return httpContext.User.IsInRole("Admin");
        }
    }
}
