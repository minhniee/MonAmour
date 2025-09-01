using System;
using System.Collections.Generic;

namespace MonAmour.Models;

public partial class ConceptCategory
{
    public int CategoryId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Concept> Concepts { get; set; } = new List<Concept>();
}
