using System.ComponentModel.DataAnnotations;

namespace MonAmourDb_BE.DTOs
{
    // DTO cho danh sách concept
    public class ConceptListDto
    {
        public int ConceptId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int? PreparationTime { get; set; }
        public bool? AvailabilityStatus { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Related info
        public string? LocationName { get; set; }
        public string? CategoryName { get; set; }
        public string? ColorName { get; set; }
        public string? AmbienceName { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public int TotalImages { get; set; }
        public int TotalBookings { get; set; }
    }

    // DTO chi tiết concept
    public class ConceptDetailDto : ConceptListDto
    {
        public int? LocationId { get; set; }
        public int? ColorId { get; set; }
        public int? CategoryId { get; set; }
        public int? AmbienceId { get; set; }

        // Related objects
        public LocationBasicDto? Location { get; set; }
        public ConceptCategoryDto? Category { get; set; }
        public ConceptColorDto? Color { get; set; }
        public ConceptAmbienceDto? Ambience { get; set; }
        public List<ConceptImageDto> Images { get; set; } = new List<ConceptImageDto>();
        public List<BookingBasicDto> RecentBookings { get; set; } = new List<BookingBasicDto>();
    }

    // DTO tạo concept mới
    public class CreateConceptDto
    {
        [Required(ErrorMessage = "Tên concept là bắt buộc")]
        [StringLength(255, ErrorMessage = "Tên concept không được vượt quá 255 ký tự")]
        public string Name { get; set; } = null!;

        [StringLength(2000, ErrorMessage = "Mô tả không được vượt quá 2000 ký tự")]
        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "Vị trí là bắt buộc")]
        public int LocationId { get; set; }

        public int? ColorId { get; set; }
        public int? CategoryId { get; set; }
        public int? AmbienceId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Thời gian chuẩn bị phải lớn hơn hoặc bằng 0")]
        public int? PreparationTime { get; set; }

        public bool AvailabilityStatus { get; set; } = true;
    }

    // DTO cập nhật concept
    public class UpdateConceptDto
    {
        [StringLength(255, ErrorMessage = "Tên concept không được vượt quá 255 ký tự")]
        public string? Name { get; set; }

        [StringLength(2000, ErrorMessage = "Mô tả không được vượt quá 2000 ký tự")]
        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        public decimal? Price { get; set; }

        public int? LocationId { get; set; }
        public int? ColorId { get; set; }
        public int? CategoryId { get; set; }
        public int? AmbienceId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Thời gian chuẩn bị phải lớn hơn hoặc bằng 0")]
        public int? PreparationTime { get; set; }

        public bool? AvailabilityStatus { get; set; }
    }

    // DTO filter concept
    public class ConceptFilterDto : PaginationDto
    {
        public string? SearchTerm { get; set; }
        public int? LocationId { get; set; }
        public int? CategoryId { get; set; }
        public int? ColorId { get; set; }
        public int? AmbienceId { get; set; }
        public bool? AvailabilityStatus { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string SortBy { get; set; } = "CreatedAt";
        public string SortOrder { get; set; } = "desc";
    }

    // DTO thống kê concept
    public class ConceptStatsDto
    {
        public int TotalConcepts { get; set; }
        public int AvailableConcepts { get; set; }
        public int UnavailableConcepts { get; set; }
        public int NewConceptsThisMonth { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<ConceptCategoryStatsDto> CategoryStats { get; set; } = new List<ConceptCategoryStatsDto>();
        public List<ConceptPopularityDto> PopularConcepts { get; set; } = new List<ConceptPopularityDto>();
    }

    public class ConceptCategoryStatsDto
    {
        public string CategoryName { get; set; } = null!;
        public int ConceptCount { get; set; }
        public int BookingCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class ConceptPopularityDto
    {
        public int ConceptId { get; set; }
        public string ConceptName { get; set; } = null!;
        public int BookingCount { get; set; }
        public decimal Revenue { get; set; }
        public string? PrimaryImageUrl { get; set; }
    }

