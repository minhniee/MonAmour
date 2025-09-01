using System.ComponentModel.DataAnnotations;

namespace MonAmourDb_BE.DTOs
{
    public class RegisterRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required, MinLength(6)]
        public string Password { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Phone { get; set; }

    }

    public class LoginRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }

        public bool RememberMe { get; set; } = false;
    }

    public class ForgotPasswordRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }

    public class ResetPasswordRequest
    {
        [Required]
        public string Token { get; set; }
        [Required, MinLength(6)]
        public string NewPassword { get; set; }
    }

    public class VerifyEmailRequest
    {
        [Required]
        public string Token { get; set; }
    }

    public class UpdateProfileRequest
    {
        [Required]
        public string Name { get; set; }
        public string? Avatar { get; set; }
        public string? Phone { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Avatar { get; set; }
        public bool Verified { get; set; }
    }

    public class AuthResponse
    {
        public string AccessToken { get; set; }
        public DateTime AccessTokenExpiresAt { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }
        public UserDto User { get; set; }
    }
}


