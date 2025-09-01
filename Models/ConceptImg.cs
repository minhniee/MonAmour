using System;
using System.Collections.Generic;

namespace MonAmourDb_BE.Models;

public partial class ConceptImg
{
    public int ImgId { get; set; }

    public int? ConceptId { get; set; }

    public string? ImgUrl { get; set; }

    public string? ImgName { get; set; }

    public string? AltText { get; set; }

    public bool? IsPrimary { get; set; }

    public int? DisplayOrder { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Concept? Concept { get; set; }
}
