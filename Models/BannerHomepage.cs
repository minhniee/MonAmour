using System;
using System.ComponentModel.DataAnnotations;

namespace MonAmour.Models
{
    public partial class BannerHomepage
    {
        public int BannerId { get; set; }
        
        [Required]
        [StringLength(500)]
        public string ImgUrl { get; set; } = null!;
        
        public bool IsPrimary { get; set; } = false;
        
        public int DisplayOrder { get; set; } = 0;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
