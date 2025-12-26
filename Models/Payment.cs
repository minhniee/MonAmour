using System;
using System.Collections.Generic;

namespace MonAmour.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public decimal? Amount { get; set; }

    public string? Status { get; set; }

    public int? PaymentMethodId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public string? PaymentReference { get; set; }

    public long? TransactionId { get; set; }

    public int? UserId { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<PaymentDetail> PaymentDetails { get; set; } = new List<PaymentDetail>();

    public virtual PaymentMethod? PaymentMethod { get; set; }

    public virtual ICollection<PaymentStatus> PaymentStatuses { get; set; } = new List<PaymentStatus>();

    public virtual User? User { get; set; }
}
