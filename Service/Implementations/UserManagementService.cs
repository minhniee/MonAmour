using Microsoft.EntityFrameworkCore;
using MonAmourDb_BE.DTOs;
using MonAmourDb_BE.Models;
using MonAmourDb_BE.Service.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace MonAmourDb_BE.Service.Implementations
{
    public class UserManagementService : IUserManagementService
    {
        private readonly MonAmourDbContext _context;

        public UserManagementService(MonAmourDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<UserListDto>> GetUsersAsync(UserFilterDto filter)
        {
            var query = _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(u => u.Name!.Contains(filter.SearchTerm) ||
                                       u.Email.Contains(filter.SearchTerm) ||
                                       u.Phone!.Contains(filter.SearchTerm));
            }

            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(u => u.Status == filter.Status);
            }

            if (!string.IsNullOrEmpty(filter.Gender))
            {
                query = query.Where(u => u.Gender == filter.Gender);
            }

            if (filter.Verified.HasValue)
            {
                query = query.Where(u => u.Verified == filter.Verified.Value);
            }

            if (filter.RoleIds != null && filter.RoleIds.Any())
            {
                query = query.Where(u => u.UserRoles.Any(ur => filter.RoleIds.Contains(ur.RoleId)));
            }

            if (filter.CreatedFrom.HasValue)
            {
                query = query.Where(u => u.CreatedAt >= filter.CreatedFrom.Value);
            }

            if (filter.CreatedTo.HasValue)
            {
                query = query.Where(u => u.CreatedAt <= filter.CreatedTo.Value);
            }

            // Apply sorting
            query = filter.SortBy.ToLower() switch
            {
                "name" => filter.SortOrder.ToLower() == "asc"
                    ? query.OrderBy(u => u.Name)
                    : query.OrderByDescending(u => u.Name),
                "email" => filter.SortOrder.ToLower() == "asc"
                    ? query.OrderBy(u => u.Email)
                    : query.OrderByDescending(u => u.Email),
                "status" => filter.SortOrder.ToLower() == "asc"
                    ? query.OrderBy(u => u.Status)
                    : query.OrderByDescending(u => u.Status),
                _ => filter.SortOrder.ToLower() == "asc"
                    ? query.OrderBy(u => u.CreatedAt)
                    : query.OrderByDescending(u => u.CreatedAt)
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

            var users = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(u => new UserListDto
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
                    Roles = u.UserRoles.Select(ur => ur.Role.RoleName!).ToList()
                })
                .ToListAsync();

            return new PaginatedResult<UserListDto>
            {
                Data = users,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = totalPages,
                HasNextPage = filter.Page < totalPages,
                HasPreviousPage = filter.Page > 1
            };
        }

        public async Task<UserDetailDto?> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Include(u => u.Bookings)
                .Include(u => u.Orders)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return null;

            return new UserDetailDto
            {
                UserId = user.UserId,
                Email = user.Email,
                Name = user.Name,
                Phone = user.Phone,
                Avatar = user.Avatar,
                BirthDate = user.BirthDate,
                Verified = user.Verified,
                Gender = user.Gender,
                Status = user.Status,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = user.UserRoles.Select(ur => ur.Role.RoleName!).ToList(),
                TotalBookings = user.Bookings.Count,
                TotalOrders = user.Orders.Count,
                TotalSpent = user.Orders.Where(o => o.Status == "completed").Sum(o => o.TotalPrice ?? 0) 
            };
        }


        public async Task<UserDetailDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == createUserDto.Email))
            {
                throw new InvalidOperationException("Email đã tồn tại trong hệ thống");
            }

            // Check if phone already exists
            if (!string.IsNullOrEmpty(createUserDto.Phone) &&
                await _context.Users.AnyAsync(u => u.Phone == createUserDto.Phone))
            {
                throw new InvalidOperationException("Số điện thoại đã tồn tại trong hệ thống");
            }

            var user = new User
            {
                Email = createUserDto.Email,
                Password = HashPassword(createUserDto.Password),
                Name = createUserDto.Name,
                Phone = createUserDto.Phone,
                BirthDate = createUserDto.BirthDate,
                Gender = createUserDto.Gender,
                Verified = true, // Admin tạo user thì mặc định đã verify
                Status = "active",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Assign roles
            if (createUserDto.RoleIds.Any())
            {
                await AssignRolesToUserAsync(user.UserId, createUserDto.RoleIds);
            }

            return await GetUserByIdAsync(user.UserId) ?? throw new InvalidOperationException("Không thể tạo user");
        }

        public async Task<UserDetailDto?> UpdateUserAsync(int userId, UpdateUserDto updateUserDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            // Check if phone already exists (excluding current user)
            if (!string.IsNullOrEmpty(updateUserDto.Phone) &&
                await _context.Users.AnyAsync(u => u.Phone == updateUserDto.Phone && u.UserId != userId))
            {
                throw new InvalidOperationException("Số điện thoại đã tồn tại trong hệ thống");
            }

            user.Name = updateUserDto.Name ?? user.Name;
            user.Phone = updateUserDto.Phone ?? user.Phone;
            user.BirthDate = updateUserDto.BirthDate ?? user.BirthDate;
            user.Gender = updateUserDto.Gender ?? user.Gender;
            user.Avatar = updateUserDto.Avatar ?? user.Avatar;
            user.Verified = updateUserDto.Verified ?? user.Verified;
            user.Status = updateUserDto.Status ?? user.Status;
            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            // Update roles
            if (updateUserDto.RoleIds.Any())
            {
                await AssignRolesToUserAsync(userId, updateUserDto.RoleIds);
            }

            return await GetUserByIdAsync(userId);
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // Soft delete - chỉ thay đổi status
            user.Status = "inactive";
            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangeUserPasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.Password = HashPassword(changePasswordDto.NewPassword);
            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleUserStatusAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.Status = user.Status == "active" ? "inactive" : "active";
            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> VerifyUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.Verified = true;
            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserStatsDto> GetUserStatsAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.Status == "active");
            var verifiedUsers = await _context.Users.CountAsync(u => u.Verified == true);
            var newUsersThisMonth = await _context.Users
                .CountAsync(u => u.CreatedAt!.Value.Month == DateTime.Now.Month &&
                               u.CreatedAt.Value.Year == DateTime.Now.Year);

            // User growth for last 6 months
            var userGrowth = await _context.Users
                .Where(u => u.CreatedAt >= DateTime.Now.AddMonths(-6))
                .GroupBy(u => new { u.CreatedAt!.Value.Year, u.CreatedAt.Value.Month })
                .Select(g => new UserGrowthDto
                {
                    Month = $"{g.Key.Month:00}/{g.Key.Year}",
                    Count = g.Count()
                })
                .OrderBy(g => g.Month)
                .ToListAsync();

            return new UserStatsDto
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                VerifiedUsers = verifiedUsers,
                NewUsersThisMonth = newUsersThisMonth,
                UserGrowth = userGrowth
            };
        }

        public async Task<List<Role>> GetRolesAsync()
        {
            return await _context.Roles.ToListAsync();
        }

        public async Task<bool> AssignRolesToUserAsync(int userId, List<int> roleIds)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // Remove existing roles
            var existingRoles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .ToListAsync();
            _context.UserRoles.RemoveRange(existingRoles);

            // Add new roles
            var newUserRoles = roleIds.Select(roleId => new UserRole
            {
                UserId = userId,
                RoleId = roleId,
                AssignedAt = DateTime.Now
            }).ToList();

            _context.UserRoles.AddRange(newUserRoles);
            await _context.SaveChangesAsync();

            return true;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
