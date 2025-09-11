using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MonAmour.Models;
using MonAmour.Services.Interfaces;
using MonAmour.Hubs;
using MonAmour.Helpers;
using MonAmour.ViewModels;

namespace MonAmour.Controllers;

public class BlogController : Controller
{
    private readonly IBlogService _blogService;
    private readonly IHubContext<CommentHub, ICommentHubClient> _commentHub;

    public BlogController(IBlogService blogService, IHubContext<CommentHub, ICommentHubClient> commentHub)
    {
        _blogService = blogService;
        _commentHub = commentHub;
    }

    // GET: Blog
    public async Task<IActionResult> Index(string searchTerm = "", string filter = "", int? categoryId = null, int page = 1)
    {
        // Get all published blogs
        var allBlogs = await _blogService.GetPublishedBlogsAsync();
        
        // Get featured blogs separately to ensure they're always shown regardless of filters
        var featuredBlogs = await _blogService.GetFeaturedBlogsAsync();
        
        // Apply search filter if provided
        if (!string.IsNullOrEmpty(searchTerm))
        {
            allBlogs = await _blogService.SearchBlogsAsync(searchTerm);
        }

        // Apply category filter if provided
        if (categoryId.HasValue)
        {
            allBlogs = allBlogs.Where(b => b.Category?.CategoryId == categoryId);
        }

        // Create a separate list for non-featured blogs
        var nonFeaturedBlogs = allBlogs.Where(b => !(b.IsFeatured ?? false)).ToList();

        // Apply sorting filter to non-featured blogs
        IEnumerable<Blog> sortedBlogs = filter.ToLower() switch
        {
            "new" => nonFeaturedBlogs.OrderByDescending(b => b.PublishedDate),
            "time" => nonFeaturedBlogs.OrderBy(b => b.ReadTime),
            "type" => nonFeaturedBlogs.OrderBy(b => b.Category?.Name),
            _ => nonFeaturedBlogs.OrderByDescending(b => b.PublishedDate)
        };

        // Get categories for the filter dropdown
        var categories = await _blogService.GetAllCategoriesAsync();
        
        // Calculate pagination
        int pageSize = 6; // Number of items per page for daily posts
        int totalItems = sortedBlogs.Count();
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        page = Math.Max(1, Math.Min(page, totalPages)); // Ensure page is within valid range
        
        var viewModel = new BlogIndexViewModel
        {
            // Featured posts are always shown at the top
            FeaturedPosts = featuredBlogs.Take(1).Select(b => new BlogListViewModel
            {
                BlogId = b.BlogId,
                Title = b.Title,
                Excerpt = b.Excerpt ?? "",
                AuthorName = b.Author?.Name ?? "Admin",
                CategoryName = b.Category?.Name ?? "General",
                PublishedDate = b.PublishedDate ?? DateTime.Now,
                FeaturedImage = b.FeaturedImage ?? "",
                ReadTime = b.ReadTime ?? 0,
                IsFeatured = b.IsFeatured ?? false
            }).ToList(),

            // Recent posts are the first 4 non-featured posts
            RecentPosts = sortedBlogs.Take(4).Select(b => new BlogListViewModel
            {
                BlogId = b.BlogId,
                Title = b.Title,
                Excerpt = b.Excerpt ?? "",
                AuthorName = b.Author?.Name ?? "Admin",
                CategoryName = b.Category?.Name ?? "General",
                PublishedDate = b.PublishedDate ?? DateTime.Now,
                FeaturedImage = b.FeaturedImage ?? "",
                ReadTime = b.ReadTime ?? 0,
                IsFeatured = b.IsFeatured ?? false
            }).ToList(),

            // Daily posts are paginated
            DailyPosts = sortedBlogs.Skip((page - 1) * pageSize).Take(pageSize).Select(b => new BlogListViewModel
            {
                BlogId = b.BlogId,
                Title = b.Title,
                Excerpt = b.Excerpt ?? "",
                AuthorName = b.Author?.Name ?? "Admin",
                CategoryName = b.Category?.Name ?? "General",
                PublishedDate = b.PublishedDate ?? DateTime.Now,
                FeaturedImage = b.FeaturedImage ?? "",
                ReadTime = b.ReadTime ?? 0,
                IsFeatured = b.IsFeatured ?? false
            }).ToList(),

            // Add categories for the filter
            Categories = categories.Select(c => new BlogCategoryListViewModel
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                Description = c.Description,
                BlogCount = c.Blogs?.Count ?? 0
            }).ToList(),

            // Pagination and filter state
            CurrentPage = page,
            TotalPages = totalPages,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            SelectedFilter = filter,
            CategoryId = categoryId
        };

