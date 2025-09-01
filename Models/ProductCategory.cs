using System;
using System.Collections.Generic;

namespace MonAmourDb_BE.Models;

public partial class ProductCategory
{
    public int CategoryId { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
