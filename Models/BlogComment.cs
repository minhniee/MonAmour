using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonAmour.Models;

[Table("Blog_Comment")]
public partial class BlogComment
{
    [Key]
    [Column("comment_id")]
    public int CommentId { get; set; }

    [Column("blog_id")]
    public int BlogId { get; set; }

    [Column("user_id")]
    public int? UserId { get; set; }

    [StringLength(100)]
    [Column("author_name")]
    public string? AuthorName { get; set; }

    public string? AuthorEmail { get; set; }

    [Required]
    [Column("content")]
    public string Content { get; set; } = "";

    [Column("is_approved")]
    public bool IsApproved { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navigation properties
    [ForeignKey("BlogId")]
    public virtual Blog Blog { get; set; } = null!;

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}
