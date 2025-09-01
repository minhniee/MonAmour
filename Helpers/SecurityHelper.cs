using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MonAmourDb_BE.Helpers
{
    public static class SecurityHelper
    {
        public static string HashPassword(string password)
        {
            // PBKDF2 with HMACSHA256
            using var rng = RandomNumberGenerator.Create();
            byte[] salt = new byte[16];
            rng.GetBytes(salt);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(32);
            var combined = new byte[1 + salt.Length + hash.Length];
            combined[0] = 0x01; // version
            Buffer.BlockCopy(salt, 0, combined, 1, salt.Length);
            Buffer.BlockCopy(hash, 0, combined, 1 + salt.Length, hash.Length);
            return Convert.ToBase64String(combined);
        }

        public static bool VerifyPassword(string password, string stored)
        {
            var bytes = Convert.FromBase64String(stored);
            if (bytes.Length < 1 + 16 + 32) return false;
            var version = bytes[0];
            if (version != 0x01) return false;
            var salt = new byte[16];
            Buffer.BlockCopy(bytes, 1, salt, 0, 16);
            var hash = new byte[32];
            Buffer.BlockCopy(bytes, 17, hash, 0, 32);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            var computed = pbkdf2.GetBytes(32);
            return CryptographicOperations.FixedTimeEquals(hash, computed);
        }

        public static string GenerateSecureToken(int bytesLength = 32)
        {
            var bytes = new byte[bytesLength];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
        }

        public static (string token, DateTime expiresAt) CreateJwt(int userId, string email, IConfiguration configuration, IEnumerable<string>? roles = null)
        {
            var key = configuration["Jwt:Key"];
            var issuer = configuration["Jwt:Issuer"];
            var audience = configuration["Jwt:Audience"];
            var expireMinutes = int.TryParse(configuration["Jwt:ExpireMinutes"], out var m) ? m : 60;

            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key), "JWT key cannot be null or empty");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddMinutes(expireMinutes);
            
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };

            // Add role claims
            if (roles != null)
            {
                claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            }

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: expires,
                signingCredentials: credentials
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expires);
        }
    }
}


