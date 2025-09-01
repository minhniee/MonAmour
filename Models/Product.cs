using System;
using System.Collections.Generic;

namespace MonAmour.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string? Name { get; set; }

    public int? CategoryId { get; set; }

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public string? Material { get; set; }

    public string? TargetAudience { get; set; }

    public int? StockQuantity { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ProductCategory? Category { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<ProductImg> ProductImgs { get; set; } = new List<ProductImg>();

    public virtual ICollection<WishList> WishLists { get; set; } = new List<WishList>();
}
