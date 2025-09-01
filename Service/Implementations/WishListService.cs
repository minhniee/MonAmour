using Microsoft.EntityFrameworkCore;
using MonAmourDb_BE.DTOs;
using MonAmourDb_BE.Models;
using MonAmourDb_BE.Service.Interfaces;

namespace MonAmourDb_BE.Service.Implementations;

public class WishListService : IWishListService
{
    private readonly MonAmourDbContext _context;

    public WishListService(MonAmourDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<UserWishListDto>> GetUserWishListAsync(int userId)
    {
        try
        {
            var wishListItems = await _context.WishLists
                .Include(w => w.Product)
                    .ThenInclude(p => p.ProductImgs)
                .Include(w => w.Concept)
                    .ThenInclude(c => c.ConceptImgs)
                .Where(w => w.UserId == userId)
                .ToListAsync();

            var userWishList = new UserWishListDto
            {
                UserId = userId,
                Products = new List<WishListDto>(),
                Concepts = new List<WishListDto>()
            };

            foreach (var item in wishListItems)
            {
                var wishListDto = new WishListDto
                {
                    Id = item.Id,
                    UserId = item.UserId,
                    ProductId = item.ProductId,
                    ConceptId = item.ConceptId,
                    CreatedAt = item.CreatedAt
                };

                if (item.Product != null)
                {
                    wishListDto.ProductName = item.Product.Name;
                    wishListDto.ProductDescription = item.Product.Description;
                    wishListDto.ProductPrice = item.Product.Price;
                    wishListDto.ProductStatus = item.Product.Status;
                    wishListDto.ProductImages = item.Product.ProductImgs?.Select(img => img.ImgUrl).ToList();
                    userWishList.Products.Add(wishListDto);
                }

                if (item.Concept != null)
                {
                    wishListDto.ConceptName = item.Concept.Name;
                    wishListDto.ConceptDescription = item.Concept.Description;
                    wishListDto.ConceptPrice = item.Concept.Price;
                    wishListDto.ConceptAvailabilityStatus = item.Concept.AvailabilityStatus;
                    wishListDto.ConceptImages = item.Concept.ConceptImgs?.Select(img => img.ImgUrl).ToList();
                    userWishList.Concepts.Add(wishListDto);
                }
            }

            return new ApiResponse<UserWishListDto>
            {
                Success = true,
                Message = "Wish list retrieved successfully",
                Data = userWishList
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<UserWishListDto>
            {
                Success = false,
                Message = $"Error retrieving wish list: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<WishListDto>> AddToWishListAsync(AddToWishListDto dto)
    {
        try
        {
            // Validate that either ProductId or ConceptId is provided
            if (!dto.ProductId.HasValue && !dto.ConceptId.HasValue)
            {
                return new ApiResponse<WishListDto>
                {
                    Success = false,
                    Message = "Either ProductId or ConceptId must be provided"
                };
            }

            // Check if user exists
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
            {
                return new ApiResponse<WishListDto>
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            // Check if item already exists in wish list
            var existingItem = await _context.WishLists
                .FirstOrDefaultAsync(w => w.UserId == dto.UserId &&
                                        ((dto.ProductId.HasValue && w.ProductId == dto.ProductId) ||
                                         (dto.ConceptId.HasValue && w.ConceptId == dto.ConceptId)));

            if (existingItem != null)
            {
                return new ApiResponse<WishListDto>
                {
                    Success = false,
                    Message = "Item already exists in wish list"
                };
            }

            // Validate product or concept exists
            if (dto.ProductId.HasValue)
            {
                var product = await _context.Products.FindAsync(dto.ProductId.Value);
                if (product == null)
                {
                    return new ApiResponse<WishListDto>
                    {
                        Success = false,
                        Message = "Product not found"
                    };
                }
            }

            if (dto.ConceptId.HasValue)
            {
                var concept = await _context.Concepts.FindAsync(dto.ConceptId.Value);
                if (concept == null)
                {
                    return new ApiResponse<WishListDto>
                    {
                        Success = false,
                        Message = "Concept not found"
                    };
                }
            }

            // Add to wish list
            var wishListItem = new WishList
            {
                UserId = dto.UserId,
                ProductId = dto.ProductId,
                ConceptId = dto.ConceptId,
                CreatedAt = DateTime.UtcNow
            };

            _context.WishLists.Add(wishListItem);
            await _context.SaveChangesAsync();

            // Return the added item with details
            var addedItem = await _context.WishLists
                .Include(w => w.Product)
                    .ThenInclude(p => p.ProductImgs)
                .Include(w => w.Concept)
                    .ThenInclude(c => c.ConceptImgs)
                .FirstOrDefaultAsync(w => w.Id == wishListItem.Id);

            var wishListDto = new WishListDto
            {
                Id = addedItem.Id,
                UserId = addedItem.UserId,
                ProductId = addedItem.ProductId,
                ConceptId = addedItem.ConceptId,
                CreatedAt = addedItem.CreatedAt
            };

            if (addedItem.Product != null)
            {
                wishListDto.ProductName = addedItem.Product.Name;
                wishListDto.ProductDescription = addedItem.Product.Description;
                wishListDto.ProductPrice = addedItem.Product.Price;
                wishListDto.ProductStatus = addedItem.Product.Status;
                wishListDto.ProductImages = addedItem.Product.ProductImgs?.Select(img => img.ImgUrl).ToList();
            }

            if (addedItem.Concept != null)
            {
                wishListDto.ConceptName = addedItem.Concept.Name;
                wishListDto.ConceptDescription = addedItem.Concept.Description;
                wishListDto.ConceptPrice = addedItem.Concept.Price;
                wishListDto.ConceptAvailabilityStatus = addedItem.Concept.AvailabilityStatus;
                wishListDto.ConceptImages = addedItem.Concept.ConceptImgs?.Select(img => img.ImgUrl).ToList();
            }

            return new ApiResponse<WishListDto>
            {
                Success = true,
                Message = "Item added to wish list successfully",
                Data = wishListDto
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<WishListDto>
            {
                Success = false,
                Message = $"Error adding item to wish list: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<bool>> RemoveFromWishListAsync(RemoveFromWishListDto dto)
    {
        try
        {
            // Validate that either ProductId or ConceptId is provided
            if (!dto.ProductId.HasValue && !dto.ConceptId.HasValue)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Either ProductId or ConceptId must be provided"
                };
            }

            // Find the wish list item
            var wishListItem = await _context.WishLists
                .FirstOrDefaultAsync(w => w.UserId == dto.UserId &&
                                        ((dto.ProductId.HasValue && w.ProductId == dto.ProductId) ||
                                         (dto.ConceptId.HasValue && w.ConceptId == dto.ConceptId)));

            if (wishListItem == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Item not found in wish list"
                };
            }

            _context.WishLists.Remove(wishListItem);
            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Item removed from wish list successfully",
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error removing item from wish list: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<bool>> IsInWishListAsync(int userId, int? productId, int? conceptId)
    {
        try
        {
            // Validate that either ProductId or ConceptId is provided
            if (!productId.HasValue && !conceptId.HasValue)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Either ProductId or ConceptId must be provided"
                };
            }

            var exists = await _context.WishLists
                .AnyAsync(w => w.UserId == userId &&
                              ((productId.HasValue && w.ProductId == productId) ||
                               (conceptId.HasValue && w.ConceptId == conceptId)));

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Check completed successfully",
                Data = exists
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error checking wish list: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<bool>> ClearWishListAsync(int userId)
    {
        try
        {
            var wishListItems = await _context.WishLists
                .Where(w => w.UserId == userId)
                .ToListAsync();

            if (!wishListItems.Any())
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Wish list is already empty",
                    Data = true
                };
            }

            _context.WishLists.RemoveRange(wishListItems);
            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Wish list cleared successfully",
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error clearing wish list: {ex.Message}"
            };
        }
    }
}
