using Microsoft.EntityFrameworkCore;
using MonAmourDb_BE.DTOs;
using MonAmourDb_BE.Models;
using MonAmourDb_BE.Service.Interfaces;

namespace MonAmourDb_BE.Service.Implementations
{
    public class ProductManagementService : IProductManagementService
    {
        private readonly MonAmourDbContext _context;

        public ProductManagementService(MonAmourDbContext context)
        {
            _context = context;
        }

        // ============ PRODUCT MANAGEMENT ============
        public async Task<PaginatedResult<ProductListDto>> GetProductsAsync(ProductFilterDto filter)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImgs)
                .Include(p => p.OrderItems)
                .Include(p => p.WishLists)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(p => p.Name!.Contains(filter.SearchTerm) ||
                                       p.Description!.Contains(filter.SearchTerm) ||
                                       p.Material!.Contains(filter.SearchTerm));
            }

            if (filter.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == filter.CategoryId);

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(p => p.Status == filter.Status);

            if (!string.IsNullOrEmpty(filter.Material))
                query = query.Where(p => p.Material!.Contains(filter.Material));

            if (!string.IsNullOrEmpty(filter.TargetAudience))
                query = query.Where(p => p.TargetAudience!.Contains(filter.TargetAudience));

            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.Price >= filter.MinPrice);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= filter.MaxPrice);

            if (filter.MinStock.HasValue)
                query = query.Where(p => p.StockQuantity >= filter.MinStock);

            if (filter.MaxStock.HasValue)
                query = query.Where(p => p.StockQuantity <= filter.MaxStock);

            if (filter.CreatedFrom.HasValue)
                query = query.Where(p => p.CreatedAt >= filter.CreatedFrom);

            if (filter.CreatedTo.HasValue)
                query = query.Where(p => p.CreatedAt <= filter.CreatedTo);

            // Apply sorting
            query = filter.SortBy.ToLower() switch
            {
                "name" => filter.SortOrder.ToLower() == "asc" ?
                    query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                "price" => filter.SortOrder.ToLower() == "asc" ?
                    query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
                "stock" => filter.SortOrder.ToLower() == "asc" ?
                    query.OrderBy(p => p.StockQuantity) : query.OrderByDescending(p => p.StockQuantity),
                "createdat" => filter.SortOrder.ToLower() == "asc" ?
                    query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

            var products = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(p => new ProductListDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Material = p.Material,
                    TargetAudience = p.TargetAudience,
                    StockQuantity = p.StockQuantity,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    CategoryName = p.Category!.Name,
                    PrimaryImageUrl = p.ProductImgs
                        .Where(img => img.IsPrimary == true)
                        .Select(img => img.ImgUrl)
                        .FirstOrDefault(),
                    TotalImages = p.ProductImgs.Count,
                    TotalOrders = p.OrderItems.Count,
                    TotalWishlist = p.WishLists.Count,
                    AverageRating = _context.Reviews
                        .Where(r => r.TargetType == "product" && r.TargetId == p.ProductId)
                        .Average(r => (decimal?)r.Rating),
                    ReviewCount = _context.Reviews
                        .Count(r => r.TargetType == "product" && r.TargetId == p.ProductId)
                })
                .ToListAsync();

            return new PaginatedResult<ProductListDto>
            {
                Data = products,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = totalPages,
                HasNextPage = filter.Page < totalPages,
                HasPreviousPage = filter.Page > 1
            };
        }

        public async Task<ProductDetailDto?> GetProductByIdAsync(int productId)
        {
            // Query chính - chỉ lấy dữ liệu cần thiết
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImgs.OrderBy(img => img.DisplayOrder))
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null) return null;

            // Query riêng cho reviews để tính toán chính xác
            var reviewsQuery = _context.Reviews
                .Where(r => r.TargetType == "product" && r.TargetId == productId);

            var reviews = await reviewsQuery
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .Select(r => new ReviewBasicDto
                {
                    ReviewId = r.ReviewId,
                    UserId = r.UserId,
                    UserName = r.User != null ? r.User.Name : "Unknown User",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            // Tính rating trung bình từ tất cả reviews (không chỉ 5 reviews gần nhất)
            var averageRating = await reviewsQuery.AverageAsync(r => (decimal?)r.Rating);
            var totalReviewCount = await reviewsQuery.CountAsync();

            // Query riêng cho order items với proper joins
            var recentOrders = await _context.OrderItems
                .Where(oi => oi.ProductId == productId)
                .Include(oi => oi.Order)
                    .ThenInclude(o => o != null ? o.User : null)
                .OrderByDescending(oi => oi.Order != null ? oi.Order.CreatedAt : DateTime.MinValue)
                .Take(5)
                .Select(oi => new OrderItemBasicDto
                {
                    OrderItemId = oi.OrderItemId,
                    OrderId = oi.OrderId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice,
                    OrderDate = oi.Order != null ? oi.Order.CreatedAt : null,
                    OrderStatus = oi.Order != null ? oi.Order.Status : "Unknown",
                    UserName = oi.Order != null && oi.Order.User != null ? oi.Order.User.Name : "Unknown User",
                    UserEmail = oi.Order != null && oi.Order.User != null ? oi.Order.User.Email : "Unknown Email"
                })
                .ToListAsync();

            // Query riêng cho statistics
            var totalOrders = await _context.OrderItems
                .CountAsync(oi => oi.ProductId == productId);

            var totalWishlist = await _context.WishLists
                .CountAsync(w => w.ProductId == productId);

            var totalRevenue = await _context.OrderItems
                .Where(oi => oi.ProductId == productId &&
                            oi.Order != null &&
                            oi.Order.Status == "completed")
                .SumAsync(oi => oi.TotalPrice);

            return new ProductDetailDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                CategoryId = product.CategoryId,
                Description = product.Description,
                Price = product.Price,
                Material = product.Material,
                TargetAudience = product.TargetAudience,
                StockQuantity = product.StockQuantity,
                Status = product.Status,
                CreatedAt = product.CreatedAt,
                CategoryName = product.Category?.Name,
                Category = product.Category != null ? new ProductCategoryDto
                {
                    CategoryId = product.Category.CategoryId,
                    Name = product.Category.Name
                } : null,
                Images = product.ProductImgs
                    .Select(img => new ProductImageDto
                    {
                        ImgId = img.ImgId,
                        ProductId = img.ProductId,
                        ImgUrl = img.ImgUrl,
                        ImgName = img.ImgName,
                        AltText = img.AltText,
                        IsPrimary = img.IsPrimary,
                        DisplayOrder = img.DisplayOrder,
                        CreatedAt = img.CreatedAt
                    }).ToList(),
                RecentReviews = reviews,
                RecentOrders = recentOrders,
                PrimaryImageUrl = product.ProductImgs
                    .Where(img => img.IsPrimary == true)
                    .Select(img => img.ImgUrl)
                    .FirstOrDefault(),
                TotalImages = product.ProductImgs.Count,
                TotalOrders = totalOrders,
                TotalWishlist = totalWishlist,
                AverageRating = averageRating,
                ReviewCount = totalReviewCount,
                TotalRevenue = totalRevenue
            };
        }


        public async Task<ProductDetailDto> CreateProductAsync(CreateProductDto createProductDto)
        {
            // Validate category exists
            var categoryExists = await _context.ProductCategories
                .AnyAsync(c => c.CategoryId == createProductDto.CategoryId);
            if (!categoryExists)
                throw new ArgumentException("Danh mục không tồn tại");

            var product = new Product
            {
                Name = createProductDto.Name,
                CategoryId = createProductDto.CategoryId,
                Description = createProductDto.Description,
                Price = createProductDto.Price,
                Material = createProductDto.Material,
                TargetAudience = createProductDto.TargetAudience,
                StockQuantity = createProductDto.StockQuantity,
                Status = createProductDto.Status,
                CreatedAt = DateTime.Now
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return (await GetProductByIdAsync(product.ProductId))!;
        }

        public async Task<ProductDetailDto?> UpdateProductAsync(int productId, UpdateProductDto updateProductDto)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return null;

            // Update only provided fields
            if (!string.IsNullOrEmpty(updateProductDto.Name))
                product.Name = updateProductDto.Name;

            if (updateProductDto.CategoryId.HasValue)
            {
                var categoryExists = await _context.ProductCategories
                    .AnyAsync(c => c.CategoryId == updateProductDto.CategoryId);
                if (!categoryExists)
                    throw new ArgumentException("Danh mục không tồn tại");
                product.CategoryId = updateProductDto.CategoryId;
            }

            if (updateProductDto.Description != null)
                product.Description = updateProductDto.Description;

            if (updateProductDto.Price.HasValue)
                product.Price = updateProductDto.Price;

            if (updateProductDto.Material != null)
                product.Material = updateProductDto.Material;

            if (updateProductDto.TargetAudience != null)
                product.TargetAudience = updateProductDto.TargetAudience;

            if (updateProductDto.StockQuantity.HasValue)
                product.StockQuantity = updateProductDto.StockQuantity;

            if (!string.IsNullOrEmpty(updateProductDto.Status))
                product.Status = updateProductDto.Status;

            await _context.SaveChangesAsync();
            return await GetProductByIdAsync(productId);
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            var product = await _context.Products
                .Include(p => p.OrderItems)
                .Include(p => p.ProductImgs)
                .Include(p => p.WishLists)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null) return false;

            // Check if product has orders
            if (product.OrderItems.Any())
                throw new InvalidOperationException("Không thể xóa sản phẩm đã có đơn hàng");

            // Remove related data
            _context.ProductImgs.RemoveRange(product.ProductImgs);
            _context.WishLists.RemoveRange(product.WishLists);

            // Remove reviews
            var reviews = await _context.Reviews
                .Where(r => r.TargetType == "product" && r.TargetId == productId)
                .ToListAsync();
            _context.Reviews.RemoveRange(reviews);

            // Remove product
            _context.Products.Remove(product);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleProductStatusAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            product.Status = product.Status switch
            {
                "active" => "inactive",
                "inactive" => "active",
                _ => product.Status
            };

            await _context.SaveChangesAsync();
            return true;
        }

        // ============ PRODUCT IMAGE MANAGEMENT ============
        public async Task<List<ProductImageDto>> GetProductImagesAsync(int productId)
        {
            return await _context.ProductImgs
                .Where(img => img.ProductId == productId)
                .OrderBy(img => img.DisplayOrder)
                .Select(img => new ProductImageDto
                {
                    ImgId = img.ImgId,
                    ProductId = img.ProductId,
                    ImgUrl = img.ImgUrl,
                    ImgName = img.ImgName,
                    AltText = img.AltText,
                    IsPrimary = img.IsPrimary,
                    DisplayOrder = img.DisplayOrder,
                    CreatedAt = img.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<ProductImageDto> AddProductImageAsync(int productId, CreateProductImageDto createImageDto)
        {
            // Verify product exists
            var productExists = await _context.Products.AnyAsync(p => p.ProductId == productId);
            if (!productExists)
                throw new ArgumentException("Sản phẩm không tồn tại");

            // If this is set as primary, unset other primary images
            if (createImageDto.IsPrimary)
            {
                await _context.ProductImgs
                    .Where(img => img.ProductId == productId && img.IsPrimary == true)
                    .ExecuteUpdateAsync(img => img.SetProperty(i => i.IsPrimary, false));
            }

            var productImg = new ProductImg
            {
                ProductId = productId,
                ImgUrl = createImageDto.ImgUrl,
                ImgName = createImageDto.ImgName,
                AltText = createImageDto.AltText,
                IsPrimary = createImageDto.IsPrimary,
                DisplayOrder = createImageDto.DisplayOrder,
                CreatedAt = DateTime.Now
            };

            _context.ProductImgs.Add(productImg);
            await _context.SaveChangesAsync();

            return new ProductImageDto
            {
                ImgId = productImg.ImgId,
                ProductId = productImg.ProductId,
                ImgUrl = productImg.ImgUrl,
                ImgName = productImg.ImgName,
                AltText = productImg.AltText,
                IsPrimary = productImg.IsPrimary,
                DisplayOrder = productImg.DisplayOrder,
                CreatedAt = productImg.CreatedAt
            };
        }

        public async Task<ProductImageDto?> UpdateProductImageAsync(int productId, int imageId, UpdateProductImageDto updateImageDto)
        {
            var image = await _context.ProductImgs
                .FirstOrDefaultAsync(img => img.ImgId == imageId && img.ProductId == productId);

            if (image == null) return null;

            // If setting as primary, unset other primary images
            if (updateImageDto.IsPrimary == true)
            {
                await _context.ProductImgs
                    .Where(img => img.ProductId == productId && img.ImgId != imageId && img.IsPrimary == true)
                    .ExecuteUpdateAsync(img => img.SetProperty(i => i.IsPrimary, false));
            }

            // Update fields
            if (!string.IsNullOrEmpty(updateImageDto.ImgUrl))
                image.ImgUrl = updateImageDto.ImgUrl;

            if (updateImageDto.ImgName != null)
                image.ImgName = updateImageDto.ImgName;

            if (updateImageDto.AltText != null)
                image.AltText = updateImageDto.AltText;

            if (updateImageDto.IsPrimary.HasValue)
                image.IsPrimary = updateImageDto.IsPrimary;

            if (updateImageDto.DisplayOrder.HasValue)
                image.DisplayOrder = updateImageDto.DisplayOrder;

            await _context.SaveChangesAsync();

            return new ProductImageDto
            {
                ImgId = image.ImgId,
                ProductId = image.ProductId,
                ImgUrl = image.ImgUrl,
                ImgName = image.ImgName,
                AltText = image.AltText,
                IsPrimary = image.IsPrimary,
                DisplayOrder = image.DisplayOrder,
                CreatedAt = image.CreatedAt
            };
        }

        public async Task<bool> DeleteProductImageAsync(int productId, int imageId)
        {
            var image = await _context.ProductImgs
                .FirstOrDefaultAsync(img => img.ImgId == imageId && img.ProductId == productId);

            if (image == null) return false;

            _context.ProductImgs.Remove(image);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetPrimaryImageAsync(int productId, int imageId)
        {
            var image = await _context.ProductImgs
                .FirstOrDefaultAsync(img => img.ImgId == imageId && img.ProductId == productId);

            if (image == null) return false;

            // Unset all primary images for this product
            await _context.ProductImgs
                .Where(img => img.ProductId == productId && img.IsPrimary == true)
                .ExecuteUpdateAsync(img => img.SetProperty(i => i.IsPrimary, false));

            // Set this image as primary
            image.IsPrimary = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReorderImagesAsync(int productId, List<int> imageIds)
        {
            var images = await _context.ProductImgs
                .Where(img => img.ProductId == productId && imageIds.Contains(img.ImgId))
                .ToListAsync();

            if (images.Count != imageIds.Count) return false;

            // Update display order based on position in list
            for (int i = 0; i < imageIds.Count; i++)
            {
                var image = images.FirstOrDefault(img => img.ImgId == imageIds[i]);
                if (image != null)
                {
                    image.DisplayOrder = i + 1;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ============ PRODUCT CATEGORY MANAGEMENT ============
        public async Task<PaginatedResult<ProductCategoryDto>> GetProductCategoriesPagedAsync(ProductCategoryFilterDto filter)
        {
            var query = _context.ProductCategories.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(c => c.Name!.Contains(filter.SearchTerm));
            }

            // Apply sorting
            query = filter.SortBy.ToLower() switch
            {
                "name" => filter.SortOrder.ToLower() == "asc" ?
                    query.OrderBy(c => c.Name) : query.OrderByDescending(c => c.Name),
                _ => query.OrderBy(c => c.Name)
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

            var categories = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(c => new ProductCategoryDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name
                })
                .ToListAsync();

            return new PaginatedResult<ProductCategoryDto>
            {
                Data = categories,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = totalPages,
                HasNextPage = filter.Page < totalPages,
                HasPreviousPage = filter.Page > 1
            };
        }

        public async Task<ProductCategoryDetailDto?> GetProductCategoryByIdAsync(int categoryId)
        {
            var category = await _context.ProductCategories
                .Include(c => c.Products)
                    .ThenInclude(p => p.ProductImgs)
                .Include(c => c.Products)
                    .ThenInclude(p => p.OrderItems)
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            if (category == null) return null;

            var totalRevenue = category.Products
                .SelectMany(p => p.OrderItems)
                .Where(oi => oi.Order!.Status == "completed")
                .Sum(oi => oi.TotalPrice) ;

            var topProducts = category.Products
                .OrderByDescending(p => p.OrderItems.Count)
                .Take(5)
                .Select(p => new ProductListDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    CategoryName = category.Name,
                    PrimaryImageUrl = p.ProductImgs
                        .Where(img => img.IsPrimary == true)
                        .Select(img => img.ImgUrl)
                        .FirstOrDefault(),
                    TotalOrders = p.OrderItems.Count
                })
                .ToList();

            return new ProductCategoryDetailDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                ProductCount = category.Products.Count,
                TotalRevenue = totalRevenue,
                TopProducts = topProducts
            };
        }

        public async Task<ProductCategoryDetailDto> CreateProductCategoryAsync(CreateProductCategoryDto createCategoryDto)
        {
            var category = new ProductCategory
            {
                Name = createCategoryDto.Name
            };

            _context.ProductCategories.Add(category);
            await _context.SaveChangesAsync();

            return (await GetProductCategoryByIdAsync(category.CategoryId))!;
        }

        public async Task<ProductCategoryDetailDto?> UpdateProductCategoryAsync(int categoryId, UpdateProductCategoryDto updateCategoryDto)
        {
            var category = await _context.ProductCategories.FindAsync(categoryId);
            if (category == null) return null;

            if (!string.IsNullOrEmpty(updateCategoryDto.Name))
                category.Name = updateCategoryDto.Name;

            await _context.SaveChangesAsync();
            return await GetProductCategoryByIdAsync(categoryId);
        }

        public async Task<bool> DeleteProductCategoryAsync(int categoryId)
        {
            var category = await _context.ProductCategories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            if (category == null) return false;

            // Check if category has products
            if (category.Products.Any())
                throw new InvalidOperationException("Không thể xóa danh mục đã có sản phẩm");

            _context.ProductCategories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        // ============ LOOKUP DATA ============
        public async Task<List<ProductCategoryDto>> GetProductCategoriesAsync()
        {
            return await _context.ProductCategories
                .Select(c => new ProductCategoryDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name
                })
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        // ============ STATISTICS ============
        public async Task<ProductStatsDto> GetProductStatsAsync()
        {
            var totalProducts = await _context.Products.CountAsync();
            var activeProducts = await _context.Products.CountAsync(p => p.Status == "active");
            var inactiveProducts = await _context.Products.CountAsync(p => p.Status == "inactive");
            var outOfStockProducts = await _context.Products.CountAsync(p => p.Status == "out_of_stock");
            var discontinuedProducts = await _context.Products.CountAsync(p => p.Status == "discontinued");

            var thisMonth = DateTime.Now.Date.AddDays(-DateTime.Now.Day + 1);
            var newProductsThisMonth = await _context.Products
                .CountAsync(p => p.CreatedAt >= thisMonth);

            var averagePrice = await _context.Products
                .Where(p => p.Price.HasValue)
                .AverageAsync(p => p.Price) ?? 0;

            var totalInventoryValue = await _context.Products
                .Where(p => p.Price.HasValue && p.StockQuantity.HasValue)
                .SumAsync(p => p.Price * p.StockQuantity) ?? 0;

            var totalRevenue = await _context.OrderItems
                .Where(oi => oi.Order!.Status == "completed")
                .SumAsync(oi => oi.TotalPrice) ;

            var categoryStats = await _context.ProductCategories
                .Select(cat => new ProductCategoryStatsDto
                {
                    CategoryName = cat.Name ?? "Unknown",
                    ProductCount = cat.Products.Count,
                    OrderCount = cat.Products.SelectMany(p => p.OrderItems).Count(),
                    Revenue = cat.Products
                        .SelectMany(p => p.OrderItems)
                        .Where(oi => oi.Order!.Status == "completed")
                        .Sum(oi => oi.TotalPrice),
                    AveragePrice = cat.Products.Average(p => p.Price) ?? 0
                })
                .ToListAsync();

            var popularProducts = await GetPopularProductsAsync(5);
            var lowStockProducts = await GetLowStockProductsAsync(10);

            return new ProductStatsDto
            {
                TotalProducts = totalProducts,
                ActiveProducts = activeProducts,
                InactiveProducts = inactiveProducts,
                OutOfStockProducts = outOfStockProducts,
                DiscontinuedProducts = discontinuedProducts,
                NewProductsThisMonth = newProductsThisMonth,
                AveragePrice = averagePrice,
                TotalInventoryValue = totalInventoryValue,
                TotalRevenue = totalRevenue,
                CategoryStats = categoryStats,
                PopularProducts = popularProducts,
                LowStockProducts = lowStockProducts
            };
        }

        public async Task<List<ProductPopularityDto>> GetPopularProductsAsync(int limit = 10)
        {
            return await _context.Products
                .Include(p => p.OrderItems)
                .Include(p => p.ProductImgs)
                .Select(p => new ProductPopularityDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.Name ?? "Unknown",
                    OrderCount = p.OrderItems.Count,
                    QuantitySold = p.OrderItems.Sum(oi => oi.Quantity) ?? 0,
                    Revenue = p.OrderItems
                        .Where(oi => oi.Order!.Status == "completed")
                        .Sum(oi => oi.TotalPrice),
                    PrimaryImageUrl = p.ProductImgs
                        .Where(img => img.IsPrimary == true)
                        .Select(img => img.ImgUrl)
                        .FirstOrDefault(),
                    AverageRating = _context.Reviews
                        .Where(r => r.TargetType == "product" && r.TargetId == p.ProductId)
                        .Average(r => (decimal?)r.Rating)
                })
                .OrderByDescending(p => p.OrderCount)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<ProductLowStockDto>> GetLowStockProductsAsync(int threshold = 10)
        {
            return await _context.Products
                .Include(p => p.ProductImgs)
                .Where(p => p.StockQuantity <= threshold && p.Status == "active")
                .Select(p => new ProductLowStockDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.Name ?? "Unknown",
                    CurrentStock = p.StockQuantity ?? 0,
                    Status = p.Status,
                    PrimaryImageUrl = p.ProductImgs
                        .Where(img => img.IsPrimary == true)
                        .Select(img => img.ImgUrl)
                        .FirstOrDefault(),
                    LastRestocked = p.CreatedAt
                })
                .OrderBy(p => p.CurrentStock)
                .ToListAsync();
        }

        // ============ BULK OPERATIONS ============
        public async Task<bool> BulkUpdateProductStatusAsync(List<int> productIds, string status)
        {
            await _context.Products
                .Where(p => productIds.Contains(p.ProductId))
                .ExecuteUpdateAsync(p => p.SetProperty(x => x.Status, status));

            return true;
        }

        public async Task<bool> BulkUpdateProductCategoryAsync(List<int> productIds, int categoryId)
        {
            // Verify category exists
            var categoryExists = await _context.ProductCategories
                .AnyAsync(c => c.CategoryId == categoryId);
            if (!categoryExists)
                throw new ArgumentException("Danh mục không tồn tại");

            await _context.Products
                .Where(p => productIds.Contains(p.ProductId))
                .ExecuteUpdateAsync(p => p.SetProperty(x => x.CategoryId, categoryId));

            return true;
        }

        public async Task<bool> BulkUpdateStockAsync(List<StockUpdateItem> stockUpdates)
        {
            foreach (var update in stockUpdates)
            {
                await _context.Products
                    .Where(p => p.ProductId == update.ProductId)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.StockQuantity, update.NewStock));
            }

            return true;
        }

        public async Task<bool> BulkDeleteProductsAsync(List<int> productIds)
        {
            // Check if any products have orders
            var productsWithOrders = await _context.Products
                .Where(p => productIds.Contains(p.ProductId))
                .Include(p => p.OrderItems)
                .Where(p => p.OrderItems.Any())
                .Select(p => p.Name)
                .ToListAsync();

            if (productsWithOrders.Any())
                throw new InvalidOperationException($"Không thể xóa các sản phẩm sau vì đã có đơn hàng: {string.Join(", ", productsWithOrders)}");

            // Delete images first
            await _context.ProductImgs
                .Where(img => productIds.Contains(img.ProductId ?? 0))
                .ExecuteDeleteAsync();

            // Delete wishlist items
            await _context.WishLists
                .Where(w => productIds.Contains(w.ProductId ?? 0))
                .ExecuteDeleteAsync();

            // Delete reviews
            await _context.Reviews
                .Where(r => r.TargetType == "product" && productIds.Contains(r.TargetId))
                .ExecuteDeleteAsync();

            // Delete products
            await _context.Products
                .Where(p => productIds.Contains(p.ProductId))
                .ExecuteDeleteAsync();

            return true;
        }

        // ============ INVENTORY MANAGEMENT ============
        public async Task<bool> UpdateStockAsync(int productId, int newStock, string? note = null)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            product.StockQuantity = newStock;

            // Update status based on stock
            if (newStock == 0)
                product.Status = "out_of_stock";
            else if (product.Status == "out_of_stock" && newStock > 0)
                product.Status = "active";

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AdjustStockAsync(int productId, int adjustment, string? note = null)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            var newStock = (product.StockQuantity ?? 0) + adjustment;
            if (newStock < 0) newStock = 0;

            return await UpdateStockAsync(productId, newStock, note);
        }

        public async Task<List<ProductListDto>> GetProductsByStockLevelAsync(int minStock, int maxStock)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImgs)
                .Where(p => p.StockQuantity >= minStock && p.StockQuantity <= maxStock)
                .Select(p => new ProductListDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    CategoryName = p.Category!.Name,
                    PrimaryImageUrl = p.ProductImgs
                        .Where(img => img.IsPrimary == true)
                        .Select(img => img.ImgUrl)
                        .FirstOrDefault(),
                    TotalImages = p.ProductImgs.Count
                })
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();
        }
    }
}

