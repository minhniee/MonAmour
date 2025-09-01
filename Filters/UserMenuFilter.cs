using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MonAmour.Helpers;

namespace MonAmour.Filters;

public class UserMenuFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.Controller is Controller controller)
        {
            if (AuthHelper.IsLoggedIn(controller.HttpContext))
            {
                controller.ViewBag.UserName = AuthHelper.GetUserName(controller.HttpContext);
                controller.ViewBag.UserEmail = AuthHelper.GetUserEmail(controller.HttpContext);
                controller.ViewBag.IsLoggedIn = true;
            }
            else
            {
                controller.ViewBag.IsLoggedIn = false;
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Do nothing
    }
}
