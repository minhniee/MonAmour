using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonAmour.Models;

namespace MonAmour.Helpers;

/// <summary>
/// Example usage of AuthHelper and RoleHelper
/// This file demonstrates how to use the authentication and role helpers
/// </summary>
public static class AuthHelperExample
{
    /// <summary>
    /// Example: How to use AuthHelper in a controller
    /// </summary>
    public static class ControllerExamples
    {
        /// <summary>
        /// Example controller method showing authentication checks
        /// </summary>
        public static IActionResult ExampleControllerMethod(HttpContext context)
        {
            // Check if user is authenticated
            if (!AuthHelper.IsAuthenticated(context))
            {
                return new RedirectToActionResult("Login", "Auth", null);
            }

            // Get current user information
            var userId = AuthHelper.GetUserId(context);
            var userEmail = AuthHelper.GetUserEmail(context);
            var userName = AuthHelper.GetUserName(context);
            var userRoles = AuthHelper.GetUserRoles(context);

            // Check specific roles
            if (AuthHelper.IsAdmin(context))
            {
                // Admin-only logic
                return new OkObjectResult("Admin access granted");
            }

            if (AuthHelper.IsUser(context))
            {
                // User-only logic
                return new OkObjectResult("User access granted");
            }

            // Check multiple roles
            if (AuthHelper.HasAnyRole(context, "admin", "moderator"))
            {
                // User has admin or moderator role
                return new OkObjectResult("Moderator or admin access");
            }

            return new ForbidResult();
        }

        /// <summary>
        /// Example of getting current user session info
        /// </summary>
        public static UserSessionInfo? GetCurrentUserInfo(HttpContext context)
        {
            return AuthHelper.GetCurrentUser(context);
        }

        /// <summary>
        /// Example of updating user session after profile changes
        /// </summary>
        public static void UpdateUserSessionExample(HttpContext context, User updatedUser, List<string> updatedRoles)
        {
            AuthHelper.UpdateUserSession(context, updatedUser, updatedRoles);
        }
    }

    /// <summary>
    /// Example: How to use RoleHelper with database context
    /// </summary>
    public static class RoleHelperExamples
    {
        /// <summary>
        /// Example of checking user roles in database
        /// </summary>
        public static async Task<bool> CheckUserRoleExample(MonAmourDbContext context, int userId, string roleName)
        {
            return await RoleHelper.UserHasRoleAsync(context, userId, roleName);
        }

        /// <summary>
        /// Example of assigning role to user
        /// </summary>
        public static async Task<bool> AssignRoleExample(MonAmourDbContext context, int userId, string roleName, int? assignedBy = null)
        {
            return await RoleHelper.AssignRoleToUserAsync(context, userId, roleName, assignedBy);
        }

        /// <summary>
        /// Example of getting all users with specific role
        /// </summary>
        public static async Task<List<User>> GetUsersWithRoleExample(MonAmourDbContext context, string roleName)
        {
            return await RoleHelper.GetUsersWithRoleAsync(context, roleName);
        }

        /// <summary>
        /// Example of getting all admins
        /// </summary>
        public static async Task<List<User>> GetAllAdminsExample(MonAmourDbContext context)
        {
            return await RoleHelper.GetAdminsAsync(context);
        }
    }

    /// <summary>
    /// Example: How to use in middleware or filters
    /// </summary>
    public static class MiddlewareExamples
    {
        /// <summary>
        /// Example middleware logic for authentication
        /// </summary>
        public static async Task<bool> ProcessRequestAsync(HttpContext context)
        {
            // Check if user is authenticated
            if (!AuthHelper.IsAuthenticated(context))
            {
                // Handle unauthenticated request
                return false;
            }

            // Update last activity
            AuthHelper.UpdateLastActivity(context);

            // Check if session is expiring
            if (AuthHelper.IsSessionExpiring(context))
            {
                // Optionally extend session or warn user
                // This is just an example - actual implementation depends on requirements
            }

            return true;
        }
    }

    /// <summary>
    /// Example: How to use in views
    /// </summary>
    public static class ViewExamples
    {
        /// <summary>
        /// Example of checking authentication in view logic
        /// </summary>
        public static bool ShouldShowAdminMenu(HttpContext context)
        {
            return AuthHelper.IsAdmin(context);
        }

        /// <summary>
        /// Example of getting user display name
        /// </summary>
        public static string GetUserDisplayName(HttpContext context)
        {
            var userName = AuthHelper.GetUserName(context);
            var userEmail = AuthHelper.GetUserEmail(context);

            return !string.IsNullOrEmpty(userName) ? userName : userEmail ?? "Unknown User";
        }

        /// <summary>
        /// Example of checking if user can access specific feature
        /// </summary>
        public static bool CanAccessFeature(HttpContext context, string featureName)
        {
            // Example business logic for feature access
            switch (featureName.ToLower())
            {
                case "admin_panel":
                    return AuthHelper.IsAdmin(context);
                case "user_profile":
                    return AuthHelper.IsAuthenticated(context);
                case "moderator_tools":
                    return AuthHelper.HasAnyRole(context, "admin", "moderator");
                default:
                    return false;
            }
        }
    }
}

/// <summary>
/// Example usage in a real controller
/// </summary>
public class ExampleController : Controller
{
    private readonly MonAmourDbContext _context;

    public ExampleController(MonAmourDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Example action that requires authentication
    /// </summary>
    public IActionResult Profile()
    {
        // Check authentication using AuthHelper
        if (!AuthHelper.IsAuthenticated(HttpContext))
        {
            return RedirectToAction("Login", "Auth");
        }

        var userInfo = AuthHelper.GetCurrentUser(HttpContext);
        return View(userInfo);
    }

    /// <summary>
    /// Example action that requires admin role
    /// </summary>
    public IActionResult AdminPanel()
    {
        // Check authentication and admin role
        if (!AuthHelper.IsAuthenticated(HttpContext))
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!AuthHelper.IsAdmin(HttpContext))
        {
            TempData["ErrorMessage"] = "You don't have permission to access this page.";
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    /// <summary>
    /// Example action that works with roles in database
    /// </summary>
    public async Task<IActionResult> ManageUsers()
    {
        // Check authentication and admin role
        if (!AuthHelper.IsAuthenticated(HttpContext) || !AuthHelper.IsAdmin(HttpContext))
        {
            return Forbid();
        }

        // Get all users with their roles
        var allUsers = await _context.Users.ToListAsync();
        var userRoles = new Dictionary<int, List<string>>();

        foreach (var user in allUsers)
        {
            userRoles[user.UserId] = await RoleHelper.GetUserRolesAsync(_context, user.UserId);
        }

        ViewBag.UserRoles = userRoles;
        return View(allUsers);
    }

    /// <summary>
    /// Example action to assign role to user
    /// </summary>
    public async Task<IActionResult> AssignRole(int userId, string roleName)
    {
        // Check authentication and admin role
        if (!AuthHelper.IsAuthenticated(HttpContext) || !AuthHelper.IsAdmin(HttpContext))
        {
            return Forbid();
        }

        var currentUserId = AuthHelper.GetUserId(HttpContext);
        var success = await RoleHelper.AssignRoleToUserAsync(_context, userId, roleName, currentUserId);

        if (success)
        {
            TempData["SuccessMessage"] = $"Role '{roleName}' assigned successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to assign role.";
        }

        return RedirectToAction("ManageUsers");
    }
}
