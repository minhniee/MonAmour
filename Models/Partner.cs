using System;
using System.Collections.Generic;

namespace MonAmour.Models;

public partial class Partner
{
    public int PartnerId { get; set; }

    public string? Name { get; set; }

    public string? ContactInfo { get; set; }

    public int? UserId { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? Avatar { get; set; }

    public virtual ICollection<Location> Locations { get; set; } = new List<Location>();

    public virtual User? User { get; set; }
}