    // Supporting DTOs
    public class ConceptImageDto
    {
        public int ImgId { get; set; }
        public int? ConceptId { get; set; }
        public string? ImgUrl { get; set; }
        public string? ImgName { get; set; }
        public string? AltText { get; set; }
        public bool? IsPrimary { get; set; }
        public int? DisplayOrder { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class CreateConceptImageDto
    {
        [Required(ErrorMessage = "URL hình ảnh là bắt buộc")]
        public string ImgUrl { get; set; } = null!;

        [StringLength(255, ErrorMessage = "Tên hình ảnh không được vượt quá 255 ký tự")]
        public string? ImgName { get; set; }

        [StringLength(255, ErrorMessage = "Alt text không được vượt quá 255 ký tự")]
        public string? AltText { get; set; }

        public bool IsPrimary { get; set; } = false;

        [Range(0, int.MaxValue, ErrorMessage = "Thứ tự hiển thị phải lớn hơn hoặc bằng 0")]
        public int DisplayOrder { get; set; } = 0;
    }

    public class UpdateConceptImageDto
    {
        public string? ImgUrl { get; set; }
        public string? ImgName { get; set; }
        public string? AltText { get; set; }
        public bool? IsPrimary { get; set; }
        public int? DisplayOrder { get; set; }
    }

    public class ConceptCategoryDto
    {
        public int CategoryId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ConceptColorDto
    {
        public int ColorId { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
    }

    public class ConceptAmbienceDto
    {
        public int AmbienceId { get; set; }
        public string? Name { get; set; }
    }

    public class LocationBasicDto
    {
        public int LocationId { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? District { get; set; }
        public string? City { get; set; }
        public string? Status { get; set; }
    }

    public class BookingBasicDto
    {
        public int BookingId { get; set; }
        public DateOnly? BookingDate { get; set; }  // Đổi từ DateTime? thành DateOnly?
        public TimeOnly? BookingTime { get; set; }  // Đổi từ TimeSpan? thành TimeOnly?
        public string? Status { get; set; }
        public decimal? TotalPrice { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
    }

    // ============ CONCEPT CATEGORY MANAGEMENT ============
    public class CreateConceptCategoryDto
    {
        [Required(ErrorMessage = "Tên category là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên category không được vượt quá 100 ký tự")]
        public string Name { get; set; } = null!;

        [StringLength(255, ErrorMessage = "Mô tả không được vượt quá 255 ký tự")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateConceptCategoryDto
    {
        [StringLength(100, ErrorMessage = "Tên category không được vượt quá 100 ký tự")]
        public string? Name { get; set; }

        [StringLength(255, ErrorMessage = "Mô tả không được vượt quá 255 ký tự")]
        public string? Description { get; set; }

        public bool? IsActive { get; set; }
    }

    public class ConceptCategoryDetailDto : ConceptCategoryDto
    {
        public int ConceptCount { get; set; }
        public int BookingCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<ConceptListDto> RecentConcepts { get; set; } = new List<ConceptListDto>();
    }

    // ============ CONCEPT COLOR MANAGEMENT ============
    public class CreateConceptColorDto
    {
        [Required(ErrorMessage = "Tên màu là bắt buộc")]
        [StringLength(50, ErrorMessage = "Tên màu không được vượt quá 50 ký tự")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Mã màu là bắt buộc")]
        [StringLength(20, ErrorMessage = "Mã màu không được vượt quá 20 ký tự")]
        [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Mã màu phải có định dạng hex (ví dụ: #FF0000)")]
        public string Code { get; set; } = null!;
    }

    public class UpdateConceptColorDto
    {
        [StringLength(50, ErrorMessage = "Tên màu không được vượt quá 50 ký tự")]
        public string? Name { get; set; }

        [StringLength(20, ErrorMessage = "Mã màu không được vượt quá 20 ký tự")]
        [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Mã màu phải có định dạng hex (ví dụ: #FF0000)")]
        public string? Code { get; set; }
    }

    public class ConceptColorDetailDto : ConceptColorDto
    {
        public int ConceptCount { get; set; }
        public int BookingCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<ConceptListDto> RecentConcepts { get; set; } = new List<ConceptListDto>();
    }

    // ============ CONCEPT AMBIENCE MANAGEMENT ============
    public class CreateConceptAmbienceDto
    {
        [Required(ErrorMessage = "Tên ambience là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên ambience không được vượt quá 100 ký tự")]
        public string Name { get; set; } = null!;
    }

    public class UpdateConceptAmbienceDto
    {
        [StringLength(100, ErrorMessage = "Tên ambience không được vượt quá 100 ký tự")]
        public string? Name { get; set; }
    }

    public class ConceptAmbienceDetailDto : ConceptAmbienceDto
    {
        public int ConceptCount { get; set; }
        public int BookingCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<ConceptListDto> RecentConcepts { get; set; } = new List<ConceptListDto>();
    }

    // ============ FILTER DTOs ============
    public class ConceptCategoryFilterDto : PaginationDto
    {
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public string SortBy { get; set; } = "Name";
        public string SortOrder { get; set; } = "asc";
    }

    public class ConceptColorFilterDto : PaginationDto
    {
        public string? SearchTerm { get; set; }
        public string SortBy { get; set; } = "Name";
        public string SortOrder { get; set; } = "asc";
    }

    public class ConceptAmbienceFilterDto : PaginationDto
    {
        public string? SearchTerm { get; set; }
        public string SortBy { get; set; } = "Name";
        public string SortOrder { get; set; } = "asc";
    }
}
