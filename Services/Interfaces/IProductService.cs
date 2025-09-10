using MonAmour.Models;
using MonAmour.ViewModels;

namespace MonAmour.Services.Interfaces
{
    public interface IProductService
    {
        // Product CRUD operations
        Task<(List<ProductListViewModel> products, int totalCount)> GetProductsAsync(ProductSearchViewModel searchModel);
        Task<ProductDetailViewModel?> GetProductByIdAsync(int id);
        Task<bool> CreateProductAsync(ProductCreateViewModel model);
        Task<bool> UpdateProductAsync(ProductEditViewModel model);
        Task<bool> DeleteProductAsync(int id);
        
        // Product Category operations
        Task<List<ProductCategoryViewModel>> GetAllCategoriesAsync();
        Task<ProductCategoryViewModel?> GetCategoryByIdAsync(int id);
        Task<bool> CreateCategoryAsync(ProductCategoryViewModel model);
        Task<bool> UpdateCategoryAsync(ProductCategoryViewModel model);
        Task<bool> DeleteCategoryAsync(int id);
        Task<bool> DeleteCategoryWithProductsAsync(int id, int? reassignToCategoryId = null);
        Task<List<object>> GetCategoriesForReassignmentAsync(int excludeCategoryId);
        
        // Product Image operations
        Task<List<ProductImgViewModel>> GetProductImagesAsync(int productId);
        Task<ProductImgViewModel?> GetProductImageByIdAsync(int imageId);
        Task<bool> AddProductImageAsync(ProductImgViewModel model);
        Task<bool> UpdateProductImageAsync(ProductImgViewModel model);
        Task<bool> DeleteProductImageAsync(int imageId);
        Task<bool> SetPrimaryImageAsync(int productId, int imageId);
        
        // Statistics
        Task<Dictionary<string, int>> GetProductStatisticsAsync();
        
        // Additional methods for image management
        Task<List<ProductImgViewModel>> GetAllProductImagesAsync();
        Task<List<object>> GetProductsForDropdownAsync();
        Task<List<object>> SearchProductsByNameAsync(string searchTerm);
        Task<List<object>> GetProductImagesGroupedByProductAsync();
        Task<int> GetProductImageCountAsync(int productId);
        Task<bool> CanProductAddMoreImagesAsync(int productId);
    }
}
