namespace MonAmour.ViewModels;

public class OrderDetailUserViewModel
{
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal? ShippingCost { get; set; }
    public string? ShippingAddress { get; set; } = "Kim Bai";
    public string? ShippingPhone { get; set; }
    public string? ShippingNote { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentStatus { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? TransactionId { get; set; }
    public List<OrderItemUserViewModel> Items { get; set; } = new();
}