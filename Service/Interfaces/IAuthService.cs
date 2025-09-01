using System.Threading.Tasks;
using MonAmourDb_BE.DTOs;

namespace MonAmourDb_BE.Service.Interfaces
{
    public interface IAuthService
    {
        Task<UserDto> RegisterAsync(RegisterRequest request, string baseUrl);
        Task VerifyEmailAsync(string token);
        Task<AuthResponse> LoginAsync(LoginRequest request, string ip, string userAgent);
        Task<AuthResponse> RefreshAsync(string refreshToken, string ip, string userAgent);
        Task ForgotPasswordAsync(ForgotPasswordRequest request, string baseUrl);
        Task ResetPasswordAsync(ResetPasswordRequest request);
        Task LogoutAsync(string refreshToken);
        Task<UserDto> GetCurrentUserAsync(int userId);
    }
}


