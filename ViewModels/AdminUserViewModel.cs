using System.ComponentModel.DataAnnotations;

namespace MonAmour.ViewModels;

public class AdminUserViewModel
{
    public class UserListViewModel
    {
        public int UserId { get; set; }
        public string Email { get; set; } = "";
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

    public class UserCreateViewModel
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "Mật khẩu phải chứa ít nhất 1 chữ hoa, 1 chữ thường và 1 số")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = "";

        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [MinLength(2, ErrorMessage = "Họ và tên phải có ít nhất 2 ký tự")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [RegularExpression(@"^[0-9]{10,11}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = "";

        public string? Avatar { get; set; }

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        public DateTime? BirthDate { get; set; }

        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        public string Gender { get; set; } = "";

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        public string Status { get; set; } = "";

        public bool Verified { get; set; } = false;

        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        public List<int> RoleIds { get; set; } = new List<int>();
    }

    public class UserEditViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [MinLength(2, ErrorMessage = "Họ và tên phải có ít nhất 2 ký tự")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [RegularExpression(@"^[0-9]{10,11}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = "";

        public string? Avatar { get; set; }

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        public DateTime? BirthDate { get; set; }

        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        public string Gender { get; set; } = "";

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        public string Status { get; set; } = "";

        public bool Verified { get; set; }

        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        public List<int> RoleIds { get; set; } = new List<int>();
    }

    public class UserChangePasswordViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "Mật khẩu phải chứa ít nhất 1 chữ hoa, 1 chữ thường và 1 số")]
        public string NewPassword { get; set; } = "";

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = "";
    }

    public class UserDetailViewModel
    {
        public int UserId { get; set; }
        public string Email { get; set; } = "";
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Avatar { get; set; }
        public DateTime? BirthDate { get; set; }
        public bool? Verified { get; set; }
        public string? Gender { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<RoleViewModel> Roles { get; set; } = new List<RoleViewModel>();
        public int TotalOrders { get; set; }
        public int TotalBookings { get; set; }
        public int TotalReviews { get; set; }
    }

    public class RoleViewModel
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = "";
        public DateTime? AssignedAt { get; set; }
        public int? AssignedBy { get; set; }
    }

    public class UserSearchViewModel
    {
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public string? Gender { get; set; }
        public bool? Verified { get; set; }
        public int? RoleId { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
