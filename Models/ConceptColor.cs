using System;
using System.Collections.Generic;

namespace MonAmourDb_BE.Models;

public partial class ConceptColor
{
    public int ColorId { get; set; }

    public string? Name { get; set; }

    public string? Code { get; set; }

    public virtual ICollection<Concept> Concepts { get; set; } = new List<Concept>();
}
