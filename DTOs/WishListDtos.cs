using System.ComponentModel.DataAnnotations;

namespace MonAmourDb_BE.DTOs;

public class AddToWishListDto
{
    [Required]
    public int UserId { get; set; }
    
    public int? ProductId { get; set; }
    
    public int? ConceptId { get; set; }
}

public class RemoveFromWishListDto
{
    [Required]
    public int UserId { get; set; }
    
    public int? ProductId { get; set; }
    
    public int? ConceptId { get; set; }
}

public class WishListDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? ProductId { get; set; }
    public int? ConceptId { get; set; }
    public DateTime? CreatedAt { get; set; }
    
    // Product details
    public string? ProductName { get; set; }
    public string? ProductDescription { get; set; }
    public decimal? ProductPrice { get; set; }
    public string? ProductStatus { get; set; }
    public List<string>? ProductImages { get; set; }
    
    // Concept details
    public string? ConceptName { get; set; }
    public string? ConceptDescription { get; set; }
    public decimal? ConceptPrice { get; set; }
    public bool? ConceptAvailabilityStatus { get; set; }
    public List<string>? ConceptImages { get; set; }
}

public class UserWishListDto
{
    public int UserId { get; set; }
    public List<WishListDto> Products { get; set; } = new List<WishListDto>();
    public List<WishListDto> Concepts { get; set; } = new List<WishListDto>();
}
