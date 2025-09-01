using Microsoft.EntityFrameworkCore;
using MonAmourDb_BE.DTOs;
using MonAmourDb_BE.Helpers;
using MonAmourDb_BE.Models;
using MonAmourDb_BE.Service.Interfaces;

namespace MonAmourDb_BE.Service.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly MonAmourDbContext _db;
        private readonly IEmailService _email;
        private readonly IEmailTemplateService _emailTemplates;
        private readonly IConfiguration _cfg;

        public AuthService(MonAmourDbContext db, IEmailService email, IEmailTemplateService emailTemplates, IConfiguration cfg)
        {
            _db = db;
            _email = email;
            _emailTemplates = emailTemplates;
            _cfg = cfg;
        }

        public async Task<UserDto> RegisterAsync(RegisterRequest request, string baseUrl)
        {
            var emailLower = request.Email.Trim().ToLowerInvariant();
            var exists = await _db.Users.AnyAsync(u => u.Email == emailLower);
            if (exists) throw new InvalidOperationException("Email already in use");

            var user = new User
            {
                Email = emailLower,
                Password = SecurityHelper.HashPassword(request.Password),
                Name = request.Name,
                Phone = request.Phone,
                Verified = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var userRole = new UserRole 
                { 
                    RoleId = 1, 
                    UserId = user.UserId 
                };
            _db.UserRoles.Add(userRole); // Default role: User

            // Create email verification token
            var token = new Token
            {
                UserId = user.UserId,
                TokenValue = SecurityHelper.GenerateSecureToken(48),
                TokenType = "email_verification",
                ExpiresAt = DateTime.UtcNow.AddDays(2),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _db.Tokens.Add(token);
            await _db.SaveChangesAsync();

            var verifyUrl = $"{baseUrl.TrimEnd('/')}/api/auth/verify-email?token={Uri.EscapeDataString(token.TokenValue)}";
            var template = await _emailTemplates.GetActiveTemplateAsync("system", "RegisterVerifyEmail");
            var subject = template?.Subject ?? (_cfg["EmailSettings:Titles:Register"] ?? "Welcome to MonAmour");
            var body = (template?.Body ?? "<p>Hi {{name}},</p><p>Please verify your email by clicking <a href=\"{{link}}\">here</a>.</p>")
                .Replace("{{name}}", System.Net.WebUtility.HtmlEncode(user.Name ?? user.Email))
                .Replace("{{link}}", verifyUrl);
            await _email.SendAsync(user.Email, subject, body);

            return MapUser(user);
        }

        public async Task VerifyEmailAsync(string token)
        {
            var tokenEntity = await _db.Tokens.Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TokenValue == token && t.TokenType == "email_verification" && t.IsActive == true);
            if (tokenEntity == null || tokenEntity.ExpiresAt < DateTime.UtcNow) throw new InvalidOperationException("Invalid or expired token");

            tokenEntity.IsActive = false;
            tokenEntity.UsedAt = DateTime.UtcNow;
            tokenEntity.User.Verified = true;
            tokenEntity.User.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, string ip, string userAgent)
        {
            var emailLower = request.Email.Trim().ToLowerInvariant();
            var user = await _db.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == emailLower);
            if (user == null) throw new InvalidOperationException("Invalid credentials");
            if (!SecurityHelper.VerifyPassword(request.Password, user.Password)) throw new InvalidOperationException("Invalid credentials");
            if (user.Verified != true) throw new InvalidOperationException("Email not verified");

            var userRoles = user.UserRoles.Select(ur => ur.Role.RoleName!).ToList();
            var (accessToken, accessExp) = SecurityHelper.CreateJwt(user.UserId, user.Email, _cfg, userRoles);
            string? refreshToken = null; DateTime? refreshExp = null;

            if (request.RememberMe)
            {
                var rt = new Token
                {
                    UserId = user.UserId,
                    TokenValue = SecurityHelper.GenerateSecureToken(48),
                    TokenType = "refresh_token",
                    ExpiresAt = DateTime.UtcNow.AddDays(30),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    IpAddress = ip,
                    UserAgent = userAgent
                };
                _db.Tokens.Add(rt);
                await _db.SaveChangesAsync();
                refreshToken = rt.TokenValue;
                refreshExp = rt.ExpiresAt;
            }

            return new AuthResponse
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessExp,
                RefreshToken = refreshToken,
                RefreshTokenExpiresAt = refreshExp,
                User = MapUser(user)
            };
        }

        public async Task<AuthResponse> RefreshAsync(string refreshToken, string ip, string userAgent)
        {
            var tokenEntity = await _db.Tokens
                .Include(t => t.User)
                .ThenInclude(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(t => t.TokenValue == refreshToken && t.TokenType == "refresh_token" && t.IsActive == true);
            if (tokenEntity == null || tokenEntity.ExpiresAt < DateTime.UtcNow) throw new InvalidOperationException("Invalid or expired token");

            var user = tokenEntity.User;
            var userRoles = user.UserRoles.Select(ur => ur.Role.RoleName!).ToList();
            var (accessToken, accessExp) = SecurityHelper.CreateJwt(user.UserId, user.Email, _cfg, userRoles);

            // Optionally rotate refresh token
            tokenEntity.TokenValue = SecurityHelper.GenerateSecureToken(48);
            tokenEntity.ExpiresAt = DateTime.UtcNow.AddDays(30);
            tokenEntity.UpdatedAt = DateTime.UtcNow;
            tokenEntity.IpAddress = ip;
            tokenEntity.UserAgent = userAgent;
            await _db.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessExp,
                RefreshToken = tokenEntity.TokenValue,
                RefreshTokenExpiresAt = tokenEntity.ExpiresAt,
                User = MapUser(user)
            };
        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequest request, string baseUrl)
        {
            var emailLower = request.Email.Trim().ToLowerInvariant();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == emailLower);
            if (user == null) return; // do not leak existence

            var token = new Token
            {
                UserId = user.UserId,
                TokenValue = SecurityHelper.GenerateSecureToken(48),
                TokenType = "reset_password",
                ExpiresAt = DateTime.UtcNow.AddHours(2),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _db.Tokens.Add(token);
            await _db.SaveChangesAsync();

            var resetUrl = $"{baseUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(token.TokenValue)}";
            var template = await _emailTemplates.GetActiveTemplateAsync("system", "ForgotPassword");
            var subject = template?.Subject ?? (_cfg["EmailSettings:Titles:ForgotPassword"] ?? "Password Reset Request");
            var body = (template?.Body ?? "<p>Hello,</p><p>Click <a href=\"{{link}}\">here</a> to reset your password. This link expires in 2 hours.</p>")
                .Replace("{{link}}", resetUrl);
            await _email.SendAsync(user.Email, subject, body);
        }

        public async Task ResetPasswordAsync(ResetPasswordRequest request)
        {
            var tokenEntity = await _db.Tokens.Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TokenValue == request.Token && t.TokenType == "reset_password" && t.IsActive == true);
            if (tokenEntity == null || tokenEntity.ExpiresAt < DateTime.UtcNow) throw new InvalidOperationException("Invalid or expired token");

            tokenEntity.IsActive = false;
            tokenEntity.UsedAt = DateTime.UtcNow;
            tokenEntity.User.Password = SecurityHelper.HashPassword(request.NewPassword);
            tokenEntity.User.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var tokenEntity = await _db.Tokens.FirstOrDefaultAsync(t => t.TokenValue == refreshToken && t.TokenType == "refresh" && t.IsActive == true);
            if (tokenEntity != null)
            {
                tokenEntity.IsActive = false;
                tokenEntity.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }

        public async Task<UserDto> GetCurrentUserAsync(int userId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) throw new InvalidOperationException("User not found");
            return MapUser(user);
        }

        private static UserDto MapUser(User user)
        {
            return new UserDto
            {
                Id = user.UserId,
                Email = user.Email,
                Name = user.Name,
                Phone = user.Phone,
                Avatar = user.Avatar,
                Verified = user.Verified == true
            };
        }
    }
}


