namespace MonAmour.ViewModels;

public class HomeIndexViewModel
{
    public List<BannerHomepageListViewModel> HomepageBanners { get; set; } = new();
    public List<BannerServiceListViewModel> ServiceBanners { get; set; } = new();
    public List<PartnerViewModel> Partners { get; set; } = new();
}

public class OrderHistoryUserViewModel
{
    public List<OrderCategoryUserViewModel> Categories { get; set; } = new();
}

public class OrderCategoryUserViewModel
{
    public string Name { get; set; } = string.Empty; // "Concept" or "Gift Box"
    public List<OrderSummaryUserViewModel> Orders { get; set; } = new();
}

public class OrderSummaryUserViewModel
{
    public DateTime OrderDate { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Confirmed, Shipping, Completed, Canceled
    public bool CanReview { get; set; }
    public bool HasReview { get; set; }
    public int? ReviewId { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemUserViewModel> Items { get; set; } = new();
}

public class OrderItemUserViewModel
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

public class ConceptDropdownViewModel
{
    public int ConceptId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public string? LocationName { get; set; }
}

public class PaymentMethodDropdownViewModel
{
    public int PaymentMethodId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class PaymentDetailViewModel
{
    public int PaymentDetailId { get; set; }
    public int PaymentId { get; set; }
    public int? OrderId { get; set; }
    public decimal? Amount { get; set; }
    public string? PaymentMethodName { get; set; }
    public string? PaymentStatus { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class UserDropdownViewModel
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}