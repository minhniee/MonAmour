using MonAmour.ViewModels;

namespace MonAmour.Services.Interfaces;

public interface IUserManagementService
{
    Task<(List<AdminUserViewModel.UserListViewModel> Users, int TotalCount)> GetUsersAsync(AdminUserViewModel.UserSearchViewModel searchModel);
    Task<AdminUserViewModel.UserDetailViewModel?> GetUserByIdAsync(int userId);
    Task<bool> CreateUserAsync(AdminUserViewModel.UserCreateViewModel model, int adminUserId);
    Task<bool> UpdateUserAsync(AdminUserViewModel.UserEditViewModel model, int adminUserId);
    Task<bool> DeleteUserAsync(int userId, int adminUserId);
    Task<bool> ChangeUserPasswordAsync(AdminUserViewModel.UserChangePasswordViewModel model, int adminUserId);
    Task<bool> ToggleUserVerificationAsync(int userId, int adminUserId);
    Task<bool> ToggleUserStatusAsync(int userId, string status, int adminUserId);
    Task<List<AdminUserViewModel.RoleViewModel>> GetAllRolesAsync();
    Task<bool> IsEmailExistsAsync(string email, int? excludeUserId = null);
    Task<Dictionary<string, int>> GetUserStatisticsAsync();
    Task<bool> IsPhoneExistsAsync(string phone, int? excludeUserId = null); 
    Task<(bool EmailExists, bool PhoneExists)> CheckDuplicateAsync(string email, string phone, int? excludeUserId = null); 
}
