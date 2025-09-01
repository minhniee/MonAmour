using MonAmourDb_BE.DTOs;
using MonAmourDb_BE.Models;

namespace MonAmourDb_BE.Service.Interfaces
{
    public interface IUserManagementService
    {
        Task<PaginatedResult<UserListDto>> GetUsersAsync(UserFilterDto filter);
        Task<UserDetailDto?> GetUserByIdAsync(int userId);
        Task<UserDetailDto> CreateUserAsync(CreateUserDto createUserDto);
        Task<UserDetailDto?> UpdateUserAsync(int userId, UpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> ChangeUserPasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task<bool> ToggleUserStatusAsync(int userId);
        Task<bool> VerifyUserAsync(int userId);
        Task<UserStatsDto> GetUserStatsAsync();
        Task<List<Role>> GetRolesAsync();
        Task<bool> AssignRolesToUserAsync(int userId, List<int> roleIds);
    }
}
