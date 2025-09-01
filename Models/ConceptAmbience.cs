using System;
using System.Collections.Generic;

namespace MonAmourDb_BE.Models;

public partial class ConceptAmbience
{
    public int AmbienceId { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Concept> Concepts { get; set; } = new List<Concept>();
}
