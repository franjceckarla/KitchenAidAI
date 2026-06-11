using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace KitchenAidAI.Filters
{
    public class RequireSessionAttribute : ActionFilterAttribute
    {
        private const string LegacyUserIdClaimType = "legacy_user_id";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())
            {
                base.OnActionExecuting(context);
                return;
            }

            var identity = context.HttpContext.User;
            if (identity?.Identity?.IsAuthenticated != true)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            var identityUserId = identity.FindFirstValue(LegacyUserIdClaimType)
                ?? identity.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(identityUserId, out var userId))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            if (context.Controller is Controller controller)
            {
                controller.ViewBag.CurrentUserId = userId;
                controller.ViewBag.CurrentUsername = identity.Identity?.Name;
                controller.ViewBag.IsAdmin = identity.IsInRole("Admin");
            }

            base.OnActionExecuting(context);
        }
    }
}
