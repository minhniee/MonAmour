using Microsoft.EntityFrameworkCore;
using MonAmour.Models;
using MonAmour.Services.Interfaces;
using MonAmour.ViewModels;
using System.Security.Cryptography;
using System.Text;

namespace MonAmour.Services.Implements;

public class UserManagementService : IUserManagementService
{
    private readonly MonAmourDbContext _context;
    private readonly ILogger<UserManagementService> _logger;
    private readonly ICloudinaryService _cloudinaryService;

    public UserManagementService(MonAmourDbContext context, ILogger<UserManagementService> logger, ICloudinaryService cloudinaryService)
    {
        _context = context;
        _logger = logger;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<(List<AdminUserViewModel.UserListViewModel> Users, int TotalCount)> GetUsersAsync(AdminUserViewModel.UserSearchViewModel searchModel)
    {
        try
        {
            var query = _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsQueryable();

            // Apply search filters
            if (!string.IsNullOrEmpty(searchModel.SearchTerm))
            {
                query = query.Where(u =>
                    u.Email.Contains(searchModel.SearchTerm) ||
                    u.Name!.Contains(searchModel.SearchTerm) ||
                    u.Phone!.Contains(searchModel.SearchTerm));
            }

            if (!string.IsNullOrEmpty(searchModel.Status))
            {
                query = query.Where(u => u.Status == searchModel.Status);
            }

            if (!string.IsNullOrEmpty(searchModel.Gender))
            {
                query = query.Where(u => u.Gender == searchModel.Gender);
            }

            if (searchModel.Verified.HasValue)
            {
                query = query.Where(u => u.Verified == searchModel.Verified.Value);
            }

            if (searchModel.RoleId.HasValue)
            {
                query = query.Where(u => u.UserRoles.Any(ur => ur.RoleId == searchModel.RoleId.Value));
            }

            if (searchModel.CreatedFrom.HasValue)
            {
                query = query.Where(u => u.CreatedAt >= searchModel.CreatedFrom.Value);
            }

            if (searchModel.CreatedTo.HasValue)
            {
                query = query.Where(u => u.CreatedAt <= searchModel.CreatedTo.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((searchModel.Page - 1) * searchModel.PageSize)
                .Take(searchModel.PageSize)
                .Select(u => new AdminUserViewModel.UserListViewModel
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    Name = u.Name,
                    Phone = u.Phone,
                    Avatar = u.Avatar,
                    Verified = u.Verified,
                    Gender = u.Gender,
                    Status = u.Status,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    Roles = u.UserRoles.Select(ur => ur.Role.RoleName).ToList()
                })
                .ToListAsync();

            return (users, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            throw;
        }
    }

    public async Task<List<AdminUserViewModel.UserListViewModel>> GetAllUsersAsync()
    {
        try
        {
            var users = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Select(u => new AdminUserViewModel.UserListViewModel
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    Name = u.Name,
                    Phone = u.Phone,
                    Avatar = u.Avatar,
                    Verified = u.Verified,
                    Gender = u.Gender,
                    Status = u.Status,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    Roles = u.UserRoles.Select(ur => ur.Role.RoleName).ToList()
                })
                .ToListAsync();

            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            throw;
        }
    }

    public async Task<AdminUserViewModel.UserDetailViewModel?> GetUserByIdAsync(int userId)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Include(u => u.Orders)
                .Include(u => u.Reviews)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return null;

            return new AdminUserViewModel.UserDetailViewModel
            {
                UserId = user.UserId,
                Email = user.Email,
                Name = user.Name,
                Phone = user.Phone,
                Avatar = user.Avatar,
                BirthDate = user.BirthDate.HasValue ? new DateTime(user.BirthDate.Value.Year, user.BirthDate.Value.Month, user.BirthDate.Value.Day) : null,
                Verified = user.Verified,
                Gender = user.Gender,
                Status = user.Status,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = user.UserRoles.Select(ur => new AdminUserViewModel.RoleViewModel
                {
                    RoleId = ur.RoleId,
                    RoleName = ur.Role.RoleName,
                    AssignedAt = ur.AssignedAt,
                    AssignedBy = ur.AssignedBy
                }).ToList(),
                TotalOrders = user.Orders.Count,
                TotalReviews = user.Reviews.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> CreateUserAsync(AdminUserViewModel.UserCreateViewModel model, int adminUserId)
    {
        try
        {
            // Use execution strategy to handle retries and transactions
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _logger.LogInformation("CreateUserAsync started with email: {Email}, phone: {Phone}, adminUserId: {AdminUserId}",
                        model.Email, model.Phone, adminUserId);

                    // Check for duplicates
                    var (emailExists, phoneExists) = await CheckDuplicateAsync(model.Email, model.Phone);

                    if (emailExists)
                    {
                        _logger.LogWarning("Email already exists: {Email}", model.Email);
                        throw new InvalidOperationException("EMAIL_EXISTS");
                    }

                    if (phoneExists)
                    {
                        _logger.LogWarning("Phone already exists: {Phone}", model.Phone);
                        throw new InvalidOperationException("PHONE_EXISTS");
                    }

                    _logger.LogInformation("Email and phone are available: {Email}, {Phone}", model.Email, model.Phone);

                    // Validate roles exist
                    var validRoleIds = await _context.Roles
                        .Where(r => model.RoleIds.Contains(r.RoleId))
                        .Select(r => r.RoleId)
                        .ToListAsync();

                    if (validRoleIds.Count != model.RoleIds.Count)
                    {
                        _logger.LogWarning("Some roles don't exist. Requested: {RequestedRoles}, Valid: {ValidRoles}",
                            string.Join(",", model.RoleIds), string.Join(",", validRoleIds));
                        throw new InvalidOperationException("INVALID_ROLES");
                    }

                    var user = new User
                    {
                        Email = model.Email.Trim(),
                        Password = HashPassword(model.Password),
                        Name = model.Name?.Trim(),
                        Phone = model.Phone?.Trim(),
                        Avatar = string.IsNullOrWhiteSpace(model.Avatar) ? null : model.Avatar.Trim(),
                        BirthDate = model.BirthDate.HasValue ? DateOnly.FromDateTime(model.BirthDate.Value) : null,
                        Gender = model.Gender,
                        Status = model.Status,
                        Verified = model.Verified,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    _logger.LogInformation("Creating user object: Email={Email}, Name={Name}, Phone={Phone}",
                        user.Email, user.Name, user.Phone);

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("User saved to database, UserId: {UserId}", user.UserId);

                    // Assign roles
                    _logger.LogInformation("Assigning roles: {RoleIds}", string.Join(",", model.RoleIds));
                    foreach (var roleId in model.RoleIds)
                    {
                        var userRole = new UserRole
                        {
                            UserId = user.UserId,
                            RoleId = roleId,
                            AssignedAt = DateTime.Now,
                            AssignedBy = adminUserId
                        };
                        _context.UserRoles.Add(userRole);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("User created successfully: {UserId} by admin {AdminUserId}",
                        user.UserId, adminUserId);
                    return true;
                }
                catch (InvalidOperationException)
                {
                    await transaction.RollbackAsync();
                    throw; // Re-throw để controller xử lý
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error in transaction for user creation with email: {Email}", model.Email);
                    throw;
                }
            });
        }
        catch (InvalidOperationException)
        {
            throw; // Re-throw business logic exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user with email: {Email}, phone: {Phone}", model.Email, model.Phone);
            return false;
        }
    }


    public async Task<bool> UpdateUserAsync(AdminUserViewModel.UserEditViewModel model, int adminUserId)
    {
        try
        {
            var user = await _context.Users
                .AsTracking()
                .FirstOrDefaultAsync(u => u.UserId == model.UserId);
            if (user == null) return false;

            // Check if email already exists (excluding current user)
            if (await IsEmailExistsAsync(model.Email, model.UserId))
            {
                return false;
            }

            user.Email = model.Email;
            user.Name = model.Name;
            user.Phone = model.Phone;
            user.Avatar = model.Avatar;
            user.BirthDate = model.BirthDate.HasValue ? DateOnly.FromDateTime(model.BirthDate.Value) : null;
            user.Gender = model.Gender;
            user.Status = model.Status;
            user.Verified = model.Verified;
            user.UpdatedAt = DateTime.Now;

            // Update roles
            var existingRoles = await _context.UserRoles
                .Where(ur => ur.UserId == model.UserId)
                .AsTracking()
                .ToListAsync();

            _context.UserRoles.RemoveRange(existingRoles);

            foreach (var roleId in model.RoleIds)
            {
                var userRole = new UserRole
                {
                    UserId = model.UserId,
                    RoleId = roleId,
                    AssignedAt = DateTime.Now,
                    AssignedBy = adminUserId
                };
                _context.UserRoles.Add(userRole);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("User updated successfully: {UserId} by admin {AdminUserId}", model.UserId, adminUserId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", model.UserId);
            return false;
        }
    }

    public async Task<bool> DeleteUserAsync(int userId, int adminUserId)
    {
        try
        {
            var user = await _context.Users
                .AsTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return false;

            // Check if user is admin (prevent deleting admin)
            var isAdmin = await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.Role.RoleName == Role.Names.Admin);

            if (isAdmin)
            {
                _logger.LogWarning("Attempt to delete admin user: {UserId} by admin {AdminUserId}", userId, adminUserId);
                return false;
            }

            // Delete related data
            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .AsTracking()
                .ToListAsync();
            _context.UserRoles.RemoveRange(userRoles);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User deleted successfully: {UserId} by admin {AdminUserId}", userId, adminUserId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> ChangeUserPasswordAsync(AdminUserViewModel.UserChangePasswordViewModel model, int adminUserId)
    {

        try
        {
            var user = await _context.Users
                .AsTracking()
                .FirstOrDefaultAsync(u => u.UserId == model.UserId);
            if (user == null) return false;

            user.Password = HashPassword(model.NewPassword);
            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            _logger.LogInformation("User password changed successfully: {UserId} by admin {AdminUserId}", model.UserId, adminUserId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing user password: {UserId}", model.UserId);
            return false;
        }

    }

    public async Task<bool> ToggleUserVerificationAsync(int userId, int adminUserId)
    {
        try
        {
            var user = await _context.Users
                .AsTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return false;

            user.Verified = !user.Verified;
            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            _logger.LogInformation("User verification toggled: {UserId} to {Verified} by admin {AdminUserId}",
                userId, user.Verified, adminUserId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling user verification: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> ToggleUserStatusAsync(int userId, string status, int adminUserId)
    {
        try
        {
            var user = await _context.Users
                .AsTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return false;

            user.Status = status;
            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            _logger.LogInformation("User status changed: {UserId} to {Status} by admin {AdminUserId}",
                userId, status, adminUserId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing user status: {UserId}", userId);
            return false;
        }
    }

    public async Task<List<AdminUserViewModel.RoleViewModel>> GetAllRolesAsync()
    {
        try
        {
            _logger.LogInformation("GetAllRolesAsync called");

            var roles = await _context.Roles
                .Select(r => new AdminUserViewModel.RoleViewModel
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName
                })
                .ToListAsync();

            _logger.LogInformation("Found {Count} roles: {Roles}",
                roles.Count, string.Join(", ", roles.Select(r => $"{r.RoleName}({r.RoleId})")));

            return roles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all roles");
            throw;
        }
    }

    public async Task<bool> IsEmailExistsAsync(string email, int? excludeUserId = null)
    {
        try
        {
            _logger.LogInformation("IsEmailExistsAsync called with email: {Email}, excludeUserId: {ExcludeUserId}",
                email, excludeUserId);

            // Normalize email để tránh case-sensitive
            var normalizedEmail = email.Trim().ToLower();

            var query = _context.Users.Where(u => u.Email.ToLower() == normalizedEmail);
            if (excludeUserId.HasValue)
            {
                query = query.Where(u => u.UserId != excludeUserId.Value);
            }

            var exists = await query.AnyAsync();
            _logger.LogInformation("Email {Email} exists: {Exists} (normalized: {NormalizedEmail})",
                email, exists, normalizedEmail);

            // Debug: Log existing emails
            var existingEmails = await _context.Users.Select(u => u.Email).ToListAsync();
            _logger.LogInformation("All existing emails: {Emails}", string.Join(", ", existingEmails));
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email existence: {Email}", email);
            throw;
        }
    }


    public async Task<Dictionary<string, int>> GetUserStatisticsAsync()
    {
        try
        {
            var totalUsers = await _context.Users.CountAsync();
            var verifiedUsers = await _context.Users.CountAsync(u => u.Verified == true);
            var activeUsers = await _context.Users.CountAsync(u => u.Status == "active");
            var adminUsers = await _context.UserRoles
                .CountAsync(ur => ur.Role.RoleName == Role.Names.Admin);

            return new Dictionary<string, int>
            {
                ["TotalUsers"] = totalUsers,
                ["VerifiedUsers"] = verifiedUsers,
                ["ActiveUsers"] = activeUsers,
                ["AdminUsers"] = adminUsers
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user statistics");
            throw;
        }
    }
    public async Task<bool> IsPhoneExistsAsync(string phone, int? excludeUserId = null)
    {
        try
        {
            _logger.LogInformation("IsPhoneExistsAsync called with phone: {Phone}, excludeUserId: {ExcludeUserId}",
                phone, excludeUserId);

            if (string.IsNullOrWhiteSpace(phone))
                return false;

            var normalizedPhone = phone.Trim();

            var query = _context.Users.Where(u => u.Phone == normalizedPhone);

            if (excludeUserId.HasValue)
            {
                query = query.Where(u => u.UserId != excludeUserId.Value);
            }

            var exists = await query.AnyAsync();
            _logger.LogInformation("Phone {Phone} exists: {Exists}", phone, exists);

            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking phone existence: {Phone}", phone);
            throw;
        }
    }

    public async Task<(bool EmailExists, bool PhoneExists)> CheckDuplicateAsync(string email, string phone, int? excludeUserId = null)
    {
        try
        {
            _logger.LogInformation("CheckDuplicateAsync called with email: {Email}, phone: {Phone}, excludeUserId: {ExcludeUserId}",
                email, phone, excludeUserId);

            var emailExists = await IsEmailExistsAsync(email, excludeUserId);
            var phoneExists = await IsPhoneExistsAsync(phone, excludeUserId);

            return (emailExists, phoneExists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking duplicates: email={Email}, phone={Phone}", email, phone);
            throw;
        }
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
