using Microsoft.AspNetCore.Http;
using MonAmour.Models;

namespace MonAmour.Helpers;

/// <summary>
/// Helper class for authentication and session management
/// </summary>
public static class AuthHelper
{
    // Session keys
    private const string USER_ID_KEY = "UserId";
    private const string USER_EMAIL_KEY = "UserEmail";
    private const string USER_NAME_KEY = "UserName";
    private const string USER_ROLES_KEY = "UserRoles";
    private const string IS_AUTHENTICATED_KEY = "IsAuthenticated";

    /// <summary>
    /// Set user session data after successful login
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <param name="user">User object</param>
    /// <param name="roles">List of user roles</param>
    public static void SetUserSession(HttpContext context, User user, List<string> roles)
    {
        if (context?.Session == null) return;

        context.Session.SetInt32(USER_ID_KEY, user.UserId);
        context.Session.SetString(USER_EMAIL_KEY, user.Email);
        context.Session.SetString(USER_NAME_KEY, user.Name ?? "");
        context.Session.SetString(USER_ROLES_KEY, string.Join(",", roles));
        context.Session.SetString(IS_AUTHENTICATED_KEY, "true");
    }

    /// <summary>
    /// Clear user session data on logout
    /// </summary>
    /// <param name="context">HTTP context</param>
    public static void ClearUserSession(HttpContext context)
    {
        if (context?.Session == null) return;

        context.Session.Remove(USER_ID_KEY);
        context.Session.Remove(USER_EMAIL_KEY);
        context.Session.Remove(USER_NAME_KEY);
        context.Session.Remove(USER_ROLES_KEY);
        context.Session.Remove(IS_AUTHENTICATED_KEY);
        context.Session.Clear();
    }

    /// <summary>
    /// Check if user is authenticated
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <returns>True if user is authenticated</returns>
    public static bool IsAuthenticated(HttpContext context)
    {
        if (context?.Session == null) return false;
        
        var isAuthenticated = context.Session.GetString(IS_AUTHENTICATED_KEY);
        var userId = context.Session.GetInt32(USER_ID_KEY);
        
        return isAuthenticated == "true" && userId.HasValue;
    }

    /// <summary>
    /// Check if user is logged in (alias for IsAuthenticated)
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <returns>True if user is logged in</returns>
    public static bool IsLoggedIn(HttpContext context)
    {
        return IsAuthenticated(context);
    }

    /// <summary>
    /// Get current user ID from session
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <returns>User ID or null if not authenticated</returns>
    public static int? GetUserId(HttpContext context)
    {
        if (!IsAuthenticated(context)) return null;
        return context.Session.GetInt32(USER_ID_KEY);
    }

    /// <summary>
    /// Get current user email from session
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <returns>User email or null if not authenticated</returns>
    public static string? GetUserEmail(HttpContext context)
    {
        if (!IsAuthenticated(context)) return null;
        return context.Session.GetString(USER_EMAIL_KEY);
    }

    /// <summary>
    /// Get current user name from session
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <returns>User name or null if not authenticated</returns>
    public static string? GetUserName(HttpContext context)
    {
        if (!IsAuthenticated(context)) return null;
        return context.Session.GetString(USER_NAME_KEY);
    }

