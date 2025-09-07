using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonAmour.Models;

[Table("Blog_Category")]
public partial class BlogCategory
{
    [Key]
    [Column("category_id")]
    public int CategoryId { get; set; }

    [Required]
    [StringLength(100)]
    [Column("name")]
    public string Name { get; set; } = "";

    [StringLength(255)]
    [Column("description")]
    public string? Description { get; set; }

    [StringLength(50)]
    [Column("slug")]
    public string? Slug { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navigation properties
    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();
}
