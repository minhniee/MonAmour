using System.ComponentModel.DataAnnotations;

namespace MonAmour.ViewModels
{
    public class BannerUploadViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn ảnh")]
        public IFormFile? ImageFile { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại banner")]
        public string? BannerType { get; set; } // "Homepage", "Service", "Product"

        public bool IsPrimary { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Thứ tự hiển thị phải là số dương")]
        public int DisplayOrder { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // Resize options
        public bool EnableResize { get; set; }
        public int? MaxWidth { get; set; }
        public int? MaxHeight { get; set; }
        public bool MaintainAspectRatio { get; set; } = true;
    }

    public class BannerTypeOption
    {
        public required string Value { get; set; }
        public required string Label { get; set; }
        public required string Description { get; set; }
        public int DefaultMaxWidth { get; set; }
        public int DefaultMaxHeight { get; set; }

        public static List<BannerTypeOption> GetOptions()
        {
            return new List<BannerTypeOption>
            {
                new BannerTypeOption 
                { 
                    Value = "Homepage",
                    Label = "Banner Trang Chủ",
                    Description = "Banner hiển thị ở slider trang chủ",
                    DefaultMaxWidth = 1920,
                    DefaultMaxHeight = 1080
                },
                new BannerTypeOption 
                { 
                    Value = "Service",
                    Label = "Banner Dịch Vụ",
                    Description = "Banner hiển thị ở phần dịch vụ",
                    DefaultMaxWidth = 800,
                    DefaultMaxHeight = 600
                },
                new BannerTypeOption 
                { 
                    Value = "Product",
                    Label = "Banner Sản Phẩm",
                    Description = "Banner hiển thị ở phần sản phẩm",
                    DefaultMaxWidth = 800,
                    DefaultMaxHeight = 600
                }
            };
        }
    }
}
