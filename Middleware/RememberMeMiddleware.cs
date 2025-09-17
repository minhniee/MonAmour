using MonAmour.Helpers;
using MonAmour.Services.Interfaces;

namespace MonAmour.Middleware;

public class RememberMeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RememberMeMiddleware> _logger;

    public RememberMeMiddleware(RequestDelegate next, ILogger<RememberMeMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuthService authService)
    {
        try
        {
            // Kiểm tra nếu user chưa đăng nhập nhưng có remember me cookie
            if (!context.Session.Keys.Contains("UserId"))
            {
                var rememberToken = context.Request.Cookies["RememberToken"];
                if (!string.IsNullOrEmpty(rememberToken))
                {
                    _logger.LogDebug("Processing remember me token");

                    // Tìm user từ token
                    var user = await authService.GetUserByTokenAsync(rememberToken, "remember_me");
                    if (user is not null)
                    {
                        // Lấy roles của user
                        var roles = await authService.GetUserRolesAsync(user.UserId);

                        // Set session với roles
                        AuthHelper.SetUserSession(context, user, roles);

                        _logger.LogInformation("User automatically logged in via remember me token: {Email}", user.Email);
                    }
                    else
                    {
                        // Token không hợp lệ, xóa cookie
                        context.Response.Cookies.Delete("RememberToken");
                        _logger.LogWarning("Invalid remember me token found and removed");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing remember me token");
            // Xóa cookie nếu có lỗi
            context.Response.Cookies.Delete("RememberToken");
        }

        await _next(context);
    }
}
