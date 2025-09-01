using System;
using System.Collections.Generic;

namespace MonAmour.Models;

public partial class Location
{
    public int LocationId { get; set; }

    public string? Name { get; set; }

    public string? Address { get; set; }

    public string? District { get; set; }

    public string? City { get; set; }

    public string? Status { get; set; }

    public int? PartnerId { get; set; }

    public string? GgmapLink { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Concept> Concepts { get; set; } = new List<Concept>();

    public virtual Partner? Partner { get; set; }
}
