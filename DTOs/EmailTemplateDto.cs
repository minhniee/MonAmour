using MonAmourDb_BE.Models;

namespace MonAmourDb_BE.DTOs;

public class EmailTemplateDto
{

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