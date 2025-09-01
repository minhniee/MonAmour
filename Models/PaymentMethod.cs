using System;
using System.Collections.Generic;

namespace MonAmourDb_BE.Models;

public partial class PaymentMethod
{
    public int PaymentMethodId { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
