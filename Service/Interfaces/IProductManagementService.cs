using MonAmourDb_BE.DTOs;

namespace MonAmourDb_BE.Service.Interfaces
{
    public interface IProductManagementService
    {
        // ============ PRODUCT MANAGEMENT ============
        Task<PaginatedResult<ProductListDto>> GetProductsAsync(ProductFilterDto filter);
        Task<ProductDetailDto?> GetProductByIdAsync(int productId);
        Task<ProductDetailDto> CreateProductAsync(CreateProductDto createProductDto);
        Task<ProductDetailDto?> UpdateProductAsync(int productId, UpdateProductDto updateProductDto);
        Task<bool> DeleteProductAsync(int productId);
        Task<bool> ToggleProductStatusAsync(int productId);

        // ============ PRODUCT IMAGE MANAGEMENT ============
        Task<List<ProductImageDto>> GetProductImagesAsync(int productId);
        Task<ProductImageDto> AddProductImageAsync(int productId, CreateProductImageDto createImageDto);
        Task<ProductImageDto?> UpdateProductImageAsync(int productId, int imageId, UpdateProductImageDto updateImageDto);
        Task<bool> DeleteProductImageAsync(int productId, int imageId);
        Task<bool> SetPrimaryImageAsync(int productId, int imageId);
        Task<bool> ReorderImagesAsync(int productId, List<int> imageIds);

        // ============ PRODUCT CATEGORY MANAGEMENT ============
        Task<PaginatedResult<ProductCategoryDto>> GetProductCategoriesPagedAsync(ProductCategoryFilterDto filter);
        Task<ProductCategoryDetailDto?> GetProductCategoryByIdAsync(int categoryId);
        Task<ProductCategoryDetailDto> CreateProductCategoryAsync(CreateProductCategoryDto createCategoryDto);
        Task<ProductCategoryDetailDto?> UpdateProductCategoryAsync(int categoryId, UpdateProductCategoryDto updateCategoryDto);
        Task<bool> DeleteProductCategoryAsync(int categoryId);

        // ============ LOOKUP DATA ============
        Task<List<ProductCategoryDto>> GetProductCategoriesAsync();

        // ============ STATISTICS ============
        Task<ProductStatsDto> GetProductStatsAsync();
        Task<List<ProductPopularityDto>> GetPopularProductsAsync(int limit = 10);
        Task<List<ProductLowStockDto>> GetLowStockProductsAsync(int threshold = 10);

        // ============ BULK OPERATIONS ============
        Task<bool> BulkUpdateProductStatusAsync(List<int> productIds, string status);
        Task<bool> BulkUpdateProductCategoryAsync(List<int> productIds, int categoryId);
        Task<bool> BulkUpdateStockAsync(List<StockUpdateItem> stockUpdates);
        Task<bool> BulkDeleteProductsAsync(List<int> productIds);

        // ============ INVENTORY MANAGEMENT ============
        Task<bool> UpdateStockAsync(int productId, int newStock, string? note = null);
        Task<bool> AdjustStockAsync(int productId, int adjustment, string? note = null);
        Task<List<ProductListDto>> GetProductsByStockLevelAsync(int minStock, int maxStock);
    }
}
