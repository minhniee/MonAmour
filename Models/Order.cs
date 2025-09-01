using System;
using System.Collections.Generic;

namespace MonAmour.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? UserId { get; set; }

    public decimal? TotalPrice { get; set; }

    public decimal? ShippingCost { get; set; }

    public string? Status { get; set; }

    public int? ShippingOptionId { get; set; }

    public string? TrackingNumber { get; set; }

    public DateOnly? EstimatedDelivery { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<PaymentDetail> PaymentDetails { get; set; } = new List<PaymentDetail>();

    public virtual ShippingOption? ShippingOption { get; set; }

    public virtual User? User { get; set; }
}
