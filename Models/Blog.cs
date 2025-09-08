using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonAmour.Models;

[Table("Blog")]
public partial class Blog
{
    [Key]
    [Column("blog_id")]
    public int BlogId { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string? Excerpt { get; set; }

    [StringLength(255)]
    [Column("featured_image")]
    public string? FeaturedImage { get; set; }

    [Column("author_id")]
    public int? AuthorId { get; set; }

    [Column("category_id")]
    public int? CategoryId { get; set; }

    [StringLength(255)]
    [Column("tags")]
    public string? Tags { get; set; }

    [Column("published_date")]
    public DateTime? PublishedDate { get; set; }

    public bool? IsFeatured { get; set; }

    public bool? IsPublished { get; set; }

    public int? ReadTime { get; set; }

    public int? ViewCount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual User? Author { get; set; }

    public virtual BlogCategory? Category { get; set; }

    public virtual ICollection<BlogComment> Comments { get; set; } = new List<BlogComment>();
}
