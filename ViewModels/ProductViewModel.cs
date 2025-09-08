using System.ComponentModel.DataAnnotations;

namespace MonAmour.ViewModels
{
    // Product List View Model
    public class ProductListViewModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string PrimaryImageUrl { get; set; } = string.Empty;
    }

    // Product Search View Model
    public class ProductSearchViewModel
    {
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public string? Status { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "CreatedAt";
        public string SortOrder { get; set; } = "desc";
    }

    // Product Create View Model
    public class ProductCreateViewModel
    {
        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên sản phẩm không được vượt quá 200 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        public int CategoryId { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Giá là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal Price { get; set; }

        [StringLength(100, ErrorMessage = "Chất liệu không được vượt quá 100 ký tự")]
        public string? Material { get; set; }

        [StringLength(100, ErrorMessage = "Đối tượng khách hàng không được vượt quá 100 ký tự")]
        public string? TargetAudience { get; set; }

        [Required(ErrorMessage = "Số lượng tồn kho là bắt buộc")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho phải lớn hơn hoặc bằng 0")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        public string Status { get; set; } = "active";
    }

    // Product Edit View Model
    public class ProductEditViewModel : ProductCreateViewModel
    {
        public int ProductId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Product Detail View Model
    public class ProductDetailViewModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Material { get; set; }
        public string? TargetAudience { get; set; }
        public int StockQuantity { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<ProductImgViewModel> Images { get; set; } = new List<ProductImgViewModel>();
    }

    // Product Category View Model
    public class ProductCategoryViewModel
    {
        public int CategoryId { get; set; }
        
        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;
        
        public int ProductCount { get; set; }
    }

    // Product Image View Model
    public class ProductImgViewModel
    {
        public int ImgId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty; // Thêm tên sản phẩm
        
        [Required(ErrorMessage = "URL hình ảnh là bắt buộc")]
        [Url(ErrorMessage = "URL hình ảnh không hợp lệ")]
        public string ImgUrl { get; set; } = string.Empty;
        
        [StringLength(200, ErrorMessage = "Tên hình ảnh không được vượt quá 200 ký tự")]
        public string? ImgName { get; set; }
        
        [StringLength(200, ErrorMessage = "Alt text không được vượt quá 200 ký tự")]
        public string? AltText { get; set; }
        
        public bool? IsPrimary { get; set; } // Thay đổi thành nullable
        public int DisplayOrder { get; set; }
        public DateTime? CreatedAt { get; set; } // Thay đổi thành nullable
    }
}
