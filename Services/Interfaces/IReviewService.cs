using MonAmour.AuthViewModel;

namespace MonAmour.Services.Interfaces;

public interface IReviewService
{
    Task<ReviewViewModel> CreateReviewAsync(CreateReviewViewModel dto);
    Task<ReviewViewModel> UpdateReviewAsync(UpdateReviewViewModel dto);
    Task<bool> DeleteReviewAsync(int reviewId, int userId);
    Task<ReviewViewModel> GetReviewByIdAsync(int reviewId);
    Task<ProductReviewViewModel> GetProductReviewsAsync(int productId, int page = 1, int pageSize = 10);
    Task<ConceptReviewViewModel> GetConceptReviewsAsync(int conceptId, int page = 1, int pageSize = 10);
    Task<UserReviewViewModel> GetUserReviewsAsync(int userId, int page = 1, int pageSize = 10);
    Task<ReviewSummaryViewModel> GetReviewSummaryAsync();
    Task<bool> CanUserReviewAsync(int userId, string targetType, int targetId);
    Task<bool> HasUserReviewedAsync(int userId, string targetType, int targetId);
}