using System.ComponentModel.DataAnnotations;

namespace MonAmour.ViewModels
{
    public class ConceptViewModel
    {
        public int ConceptId { get; set; }

        [Required(ErrorMessage = "Tên concept là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên concept không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        public decimal? Price { get; set; }

        public int? LocationId { get; set; }
        public List<int> ColorIds { get; set; } = new List<int>();
        public int? CategoryId { get; set; }
        public int? AmbienceId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Thời gian chuẩn bị phải lớn hơn hoặc bằng 0")]
        public int? PreparationTime { get; set; }

        public bool AvailabilityStatus { get; set; } = true;

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public string? LocationName { get; set; }
        public List<string> ColorNames { get; set; } = new List<string>();
        public string? CategoryName { get; set; }
        public string? AmbienceName { get; set; }
        public int ImageCount { get; set; }
    }

    public class ConceptCreateViewModel
    {
        [Required(ErrorMessage = "Tên concept là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên concept không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        public decimal? Price { get; set; }

        public int? LocationId { get; set; }
        public List<int> ColorIds { get; set; } = new List<int>();
        public int? CategoryId { get; set; }
        public int? AmbienceId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Thời gian chuẩn bị phải lớn hơn hoặc bằng 0")]
        public int? PreparationTime { get; set; }

        public bool AvailabilityStatus { get; set; } = true;
    }

    public class ConceptEditViewModel
    {
        public int ConceptId { get; set; }

        [Required(ErrorMessage = "Tên concept là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên concept không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        public decimal? Price { get; set; }

        public int? LocationId { get; set; }
        public List<int> ColorIds { get; set; } = new List<int>();
        public int? CategoryId { get; set; }
        public int? AmbienceId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Thời gian chuẩn bị phải lớn hơn hoặc bằng 0")]
        public int? PreparationTime { get; set; }

        public bool AvailabilityStatus { get; set; } = true;

        public DateTime? CreatedAt { get; set; }
    }

    public class ConceptDetailViewModel
    {
        public int ConceptId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int? LocationId { get; set; }
        public List<int> ColorIds { get; set; } = new List<int>();
        public int? CategoryId { get; set; }
        public int? AmbienceId { get; set; }
        public int? PreparationTime { get; set; }
        public bool AvailabilityStatus { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public string? LocationName { get; set; }
        public List<string> ColorNames { get; set; } = new List<string>();
        public string? CategoryName { get; set; }
        public string? AmbienceName { get; set; }
        public List<ConceptImgViewModel> Images { get; set; } = new List<ConceptImgViewModel>();
    }

    

    public class ConceptImgViewModel
    {
        public int ImgId { get; set; }
        public int ConceptId { get; set; }
        public string? ImgUrl { get; set; }
        public string? ImgName { get; set; }
        public string? AltText { get; set; }
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime? CreatedAt { get; set; }
    }


    // Dropdown ViewModels

    public class ConceptCategoryDropdownViewModel
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class ConceptColorDropdownViewModel
    {
        public int ColorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
    }

    public class ConceptAmbienceDropdownViewModel
    {
        public int AmbienceId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

}
