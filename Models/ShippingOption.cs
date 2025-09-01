using System;
using System.Collections.Generic;

namespace MonAmourDb_BE.Models;

public partial class ShippingOption
{
    public int ShippingOptionId { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
