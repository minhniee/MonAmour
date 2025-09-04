using Microsoft.EntityFrameworkCore;
using MonAmour.Models;

namespace MonAmour.Helpers;

/// <summary>
/// Helper class for role management and authorization
/// </summary>
public static class RoleHelper
{
    /// <summary>
    /// Get all roles for a specific user
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="userId">User ID</param>
    /// <returns>List of role names</returns>
    public static async Task<List<string>> GetUserRolesAsync(MonAmourDbContext context, int userId)
    {
        try
        {
            return await context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role.RoleName)
                .ToListAsync();
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// Check if user has a specific role
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="userId">User ID</param>
    /// <param name="roleName">Role name to check</param>
    /// <returns>True if user has the role</returns>
    public static async Task<bool> UserHasRoleAsync(MonAmourDbContext context, int userId, string roleName)
    {
        try
        {
            return await context.UserRoles
                .AnyAsync(ur => ur.UserId == userId &&
                               ur.Role.RoleName == roleName);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Check if user is admin
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="userId">User ID</param>
    /// <returns>True if user is admin</returns>
    public static async Task<bool> IsAdminAsync(MonAmourDbContext context, int userId)
    {
        return await UserHasRoleAsync(context, userId, Role.Names.Admin);
    }

    /// <summary>
    /// Check if user is regular user
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="userId">User ID</param>
    /// <returns>True if user has user role</returns>
    public static async Task<bool> IsUserAsync(MonAmourDbContext context, int userId)
    {
        return await UserHasRoleAsync(context, userId, Role.Names.User);
    }

    /// <summary>
    /// Check if user has any of the specified roles
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="userId">User ID</param>
    /// <param name="roles">Roles to check</param>
    /// <returns>True if user has any of the roles</returns>
    public static async Task<bool> UserHasAnyRoleAsync(MonAmourDbContext context, int userId, params string[] roles)
    {
        try
        {
            return await context.UserRoles
                .AnyAsync(ur => ur.UserId == userId &&
                               roles.Contains(ur.Role.RoleName));
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Check if user has all of the specified roles
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="userId">User ID</param>
    /// <param name="roles">Roles to check</param>
    /// <returns>True if user has all of the roles</returns>
    public static async Task<bool> UserHasAllRolesAsync(MonAmourDbContext context, int userId, params string[] roles)
    {
        try
        {
            var userRoleCount = await context.UserRoles
                .Where(ur => ur.UserId == userId &&
                            roles.Contains(ur.Role.RoleName))
                .CountAsync();

            return userRoleCount == roles.Length;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Assign a role to a user
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="userId">User ID</param>
    /// <param name="roleName">Role name to assign</param>
    /// <param name="assignedBy">ID of user who assigned the role (optional)</param>
    /// <returns>True if role was assigned successfully</returns>
    public static async Task<bool> AssignRoleToUserAsync(MonAmourDbContext context, int userId, string roleName, int? assignedBy = null)
    {
        try
        {
            // Check if user exists
            var user = await context.Users.FindAsync(userId);
            if (user == null) return false;

            // Check if role exists
            var role = await context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == roleName);
            if (role == null) return false;

            // Check if user already has this role
            var existingUserRole = await context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == role.RoleId);

            if (existingUserRole != null) return true; // User already has this role

            // Assign role to user
            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = role.RoleId,
                AssignedAt = DateTime.Now,
                AssignedBy = assignedBy
            };

            context.UserRoles.Add(userRole);
            await context.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Remove a role from a user
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="userId">User ID</param>
    /// <param name="roleName">Role name to remove</param>
    /// <returns>True if role was removed successfully</returns>
    public static async Task<bool> RemoveRoleFromUserAsync(MonAmourDbContext context, int userId, string roleName)
    {
        try
        {
            // Find the role
            var role = await context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == roleName);
            if (role == null) return false;

            // Find the user role
            var userRole = await context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == role.RoleId);

            if (userRole == null) return true; // User doesn't have this role

            // Remove the role
            context.UserRoles.Remove(userRole);
            await context.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get all users with a specific role
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="roleName">Role name</param>
    /// <returns>List of users with the role</returns>
    public static async Task<List<User>> GetUsersWithRoleAsync(MonAmourDbContext context, string roleName)
    {
        try
        {
            return await context.UserRoles
                .Where(ur => ur.Role.RoleName == roleName)
                .Select(ur => ur.User)
                .ToListAsync();
        }
        catch
        {
            return new List<User>();
        }
    }

    /// <summary>
    /// Get all admins
    /// </summary>
    /// <param name="context">Database context</param>
    /// <returns>List of admin users</returns>
    public static async Task<List<User>> GetAdminsAsync(MonAmourDbContext context)
    {
        return await GetUsersWithRoleAsync(context, Role.Names.Admin);
    }

    /// <summary>
    /// Get all regular users
    /// </summary>
    /// <param name="context">Database context</param>
    /// <returns>List of regular users</returns>
    public static async Task<List<User>> GetRegularUsersAsync(MonAmourDbContext context)
    {
        return await GetUsersWithRoleAsync(context, Role.Names.User);
    }

    /// <summary>
    /// Get all available roles
    /// </summary>
    /// <param name="context">Database context</param>
    /// <returns>List of roles</returns>
    public static async Task<List<Role>> GetAllRolesAsync(MonAmourDbContext context)
    {
        try
        {
            return await context.Roles
                .OrderBy(r => r.RoleName)
                .ToListAsync();
        }
        catch
        {
            return new List<Role>();
        }
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="roleName">Role name</param>
    /// <returns>True if role was created successfully</returns>
    public static async Task<bool> CreateRoleAsync(MonAmourDbContext context, string roleName)
    {
        try
        {
            // Check if role already exists
            var existingRole = await context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == roleName);
            if (existingRole != null) return false;

            // Create new role
            var role = new Role
            {
                RoleName = roleName
            };

            context.Roles.Add(role);
            await context.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Initialize default system roles (Admin and User)
    /// </summary>
    /// <param name="context">Database context</param>
    /// <returns>True if initialization was successful</returns>
    public static async Task<bool> InitializeDefaultRolesAsync(MonAmourDbContext context)
    {
        try
        {
            // Create Admin role if it doesn't exist
            var adminRole = await context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == Role.Names.Admin);
            if (adminRole == null)
            {
                adminRole = new Role
                {
                    RoleName = Role.Names.Admin
                };
                context.Roles.Add(adminRole);
            }

            // Create User role if it doesn't exist
            var userRole = await context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == Role.Names.User);
            if (userRole == null)
            {
                userRole = new Role
                {
                    RoleName = Role.Names.User
                };
                context.Roles.Add(userRole);
            }

            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Check if a role exists
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="roleName">Role name to check</param>
    /// <returns>True if role exists</returns>
    public static async Task<bool> RoleExistsAsync(MonAmourDbContext context, string roleName)
    {
        try
        {
            return await context.Roles
                .AnyAsync(r => r.RoleName == roleName);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get role by name
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="roleName">Role name</param>
    /// <returns>Role object or null if not found</returns>
    public static async Task<Role?> GetRoleByNameAsync(MonAmourDbContext context, string roleName)
    {
        try
        {
            return await context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == roleName);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Get user role assignments with details
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="userId">User ID</param>
    /// <returns>List of user role assignments</returns>
    public static async Task<List<UserRole>> GetUserRoleAssignmentsAsync(MonAmourDbContext context, int userId)
    {
        try
        {
            return await context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId)
                .ToListAsync();
        }
        catch
        {
            return new List<UserRole>();
        }
    }

    /// <summary>
    /// Check if user can be assigned a specific role (business logic validation)
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="userId">User ID</param>
    /// <param name="roleName">Role name</param>
    /// <returns>True if user can be assigned the role</returns>
    public static async Task<bool> CanAssignRoleAsync(MonAmourDbContext context, int userId, string roleName)
    {
        try
        {
            // Check if user exists and is active
            var user = await context.Users.FindAsync(userId);
            if (user == null || user.Status != "active") return false;

            // Check if role exists
            var role = await context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == roleName);
            if (role == null) return false;

            // Additional business logic can be added here
            // For example: prevent removing the last admin, etc.

            return true;
        }
        catch
        {
            return false;
        }
    }
}
