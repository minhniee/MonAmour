using System.ComponentModel.DataAnnotations;

namespace MonAmour.ViewModels
{
    public class PartnerViewModel
    {
        public int PartnerId { get; set; }

        [Required(ErrorMessage = "Tên đối tác là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên đối tác không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Thông tin liên hệ không được vượt quá 500 ký tự")]
        public string? ContactInfo { get; set; }

        public int? UserId { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        public string? Phone { get; set; }

        public string? Avatar { get; set; }

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        public string Status { get; set; } = "Active";

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public string? UserName { get; set; }
        public int LocationCount { get; set; }
    }

    public class PartnerCreateViewModel
    {
        [Required(ErrorMessage = "Tên đối tác là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên đối tác không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Thông tin liên hệ không được vượt quá 500 ký tự")]
        public string? ContactInfo { get; set; }

        public int? UserId { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        public string? Phone { get; set; }

        public string? Avatar { get; set; }

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        public string Status { get; set; } = "Active";
    }

    public class PartnerEditViewModel
    {
        public int PartnerId { get; set; }

        [Required(ErrorMessage = "Tên đối tác là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên đối tác không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Thông tin liên hệ không được vượt quá 500 ký tự")]
        public string? ContactInfo { get; set; }

        public int? UserId { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        public string? Phone { get; set; }

        public string? Avatar { get; set; }

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        public string Status { get; set; } = "Active";

        public DateTime? CreatedAt { get; set; }
    }

    public class PartnerDetailViewModel
    {
        public int PartnerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ContactInfo { get; set; }
        public int? UserId { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Avatar { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public string? UserName { get; set; }
        public List<LocationViewModel> Locations { get; set; } = new List<LocationViewModel>();
    }

    public class PartnerSearchViewModel
    {
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "Name";
        public string? SortOrder { get; set; } = "asc";
    }

    public class PartnerDropdownViewModel
    {
        public int PartnerId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
