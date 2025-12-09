using Microsoft.EntityFrameworkCore;
using MonAmour.AuthViewModel;
using MonAmour.Models;
using MonAmour.Services.Interfaces;

namespace MonAmour.Services.Implements;

public class ReviewService : IReviewService
{
    private readonly MonAmourDbContext _context;
    private readonly ILogger<ReviewService> _logger;
    private readonly ICloudinaryService? _cloudinaryService;

    public ReviewService(MonAmourDbContext context, ILogger<ReviewService> logger, ICloudinaryService? cloudinaryService = null)
    {
        _context = context;
        _logger = logger;
        _cloudinaryService = cloudinaryService; // Will be injected by DI container if registered
    }

    public async Task<ReviewViewModel> CreateReviewAsync(CreateReviewViewModel dto)
    {
        try
        {
            _logger.LogInformation("Creating review for user {UserId} on {TargetType} {TargetId}", 
                dto.UserId, dto.TargetType, dto.TargetId);

            // Validate input
            if (dto.TargetType.ToLower() != "product" && dto.TargetType.ToLower() != "concept")
            {
                throw new ArgumentException("TargetType must be 'Product' or 'Concept'.");
            }

            // Verify user exists
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {dto.UserId} not found.");
            }

            // Verify target exists
            await ValidateTargetExistsAsync(dto.TargetType, dto.TargetId);

            // Check if user already reviewed this item
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.UserId == dto.UserId 
                                    && r.TargetType == dto.TargetType 
                                    && r.TargetId == dto.TargetId);

            if (existingReview != null)
            {
                throw new InvalidOperationException("User has already reviewed this item. Use update instead.");
            }

            // Upload image if provided
            string? imageUrl = null;
            if (dto.ImageFile != null && dto.ImageFile.Length > 0 && _cloudinaryService != null)
            {
                try
                {
                    imageUrl = await _cloudinaryService.UploadImageAsync(dto.ImageFile, "reviews");
                    if (string.IsNullOrEmpty(imageUrl))
                    {
                        _logger.LogWarning("Failed to upload review image for user {UserId}", dto.UserId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading review image for user {UserId}", dto.UserId);
                    // Continue without image if upload fails
                }
            }

            // Create new review
            var review = new Review
            {
                UserId = dto.UserId,
                TargetType = dto.TargetType,
                TargetId = dto.TargetId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Return the created review with user and target info
            var result = await GetReviewViewModelAsync(review.ReviewId);
            
            _logger.LogInformation("Successfully created review with ID {ReviewId}", result.ReviewId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating review for user {UserId}", dto.UserId);
            throw;
        }
    }

    public async Task<ReviewViewModel> UpdateReviewAsync(UpdateReviewViewModel dto)
    {
        try
        {
            _logger.LogInformation("Updating review {ReviewId}", dto.ReviewId);

            var review = await _context.Reviews.FindAsync(dto.ReviewId);
            if (review == null)
            {
                throw new ArgumentException($"Review with ID {dto.ReviewId} not found.");
            }

            // Upload new image if provided
            if (dto.ImageFile != null && dto.ImageFile.Length > 0 && _cloudinaryService != null)
            {
                try
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(review.ImageUrl) && _cloudinaryService != null)
                    {
                        // Extract public ID from URL if needed (Cloudinary specific)
                        // For now, we'll just upload new image
                    }

                    var imageUrl = await _cloudinaryService.UploadImageAsync(dto.ImageFile, "reviews");
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        review.ImageUrl = imageUrl;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading review image for review {ReviewId}", dto.ReviewId);
                    // Continue without updating image if upload fails
                }
            }

            // Update review
            review.Rating = dto.Rating;
            review.Comment = dto.Comment;
            review.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            // Return the updated review
            var result = await GetReviewViewModelAsync(review.ReviewId);
            
            _logger.LogInformation("Successfully updated review {ReviewId}", dto.ReviewId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating review {ReviewId}", dto.ReviewId);
            throw;
        }
    }

    public async Task<bool> DeleteReviewAsync(int reviewId, int userId)
    {
        try
        {
            _logger.LogInformation("Deleting review {ReviewId} by user {UserId}", reviewId, userId);

            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId && r.UserId == userId);

            if (review == null)
            {
                _logger.LogWarning("Review {ReviewId} not found or user {UserId} not authorized", reviewId, userId);
                return false;
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted review {ReviewId}", reviewId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting review {ReviewId}", reviewId);
            throw;
        }
    }

