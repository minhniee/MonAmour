using System.ComponentModel.DataAnnotations;

namespace MonAmour.AuthViewModel;


public class AddToWishListViewModel
{
    [Required(ErrorMessage = "User is required")]
    public int UserId { get; set; }

    public int? ProductId { get; set; }
    public int? ConceptId { get; set; }
}

public class RemoveFromWishListViewModel
{
    [Required(ErrorMessage = "User is required")]
    public int UserId { get; set; }

    public int? ProductId { get; set; }
    public int? ConceptId { get; set; }
}

public class WishListViewModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? ProductId { get; set; }
    public int? ConceptId { get; set; }
    public DateTime? CreatedAt { get; set; }

    // Product
    public string? ProductName { get; set; }
    public string? ProductDescription { get; set; }
    public decimal? ProductPrice { get; set; }
    public string? ProductStatus { get; set; }
    public List<string>? ProductImages { get; set; }

    // Concept
    public string? ConceptName { get; set; }
    public string? ConceptDescription { get; set; }
    public decimal? ConceptPrice { get; set; }
    public bool? ConceptAvailabilityStatus { get; set; }
    public List<string>? ConceptImages { get; set; }
}

public class UserWishListViewModel
{
    public int UserId { get; set; }

    // có thể customize cho View: tách Product vs Concept
    public List<WishListViewModel> Products { get; set; } = new();
    public List<WishListViewModel> Concepts { get; set; } = new();
}


