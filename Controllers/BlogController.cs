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
    public async Task<IActionResult> Index(string searchTerm = "", string filter = "")
    {
        var allBlogs = await _blogService.GetPublishedBlogsAsync();
        
        // Apply search filter
        if (!string.IsNullOrEmpty(searchTerm))
        {
            allBlogs = await _blogService.SearchBlogsAsync(searchTerm);
        }

        // Apply sorting filter
        switch (filter.ToLower())
        {
            case "new":
                allBlogs = allBlogs.OrderByDescending(b => b.PublishedDate);
                break;
            case "time":
                allBlogs = allBlogs.OrderBy(b => b.ReadTime);
                break;
            case "type":
                allBlogs = allBlogs.OrderBy(b => b.Category?.Name);
                break;
            default:
                allBlogs = allBlogs.OrderByDescending(b => b.PublishedDate);
                break;
        }

        var featuredBlogs = await _blogService.GetFeaturedBlogsAsync();
        
        var viewModel = new BlogIndexViewModel
        {
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
            RecentPosts = allBlogs.Skip(1).Take(4).Select(b => new BlogListViewModel
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
            DailyPosts = allBlogs.Skip(5).Take(3).Select(b => new BlogListViewModel
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
            SearchTerm = searchTerm,
            SelectedFilter = filter
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
            Tags = !string.IsNullOrEmpty(blog.Tags) ? string.Join(", ", blog.Tags.Split(',')) : "",
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