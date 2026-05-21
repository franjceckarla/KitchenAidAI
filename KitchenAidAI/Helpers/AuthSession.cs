using KitchenAidAI.Models;
using Microsoft.AspNetCore.Http;

namespace KitchenAidAI.Helpers
{
    public static class AuthSession
    {
        public const string UserIdKey = "UserId";
        public const string UsernameKey = "Username";
        public const string IsAdminKey = "IsAdmin";

        public static void SignIn(HttpContext httpContext, User user)
        {
            httpContext.Session.SetInt32(UserIdKey, user.id);
            httpContext.Session.SetString(UsernameKey, user.username ?? string.Empty);
            httpContext.Session.SetString(IsAdminKey, user.isAdmin ? "true" : "false");
        }

        public static void SignOut(HttpContext httpContext)
        {
            httpContext.Session.Clear();
        }

        public static int? GetUserId(HttpContext httpContext)
        {
            return httpContext.Session.GetInt32(UserIdKey);
        }

        public static string? GetUsername(HttpContext httpContext)
        {
            return httpContext.Session.GetString(UsernameKey);
        }

        public static bool IsAdmin(HttpContext httpContext)
        {
            return string.Equals(httpContext.Session.GetString(IsAdminKey), "true", StringComparison.OrdinalIgnoreCase);
        }
    }
}
