using System.ComponentModel.DataAnnotations;

namespace MonAmour.ViewModels
{
    public class LocationViewModel
    {
        public int LocationId { get; set; }

        [Required(ErrorMessage = "Tên địa điểm là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên địa điểm không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        [StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự")]
        public string Address { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Quận/Huyện không được vượt quá 50 ký tự")]
        public string? District { get; set; }

        [StringLength(50, ErrorMessage = "Thành phố không được vượt quá 50 ký tự")]
        public string? City { get; set; }

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        public string Status { get; set; } = "Active";

        public int? PartnerId { get; set; }

        [Url(ErrorMessage = "Link Google Maps không hợp lệ")]
        [StringLength(500, ErrorMessage = "Link Google Maps không được vượt quá 500 ký tự")]
        public string? GgmapLink { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public string? PartnerName { get; set; }
        public int ConceptCount { get; set; }
    }

    public class LocationCreateViewModel
    {
        [Required(ErrorMessage = "Tên địa điểm là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên địa điểm không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        [StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự")]
        public string Address { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Quận/Huyện không được vượt quá 50 ký tự")]
        public string? District { get; set; }

        [StringLength(50, ErrorMessage = "Thành phố không được vượt quá 50 ký tự")]
        public string? City { get; set; }

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        public string Status { get; set; } = "Active";

        public int? PartnerId { get; set; }

        [Url(ErrorMessage = "Link Google Maps không hợp lệ")]
        [StringLength(500, ErrorMessage = "Link Google Maps không được vượt quá 500 ký tự")]
        public string? GgmapLink { get; set; }
    }

    public class LocationEditViewModel
    {
        public int LocationId { get; set; }

        [Required(ErrorMessage = "Tên địa điểm là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên địa điểm không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        [StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự")]
        public string Address { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Quận/Huyện không được vượt quá 50 ký tự")]
        public string? District { get; set; }

        [StringLength(50, ErrorMessage = "Thành phố không được vượt quá 50 ký tự")]
        public string? City { get; set; }

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        public string Status { get; set; } = "Active";

        public int? PartnerId { get; set; }

        [Url(ErrorMessage = "Link Google Maps không hợp lệ")]
        [StringLength(500, ErrorMessage = "Link Google Maps không được vượt quá 500 ký tự")]
        public string? GgmapLink { get; set; }

        public DateTime? CreatedAt { get; set; }
    }

    public class LocationDetailViewModel
    {
        public int LocationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? District { get; set; }
        public string? City { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? PartnerId { get; set; }
        public string? GgmapLink { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public string? PartnerName { get; set; }
        public List<LocationConceptViewModel> Concepts { get; set; } = new List<LocationConceptViewModel>();
    }

    public class LocationSearchViewModel
    {
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public int? PartnerId { get; set; }
        public string? City { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "Name";
        public string? SortOrder { get; set; } = "asc";
    }

    // ConceptViewModel for Location context
    public class LocationConceptViewModel
    {
        public int ConceptId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
    }
}
