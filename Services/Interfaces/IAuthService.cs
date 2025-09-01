using MonAmour.AuthViewModel;
using MonAmour.Models;

namespace MonAmour.Services.Interfaces;

public interface IAuthService
{
    // Authentication methods
    Task<(bool Success, string? ErrorMessage)> LoginAsync(LoginViewModel model);
    Task<(bool Success, string? ErrorMessage)> SignupAsync(SignupViewModel model);
    Task<(bool Success, string? ErrorMessage)> ForgotPasswordAsync(string email);
    Task<(bool Success, string? ErrorMessage)> ResetPasswordAsync(ResetPasswordViewModel model);
    Task<(bool Success, string? ErrorMessage)> VerifyEmailAsync(string token, string email);
    Task<(bool Success, string? ErrorMessage)> ResendVerificationAsync(string email);
    Task LogoutAsync();

    // User management methods
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(int userId);
    Task<User?> GetUserByTokenAsync(string token, string tokenType);
    Task<bool> UpdateProfileAsync(int userId, UserViewModel.ProfileViewModel model);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);

    // Email and token validation
    Task<bool> IsEmailVerifiedAsync(string email);
    Task<bool> IsTokenValidAsync(string token, string tokenType);

    // Role management methods
    Task<List<string>> GetUserRolesAsync(int userId);
    Task<bool> HasRoleAsync(int userId, string roleName);
    Task<bool> IsAdminAsync(int userId);
    Task<bool> IsUserAsync(int userId);
    Task<bool> AssignRoleToUserAsync(int userId, string roleName, int? assignedBy = null);
    Task<bool> RemoveRoleFromUserAsync(int userId, string roleName);

    // System initialization
    Task InitializeSystemAsync();
}