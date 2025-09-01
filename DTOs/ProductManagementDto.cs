using System.ComponentModel.DataAnnotations;

namespace MonAmourDb_BE.DTOs
{
    // ============ PRODUCT DTOs ============
    public class ProductListDto
    {
        public int ProductId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? Material { get; set; }
        public string? TargetAudience { get; set; }
        public int? StockQuantity { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CategoryName { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public int TotalImages { get; set; }
        public int TotalOrders { get; set; }
        public int TotalWishlist { get; set; }
        public decimal? AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }

    public class ProductDetailDto
    {
        public int ProductId { get; set; }
        public string? Name { get; set; }
        public int? CategoryId { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? Material { get; set; }
        public string? TargetAudience { get; set; }
        public int? StockQuantity { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }

        // Related data
        public string? CategoryName { get; set; }
        public ProductCategoryDto? Category { get; set; }
        public List<ProductImageDto> Images { get; set; } = new List<ProductImageDto>();
        public List<ReviewBasicDto> RecentReviews { get; set; } = new List<ReviewBasicDto>();
        public List<OrderItemBasicDto> RecentOrders { get; set; } = new List<OrderItemBasicDto>();

        // Statistics
        public string? PrimaryImageUrl { get; set; }
        public int TotalImages { get; set; }
        public int TotalOrders { get; set; }
        public int TotalWishlist { get; set; }
        public decimal? AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class CreateProductDto
    {
        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [StringLength(255, ErrorMessage = "Tên sản phẩm không được vượt quá 255 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        public int CategoryId { get; set; }

        [StringLength(2000, ErrorMessage = "Mô tả không được vượt quá 2000 ký tự")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Giá sản phẩm là bắt buộc")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal Price { get; set; }

        [StringLength(100, ErrorMessage = "Chất liệu không được vượt quá 100 ký tự")]
        public string? Material { get; set; }

        [StringLength(100, ErrorMessage = "Đối tượng mục tiêu không được vượt quá 100 ký tự")]
        public string? TargetAudience { get; set; }

        [Required(ErrorMessage = "Số lượng tồn kho là bắt buộc")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho không được âm")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        [RegularExpression("^(active|inactive|discontinued|out_of_stock)$",
            ErrorMessage = "Trạng thái phải là: active, inactive, discontinued, out_of_stock")]
        public string Status { get; set; } = "active";
    }

    public class UpdateProductDto
    {
        [StringLength(255, ErrorMessage = "Tên sản phẩm không được vượt quá 255 ký tự")]
        public string? Name { get; set; }

        public int? CategoryId { get; set; }

        [StringLength(2000, ErrorMessage = "Mô tả không được vượt quá 2000 ký tự")]
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal? Price { get; set; }

        [StringLength(100, ErrorMessage = "Chất liệu không được vượt quá 100 ký tự")]
        public string? Material { get; set; }

        [StringLength(100, ErrorMessage = "Đối tượng mục tiêu không được vượt quá 100 ký tự")]
        public string? TargetAudience { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho không được âm")]
        public int? StockQuantity { get; set; }

        [RegularExpression("^(active|inactive|discontinued|out_of_stock)$",
            ErrorMessage = "Trạng thái phải là: active, inactive, discontinued, out_of_stock")]
        public string? Status { get; set; }
    }

    public class ProductFilterDto : PaginationDto
    {
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public string? Status { get; set; }
        public string? Material { get; set; }
        public string? TargetAudience { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinStock { get; set; }
        public int? MaxStock { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string SortBy { get; set; } = "CreatedAt";
        public string SortOrder { get; set; } = "desc";
    }

    // ============ PRODUCT CATEGORY DTOs ============
    public class ProductCategoryDto
    {
        public int CategoryId { get; set; }
        public string? Name { get; set; }
    }

    public class ProductCategoryDetailDto
    {
        public int CategoryId { get; set; }
        public string? Name { get; set; }
        public int ProductCount { get; set; }
        public decimal? TotalRevenue { get; set; }
        public List<ProductListDto> TopProducts { get; set; } = new List<ProductListDto>();
    }

    public class CreateProductCategoryDto
    {
        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateProductCategoryDto
    {
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
        public string? Name { get; set; }
    }

    public class ProductCategoryFilterDto : PaginationDto
    {
        public string? SearchTerm { get; set; }
        public string SortBy { get; set; } = "Name";
        public string SortOrder { get; set; } = "asc";
    }

    // ============ PRODUCT IMAGE DTOs ============
    public class ProductImageDto
    {
        public int ImgId { get; set; }
        public int? ProductId { get; set; }
        public string? ImgUrl { get; set; }
        public string? ImgName { get; set; }
        public string? AltText { get; set; }
        public bool? IsPrimary { get; set; }
        public int? DisplayOrder { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class CreateProductImageDto
    {
        [Required(ErrorMessage = "URL hình ảnh là bắt buộc")]
        [Url(ErrorMessage = "URL hình ảnh không hợp lệ")]
        public string ImgUrl { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Tên hình ảnh không được vượt quá 255 ký tự")]
        public string? ImgName { get; set; }

        [StringLength(255, ErrorMessage = "Alt text không được vượt quá 255 ký tự")]
        public string? AltText { get; set; }

        public bool IsPrimary { get; set; } = false;

        [Range(0, int.MaxValue, ErrorMessage = "Thứ tự hiển thị không được âm")]
        public int DisplayOrder { get; set; } = 0;
    }

    public class UpdateProductImageDto
    {
        [Url(ErrorMessage = "URL hình ảnh không hợp lệ")]
        public string? ImgUrl { get; set; }

        [StringLength(255, ErrorMessage = "Tên hình ảnh không được vượt quá 255 ký tự")]
        public string? ImgName { get; set; }

        [StringLength(255, ErrorMessage = "Alt text không được vượt quá 255 ký tự")]
        public string? AltText { get; set; }

        public bool? IsPrimary { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Thứ tự hiển thị không được âm")]
        public int? DisplayOrder { get; set; }
    }

    // ============ RELATED DTOs ============
    public class ReviewBasicDto
    {
        public int ReviewId { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class OrderItemBasicDto
    {
        public int OrderItemId { get; set; }
        public int? OrderId { get; set; }
        public int? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? TotalPrice { get; set; }
        public DateTime? OrderDate { get; set; }
        public string? OrderStatus { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
    }

    // ============ STATISTICS DTOs ============
    public class ProductStatsDto
    {
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int InactiveProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public int DiscontinuedProducts { get; set; }
        public int NewProductsThisMonth { get; set; }
        public decimal? AveragePrice { get; set; }
        public decimal? TotalInventoryValue { get; set; }
        public decimal? TotalRevenue { get; set; }
        public List<ProductCategoryStatsDto> CategoryStats { get; set; } = new List<ProductCategoryStatsDto>();
        public List<ProductPopularityDto> PopularProducts { get; set; } = new List<ProductPopularityDto>();
        public List<ProductLowStockDto> LowStockProducts { get; set; } = new List<ProductLowStockDto>();
    }

    public class ProductCategoryStatsDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal AveragePrice { get; set; }
    }

    public class ProductPopularityDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public decimal? AverageRating { get; set; }
    }

    public class ProductLowStockDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public string? Status { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public DateTime? LastRestocked { get; set; }
    }

    // ============ BULK OPERATION DTOs ============
    public class BulkUpdateProductStatusDto
    {
        [Required(ErrorMessage = "Danh sách ID sản phẩm là bắt buộc")]
        [MinLength(1, ErrorMessage = "Phải chọn ít nhất 1 sản phẩm")]
        public List<int> ProductIds { get; set; } = new List<int>();

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        [RegularExpression("^(active|inactive|discontinued|out_of_stock)$",
            ErrorMessage = "Trạng thái phải là: active, inactive, discontinued, out_of_stock")]
        public string Status { get; set; } = string.Empty;
    }

    public class BulkUpdateProductCategoryDto
    {
        [Required(ErrorMessage = "Danh sách ID sản phẩm là bắt buộc")]
        [MinLength(1, ErrorMessage = "Phải chọn ít nhất 1 sản phẩm")]
        public List<int> ProductIds { get; set; } = new List<int>();

        [Required(ErrorMessage = "ID danh mục là bắt buộc")]
        public int CategoryId { get; set; }
    }

    public class BulkUpdateStockDto
    {
        [Required(ErrorMessage = "Danh sách cập nhật stock là bắt buộc")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 item để cập nhật")]
        public List<StockUpdateItem> StockUpdates { get; set; } = new List<StockUpdateItem>();
    }

    public class StockUpdateItem
    {
        [Required(ErrorMessage = "ID sản phẩm là bắt buộc")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm")]
        public int NewStock { get; set; }

        public string? Note { get; set; }
    }

}
