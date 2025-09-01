using System.ComponentModel.DataAnnotations;

namespace MonAmourDb_BE.DTOs;

public class CreateReviewDto
{
    [Required]
    public int UserId { get; set; }
    
    [Required]
    public string TargetType { get; set; } = string.Empty; // "Product" or "Concept"
    
    [Required]
    public int TargetId { get; set; }
    
    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }
    
    [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
    public string? Comment { get; set; }
}

public class UpdateReviewDto
{
    [Required]
    public int ReviewId { get; set; }
    
    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }
    
    [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
    public string? Comment { get; set; }
}

public class ReviewDto
{
    public int ReviewId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public int TargetId { get; set; }
    public string TargetName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ProductReviewDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public List<ReviewDto> Reviews { get; set; } = new List<ReviewDto>();
    public Dictionary<int, int> RatingDistribution { get; set; } = new Dictionary<int, int>(); // Rating -> Count
}

public class ConceptReviewDto
{
    public int ConceptId { get; set; }
    public string ConceptName { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public List<ReviewDto> Reviews { get; set; } = new List<ReviewDto>();
    public Dictionary<int, int> RatingDistribution { get; set; } = new Dictionary<int, int>(); // Rating -> Count
}

public class UserReviewDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public List<ReviewDto> Reviews { get; set; } = new List<ReviewDto>();
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
}

public class ReviewSummaryDto
{
    public int TotalReviews { get; set; }
    public double OverallAverageRating { get; set; }
    public Dictionary<int, int> OverallRatingDistribution { get; set; } = new Dictionary<int, int>();
    public List<ReviewDto> RecentReviews { get; set; } = new List<ReviewDto>();
}
