using System.ComponentModel.DataAnnotations;

namespace MonAmourDb_BE.DTOs
{
    public class UserListDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = null!;
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Avatar { get; set; }
        public bool? Verified { get; set; }
        public string? Gender { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class UserDetailDto : UserListDto
    {
        public DateOnly? BirthDate { get; set; }
        public int TotalBookings { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    public class CreateUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = null!;

        public string? Name { get; set; }
        public string? Phone { get; set; }
        public DateOnly? BirthDate { get; set; }
        public string? Gender { get; set; }
        public List<int> RoleIds { get; set; } = new List<int>();
    }

    public class UpdateUserDto
    {
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public DateOnly? BirthDate { get; set; }
        public string? Gender { get; set; }
        public string? Avatar { get; set; }
        public bool? Verified { get; set; }
        public string? Status { get; set; }
        public List<int> RoleIds { get; set; } = new List<int>();
    }

    public class UserFilterDto
    {
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public string? Gender { get; set; }
        public bool? Verified { get; set; }
        public List<int>? RoleIds { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "CreatedAt";
        public string SortOrder { get; set; } = "desc";
    }

    public class ChangePasswordDto
    {
        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = null!;
    }

    public class UserStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int VerifiedUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public List<UserGrowthDto> UserGrowth { get; set; } = new List<UserGrowthDto>();
    }

    public class UserGrowthDto
    {
        public string Month { get; set; } = null!;
        public int Count { get; set; }
    }

    public class PaginatedResult<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}
