using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MonAmour.AuthViewModel;


public class CreateReviewViewModel
{
    [Required(ErrorMessage = "User is required")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Target type is required")]
    [Display(Name = "Target Type")]
    public string TargetType { get; set; } = string.Empty; // "Product" or "Concept"

    [Required(ErrorMessage = "Target ID is required")]
    [Display(Name = "Target Id")]
    public int TargetId { get; set; }

    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }

    [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
    public string? Comment { get; set; }

    public IFormFile? ImageFile { get; set; }
}

public class UpdateReviewViewModel
{
    [Required(ErrorMessage = "Review ID is required")]
    public int ReviewId { get; set; }

    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }

    [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
    public string? Comment { get; set; }

    public IFormFile? ImageFile { get; set; }
}

public class ReviewViewModel
{
    public int ReviewId { get; set; }
    public int UserId { get; set; }

    [Display(Name = "User Name")]
    public string UserName { get; set; } = string.Empty;

    [Display(Name = "User Email")]
    public string UserEmail { get; set; } = string.Empty;

    [Display(Name = "Target Type")]
    public string TargetType { get; set; } = string.Empty;

    [Display(Name = "Target Id")]
    public int TargetId { get; set; }

    [Display(Name = "Target Name")]
    public string TargetName { get; set; } = string.Empty;

    public int Rating { get; set; }
    public string? Comment { get; set; }

    public string? ImageUrl { get; set; }

    [Display(Name = "Created At")]
    public DateTime? CreatedAt { get; set; }

    [Display(Name = "Updated At")]
    public DateTime? UpdatedAt { get; set; }
}

public class ProductReviewViewModel
{
    public int ProductId { get; set; }

    [Display(Name = "Product Name")]
    public string ProductName { get; set; } = string.Empty;

    [Display(Name = "Average Rating")]
    public double AverageRating { get; set; }

    [Display(Name = "Total Reviews")]
    public int TotalReviews { get; set; }

    public List<ReviewViewModel> Reviews { get; set; } = new();

    [Display(Name = "Rating Distribution")]
    public Dictionary<int, int> RatingDistribution { get; set; } = new();
}

public class ConceptReviewViewModel
{
    public int ConceptId { get; set; }

    [Display(Name = "Concept Name")]
    public string ConceptName { get; set; } = string.Empty;

    [Display(Name = "Average Rating")]
    public double AverageRating { get; set; }

    [Display(Name = "Total Reviews")]
    public int TotalReviews { get; set; }

    public List<ReviewViewModel> Reviews { get; set; } = new();

    [Display(Name = "Rating Distribution")]
    public Dictionary<int, int> RatingDistribution { get; set; } = new();
}

public class UserReviewViewModel
{
    public int UserId { get; set; }

    [Display(Name = "User Name")]
    public string UserName { get; set; } = string.Empty;

    public List<ReviewViewModel> Reviews { get; set; } = new();

    [Display(Name = "Average Rating")]
    public double AverageRating { get; set; }

    [Display(Name = "Total Reviews")]
    public int TotalReviews { get; set; }
}

public class ReviewSummaryViewModel
{
    [Display(Name = "Total Reviews")]
    public int TotalReviews { get; set; }

    [Display(Name = "Overall Average Rating")]
    public double OverallAverageRating { get; set; }

    [Display(Name = "Rating Distribution")]
    public Dictionary<int, int> OverallRatingDistribution { get; set; } = new();

    [Display(Name = "Recent Reviews")]
    public List<ReviewViewModel> RecentReviews { get; set; } = new();
}


