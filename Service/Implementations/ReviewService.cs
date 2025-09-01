using Microsoft.EntityFrameworkCore;
using MonAmourDb_BE.DTOs;
using MonAmourDb_BE.Models;
using MonAmourDb_BE.Service.Interfaces;

namespace MonAmourDb_BE.Service.Implementations;

public class ReviewService : IReviewService
{
    private readonly MonAmourDbContext _context;

    public ReviewService(MonAmourDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<ReviewDto>> CreateReviewAsync(CreateReviewDto dto)
    {
        try
        {
            // Validate target type
            if (dto.TargetType != "Product" && dto.TargetType != "Concept")
            {
                return new ApiResponse<ReviewDto>
                {
                    Success = false,
                    Message = "TargetType must be either 'Product' or 'Concept'"
                };
            }

            // Check if user can review (has purchased and paid)
            var canReview = await CanUserReviewAsync(dto.UserId, dto.TargetType, dto.TargetId);
            if (!canReview.Success || !canReview.Data)
            {
                return new ApiResponse<ReviewDto>
                {
                    Success = false,
                    Message = "You can only review items you have purchased and paid for"
                };
            }

            // Check if user has already reviewed this item
            var hasReviewed = await HasUserReviewedAsync(dto.UserId, dto.TargetType, dto.TargetId);
            if (hasReviewed.Success && hasReviewed.Data)
            {
                return new ApiResponse<ReviewDto>
                {
                    Success = false,
                    Message = "You have already reviewed this item"
                };
            }

            // Validate target exists
            if (dto.TargetType == "Product")
            {
                var product = await _context.Products.FindAsync(dto.TargetId);
                if (product == null)
                {
                    return new ApiResponse<ReviewDto>
                    {
                        Success = false,
                        Message = "Product not found"
                    };
                }
            }
            else if (dto.TargetType == "Concept")
            {
                var concept = await _context.Concepts.FindAsync(dto.TargetId);
                if (concept == null)
                {
                    return new ApiResponse<ReviewDto>
                    {
                        Success = false,
                        Message = "Concept not found"
                    };
                }
            }

            // Create review
            var review = new Review
            {
                UserId = dto.UserId,
                TargetType = dto.TargetType,
                TargetId = dto.TargetId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Return the created review with details
            var createdReview = await GetReviewWithDetailsAsync(review.ReviewId);
            return new ApiResponse<ReviewDto>
            {
                Success = true,
                Message = "Review created successfully",
                Data = createdReview
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<ReviewDto>
            {
                Success = false,
                Message = $"Error creating review: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<ReviewDto>> UpdateReviewAsync(UpdateReviewDto dto)
    {
        try
        {
            var review = await _context.Reviews
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReviewId == dto.ReviewId);

            if (review == null)
            {
                return new ApiResponse<ReviewDto>
                {
                    Success = false,
                    Message = "Review not found"
                };
            }

            // Update review
            review.Rating = dto.Rating;
            review.Comment = dto.Comment;
            review.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Return updated review
            var updatedReview = await GetReviewWithDetailsAsync(review.ReviewId);
            return new ApiResponse<ReviewDto>
            {
                Success = true,
                Message = "Review updated successfully",
                Data = updatedReview
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<ReviewDto>
            {
                Success = false,
                Message = $"Error updating review: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteReviewAsync(int reviewId, int userId)
    {
        try
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Review not found"
                };
            }

            // Check if user owns the review
            if (review.UserId != userId)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "You can only delete your own reviews"
                };
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Review deleted successfully",
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error deleting review: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<ReviewDto>> GetReviewByIdAsync(int reviewId)
    {
        try
        {
            var review = await GetReviewWithDetailsAsync(reviewId);
            if (review == null)
            {
                return new ApiResponse<ReviewDto>
                {
                    Success = false,
                    Message = "Review not found"
                };
            }

            return new ApiResponse<ReviewDto>
            {
                Success = true,
                Message = "Review retrieved successfully",
                Data = review
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<ReviewDto>
            {
                Success = false,
                Message = $"Error retrieving review: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<ProductReviewDto>> GetProductReviewsAsync(int productId, int page = 1, int pageSize = 10)
    {
        try
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return new ApiResponse<ProductReviewDto>
                {
                    Success = false,
                    Message = "Product not found"
                };
            }

            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.TargetType == "Product" && r.TargetId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalReviews = await _context.Reviews
                .CountAsync(r => r.TargetType == "Product" && r.TargetId == productId);

            var averageRating = await _context.Reviews
                .Where(r => r.TargetType == "Product" && r.TargetId == productId && r.Rating.HasValue)
                .AverageAsync(r => r.Rating.Value);

            var ratingDistribution = await _context.Reviews
                .Where(r => r.TargetType == "Product" && r.TargetId == productId && r.Rating.HasValue)
                .GroupBy(r => r.Rating.Value)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Rating, x => x.Count);

            var reviewDtos = reviews.Select(r => new ReviewDto
            {
                ReviewId = r.ReviewId,
                UserId = r.UserId ?? 0,
                UserName = r.User?.Name ?? "Anonymous",
                UserEmail = r.User?.Email ?? "",
                TargetType = r.TargetType ?? "",
                TargetId = r.TargetId,
                TargetName = product.Name ?? "",
                Rating = r.Rating ?? 0,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            }).ToList();

            var productReviewDto = new ProductReviewDto
            {
                ProductId = productId,
                ProductName = product.Name ?? "",
                AverageRating = Math.Round(averageRating, 1),
                TotalReviews = totalReviews,
                Reviews = reviewDtos,
                RatingDistribution = ratingDistribution
            };

            return new ApiResponse<ProductReviewDto>
            {
                Success = true,
                Message = "Product reviews retrieved successfully",
                Data = productReviewDto
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<ProductReviewDto>
            {
                Success = false,
                Message = $"Error retrieving product reviews: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<ConceptReviewDto>> GetConceptReviewsAsync(int conceptId, int page = 1, int pageSize = 10)
    {
        try
        {
            var concept = await _context.Concepts.FindAsync(conceptId);
            if (concept == null)
            {
                return new ApiResponse<ConceptReviewDto>
                {
                    Success = false,
                    Message = "Concept not found"
                };
            }

            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.TargetType == "Concept" && r.TargetId == conceptId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalReviews = await _context.Reviews
                .CountAsync(r => r.TargetType == "Concept" && r.TargetId == conceptId);

            var averageRating = await _context.Reviews
                .Where(r => r.TargetType == "Concept" && r.TargetId == conceptId && r.Rating.HasValue)
                .AverageAsync(r => r.Rating.Value);

            var ratingDistribution = await _context.Reviews
                .Where(r => r.TargetType == "Concept" && r.TargetId == conceptId && r.Rating.HasValue)
                .GroupBy(r => r.Rating.Value)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Rating, x => x.Count);

            var reviewDtos = reviews.Select(r => new ReviewDto
            {
                ReviewId = r.ReviewId,
                UserId = r.UserId ?? 0,
                UserName = r.User?.Name ?? "Anonymous",
                UserEmail = r.User?.Email ?? "",
                TargetType = r.TargetType ?? "",
                TargetId = r.TargetId,
                TargetName = concept.Name ?? "",
                Rating = r.Rating ?? 0,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            }).ToList();

            var conceptReviewDto = new ConceptReviewDto
            {
                ConceptId = conceptId,
                ConceptName = concept.Name ?? "",
                AverageRating = Math.Round(averageRating, 1),
                TotalReviews = totalReviews,
                Reviews = reviewDtos,
                RatingDistribution = ratingDistribution
            };

            return new ApiResponse<ConceptReviewDto>
            {
                Success = true,
                Message = "Concept reviews retrieved successfully",
                Data = conceptReviewDto
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<ConceptReviewDto>
            {
                Success = false,
                Message = $"Error retrieving concept reviews: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<UserReviewDto>> GetUserReviewsAsync(int userId, int page = 1, int pageSize = 10)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new ApiResponse<UserReviewDto>
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalReviews = await _context.Reviews.CountAsync(r => r.UserId == userId);

            var averageRating = await _context.Reviews
                .Where(r => r.UserId == userId && r.Rating.HasValue)
                .AverageAsync(r => r.Rating.Value);

            var reviewDtos = new List<ReviewDto>();
            foreach (var review in reviews)
            {
                string targetName = "";
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

                reviewDtos.Add(new ReviewDto
                {
                    ReviewId = review.ReviewId,
                    UserId = review.UserId ?? 0,
                    UserName = review.User?.Name ?? "Anonymous",
                    UserEmail = review.User?.Email ?? "",
                    TargetType = review.TargetType ?? "",
                    TargetId = review.TargetId,
                    TargetName = targetName,
                    Rating = review.Rating ?? 0,
                    Comment = review.Comment,
                    CreatedAt = review.CreatedAt,
                    UpdatedAt = review.UpdatedAt
                });
            }

            var userReviewDto = new UserReviewDto
            {
                UserId = userId,
                UserName = user.Name ?? "",
                Reviews = reviewDtos,
                AverageRating = Math.Round(averageRating, 1),
                TotalReviews = totalReviews
            };

            return new ApiResponse<UserReviewDto>
            {
                Success = true,
                Message = "User reviews retrieved successfully",
                Data = userReviewDto
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<UserReviewDto>
            {
                Success = false,
                Message = $"Error retrieving user reviews: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<ReviewSummaryDto>> GetReviewSummaryAsync()
    {
        try
        {
            var totalReviews = await _context.Reviews.CountAsync();
            var overallAverageRating = await _context.Reviews
                .Where(r => r.Rating.HasValue)
                .AverageAsync(r => r.Rating.Value);

            var overallRatingDistribution = await _context.Reviews
                .Where(r => r.Rating.HasValue)
                .GroupBy(r => r.Rating.Value)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Rating, x => x.Count);

            var recentReviews = await _context.Reviews
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Take(10)
                .ToListAsync();

            var recentReviewDtos = new List<ReviewDto>();
            foreach (var review in recentReviews)
            {
                string targetName = "";
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

                recentReviewDtos.Add(new ReviewDto
                {
                    ReviewId = review.ReviewId,
                    UserId = review.UserId ?? 0,
                    UserName = review.User?.Name ?? "Anonymous",
                    UserEmail = review.User?.Email ?? "",
                    TargetType = review.TargetType ?? "",
                    TargetId = review.TargetId,
                    TargetName = targetName,
                    Rating = review.Rating ?? 0,
                    Comment = review.Comment,
                    CreatedAt = review.CreatedAt,
                    UpdatedAt = review.UpdatedAt
                });
            }

            var reviewSummaryDto = new ReviewSummaryDto
            {
                TotalReviews = totalReviews,
                OverallAverageRating = Math.Round(overallAverageRating, 1),
                OverallRatingDistribution = overallRatingDistribution,
                RecentReviews = recentReviewDtos
            };

            return new ApiResponse<ReviewSummaryDto>
            {
                Success = true,
                Message = "Review summary retrieved successfully",
                Data = reviewSummaryDto
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<ReviewSummaryDto>
            {
                Success = false,
                Message = $"Error retrieving review summary: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<bool>> CanUserReviewAsync(int userId, string targetType, int targetId)
    {
        try
        {
            // Check if user has purchased and paid for the item
            if (targetType == "Product")
            {
                // Check if user has ordered this product and payment is completed
                var hasOrderedAndPaid = await _context.OrderItems
                    .Include(oi => oi.Order)
                        .ThenInclude(o => o.PaymentDetails)
                            .ThenInclude(pd => pd.Payment)
                    .AnyAsync(oi => oi.Order.UserId == userId &&
                                   oi.ProductId == targetId &&
                                   oi.Order.PaymentDetails.Any(pd => pd.Payment.Status == "completed"));

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Check completed",
                    Data = hasOrderedAndPaid
                };
            }
            else if (targetType == "Concept")
            {
                // Check if user has booked this concept and payment is completed
                var hasBookedAndPaid = await _context.Bookings
                    .Include(b => b.PaymentDetails)
                        .ThenInclude(pd => pd.Payment)
                    .AnyAsync(b => b.ConceptId == targetId && b.UserId == userId &&
                                  b.PaymentDetails.Any(pd => pd.Payment.Status == "completed"));

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Check completed",
                    Data = hasBookedAndPaid
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Invalid target type",
                Data = false
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error checking review eligibility: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<bool>> HasUserReviewedAsync(int userId, string targetType, int targetId)
    {
        try
        {
            var hasReviewed = await _context.Reviews
                .AnyAsync(r => r.UserId == userId &&
                              r.TargetType == targetType &&
                              r.TargetId == targetId);

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Check completed",
                Data = hasReviewed
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error checking review status: {ex.Message}"
            };
        }
    }

    private async Task<ReviewDto?> GetReviewWithDetailsAsync(int reviewId)
    {
        var review = await _context.Reviews
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.ReviewId == reviewId);

        if (review == null) return null;

        string targetName = "";
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

        return new ReviewDto
        {
            ReviewId = review.ReviewId,
            UserId = review.UserId ?? 0,
            UserName = review.User?.Name ?? "Anonymous",
            UserEmail = review.User?.Email ?? "",
            TargetType = review.TargetType ?? "",
            TargetId = review.TargetId,
            TargetName = targetName,
            Rating = review.Rating ?? 0,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt
        };
    }
}
