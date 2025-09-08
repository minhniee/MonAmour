using System;
using System.Collections.Generic;

namespace MonAmour.Models;

public partial class ConceptColor
{
    public int ColorId { get; set; }

    public string? Name { get; set; }

    public string? Code { get; set; }

    public virtual ICollection<ConceptColorJunction> ConceptColorJunctions { get; set; } = new List<ConceptColorJunction>();
}
