using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MonAmour.Helpers;
using MonAmour.Models;

namespace MonAmour.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[]? _roles;

    public AuthorizeAttribute(params string[] roles)
    {
        _roles = roles?.Length > 0 ? roles : null;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Kiểm tra đăng nhập
        if (!AuthHelper.IsAuthenticated(context.HttpContext))
        {
            // Lưu URL hiện tại để redirect sau khi đăng nhập
            var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            context.HttpContext.Session.SetString("ReturnUrl", returnUrl);

            // Redirect to login page
            context.Result = new RedirectToActionResult("Login", "Auth", null);
            return;
        }

        // Kiểm tra role nếu có yêu cầu
        if (_roles != null && _roles.Length > 0)
        {
            var userRoles = AuthHelper.GetUserRoles(context.HttpContext);
            var hasRequiredRole = _roles.Any(role => userRoles.Contains(role));

            if (!hasRequiredRole)
            {
                // User không có role yêu cầu, redirect về trang chủ với thông báo lỗi
                context.HttpContext.Session.SetString("ErrorMessage", "Bạn không có quyền truy cập trang này.");
                context.Result = new RedirectToActionResult("Index", "Home", null);
                return;
            }
        }
    }
}

/// <summary>
/// Attribute chỉ cho phép Admin truy cập
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AdminOnlyAttribute : AuthorizeAttribute
{
    public AdminOnlyAttribute() : base(Role.Names.Admin)
    {
    }
}

/// <summary>
/// Attribute chỉ cho phép User thường truy cập
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class UserOnlyAttribute : AuthorizeAttribute
{
    public UserOnlyAttribute() : base(Role.Names.User)
    {
    }
}
