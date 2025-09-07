namespace MonAmour.Models
{
    public class BlogIndexViewModel
    {
        public List<BlogPost> FeaturedPosts { get; set; } = new();
        public List<BlogPost> RecentPosts { get; set; } = new();
        public List<BlogPost> DailyPosts { get; set; } = new();
        public string SearchQuery { get; set; } = "";
        public string SelectedFilter { get; set; } = "";
    }

    public class BlogDetailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public string Excerpt { get; set; } = "";
        public string Author { get; set; } = "";
        public string Category { get; set; } = "";
        public DateTime PublishedDate { get; set; }
        public string FeaturedImage { get; set; } = "";
        public int ReadTime { get; set; }
        public List<string> Tags { get; set; } = new();
        public List<Comment> Comments { get; set; } = new();
    }

    public class BlogPost
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Excerpt { get; set; } = "";
        public string Author { get; set; } = "";
        public string Category { get; set; } = "";
        public DateTime PublishedDate { get; set; }
        public string FeaturedImage { get; set; } = "";
        public int ReadTime { get; set; }
        public bool IsFeatured { get; set; }
    }

    public class Comment
    {
        public int Id { get; set; }
        public string AuthorName { get; set; } = "";
        public string AuthorEmail { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public int BlogPostId { get; set; }
    }
}