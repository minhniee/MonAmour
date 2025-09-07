using Microsoft.EntityFrameworkCore;
using MonAmour.AuthViewModel;
using MonAmour.Models;
using MonAmour.Services.Interfaces;

namespace MonAmour.Services.Implements;

public class WishListService : IWishListService
{
    private readonly MonAmourDbContext _context;
    private readonly ILogger<WishListService> _logger;

    public WishListService(MonAmourDbContext context, ILogger<WishListService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserWishListViewModel> GetUserWishListAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Getting wishlist for user {UserId}", userId);

            // Verify user exists
            var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
            if (!userExists)
            {
                throw new ArgumentException($"User with ID {userId} not found.");
            }

            // Get all wishlist items for the user
            var wishlistItems = await _context.WishLists
                .Include(w => w.Product)
                    .ThenInclude(p => p!.ProductImgs)
                .Include(w => w.Concept)
                    .ThenInclude(c => c!.ConceptImgs)
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            var result = new UserWishListViewModel
            {
                UserId = userId,
                Products = new List<WishListViewModel>(),
                Concepts = new List<WishListViewModel>()
            };

            foreach (var item in wishlistItems)
            {
                var wishlistViewModel = new WishListViewModel
                {
                    Id = item.Id,
                    UserId = item.UserId,
                    ProductId = item.ProductId,
                    ConceptId = item.ConceptId,
                    CreatedAt = item.CreatedAt
                };

                if (item.Product != null)
                {
                    wishlistViewModel.ProductName = item.Product.Name;
                    wishlistViewModel.ProductDescription = item.Product.Description;
                    wishlistViewModel.ProductPrice = item.Product.Price;
                    wishlistViewModel.ProductStatus = item.Product.Status;
                    wishlistViewModel.ProductImages = item.Product.ProductImgs?.Select(img => img.ImgUrl ?? "").Where(url => !string.IsNullOrEmpty(url)).ToList() ?? new List<string>();
                    
                    result.Products.Add(wishlistViewModel);
                }
                else if (item.Concept != null)
                {
                    wishlistViewModel.ConceptName = item.Concept.Name;
                    wishlistViewModel.ConceptDescription = item.Concept.Description;
                    wishlistViewModel.ConceptPrice = item.Concept.Price;
                    wishlistViewModel.ConceptAvailabilityStatus = item.Concept.AvailabilityStatus;
                    wishlistViewModel.ConceptImages = item.Concept.ConceptImgs?.Select(img => img.ImgUrl ?? "").Where(url => !string.IsNullOrEmpty(url)).ToList() ?? new List<string>();
                    
                    result.Concepts.Add(wishlistViewModel);
                }
            }

            _logger.LogInformation("Retrieved {ProductCount} products and {ConceptCount} concepts for user {UserId}", 
                result.Products.Count, result.Concepts.Count, userId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wishlist for user {UserId}", userId);
            throw;
        }
    }

