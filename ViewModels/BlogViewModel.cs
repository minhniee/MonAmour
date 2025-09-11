using System.ComponentModel.DataAnnotations;
using MonAmour.Models;

namespace MonAmour.ViewModels
{
    public class BlogListViewModel
    {
        public int BlogId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Excerpt { get; set; }
        public string? FeaturedImage { get; set; }
        public string? AuthorName { get; set; }
        public string? CategoryName { get; set; }
        public DateTime? PublishedDate { get; set; }
        public bool? IsFeatured { get; set; }
        public bool? IsPublished { get; set; }
        public int? ViewCount { get; set; }
        public int? ReadTime { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int CommentCount { get; set; }
    }

    public class BlogCreateViewModel
    {
        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(255, ErrorMessage = "Tiêu đề không được vượt quá 255 ký tự")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung là bắt buộc")]
        public string Content { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Tóm tắt không được vượt quá 500 ký tự")]
        public string? Excerpt { get; set; }

        public IFormFile? ImageFile { get; set; }
        
        public string? FeaturedImage { get; set; }

        public int? AuthorId { get; set; }

        public int? CategoryId { get; set; }

        [StringLength(255, ErrorMessage = "Tags không được vượt quá 255 ký tự")]
        public string? Tags { get; set; }

        public DateTime? PublishedDate { get; set; }

        public bool IsFeatured { get; set; }

        public bool IsPublished { get; set; }

        [Range(1, 60, ErrorMessage = "Thời gian đọc phải từ 1 đến 60 phút")]
        public int? ReadTime { get; set; }

        public List<BlogCategory> Categories { get; set; } = new List<BlogCategory>();
        public List<User> Authors { get; set; } = new List<User>();
    }

    public class BlogEditViewModel
    {
        public int BlogId { get; set; }

        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(255, ErrorMessage = "Tiêu đề không được vượt quá 255 ký tự")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung là bắt buộc")]
        public string Content { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Tóm tắt không được vượt quá 500 ký tự")]
        public string? Excerpt { get; set; }

        public IFormFile? ImageFile { get; set; }
        
        public string? FeaturedImage { get; set; }

        public int? AuthorId { get; set; }

        public int? CategoryId { get; set; }

        [StringLength(255, ErrorMessage = "Tags không được vượt quá 255 ký tự")]
        public string? Tags { get; set; }

        public DateTime? PublishedDate { get; set; }

        public bool IsFeatured { get; set; }

        public bool IsPublished { get; set; }

        [Range(1, 60, ErrorMessage = "Thời gian đọc phải từ 1 đến 60 phút")]
        public int? ReadTime { get; set; }

        public List<BlogCategory> Categories { get; set; } = new List<BlogCategory>();
        public List<User> Authors { get; set; } = new List<User>();
    }

    public class BlogDetailViewModel
    {
        public int BlogId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Excerpt { get; set; }
        public string? FeaturedImage { get; set; }
        public string? AuthorName { get; set; }
        public string? CategoryName { get; set; }
        public string? Tags { get; set; }
        public DateTime? PublishedDate { get; set; }
        public bool? IsFeatured { get; set; }
        public bool? IsPublished { get; set; }
        public int? ReadTime { get; set; }
        public int? ViewCount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<BlogComment> Comments { get; set; } = new List<BlogComment>();
        public List<BlogListViewModel> RelatedPosts { get; set; } = new List<BlogListViewModel>();
    }

    public class BlogCategoryListViewModel
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Slug { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int BlogCount { get; set; }
    }

    public class BlogCategoryCreateViewModel
    {
        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Mô tả không được vượt quá 255 ký tự")]
        public string? Description { get; set; }

        [StringLength(50, ErrorMessage = "Slug không được vượt quá 50 ký tự")]
        public string? Slug { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class BlogCategoryEditViewModel
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Mô tả không được vượt quá 255 ký tự")]
        public string? Description { get; set; }

        [StringLength(50, ErrorMessage = "Slug không được vượt quá 50 ký tự")]
        public string? Slug { get; set; }

        public bool IsActive { get; set; }
    }

    public class BlogCommentListViewModel
    {
        public int CommentId { get; set; }
        public string BlogTitle { get; set; } = string.Empty;
        public string? AuthorName { get; set; }
        public string? AuthorEmail { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool? IsApproved { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? UserName { get; set; }
    }

    public class BlogCommentEditViewModel
    {
        public int CommentId { get; set; }
        public string BlogTitle { get; set; } = string.Empty;
        public string? AuthorName { get; set; }
        public string? AuthorEmail { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
    }

    public class BlogIndexViewModel
    {
        public List<BlogListViewModel> Blogs { get; set; } = new List<BlogListViewModel>();
        public List<BlogListViewModel> FeaturedPosts { get; set; } = new List<BlogListViewModel>();
        public List<BlogListViewModel> RecentPosts { get; set; } = new List<BlogListViewModel>();
        public List<BlogListViewModel> DailyPosts { get; set; } = new List<BlogListViewModel>();
        public List<BlogCategoryListViewModel> Categories { get; set; } = new List<BlogCategoryListViewModel>();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string? SelectedFilter { get; set; }
        public int? CategoryId { get; set; }
    }
}