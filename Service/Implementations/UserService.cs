using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MonAmourDb_BE.DTOs;
using MonAmourDb_BE.Models;
using MonAmourDb_BE.Service.Interfaces;

namespace MonAmourDb_BE.Service.Implementations
{
    public class UserService : IUserService
    {
        private readonly MonAmourDbContext _db;

        public UserService(MonAmourDbContext db)
        {
            _db = db;
        }

        public async Task<UserDto> GetProfileAsync(int userId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) throw new System.InvalidOperationException("User not found");
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

        public async Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) throw new System.InvalidOperationException("User not found");
            user.Name = request.Name;
            user.Avatar = request.Avatar;
            user.Phone = request.Phone;
            user.UpdatedAt = System.DateTime.UtcNow;
            await _db.SaveChangesAsync();

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


