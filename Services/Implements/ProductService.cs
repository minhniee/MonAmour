using Microsoft.EntityFrameworkCore;
using MonAmour.Models;
using MonAmour.Services.Interfaces;
using MonAmour.ViewModels;

namespace MonAmour.Services.Implements
{
    public class ProductService : IProductService
    {
        private readonly MonAmourDbContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(MonAmourDbContext context, ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Product CRUD operations
        public async Task<(List<ProductListViewModel> products, int totalCount)> GetProductsAsync(ProductSearchViewModel searchModel)
        {
            try
            {
                var query = _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.ProductImgs.Where(pi => pi.IsPrimary == true))
                    .AsQueryable();

                // Apply search filters
                if (!string.IsNullOrEmpty(searchModel.SearchTerm))
                {
                    query = query.Where(p => p.Name.Contains(searchModel.SearchTerm) || 
                                           (p.Description != null && p.Description.Contains(searchModel.SearchTerm)));
                }

                if (searchModel.CategoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == searchModel.CategoryId.Value);
                }

                if (!string.IsNullOrEmpty(searchModel.Status))
                {
                    query = query.Where(p => p.Status == searchModel.Status);
                }

                if (searchModel.MinPrice.HasValue)
                {
                    query = query.Where(p => p.Price >= searchModel.MinPrice.Value);
                }

                if (searchModel.MaxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= searchModel.MaxPrice.Value);
                }

                // Get total count before pagination
                var totalCount = await query.CountAsync();

