using System;
using System.Collections.Generic;

namespace MonAmour.Models;

public partial class PaymentStatus
{
    public int PaymentStatusId { get; set; }

    public int PaymentId { get; set; }

    public string Status { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Payment Payment { get; set; } = null!;
}
