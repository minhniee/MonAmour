using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonAmour.Models;

[Table("Blog_Category")]
public partial class BlogCategory
{
    [Key]
    [Column("category_id")]
    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    [StringLength(255)]
    [Column("description")]
    public string? Description { get; set; }

    [StringLength(50)]
    [Column("slug")]
    public string? Slug { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();
}
