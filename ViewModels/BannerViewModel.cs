using System;
using System.ComponentModel.DataAnnotations;

namespace MonAmour.ViewModels
{
    // BannerService ViewModels
    public class BannerServiceListViewModel
    {
        public int BannerId { get; set; }
        public string ImgUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class BannerServiceCreateViewModel
    {
        [Required(ErrorMessage = "Hình ảnh banner là bắt buộc")]
        public IFormFile? ImageFile { get; set; }
        
        public string? ImgUrl { get; set; }

        public bool IsPrimary { get; set; } = false;

        [Range(0, int.MaxValue, ErrorMessage = "Thứ tự hiển thị phải là số dương")]
        public int DisplayOrder { get; set; } = 0;

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class BannerServiceEditViewModel
    {
        public int BannerId { get; set; }

        public IFormFile? ImageFile { get; set; }
        
        public string? ImgUrl { get; set; }

        public bool IsPrimary { get; set; } = false;

        [Range(0, int.MaxValue, ErrorMessage = "Thứ tự hiển thị phải là số dương")]
        public int DisplayOrder { get; set; } = 0;

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    // BannerHomepage ViewModels
    public class BannerHomepageListViewModel
    {
        public int BannerId { get; set; }
        public string ImgUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class BannerHomepageCreateViewModel
    {
        [Required(ErrorMessage = "Hình ảnh banner là bắt buộc")]
        public IFormFile? ImageFile { get; set; }
        
        public string? ImgUrl { get; set; }

        public bool IsPrimary { get; set; } = false;

        [Range(0, int.MaxValue, ErrorMessage = "Thứ tự hiển thị phải là số dương")]
        public int DisplayOrder { get; set; } = 0;

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class BannerHomepageEditViewModel
    {
        public int BannerId { get; set; }

        public IFormFile? ImageFile { get; set; }
        
        public string? ImgUrl { get; set; }

        public bool IsPrimary { get; set; } = false;

        [Range(0, int.MaxValue, ErrorMessage = "Thứ tự hiển thị phải là số dương")]
        public int DisplayOrder { get; set; } = 0;

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    // BannerProduct ViewModels
    public class BannerProductListViewModel
    {
        public int BannerId { get; set; }
        public string ImgUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class BannerProductCreateViewModel
    {
        [Required(ErrorMessage = "Hình ảnh banner là bắt buộc")]
        public IFormFile? ImageFile { get; set; }
        
        public string? ImgUrl { get; set; }

        public bool IsPrimary { get; set; } = false;

        [Range(0, int.MaxValue, ErrorMessage = "Thứ tự hiển thị phải là số dương")]
        public int DisplayOrder { get; set; } = 0;

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class BannerProductEditViewModel
    {
        public int BannerId { get; set; }

        public IFormFile? ImageFile { get; set; }
        
        public string? ImgUrl { get; set; }

        public bool IsPrimary { get; set; } = false;

        [Range(0, int.MaxValue, ErrorMessage = "Thứ tự hiển thị phải là số dương")]
        public int DisplayOrder { get; set; } = 0;

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
