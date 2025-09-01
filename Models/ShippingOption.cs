using System;
using System.Collections.Generic;

namespace MonAmour.Models;

public partial class ShippingOption
{
    public int ShippingOptionId { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
