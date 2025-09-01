using System;
using System.Collections.Generic;

namespace MonAmourDb_BE.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public decimal? Amount { get; set; }

    public string? Status { get; set; }

    public int? PaymentMethodId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public virtual ICollection<PaymentDetail> PaymentDetails { get; set; } = new List<PaymentDetail>();

    public virtual PaymentMethod? PaymentMethod { get; set; }
}