    public async Task<ReviewViewModel> GetReviewByIdAsync(int reviewId)
    {
        try
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null)
            {
                throw new ArgumentException($"Review with ID {reviewId} not found.");
            }

            return await GetReviewViewModelAsync(reviewId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting review {ReviewId}", reviewId);
            throw;
        }
    }

    public async Task<ProductReviewViewModel> GetProductReviewsAsync(int productId, int page = 1, int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Getting reviews for product {ProductId}, page {Page}", productId, page);

            // Verify product exists
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                throw new ArgumentException($"Product with ID {productId} not found.");
            }

            // Get reviews with pagination
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.TargetType == "Product" && r.TargetId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReviewViewModel
                {
                    ReviewId = r.ReviewId,
                    UserId = r.UserId ?? 0,
                    UserName = r.User != null ? r.User.Name ?? "" : "",
                    UserEmail = r.User != null ? r.User.Email : "",
                    TargetType = r.TargetType ?? "",
                    TargetId = r.TargetId,
                    TargetName = product.Name ?? "",
                    Rating = r.Rating ?? 0,
                    Comment = r.Comment,
                    ImageUrl = r.ImageUrl,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .ToListAsync();

            // Calculate statistics
            var allReviews = await _context.Reviews
                .Where(r => r.TargetType == "Product" && r.TargetId == productId && r.Rating.HasValue)
                .ToListAsync();

            var averageRating = allReviews.Any() ? allReviews.Average(r => r.Rating!.Value) : 0;
            var totalReviews = allReviews.Count;
            var ratingDistribution = allReviews
                .GroupBy(r => r.Rating!.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            // Ensure all rating levels are represented
            for (int i = 1; i <= 5; i++)
            {
                if (!ratingDistribution.ContainsKey(i))
                {
                    ratingDistribution[i] = 0;
                }
            }

            return new ProductReviewViewModel
            {
                ProductId = productId,
                ProductName = product.Name ?? "",
                AverageRating = Math.Round(averageRating, 2),
                TotalReviews = totalReviews,
                Reviews = reviews,
                RatingDistribution = ratingDistribution
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reviews for product {ProductId}", productId);
            throw;
        }
    }

    public async Task<ConceptReviewViewModel> GetConceptReviewsAsync(int conceptId, int page = 1, int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Getting reviews for concept {ConceptId}, page {Page}", conceptId, page);

            // Verify concept exists
            var concept = await _context.Concepts.FindAsync(conceptId);
            if (concept == null)
            {
                throw new ArgumentException($"Concept with ID {conceptId} not found.");
            }

            // Get reviews with pagination
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.TargetType == "Concept" && r.TargetId == conceptId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReviewViewModel
                {
                    ReviewId = r.ReviewId,
                    UserId = r.UserId ?? 0,
                    UserName = r.User != null ? r.User.Name ?? "" : "",
                    UserEmail = r.User != null ? r.User.Email : "",
                    TargetType = r.TargetType ?? "",
                    TargetId = r.TargetId,
                    TargetName = concept.Name ?? "",
                    Rating = r.Rating ?? 0,
                    Comment = r.Comment,
                    ImageUrl = r.ImageUrl,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .ToListAsync();

            // Calculate statistics
            var allReviews = await _context.Reviews
                .Where(r => r.TargetType == "Concept" && r.TargetId == conceptId && r.Rating.HasValue)
                .ToListAsync();

            var averageRating = allReviews.Any() ? allReviews.Average(r => r.Rating!.Value) : 0;
            var totalReviews = allReviews.Count;
            var ratingDistribution = allReviews
                .GroupBy(r => r.Rating!.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            // Ensure all rating levels are represented
            for (int i = 1; i <= 5; i++)
            {
                if (!ratingDistribution.ContainsKey(i))
                {
                    ratingDistribution[i] = 0;
                }
            }

            return new ConceptReviewViewModel
            {
                ConceptId = conceptId,
                ConceptName = concept.Name ?? "",
                AverageRating = Math.Round(averageRating, 2),
                TotalReviews = totalReviews,
                Reviews = reviews,
                RatingDistribution = ratingDistribution
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reviews for concept {ConceptId}", conceptId);
            throw;
        }
    }

    public async Task<UserReviewViewModel> GetUserReviewsAsync(int userId, int page = 1, int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Getting reviews for user {UserId}, page {Page}", userId, page);

            // Verify user exists
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {userId} not found.");
            }

            // Get reviews with pagination and target names
            var reviewsQuery = _context.Reviews
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var reviews = new List<ReviewViewModel>();

            foreach (var review in await reviewsQuery.ToListAsync())
            {
                var targetName = "";
                if (review.TargetType == "Product")
                {
                    var product = await _context.Products.FindAsync(review.TargetId);
                    targetName = product?.Name ?? "";
                }
                else if (review.TargetType == "Concept")
                {
                    var concept = await _context.Concepts.FindAsync(review.TargetId);
                    targetName = concept?.Name ?? "";
                }

                reviews.Add(new ReviewViewModel
                {
                    ReviewId = review.ReviewId,
                    UserId = review.UserId ?? 0,
                    UserName = user.Name ?? "",
                    UserEmail = user.Email,
                    TargetType = review.TargetType ?? "",
                    TargetId = review.TargetId,
                    TargetName = targetName,
                    Rating = review.Rating ?? 0,
                    Comment = review.Comment,
                    ImageUrl = review.ImageUrl,
                    CreatedAt = review.CreatedAt,
                    UpdatedAt = review.UpdatedAt
                });
            }

            // Calculate user's average rating
            var allUserReviews = await _context.Reviews
                .Where(r => r.UserId == userId && r.Rating.HasValue)
                .ToListAsync();

            var averageRating = allUserReviews.Any() ? allUserReviews.Average(r => r.Rating!.Value) : 0;
            var totalReviews = allUserReviews.Count;

            return new UserReviewViewModel
            {
                UserId = userId,
                UserName = user.Name ?? "",
                Reviews = reviews,
                AverageRating = Math.Round(averageRating, 2),
                TotalReviews = totalReviews
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reviews for user {UserId}", userId);
            throw;
        }
    }

    public async Task<ReviewSummaryViewModel> GetReviewSummaryAsync()
    {
        try
        {
            _logger.LogInformation("Getting review summary");

            var allReviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.Rating.HasValue)
                .ToListAsync();

            var totalReviews = allReviews.Count;
            var overallAverageRating = totalReviews > 0 ? allReviews.Average(r => r.Rating!.Value) : 0;

            var ratingDistribution = allReviews
                .GroupBy(r => r.Rating!.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            // Ensure all rating levels are represented
            for (int i = 1; i <= 5; i++)
            {
                if (!ratingDistribution.ContainsKey(i))
                {
                    ratingDistribution[i] = 0;
                }
            }

            // Get recent reviews (last 10)
            var recentReviews = await _context.Reviews
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Take(10)
                .ToListAsync();

            var recentReviewViewModels = new List<ReviewViewModel>();
            foreach (var review in recentReviews)
            {
                var targetName = "";
                if (review.TargetType == "Product")
                {
                    var product = await _context.Products.FindAsync(review.TargetId);
                    targetName = product?.Name ?? "";
                }
                else if (review.TargetType == "Concept")
                {
                    var concept = await _context.Concepts.FindAsync(review.TargetId);
                    targetName = concept?.Name ?? "";
                }

                recentReviewViewModels.Add(new ReviewViewModel
                {
                    ReviewId = review.ReviewId,
                    UserId = review.UserId ?? 0,
                    UserName = review.User?.Name ?? "",
                    UserEmail = review.User?.Email ?? "",
                    TargetType = review.TargetType ?? "",
                    TargetId = review.TargetId,
                    TargetName = targetName,
                    Rating = review.Rating ?? 0,
                    Comment = review.Comment,
                    ImageUrl = review.ImageUrl,
                    CreatedAt = review.CreatedAt,
                    UpdatedAt = review.UpdatedAt
                });
            }

            return new ReviewSummaryViewModel
            {
                TotalReviews = totalReviews,
                OverallAverageRating = Math.Round(overallAverageRating, 2),
                OverallRatingDistribution = ratingDistribution,
                RecentReviews = recentReviewViewModels
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting review summary");
            throw;
        }
    }

    public async Task<bool> CanUserReviewAsync(int userId, string targetType, int targetId)
    {
        try
        {
            // Verify user exists
            var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
            if (!userExists)
            {
                return false;
            }

            // Verify target exists
            try
            {
                await ValidateTargetExistsAsync(targetType, targetId);
            }
            catch
            {
                return false;
            }

            // Check if user already reviewed this item
            var hasReviewed = await HasUserReviewedAsync(userId, targetType, targetId);
            if (hasReviewed)
            {
                return false;
            }

            // For products, check if user has purchased this product
            if (targetType.ToLower() == "product")
            {
                var hasPurchased = await _context.OrderItems
                    .Include(oi => oi.Order)
                    .AnyAsync(oi => oi.Order != null && oi.Order.UserId == userId
                                 && oi.ProductId == targetId
                                 && oi.Order.Status == "completed");
                return hasPurchased;
            }

            // For concepts, allow review if target exists (concepts don't require purchase)
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {UserId} can review {TargetType} {TargetId}",
                userId, targetType, targetId);
            return false;
        }
    }

    public async Task<bool> HasUserReviewedAsync(int userId, string targetType, int targetId)
    {
        try
        {
            var hasReviewed = await _context.Reviews
                .AnyAsync(r => r.UserId == userId 
                         && r.TargetType == targetType 
                         && r.TargetId == targetId);

            return hasReviewed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {UserId} has reviewed {TargetType} {TargetId}", 
                userId, targetType, targetId);
            throw;
        }
    }

    // Private helper methods
    private async Task<ReviewViewModel> GetReviewViewModelAsync(int reviewId)
    {
        var review = await _context.Reviews
            .Include(r => r.User)
            .FirstAsync(r => r.ReviewId == reviewId);

        var targetName = "";
        if (review.TargetType == "Product")
        {
            var product = await _context.Products.FindAsync(review.TargetId);
            targetName = product?.Name ?? "";
        }
        else if (review.TargetType == "Concept")
        {
            var concept = await _context.Concepts.FindAsync(review.TargetId);
            targetName = concept?.Name ?? "";
        }

        return new ReviewViewModel
        {
            ReviewId = review.ReviewId,
            UserId = review.UserId ?? 0,
            UserName = review.User?.Name ?? "",
            UserEmail = review.User?.Email ?? "",
            TargetType = review.TargetType ?? "",
            TargetId = review.TargetId,
            TargetName = targetName,
            Rating = review.Rating ?? 0,
            Comment = review.Comment,
            ImageUrl = review.ImageUrl,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt
        };
    }

    private async Task ValidateTargetExistsAsync(string targetType, int targetId)
    {
        if (targetType.ToLower() == "product")
        {
            var productExists = await _context.Products.AnyAsync(p => p.ProductId == targetId);
            if (!productExists)
            {
                throw new ArgumentException($"Product with ID {targetId} not found.");
            }
        }
        else if (targetType.ToLower() == "concept")
        {
            var conceptExists = await _context.Concepts.AnyAsync(c => c.ConceptId == targetId);
            if (!conceptExists)
            {
                throw new ArgumentException($"Concept with ID {targetId} not found.");
            }
        }
        else
        {
            throw new ArgumentException("TargetType must be 'Product' or 'Concept'.");
        }
    }
}
