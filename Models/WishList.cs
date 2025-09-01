using System;
using System.Collections.Generic;

namespace MonAmourDb_BE.Models;

public partial class WishList
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? ProductId { get; set; }

    public int? ConceptId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Concept? Concept { get; set; }

    public virtual Product? Product { get; set; }

    public virtual User User { get; set; } = null!;
}
