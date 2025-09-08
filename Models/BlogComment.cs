using System;
using System.Collections.Generic;
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

    public string Content { get; set; } = null!;

    public bool? IsApproved { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Blog Blog { get; set; } = null!;

    public virtual User? User { get; set; }
}