        return View(viewModel);
    }

    // GET: Blog/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var blog = await _blogService.GetBlogByIdWithDetailsAsync(id);
            if (blog == null || !(blog.IsPublished ?? false))
            {
                return NotFound();
            }

            // Increment view count
            await _blogService.IncrementViewCountAsync(id);

            // Get related posts based on tags
            var relatedPosts = new List<BlogListViewModel>();
            if (!string.IsNullOrEmpty(blog.Tags))
            {
                var tags = blog.Tags.Split(',').Select(t => t.Trim()).ToList();
                var allRelatedPosts = await _blogService.GetPublishedBlogsAsync();
                
                relatedPosts = allRelatedPosts
                    .Where(b => b.BlogId != blog.BlogId && b.IsPublished == true && 
                           !string.IsNullOrEmpty(b.Tags) &&
                           b.Tags.Split(',').Select(t => t.Trim())
                                .Intersect(tags, StringComparer.OrdinalIgnoreCase).Any())
                    .OrderByDescending(b => b.PublishedDate)
                    .Take(3)
                    .Select(b => new BlogListViewModel
                    {
                        BlogId = b.BlogId,
                        Title = b.Title,
                        Excerpt = b.Excerpt ?? "",
                        FeaturedImage = b.FeaturedImage,
                        CategoryName = b.Category?.Name ?? "General",
                        PublishedDate = b.PublishedDate ?? DateTime.Now,
                        ReadTime = b.ReadTime ?? 0
                    })
                    .ToList();
            }

            // Set ViewBag for user info
            var currentUserId = AuthHelper.GetUserId(HttpContext);
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.IsAdmin = AuthHelper.IsAdmin(HttpContext);

            var viewModel = new BlogDetailViewModel
            {
                BlogId = blog.BlogId,
                Title = blog.Title,
                Content = blog.Content,
                Excerpt = blog.Excerpt ?? "",
                AuthorName = blog.Author?.Name ?? "Admin",
                CategoryName = blog.Category?.Name ?? "General",
                PublishedDate = blog.PublishedDate ?? DateTime.Now,
                FeaturedImage = blog.FeaturedImage ?? "",
                ReadTime = blog.ReadTime ?? 0,
                Tags = blog.Tags,
                RelatedPosts = relatedPosts,
                Comments = blog.Comments.Select(c => new BlogComment
                {
                    CommentId = c.CommentId,
                    AuthorName = c.User?.Name ?? c.AuthorName ?? "Anonymous",
                    AuthorEmail = c.AuthorEmail ?? "",
                    Content = c.Content,
                    CreatedAt = c.CreatedAt ?? DateTime.Now,
                    BlogId = c.BlogId
                }).ToList()
            };

        return View(viewModel);
    }

    // POST: Blog/Search
    [HttpPost]
    public IActionResult Search(string searchTerm)
    {
        return RedirectToAction("Index", new { searchTerm });
    }

    // GET: Blog/Category/5
    // GET: Blog/Tag/{tag}
    public async Task<IActionResult> Tag(string tag)
    {
        var blogs = await _blogService.GetBlogsByTagAsync(tag);
        
        var viewModel = new BlogIndexViewModel
        {
            FeaturedPosts = new List<BlogListViewModel>(),
            RecentPosts = new List<BlogListViewModel>(),
            DailyPosts = blogs.Select(b => new BlogListViewModel
            {
                BlogId = b.BlogId,
                Title = b.Title,
                Excerpt = b.Excerpt ?? "",
                AuthorName = b.Author?.Name ?? "Admin",
                CategoryName = b.Category?.Name ?? "General",
                PublishedDate = b.PublishedDate ?? DateTime.Now,
                FeaturedImage = b.FeaturedImage ?? "",
                ReadTime = b.ReadTime ?? 0,
                IsFeatured = b.IsFeatured ?? false
            }).ToList(),
            SearchTerm = $"Tag: {tag}",
            SelectedFilter = ""
        };

        return View("Index", viewModel);
    }

    public async Task<IActionResult> Category(int id)
    {
        var blogs = await _blogService.GetBlogsByCategoryAsync(id);
        var category = await _blogService.GetCategoryByIdAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        var viewModel = new BlogIndexViewModel
        {
            FeaturedPosts = new List<BlogListViewModel>(),
            RecentPosts = new List<BlogListViewModel>(),
            DailyPosts = blogs.Select(b => new BlogListViewModel
            {
                BlogId = b.BlogId,
                Title = b.Title,
                Excerpt = b.Excerpt ?? "",
                AuthorName = b.Author?.Name ?? "Admin",
                CategoryName = b.Category?.Name ?? "General",
                PublishedDate = b.PublishedDate ?? DateTime.Now,
                FeaturedImage = b.FeaturedImage ?? "",
                ReadTime = b.ReadTime ?? 0,
                IsFeatured = b.IsFeatured ?? false
            }).ToList(),
            SearchTerm = $"Category: {category.Name}",
            SelectedFilter = ""
        };

        return View("Index", viewModel);
    }

    // POST: Blog/Comment
    [HttpPost]
    public async Task<IActionResult> AddComment(int blogId, string authorName, string authorEmail, string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return Json(new { success = false, message = "Nội dung bình luận không được để trống." });
        }

        try
        {
            // Get current user if logged in
            var currentUserId = AuthHelper.GetUserId(HttpContext);
            var currentUserName = AuthHelper.GetUserName(HttpContext);
            var currentUserEmail = AuthHelper.GetUserEmail(HttpContext);

            var comment = new BlogComment
            {
                BlogId = blogId,
                UserId = currentUserId,
                AuthorName = currentUserId.HasValue ? currentUserName : authorName,
                AuthorEmail = currentUserId.HasValue ? currentUserEmail : authorEmail,
                Content = content
            };

            var createdComment = await _blogService.CreateCommentAsync(comment);

            // Prepare comment data for SignalR
            var commentData = new
            {
                CommentId = createdComment.CommentId,
                AuthorName = createdComment.User?.Name ?? createdComment.AuthorName ?? "Anonymous",
                AuthorEmail = createdComment.AuthorEmail ?? "",
                Content = createdComment.Content,
                CreatedDate = (createdComment.CreatedAt ?? DateTime.Now).ToString("dd/MM/yyyy HH:mm"),
                BlogId = createdComment.BlogId,
                UserId = createdComment.UserId,
                IsCurrentUser = createdComment.UserId == currentUserId,
                CanDelete = currentUserId.HasValue && (createdComment.UserId == currentUserId || AuthHelper.IsAdmin(HttpContext))
            };

            // Send real-time notification to all users viewing this blog
            await _commentHub.Clients.Group($"blog_{blogId}").ReceiveComment(commentData);

            return Json(new { success = true, comment = commentData, message = "Bình luận đã được đăng thành công!" });
        }
        catch (Exception)
        {
            return Json(new { success = false, message = "Có lỗi xảy ra khi đăng bình luận." });
        }
    }

    // POST: Blog/DeleteComment
    [HttpPost]
    public async Task<IActionResult> DeleteComment(int commentId)
    {
        try
        {
            var comment = await _blogService.GetCommentByIdAsync(commentId);
            if (comment == null)
            {
                return Json(new { success = false, message = "Bình luận không tồn tại." });
            }

            var currentUserId = AuthHelper.GetUserId(HttpContext);
            var isAdmin = AuthHelper.IsAdmin(HttpContext);

            // Check permissions: only comment author or admin can delete
            if (!currentUserId.HasValue || (comment.UserId != currentUserId && !isAdmin))
            {
                return Json(new { success = false, message = "Bạn không có quyền xóa bình luận này." });
            }

            var success = await _blogService.DeleteCommentAsync(commentId);
            if (success)
            {
                // Send real-time notification to all users viewing this blog
                await _commentHub.Clients.Group($"blog_{comment.BlogId}").CommentDeleted(commentId);
                
                return Json(new { success = true, message = "Bình luận đã được xóa thành công!" });
            }

            return Json(new { success = false, message = "Không thể xóa bình luận." });
        }
        catch (Exception)
        {
            return Json(new { success = false, message = "Có lỗi xảy ra khi xóa bình luận." });
        }
    }
}