                // Apply sorting
                query = searchModel.SortBy.ToLower() switch
                {
                    "name" => searchModel.SortOrder == "asc" ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                    "price" => searchModel.SortOrder == "asc" ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
                    "stockquantity" => searchModel.SortOrder == "asc" ? query.OrderBy(p => p.StockQuantity) : query.OrderByDescending(p => p.StockQuantity),
                    "createdat" => searchModel.SortOrder == "asc" ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt),
                    _ => searchModel.SortOrder == "asc" ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt)
                };

                // Apply pagination
                var products = await query
                    .Skip((searchModel.Page - 1) * searchModel.PageSize)
                    .Take(searchModel.PageSize)
                    .Select(p => new ProductListViewModel
                    {
                        ProductId = p.ProductId,
                        Name = p.Name ?? string.Empty,
                        CategoryName = p.Category != null ? p.Category.Name ?? string.Empty : string.Empty,
                        Price = p.Price ?? 0,
                        StockQuantity = p.StockQuantity ?? 0,
                        Status = p.Status ?? string.Empty,
                        CreatedAt = p.CreatedAt ?? DateTime.Now,
                        PrimaryImageUrl = p.ProductImgs.OrderBy(pi => pi.IsPrimary == true ? 0 : 1).ThenBy(pi => pi.DisplayOrder).FirstOrDefault().ImgUrl ?? string.Empty
                    })
                    .ToListAsync();

                return (products, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                throw;
            }
        }

        public async Task<ProductDetailViewModel?> GetProductByIdAsync(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.ProductImgs.OrderBy(pi => pi.DisplayOrder))
                    .FirstOrDefaultAsync(p => p.ProductId == id);

                if (product == null) return null;

                return new ProductDetailViewModel
                {
                    ProductId = product.ProductId,
                    Name = product.Name ?? string.Empty,
                    CategoryId = product.CategoryId ?? 0,
                    CategoryName = product.Category?.Name ?? string.Empty,
                    Description = product.Description,
                    Price = product.Price ?? 0,
                    Material = product.Material,
                    TargetAudience = product.TargetAudience,
                    StockQuantity = product.StockQuantity ?? 0,
                    Status = product.Status ?? string.Empty,
                    CreatedAt = product.CreatedAt ?? DateTime.Now,
                    Images = product.ProductImgs.Select(pi => new ProductImgViewModel
                    {
                        ImgId = pi.ImgId,
                        ProductId = pi.ProductId ?? 0,
                        ImgUrl = pi.ImgUrl ?? string.Empty,
                        ImgName = pi.ImgName,
                        AltText = pi.AltText,
                        IsPrimary = pi.IsPrimary ?? false,
                        DisplayOrder = pi.DisplayOrder ?? 0,
                        CreatedAt = pi.CreatedAt ?? DateTime.Now
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product by id: {Id}", id);
                throw;
            }
        }

        public async Task<bool> CreateProductAsync(ProductCreateViewModel model)
        {
            try
            {
                var product = new Product
                {
                    Name = model.Name,
                    CategoryId = model.CategoryId,
                    Description = model.Description,
                    Price = model.Price,
                    Material = model.Material,
                    TargetAudience = model.TargetAudience,
                    StockQuantity = model.StockQuantity,
                    Status = model.Status,
                    CreatedAt = DateTime.Now
                };

                _context.Products.Add(product);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                throw;
            }
        }

        public async Task<bool> UpdateProductAsync(ProductEditViewModel model)
        {
            try
            {
                var product = await _context.Products.FindAsync(model.ProductId);
                if (product == null) return false;

                product.Name = model.Name;
                product.CategoryId = model.CategoryId;
                product.Description = model.Description;
                product.Price = model.Price;
                product.Material = model.Material;
                product.TargetAudience = model.TargetAudience;
                product.StockQuantity = model.StockQuantity;
                product.Status = model.Status;

                _context.Products.Update(product);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product: {Id}", model.ProductId);
                throw;
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null) return false;

                _context.Products.Remove(product);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product: {Id}", id);
                throw;
            }
        }

        // Product Category operations
        public async Task<List<ProductCategoryViewModel>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _context.ProductCategories
                    .Select(c => new ProductCategoryViewModel
                    {
                        CategoryId = c.CategoryId,
                        Name = c.Name ?? string.Empty,
                        ProductCount = c.Products.Count
                    })
                    .ToListAsync();

                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories");
                throw;
            }
        }

        public async Task<ProductCategoryViewModel?> GetCategoryByIdAsync(int id)
        {
            try
            {
                var category = await _context.ProductCategories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.CategoryId == id);

                if (category == null) return null;

                return new ProductCategoryViewModel
                {
                    CategoryId = category.CategoryId,
                    Name = category.Name ?? string.Empty,
                    ProductCount = category.Products.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category by id: {Id}", id);
                throw;
            }
        }

        public async Task<bool> CreateCategoryAsync(ProductCategoryViewModel model)
        {
            try
            {
                var category = new ProductCategory
                {
                    Name = model.Name
                };

                _context.ProductCategories.Add(category);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                throw;
            }
        }

        public async Task<bool> UpdateCategoryAsync(ProductCategoryViewModel model)
        {
            try
            {
                var category = await _context.ProductCategories.FindAsync(model.CategoryId);
                if (category == null) return false;

                category.Name = model.Name;

                _context.ProductCategories.Update(category);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category: {Id}", model.CategoryId);
                throw;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _context.ProductCategories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.CategoryId == id);

                if (category == null) return false;

                // Check if category has products
                if (category.Products.Any())
                {
                    return false; // Cannot delete category with products
                }

                _context.ProductCategories.Remove(category);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category: {Id}", id);
                throw;
            }
        }

        // Product Image operations
        public async Task<List<ProductImgViewModel>> GetProductImagesAsync(int productId)
        {
            try
            {
                var images = await _context.ProductImgs
                    .Where(pi => pi.ProductId == productId)
                    .OrderBy(pi => pi.DisplayOrder)
                    .Select(pi => new ProductImgViewModel
                    {
                        ImgId = pi.ImgId,
                        ProductId = pi.ProductId ?? 0,
                        ImgUrl = pi.ImgUrl ?? string.Empty,
                        ImgName = pi.ImgName,
                        AltText = pi.AltText,
                        IsPrimary = pi.IsPrimary ?? false,
                        DisplayOrder = pi.DisplayOrder ?? 0,
                        CreatedAt = pi.CreatedAt ?? DateTime.Now
                    })
                    .ToListAsync();

                return images;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product images: {ProductId}", productId);
                throw;
            }
        }

        public async Task<ProductImgViewModel?> GetProductImageByIdAsync(int imageId)
        {
            try
            {
                var image = await _context.ProductImgs
                    .FirstOrDefaultAsync(pi => pi.ImgId == imageId);

                if (image == null) return null;

                return new ProductImgViewModel
                {
                    ImgId = image.ImgId,
                    ProductId = image.ProductId ?? 0,
                    ImgUrl = image.ImgUrl ?? string.Empty,
                    ImgName = image.ImgName,
                    AltText = image.AltText,
                    IsPrimary = image.IsPrimary ?? false,
                    DisplayOrder = image.DisplayOrder ?? 0,
                    CreatedAt = image.CreatedAt ?? DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product image by ID: {ImageId}", imageId);
                throw;
            }
        }

        public async Task<bool> AddProductImageAsync(ProductImgViewModel model)
        {
            try
            {
                // Kiểm tra giới hạn 3 hình ảnh cho mỗi sản phẩm
                var existingImageCount = await _context.ProductImgs
                    .Where(pi => pi.ProductId == model.ProductId)
                    .CountAsync();

                if (existingImageCount >= 3)
                {
                    _logger.LogWarning("Product {ProductId} already has maximum 3 images", model.ProductId);
                    return false;
                }

                var image = new ProductImg
                {
                    ProductId = model.ProductId,
                    ImgUrl = model.ImgUrl,
                    ImgName = model.ImgName,
                    AltText = model.AltText,
                    IsPrimary = model.IsPrimary,
                    DisplayOrder = model.DisplayOrder,
                    CreatedAt = DateTime.Now
                };

                // If this is the first image for the product, make it primary
                if (model.IsPrimary == true)
                {
                    var existingImages = await _context.ProductImgs
                        .Where(pi => pi.ProductId == model.ProductId)
                        .ToListAsync();
                    
                    foreach (var img in existingImages)
                    {
                        img.IsPrimary = false;
                    }
                }

                _context.ProductImgs.Add(image);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product image: {ProductId}", model.ProductId);
                return false;
            }
        }

        public async Task<bool> UpdateProductImageAsync(ProductImgViewModel model)
        {
            try
            {
                var image = await _context.ProductImgs.FindAsync(model.ImgId);
                if (image == null) return false;

                image.ImgUrl = model.ImgUrl;
                image.ImgName = model.ImgName;
                image.AltText = model.AltText;
                image.IsPrimary = model.IsPrimary;
                image.DisplayOrder = model.DisplayOrder;

                _context.ProductImgs.Update(image);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product image: {Id}", model.ImgId);
                throw;
            }
        }

        public async Task<bool> DeleteProductImageAsync(int imageId)
        {
            try
            {
                var image = await _context.ProductImgs.FindAsync(imageId);
                if (image == null)
                {
                    return false;
                }

                var wasPrimary = image.IsPrimary == true;
                var productId = image.ProductId;

                _context.ProductImgs.Remove(image);
                var result = await _context.SaveChangesAsync();

                // If we deleted the primary image, set another image as primary
                if (wasPrimary && result > 0)
                {
                    var nextImage = await _context.ProductImgs
                        .Where(pi => pi.ProductId == productId)
                        .OrderBy(pi => pi.DisplayOrder)
                        .FirstOrDefaultAsync();
                    
                    if (nextImage != null)
                    {
                        nextImage.IsPrimary = true;
                        await _context.SaveChangesAsync();
                    }
                }

                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product image: {ImageId}", imageId);
                return false;
            }
        }

        public async Task<bool> SetPrimaryImageAsync(int productId, int imageId)
        {
            try
            {
                // Remove primary status from all images of this product
                var allImages = await _context.ProductImgs
                    .Where(pi => pi.ProductId == productId)
                    .ToListAsync();

                foreach (var img in allImages)
                {
                    img.IsPrimary = false;
                }

                // Set the specified image as primary
                var primaryImage = allImages.FirstOrDefault(pi => pi.ImgId == imageId);
                if (primaryImage != null)
                {
                    primaryImage.IsPrimary = true;
                }

                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting primary image: {ProductId}, {ImageId}", productId, imageId);
                throw;
            }
        }

        // Statistics
        public async Task<Dictionary<string, int>> GetProductStatisticsAsync()
        {
            try
            {
                var stats = new Dictionary<string, int>
                {
                    ["TotalProducts"] = await _context.Products.CountAsync(),
                    ["ActiveProducts"] = await _context.Products.CountAsync(p => p.Status == "active"),
                    ["InactiveProducts"] = await _context.Products.CountAsync(p => p.Status == "inactive"),
                    ["OutOfStock"] = await _context.Products.CountAsync(p => p.StockQuantity <= 0),
                    ["LowStock"] = await _context.Products.CountAsync(p => p.StockQuantity > 0 && p.StockQuantity <= 10),
                    ["TotalCategories"] = await _context.ProductCategories.CountAsync()
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product statistics");
                throw;
            }
        }

        public async Task<List<ProductImgViewModel>> GetAllProductImagesAsync()
        {
            try
            {
                var images = await _context.ProductImgs
                    .Include(pi => pi.Product)
                    .OrderBy(pi => pi.ProductId)
                    .ThenBy(pi => pi.DisplayOrder)
                    .Select(pi => new ProductImgViewModel
                    {
                        ImgId = pi.ImgId,
                        ProductId = pi.ProductId ?? 0,
                        ImgUrl = pi.ImgUrl ?? string.Empty,
                        ImgName = pi.ImgName,
                        AltText = pi.AltText,
                        IsPrimary = pi.IsPrimary,
                        DisplayOrder = pi.DisplayOrder ?? 0,
                        CreatedAt = pi.CreatedAt ?? DateTime.Now,
                        ProductName = pi.Product != null ? pi.Product.Name : "Không xác định"
                    })
                    .ToListAsync();

                return images;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all product images");
                return new List<ProductImgViewModel>();
            }
        }

        public async Task<List<object>> GetProductsForDropdownAsync()
        {
            try
            {
                var products = await _context.Products
                    .Where(p => p.Status == "active")
                    .OrderBy(p => p.Name)
                    .Select(p => new
                    {
                        Value = p.ProductId,
                        Text = p.Name
                    })
                    .ToListAsync();

                return products.Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products for dropdown");
                return new List<object>();
            }
        }

        /// <summary>
        /// Get Product Image Count - lấy số lượng hình ảnh của sản phẩm
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public async Task<int> GetProductImageCountAsync(int productId)
        {
            try
            {
                return await _context.ProductImgs
                    .Where(pi => pi.ProductId == productId)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product image count for product {ProductId}", productId);
                return 0;
            }
        }

        /// <summary>
        /// Check if product can add more images - kiểm tra sản phẩm có thể thêm hình ảnh không
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public async Task<bool> CanProductAddMoreImagesAsync(int productId)
        {
            try
            {
                var imageCount = await GetProductImageCountAsync(productId);
                return imageCount < 3;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if product can add more images for product {ProductId}", productId);
                return false;
            }
        }

        /// <summary>
        /// Get Product Images Grouped by Product - lấy hình ảnh nhóm theo sản phẩm
        /// </summary>
        /// <returns></returns>
        public async Task<List<object>> GetProductImagesGroupedByProductAsync()
        {
            try
            {
                var groupedImages = await _context.Products
                    .Where(p => p.Status == "active")
                    .OrderBy(p => p.Name)
                    .Select(p => new
                    {
                        ProductId = p.ProductId,
                        ProductName = p.Name,
                        Images = p.ProductImgs
                            .OrderBy(pi => pi.IsPrimary == true ? 0 : 1)
                            .ThenBy(pi => pi.DisplayOrder)
                            .Select(pi => new ProductImgViewModel
                            {
                                ImgId = pi.ImgId,
                                ProductId = pi.ProductId ?? 0,
                                ImgUrl = pi.ImgUrl ?? string.Empty,
                                ImgName = pi.ImgName ?? string.Empty,
                                AltText = pi.AltText ?? string.Empty,
                                IsPrimary = pi.IsPrimary,
                                DisplayOrder = pi.DisplayOrder ?? 0,
                                CreatedAt = pi.CreatedAt ?? DateTime.Now
                            }).ToList()
                    })
                    .ToListAsync();

                return groupedImages.Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product images grouped by product");
                return new List<object>();
            }
        }


    }
}
