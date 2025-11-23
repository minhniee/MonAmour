using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MonAmour.Helpers;

namespace MonAmour.Filters;

/// <summary>
/// Authorization filter that restricts access to admin users only
/// </summary>
public class AdminAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Check if user is logged in
        if (!AuthHelper.IsLoggedIn(context.HttpContext))
        {
            // Redirect to login page
            context.Result = new RedirectToActionResult("Login", "Auth", new { returnUrl = context.HttpContext.Request.Path });
            return;
        }

        // Check if user is admin
        if (!AuthHelper.IsAdmin(context.HttpContext))
        {
            // Redirect to access denied page
            context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
            return;
        }
    }
}