    public async Task<WishListViewModel> AddToWishListAsync(AddToWishListViewModel dto)
    {
        try
        {
            _logger.LogInformation("Adding item to wishlist for user {UserId}", dto.UserId);

            // Validate input
            if (dto.ProductId == null && dto.ConceptId == null)
            {
                throw new ArgumentException("Either ProductId or ConceptId must be provided.");
            }

            if (dto.ProductId != null && dto.ConceptId != null)
            {
                throw new ArgumentException("Only one of ProductId or ConceptId should be provided.");
            }

            // Verify user exists
            var userExists = await _context.Users.AnyAsync(u => u.UserId == dto.UserId);
            if (!userExists)
            {
                throw new ArgumentException($"User with ID {dto.UserId} not found.");
            }

            // Verify product or concept exists
            if (dto.ProductId.HasValue)
            {
                var productExists = await _context.Products.AnyAsync(p => p.ProductId == dto.ProductId.Value);
                if (!productExists)
                {
                    throw new ArgumentException($"Product with ID {dto.ProductId} not found.");
                }
            }

            if (dto.ConceptId.HasValue)
            {
                var conceptExists = await _context.Concepts.AnyAsync(c => c.ConceptId == dto.ConceptId.Value);
                if (!conceptExists)
                {
                    throw new ArgumentException($"Concept with ID {dto.ConceptId} not found.");
                }
            }

            // Check if item already exists in wishlist
            var existingItem = await _context.WishLists
                .FirstOrDefaultAsync(w => w.UserId == dto.UserId 
                                    && w.ProductId == dto.ProductId 
                                    && w.ConceptId == dto.ConceptId);

            if (existingItem != null)
            {
                throw new InvalidOperationException("Item is already in the wishlist.");
            }

            // Create new wishlist item
            var wishlistItem = new WishList
            {
                UserId = dto.UserId,
                ProductId = dto.ProductId,
                ConceptId = dto.ConceptId,
                CreatedAt = DateTime.Now
            };

            _context.WishLists.Add(wishlistItem);
            await _context.SaveChangesAsync();

            // Return the created item with related data
            var createdItem = await _context.WishLists
                .Include(w => w.Product)
                    .ThenInclude(p => p!.ProductImgs)
                .Include(w => w.Concept)
                    .ThenInclude(c => c!.ConceptImgs)
                .FirstAsync(w => w.Id == wishlistItem.Id);

            var result = new WishListViewModel
            {
                Id = createdItem.Id,
                UserId = createdItem.UserId,
                ProductId = createdItem.ProductId,
                ConceptId = createdItem.ConceptId,
                CreatedAt = createdItem.CreatedAt
            };

            if (createdItem.Product != null)
            {
                result.ProductName = createdItem.Product.Name;
                result.ProductDescription = createdItem.Product.Description;
                result.ProductPrice = createdItem.Product.Price;
                result.ProductStatus = createdItem.Product.Status;
                result.ProductImages = createdItem.Product.ProductImgs?.Select(img => img.ImgUrl ?? "").Where(url => !string.IsNullOrEmpty(url)).ToList() ?? new List<string>();
            }

            if (createdItem.Concept != null)
            {
                result.ConceptName = createdItem.Concept.Name;
                result.ConceptDescription = createdItem.Concept.Description;
                result.ConceptPrice = createdItem.Concept.Price;
                result.ConceptAvailabilityStatus = createdItem.Concept.AvailabilityStatus;
                result.ConceptImages = createdItem.Concept.ConceptImgs?.Select(img => img.ImgUrl ?? "").Where(url => !string.IsNullOrEmpty(url)).ToList() ?? new List<string>();
            }

            _logger.LogInformation("Successfully added item to wishlist with ID {WishlistId}", result.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to wishlist for user {UserId}", dto.UserId);
            throw;
        }
    }

    public async Task<bool> RemoveFromWishListAsync(RemoveFromWishListViewModel dto)
    {
        try
        {
            _logger.LogInformation("Removing item from wishlist for user {UserId}", dto.UserId);

            // Validate input
            if (dto.ProductId == null && dto.ConceptId == null)
            {
                throw new ArgumentException("Either ProductId or ConceptId must be provided.");
            }

            // Find the wishlist item
            var wishlistItem = await _context.WishLists
                .FirstOrDefaultAsync(w => w.UserId == dto.UserId 
                                    && w.ProductId == dto.ProductId 
                                    && w.ConceptId == dto.ConceptId);

            if (wishlistItem == null)
            {
                _logger.LogWarning("Wishlist item not found for user {UserId}", dto.UserId);
                return false;
            }

            _context.WishLists.Remove(wishlistItem);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully removed wishlist item {WishlistId}", wishlistItem.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing item from wishlist for user {UserId}", dto.UserId);
            throw;
        }
    }

    public async Task<bool> IsInWishListAsync(int userId, int? productId, int? conceptId)
    {
        try
        {
            // Validate input
            if (productId == null && conceptId == null)
            {
                throw new ArgumentException("Either ProductId or ConceptId must be provided.");
            }

            var exists = await _context.WishLists
                .AnyAsync(w => w.UserId == userId 
                         && w.ProductId == productId 
                         && w.ConceptId == conceptId);

            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if item is in wishlist for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> ClearWishListAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Clearing wishlist for user {UserId}", userId);

            // Verify user exists
            var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
            if (!userExists)
            {
                throw new ArgumentException($"User with ID {userId} not found.");
            }

            var wishlistItems = await _context.WishLists
                .Where(w => w.UserId == userId)
                .ToListAsync();

            if (wishlistItems.Any())
            {
                _context.WishLists.RemoveRange(wishlistItems);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Cleared {Count} items from wishlist for user {UserId}", 
                    wishlistItems.Count, userId);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing wishlist for user {UserId}", userId);
            throw;
        }
    }
}
