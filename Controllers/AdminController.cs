using Microsoft.AspNetCore.Mvc;
using MonAmour.Attributes;
using MonAmour.Helpers;
using MonAmour.Services.Interfaces;

namespace MonAmour.Controllers
{
    [AdminOnly]
    public class AdminController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IAuthService authService, ILogger<AdminController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Admin Dashboard - chỉ admin mới có thể truy cập
        /// </summary>
        /// <returns></returns>
        public IActionResult Dashboard()
        {
            _logger.LogInformation("Admin user accessed dashboard: {UserId}", 
                AuthHelper.GetUserId(HttpContext));

            // Set ViewBag data for admin dashboard
            ViewBag.UserName = AuthHelper.GetUserName(HttpContext);
            ViewBag.UserEmail = AuthHelper.GetUserEmail(HttpContext);

            return View();
        }

        /// <summary>
        /// User Management - quản lý users
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Users()
        {
            // TODO: Implement user management functionality
            // For now, just return a basic view
            return View();
        }

        /// <summary>
        /// System Settings
        /// </summary>
        /// <returns></returns>
        public IActionResult Settings()
        {
            return View();
        }
    }
}
