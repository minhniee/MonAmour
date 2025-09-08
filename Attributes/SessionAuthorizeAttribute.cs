using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MonAmour.Helpers;

namespace MonAmour.Attributes
{
    /// <summary>
    /// Custom authorization attribute that checks session-based authentication
    /// </summary>
    public class SessionAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Check if user is authenticated via session
            if (!AuthHelper.IsAuthenticated(context.HttpContext))
            {
                // User is not authenticated, redirect to login
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.Result = new RedirectToActionResult("Login", "Auth", new { ReturnUrl = returnUrl });
                return;
            }

            // User is authenticated, allow access
        }
    }
}