    /// <summary>
    /// Get current user roles from session
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <returns>List of user roles</returns>
    public static List<string> GetUserRoles(HttpContext context)
    {
        if (!IsAuthenticated(context)) return new List<string>();
        
        var rolesString = context.Session.GetString(USER_ROLES_KEY);
        if (string.IsNullOrEmpty(rolesString))
            return new List<string>();
            
        return rolesString.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    /// <summary>
    /// Check if current user has a specific role
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <param name="roleName">Role name to check</param>
    /// <returns>True if user has the role</returns>
    public static bool HasRole(HttpContext context, string roleName)
    {
        var roles = GetUserRoles(context);
        return roles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Check if current user is admin
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <returns>True if user is admin</returns>
    public static bool IsAdmin(HttpContext context)
    {
        return HasRole(context, Role.Names.Admin);
    }

    /// <summary>
    /// Check if current user is regular user
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <returns>True if user has user role</returns>
    public static bool IsUser(HttpContext context)
    {
        return HasRole(context, Role.Names.User);
    }

    /// <summary>
    /// Check if current user has any of the specified roles
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <param name="roles">Roles to check</param>
    /// <returns>True if user has any of the roles</returns>
    public static bool HasAnyRole(HttpContext context, params string[] roles)
    {
        var userRoles = GetUserRoles(context);
        return roles.Any(role => userRoles.Contains(role, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Check if current user has all of the specified roles
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <param name="roles">Roles to check</param>
    /// <returns>True if user has all of the roles</returns>
    public static bool HasAllRoles(HttpContext context, params string[] roles)
    {
        var userRoles = GetUserRoles(context);
        return roles.All(role => userRoles.Contains(role, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get current user information as a simple object
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <returns>User info object or null if not authenticated</returns>
    public static UserSessionInfo? GetCurrentUser(HttpContext context)
    {
        if (!IsAuthenticated(context)) return null;

        return new UserSessionInfo
        {
            UserId = GetUserId(context) ?? 0,
            Email = GetUserEmail(context) ?? "",
            Name = GetUserName(context) ?? "",
            Roles = GetUserRoles(context)
        };
    }

    /// <summary>
    /// Update user session with new data (useful after profile updates)
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <param name="user">Updated user object</param>
    /// <param name="roles">Updated roles list</param>
    public static void UpdateUserSession(HttpContext context, User user, List<string> roles)
    {
        if (context?.Session == null) return;

        // Only update if user is already authenticated
        if (!IsAuthenticated(context)) return;

        context.Session.SetString(USER_EMAIL_KEY, user.Email);
        context.Session.SetString(USER_NAME_KEY, user.Name ?? "");
        context.Session.SetString(USER_ROLES_KEY, string.Join(",", roles));
    }

    /// <summary>
    /// Check if session is about to expire (within 5 minutes)
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <returns>True if session is about to expire</returns>
    public static bool IsSessionExpiring(HttpContext context)
    {
        if (context?.Session == null) return true;
        
        // Check if session has been idle for more than 25 minutes (assuming 30 min timeout)
        var lastActivity = context.Session.GetString("LastActivity");
        if (string.IsNullOrEmpty(lastActivity)) return true;

        if (DateTime.TryParse(lastActivity, out var lastActivityTime))
        {
            return DateTime.Now.Subtract(lastActivityTime).TotalMinutes > 25;
        }

        return true;
    }

    /// <summary>
    /// Update last activity timestamp
    /// </summary>
    /// <param name="context">HTTP context</param>
    public static void UpdateLastActivity(HttpContext context)
    {
        if (context?.Session == null) return;
        context.Session.SetString("LastActivity", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    /// <summary>
    /// Get default redirect URL after login
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <returns>Default redirect URL</returns>
    public static string GetDefaultRedirectUrl(HttpContext context)
    {
        // Check if there's a return URL in session
        var returnUrl = context.Session.GetString("ReturnUrl");
        if (!string.IsNullOrEmpty(returnUrl))
        {
            // Clear the return URL from session
            context.Session.Remove("ReturnUrl");
            return returnUrl;
        }

        // Default redirect based on user role
        if (IsAdmin(context))
        {
            return "/Admin/Dashboard";
        }
        else if (IsUser(context))
        {
            return "/Profile";
        }

        // Default fallback
        return "/";
    }
}

/// <summary>
/// Simple user session information class
/// </summary>
public class UserSessionInfo
{
    public int UserId { get; set; }
    public string Email { get; set; } = "";
    public string Name { get; set; } = "";
    public List<string> Roles { get; set; } = new();
}
