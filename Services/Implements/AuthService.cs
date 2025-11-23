using Microsoft.EntityFrameworkCore;
using MonAmour.AuthViewModel;
using MonAmour.Helpers;
using MonAmour.Models;
using MonAmour.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace MonAmour.Services.Implements;

public class AuthService : IAuthService
{
    private readonly MonAmourDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        MonAmourDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IEmailService emailService,
        ILogger<AuthService> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<(bool Success, string? ErrorMessage)> LoginAsync(LoginViewModel model)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", model.Email);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                _logger.LogWarning("Login failed - user not found: {Email}", model.Email);
                return (false, "Email hoặc mật khẩu không đúng.");
            }

            // Kiểm tra mật khẩu
            if (!VerifyPassword(model.Password, user.Password))
            {
                _logger.LogWarning("Login failed - invalid password for user: {Email}", model.Email);
                return (false, "Email hoặc mật khẩu không đúng.");
            }

            // Kiểm tra trạng thái tài khoản
            if (user.Status != "active")
            {
                _logger.LogWarning("Login failed - inactive account: {Email}", model.Email);
                return (false, "Tài khoản đã bị vô hiệu hóa.");
            }

            // Kiểm tra email đã được xác thực chưa
            if (user.Verified != true)
            {
                _logger.LogWarning("Login failed - email not verified: {Email}", model.Email);
                return (false, "Vui lòng xác thực email trước khi đăng nhập.");
            }

            // Lấy roles của user
            var roles = await _context.UserRoles
                .Where(ur => ur.UserId == user.UserId)
                .Select(ur => ur.Role.RoleName)
                .ToListAsync();

            // Tạo session
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                AuthHelper.SetUserSession(httpContext, user, roles);

                // Remember me functionality
                if (model.RememberMe)
                {
                    var rememberToken = GenerateToken();
                    var rememberCookie = new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(30),
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict
                    };
                    httpContext.Response.Cookies.Append("RememberToken", rememberToken, rememberCookie);

                    // Lưu token vào database
                    var token = new Token
                    {
                        UserId = user.UserId,
                        TokenValue = rememberToken,
                        TokenType = "remember_me",
                        ExpiresAt = DateTime.Now.AddDays(30),
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
                        UserAgent = httpContext.Request.Headers["User-Agent"].ToString()
                    };
                    _context.Tokens.Add(token);
                    await _context.SaveChangesAsync();
                }
            }

            _logger.LogInformation("User logged in successfully: {Email}", model.Email);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", model.Email);
            return (false, "Có lỗi xảy ra. Vui lòng thử lại sau.");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> SignupAsync(SignupViewModel model)
    {
        try
        {
            _logger.LogInformation("Signup attempt for email: {Email}", model.Email);

            // Kiểm tra email đã tồn tại
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            var existingPhone = await _context.Users
                .FirstOrDefaultAsync(u => u.Phone == model.Phone);

            if (existingPhone != null)
            {
                _logger.LogWarning("Signup failed - phone already exists: {Phone}", model.Phone);
                return (false, "Số điện thoại đã được sử dụng.");
            }
            if (existingUser != null)
            {
                _logger.LogWarning("Signup failed - email already exists: {Email}", model.Email);
                return (false, "Email đã được sử dụng.");
            }

            // Tạo user mới
            var user = new User
            {
                Email = model.Email,
                Password = HashPassword(model.Password),
                Name = model.FullName,
                Phone = model.Phone,
                Verified = false,
                Status = "active",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Gán role "User" cho người dùng mới
            await RoleHelper.AssignRoleToUserAsync(_context, user.UserId, Role.Names.User);

            // Tạo token xác thực email
            var verificationToken = GenerateToken();
            var token = new Token
            {
                UserId = user.UserId,
                TokenValue = verificationToken,
                TokenType = "email_verification",
                ExpiresAt = DateTime.Now.AddHours(24),
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Tokens.Add(token);
            await _context.SaveChangesAsync();

            // Gửi email xác thực
            await _emailService.SendVerificationEmailAsync(user.Email, verificationToken);

            _logger.LogInformation("User registered successfully: {Email}", model.Email);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during signup for email: {Email}", model.Email);
            return (false, "Có lỗi xảy ra. Vui lòng thử lại sau.");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> ForgotPasswordAsync(string email)
    {
        try
        {
            _logger.LogInformation("Forgot password request for email: {Email}", email);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                _logger.LogWarning("Forgot password failed - user not found: {Email}", email);
                return (false, "Email không tồn tại trong hệ thống.");
            }

            // Vô hiệu hóa các token reset password cũ
            var oldTokens = await _context.Tokens
                .Where(t => t.UserId == user.UserId && t.TokenType == "reset_password" && t.IsActive == true)
                .AsTracking()
                .ToListAsync();

            foreach (var oldToken in oldTokens)
            {
                oldToken.IsActive = false;
            }

            // Tạo token reset password
            var resetToken = GenerateToken();
            var token = new Token
            {
                UserId = user.UserId,
                TokenValue = resetToken,
                TokenType = "reset_password",
                ExpiresAt = DateTime.Now.AddHours(1),
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Tokens.Add(token);
            await _context.SaveChangesAsync();

            // Gửi email reset password
            await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken);

            _logger.LogInformation("Password reset email sent successfully for: {Email}", email);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during forgot password for email: {Email}", email);
            return (false, "Có lỗi xảy ra. Vui lòng thử lại sau.");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> ResetPasswordAsync(ResetPasswordViewModel model)
    {
        try
        {
            _logger.LogInformation("Reset password attempt for token: {Token}", model.Token);

            // Tìm token hợp lệ
            var token = await _context.Tokens
                .Include(t => t.User)
                .AsTracking()
                .FirstOrDefaultAsync(t => t.TokenValue == model.Token
                    && t.TokenType == "reset_password"
                    && t.IsActive == true
                    && t.ExpiresAt > DateTime.Now);

            if (token == null)
            {
                _logger.LogWarning("Reset password failed - invalid or expired token: {Token}", model.Token);
                return (false, "Token không hợp lệ hoặc đã hết hạn.");
            }

            // Cập nhật mật khẩu
            token.User.Password = HashPassword(model.Password);
            token.User.UpdatedAt = DateTime.Now;

            // Vô hiệu hóa token
            token.IsActive = false;
            token.UsedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Password reset successfully for user: {Email}", token.User.Email);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during reset password for token: {Token}", model.Token);
            return (false, "Có lỗi xảy ra. Vui lòng thử lại sau.");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> VerifyEmailAsync(string token, string email)
    {
        try
        {
            _logger.LogInformation("Email verification attempt for email: {Email}", email);

            var verificationToken = await _context.Tokens
                .Include(t => t.User)
                .AsTracking()
                .FirstOrDefaultAsync(t => t.TokenValue == token
                    && t.TokenType == "email_verification"
                    && t.IsActive == true
                    && t.ExpiresAt > DateTime.Now
                    && t.User.Email == email);

            if (verificationToken == null)
            {
                _logger.LogWarning("Email verification failed - invalid or expired token for email: {Email}", email);
                return (false, "Link xác thực không hợp lệ hoặc đã hết hạn.");
            }

            // Xác thực email
            verificationToken.User.Verified = true;
            verificationToken.User.Status = "active";
            verificationToken.User.UpdatedAt = DateTime.Now;

            // Vô hiệu hóa token
            verificationToken.IsActive = false;
            verificationToken.UsedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            // Gửi email chào mừng
            await _emailService.SendWelcomeEmailAsync(verificationToken.User.Email, verificationToken.User.Name ?? "");

            _logger.LogInformation("Email verified successfully for: {Email}", email);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email verification for email: {Email}", email);
            return (false, "Có lỗi xảy ra. Vui lòng thử lại sau.");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> ResendVerificationAsync(string email)
    {
        try
        {
            _logger.LogInformation("Resend verification attempt for email: {Email}", email);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                _logger.LogWarning("Resend verification failed - user not found: {Email}", email);
                return (false, "Email không tồn tại trong hệ thống.");
            }

            if (user.Verified == true)
            {
                _logger.LogWarning("Resend verification failed - email already verified: {Email}", email);
                return (false, "Email đã được xác thực trước đó.");
            }

            // Vô hiệu hóa token cũ
            var oldTokens = await _context.Tokens
                .Where(t => t.UserId == user.UserId && t.TokenType == "email_verification" && t.IsActive == true)
                .AsTracking()
                .ToListAsync();

            foreach (var oldToken in oldTokens)
            {
                oldToken.IsActive = false;
            }

            // Tạo token mới
            var verificationToken = GenerateToken();
            var token = new Token
            {
                UserId = user.UserId,
                TokenValue = verificationToken,
                TokenType = "email_verification",
                ExpiresAt = DateTime.Now.AddHours(24),
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Tokens.Add(token);
            await _context.SaveChangesAsync();

            // Gửi email xác thực
            await _emailService.SendVerificationEmailAsync(user.Email, verificationToken);

            _logger.LogInformation("Verification email resent successfully for: {Email}", email);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during resend verification for email: {Email}", email);
            return (false, "Có lỗi xảy ra. Vui lòng thử lại sau.");
        }
    }

    public async Task LogoutAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            // Xóa session
            httpContext.Session.Clear();

            // Xóa remember me cookie
            var rememberToken = httpContext.Request.Cookies["RememberToken"];
            if (!string.IsNullOrEmpty(rememberToken))
            {
                // Vô hiệu hóa token trong database
                var token = await _context.Tokens
                    .AsTracking()
                    .FirstOrDefaultAsync(t => t.TokenValue == rememberToken && t.TokenType == "remember_me");

                if (token != null)
                {
                    token.IsActive = false;
                    await _context.SaveChangesAsync();
                }

                // Xóa cookie
                httpContext.Response.Cookies.Delete("RememberToken");
            }
        }
    }

    public async Task<bool> IsEmailVerifiedAsync(string email)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        return user?.Verified == true;
    }

    public async Task<bool> IsTokenValidAsync(string token, string tokenType)
    {
        var dbToken = await _context.Tokens
            .FirstOrDefaultAsync(t => t.TokenValue == token
                && t.TokenType == tokenType
                && t.IsActive == true
                && t.ExpiresAt > DateTime.Now);

        return dbToken != null;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<User?> GetUserByTokenAsync(string token, string tokenType)
    {
        var dbToken = await _context.Tokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenValue == token
                && t.TokenType == tokenType
                && t.IsActive == true
                && t.ExpiresAt > DateTime.Now);

        return dbToken?.User;
    }

    public async Task<bool> UpdateProfileAsync(int userId, UserViewModel.ProfileViewModel model)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            // Kiểm tra email và phone đã tồn tại chưa (nếu có thay đổi)
            if (user.Email != model.Email)
            {
                var emailExists = await _context.Users
                    .AnyAsync(u => u.Email == model.Email && u.UserId != userId);
                if (emailExists)
                {
                    throw new Exception("Email đã được sử dụng.");
                }
            }

            if (user.Phone != model.Phone)
            {
                var phoneExists = await _context.Users
                    .AnyAsync(u => u.Phone == model.Phone && u.UserId != userId);
                if (phoneExists)
                {
                    throw new Exception("Số điện thoại đã được sử dụng.");
                }
            }

            // Cập nhật thông tin
            user.Name = model.Name;
            user.Email = user.Email;
            user.Phone = model.Phone;
            user.Avatar = model.Avatar;
            user.BirthDate = model.BirthDate;
            user.Gender = model.Gender;
            user.UpdatedAt = DateTime.Now;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Update model: {@Model}", model);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating profile for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            // Kiểm tra mật khẩu hiện tại
            if (!VerifyPassword(currentPassword, user.Password))
                return false;

            // Cập nhật mật khẩu mới
            user.Password = HashPassword(newPassword);
            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    // Helper methods
    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private bool VerifyPassword(string password, string hashedPassword)
    {
        var hashedInput = HashPassword(password);
        return hashedInput == hashedPassword;
    }

    private string GenerateToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    // Role management methods implementation
    public async Task<List<string>> GetUserRolesAsync(int userId)
    {
        try
        {
            return await RoleHelper.GetUserRolesAsync(_context, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user roles for userId: {UserId}", userId);
            return new List<string>();
        }
    }

    public async Task<bool> HasRoleAsync(int userId, string roleName)
    {
        try
        {
            return await RoleHelper.UserHasRoleAsync(_context, userId, roleName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user role for userId: {UserId}, role: {RoleName}", userId, roleName);
            return false;
        }
    }

    public async Task<bool> IsAdminAsync(int userId)
    {
        try
        {
            return await RoleHelper.IsAdminAsync(_context, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking admin status for userId: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> IsUserAsync(int userId)
    {
        try
        {
            return await RoleHelper.IsUserAsync(_context, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user status for userId: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> AssignRoleToUserAsync(int userId, string roleName, int? assignedBy = null)
    {
        try
        {
            _logger.LogInformation("Assigning role {RoleName} to user {UserId}", roleName, userId);
            var result = await RoleHelper.AssignRoleToUserAsync(_context, userId, roleName, assignedBy);

            if (result)
            {
                _logger.LogInformation("Successfully assigned role {RoleName} to user {UserId}", roleName, userId);
            }
            else
            {
                _logger.LogWarning("Failed to assign role {RoleName} to user {UserId}", roleName, userId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role {RoleName} to user {UserId}", roleName, userId);
            return false;
        }
    }

    public async Task<bool> RemoveRoleFromUserAsync(int userId, string roleName)
    {
        try
        {
            _logger.LogInformation("Removing role {RoleName} from user {UserId}", roleName, userId);
            var result = await RoleHelper.RemoveRoleFromUserAsync(_context, userId, roleName);

            if (result)
            {
                _logger.LogInformation("Successfully removed role {RoleName} from user {UserId}", roleName, userId);
            }
            else
            {
                _logger.LogWarning("Failed to remove role {RoleName} from user {UserId}", roleName, userId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role {RoleName} from user {UserId}", roleName, userId);
            return false;
        }
    }

    public async Task InitializeSystemAsync()
    {
        try
        {
            _logger.LogInformation("Initializing system roles");
            await RoleHelper.InitializeDefaultRolesAsync(_context);
            _logger.LogInformation("System roles initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing system roles");
            throw;
        }
    }
}
