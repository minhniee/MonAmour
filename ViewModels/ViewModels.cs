namespace MonAmour.ViewModels;

public class OrderHistoryViewModel
{
    public List<OrderCategoryViewModel> Categories { get; set; } = new();
}

public class OrderCategoryViewModel
{
    public string Name { get; set; } = string.Empty; // "Concept" or "Gift Box"
    public List<OrderSummaryViewModel> Orders { get; set; } = new();
}

public class OrderSummaryViewModel
{
    public DateTime OrderDate { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Confirmed, Shipping, Completed, Canceled
    public bool CanReview { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemViewModel> Items { get; set; } = new();
}

public class OrderItemViewModel
{
    public int ItemId { get; set; }
    public string ItemType { get; set; } = string.Empty; // Product or Concept
    public int TargetId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class RatingViewModel
{
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
    public DateTime ReviewDate { get; set; } = DateTime.Now;
}