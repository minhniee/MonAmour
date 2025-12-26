using System;
using System.Collections.Generic;

namespace MonAmour.Models;

public partial class VwPaymentDetail
{
    public int PaymentId { get; set; }

    public decimal? Amount { get; set; }

    public string? Status { get; set; }

    public string? PaymentReference { get; set; }

    public long? TransactionId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UserId { get; set; }

    public string? Email { get; set; }

    public string? Username { get; set; }

    public string? PaymentMethodName { get; set; }

    public int? OrderId { get; set; }

    public decimal? TotalPrice { get; set; }

    public string? OrderStatus { get; set; }

    public decimal? PaymentDetailAmount { get; set; }
}
