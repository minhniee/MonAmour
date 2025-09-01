using System.ComponentModel.DataAnnotations;

namespace MonAmourDb_BE.DTOs
{
    // Base DTO cho danh sách partner
    public class PartnerListDto
    {
        public int PartnerId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int TotalLocations { get; set; }
        public string? UserName { get; set; } // Tên user liên kết
    }

    // DTO chi tiết partner
    public class PartnerDetailDto : PartnerListDto
    {
        public string? ContactInfo { get; set; }
        public int? UserId { get; set; }
        public List<LocationDto> Locations { get; set; } = new List<LocationDto>();
        public UserBasicDto? User { get; set; }
    }

    // DTO tạo partner mới
    public class CreatePartnerDto
    {
        [Required(ErrorMessage = "Tên partner là bắt buộc")]
        [StringLength(255, ErrorMessage = "Tên partner không được vượt quá 255 ký tự")]
        public string Name { get; set; } = null!;

        [StringLength(255, ErrorMessage = "Thông tin liên hệ không được vượt quá 255 ký tự")]
        public string? ContactInfo { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(50, ErrorMessage = "Số điện thoại không được vượt quá 50 ký tự")]
        public string? Phone { get; set; }

        public int? UserId { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "pending";
    }

    // DTO cập nhật partner
    public class UpdatePartnerDto
    {
        [StringLength(255, ErrorMessage = "Tên partner không được vượt quá 255 ký tự")]
        public string? Name { get; set; }

        [StringLength(255, ErrorMessage = "Thông tin liên hệ không được vượt quá 255 ký tự")]
        public string? ContactInfo { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(50, ErrorMessage = "Số điện thoại không được vượt quá 50 ký tự")]
        public string? Phone { get; set; }

        public int? UserId { get; set; }

        [StringLength(20)]
        public string? Status { get; set; }
    }

    // DTO filter cho partner
    public class PartnerFilterDto : PaginationDto
    {
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string SortBy { get; set; } = "CreatedAt";
        public string SortOrder { get; set; } = "desc";
    }

    // DTO thống kê partner
    public class PartnerStatsDto
    {
        public int TotalPartners { get; set; }
        public int ActivePartners { get; set; }
        public int PendingPartners { get; set; }
        public int InactivePartners { get; set; }
        public int SuspendedPartners { get; set; }
        public int NewPartnersThisMonth { get; set; }
        public List<PartnerGrowthDto> PartnerGrowth { get; set; } = new List<PartnerGrowthDto>();
    }

    public class PartnerGrowthDto
    {
        public string Month { get; set; } = null!;
        public int Count { get; set; }
    }

    // DTO cho location trong partner
    public class LocationDto
    {
        public int LocationId { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? District { get; set; }
        public string? City { get; set; }
        public string? Status { get; set; }
        public string? GgmapLink { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int TotalConcepts { get; set; }
    }

    // DTO user cơ bản
    public class UserBasicDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = null!;
        public string? Name { get; set; }
        public string? Phone { get; set; }
    }
}
