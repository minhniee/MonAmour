using System.ComponentModel.DataAnnotations;

namespace MonAmour.ViewModels
{
    public class OrderViewModel
    {
        public int OrderId { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public decimal? TotalPrice { get; set; }
        public decimal? ShippingCost { get; set; }
        public string? Status { get; set; }
        public int? ShippingOptionId { get; set; }
        public string? ShippingOptionName { get; set; }
        public string? TrackingNumber { get; set; }
        public DateOnly? EstimatedDelivery { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? ShippingAddress { get; set; }
        public int ItemCount { get; set; }
        public bool HasPayment { get; set; }
        public string? PaymentStatus { get; set; }
        public decimal? PaymentAmount { get; set; }
    }

    public class OrderDetailViewModel
    {
        public int OrderId { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? UserPhone { get; set; }
        public decimal? TotalPrice { get; set; }
        public decimal? ShippingCost { get; set; }
        public string? Status { get; set; }
        public int? ShippingOptionId { get; set; }
        public string? ShippingOptionName { get; set; }
        public string? TrackingNumber { get; set; }
        public DateOnly? EstimatedDelivery { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? ShippingAddress { get; set; }
        
        public List<OrderItemViewModel> OrderItems { get; set; } = new List<OrderItemViewModel>();
        public List<PaymentDetailViewModel> PaymentDetails { get; set; } = new List<PaymentDetailViewModel>();
    }

    public class OrderItemViewModel
    {
        public int OrderItemId { get; set; }
        public int? OrderId { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductImage { get; set; }
        public int? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }


    public class OrderEditViewModel
    {
        public int OrderId { get; set; }
        
        [Required(ErrorMessage = "Trạng thái đơn hàng là bắt buộc")]
        public string Status { get; set; } = string.Empty;
        
        public DateOnly? EstimatedDelivery { get; set; }
        
        public DateTime? DeliveredAt { get; set; }
        
        public int? ShippingOptionId { get; set; }
        
        public string? ShippingAddress { get; set; }
    }

    public class OrderSearchViewModel
    {
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public int? UserId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? HasPayment { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "CreatedAt";
        public string? SortOrder { get; set; } = "desc";
    }

    // Dropdown ViewModels
    public class OrderStatusDropdownViewModel
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    public class ShippingOptionDropdownViewModel
    {
        public int ShippingOptionId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

}
