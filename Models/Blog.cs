using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonAmour.Models;

[Table("Blog")]
public partial class Blog
{
    [Key]
    [Column("blog_id")]
    public int BlogId { get; set; }

    [Required]
    [StringLength(255)]
    [Column("title")]
    public string Title { get; set; } = "";

    [Column("content")]
    public string Content { get; set; } = "";

    [StringLength(500)]
    [Column("excerpt")]
    public string Excerpt { get; set; } = "";

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

    [Column("is_featured")]
    public bool IsFeatured { get; set; } = false;

    [Column("is_published")]
    public bool IsPublished { get; set; } = false;

    [Column("read_time")]
    public int ReadTime { get; set; } = 0;

    [Column("view_count")]
    public int ViewCount { get; set; } = 0;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navigation properties
    [ForeignKey("AuthorId")]
    public virtual User? Author { get; set; }

    [ForeignKey("CategoryId")]
    public virtual BlogCategory? Category { get; set; }

    public virtual ICollection<BlogComment> Comments { get; set; } = new List<BlogComment>();
}
