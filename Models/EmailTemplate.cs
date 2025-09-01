using System;
using System.Collections.Generic;

namespace MonAmourDb_BE.Models;

public partial class EmailTemplate
{
    public int TemplateId { get; set; }

    public string Name { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string Body { get; set; } = null!;

    public string? TemplateType { get; set; }

    public bool? IsActive { get; set; }

    public string? Variables { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }
}
