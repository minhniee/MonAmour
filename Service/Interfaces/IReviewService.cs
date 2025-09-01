using MonAmourDb_BE.DTOs;

namespace MonAmourDb_BE.Service.Interfaces;

public interface IReviewService
{
    Task<ApiResponse<ReviewDto>> CreateReviewAsync(CreateReviewDto dto);
    Task<ApiResponse<ReviewDto>> UpdateReviewAsync(UpdateReviewDto dto);
    Task<ApiResponse<bool>> DeleteReviewAsync(int reviewId, int userId);
    Task<ApiResponse<ReviewDto>> GetReviewByIdAsync(int reviewId);
    Task<ApiResponse<ProductReviewDto>> GetProductReviewsAsync(int productId, int page = 1, int pageSize = 10);
    Task<ApiResponse<ConceptReviewDto>> GetConceptReviewsAsync(int conceptId, int page = 1, int pageSize = 10);
    Task<ApiResponse<UserReviewDto>> GetUserReviewsAsync(int userId, int page = 1, int pageSize = 10);
    Task<ApiResponse<ReviewSummaryDto>> GetReviewSummaryAsync();
    Task<ApiResponse<bool>> CanUserReviewAsync(int userId, string targetType, int targetId);
    Task<ApiResponse<bool>> HasUserReviewedAsync(int userId, string targetType, int targetId);
}
