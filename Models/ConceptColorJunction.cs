using System;

namespace MonAmour.Models;

public partial class ConceptColorJunction
{
    public int ConceptId { get; set; }

    public int ColorId { get; set; }

    public virtual Concept Concept { get; set; } = null!;

    public virtual ConceptColor Color { get; set; } = null!;
}
