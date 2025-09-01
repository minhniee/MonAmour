using System;
using System.Collections.Generic;

namespace MonAmour.Models;

public partial class Content
{
    public int ContentId { get; set; }

    public string? Title { get; set; }

    public string? Body { get; set; }

    public string? Category { get; set; }

    public string? Tags { get; set; }

    public int? AuthorId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? Author { get; set; }
}
