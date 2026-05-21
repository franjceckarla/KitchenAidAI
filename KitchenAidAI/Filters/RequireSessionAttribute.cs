using KitchenAidAI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace KitchenAidAI.Filters
{
    public class RequireSessionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())
            {
                base.OnActionExecuting(context);
                return;
            }

            var userId = AuthSession.GetUserId(context.HttpContext);
            if (!userId.HasValue)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            if (context.Controller is Controller controller)
            {
                controller.ViewBag.CurrentUserId = userId.Value;
                controller.ViewBag.CurrentUsername = AuthSession.GetUsername(context.HttpContext);
                controller.ViewBag.IsAdmin = AuthSession.IsAdmin(context.HttpContext);
            }

            base.OnActionExecuting(context);
        }
    }
}
