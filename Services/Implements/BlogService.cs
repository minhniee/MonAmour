using Microsoft.EntityFrameworkCore;
using MonAmour.Models;
using MonAmour.Services.Interfaces;
using System.Text.RegularExpressions;

namespace MonAmour.Services.Implements;

public class BlogService : IBlogService
{
    private readonly MonAmourDbContext _context;

    public BlogService(MonAmourDbContext context)
    {
        _context = context;
    }

    #region Blog CRUD operations

    public async Task<IEnumerable<Blog>> GetAllBlogsAsync()
    {
        return await _context.Blogs
            .Include(b => b.Author)
            .Include(b => b.Category)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Blog>> GetPublishedBlogsAsync()
    {
        return await _context.Blogs
            .Include(b => b.Author)
            .Include(b => b.Category)
            .Where(b => b.IsPublished)
            .OrderByDescending(b => b.PublishedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Blog>> GetFeaturedBlogsAsync()
    {
        return await _context.Blogs
            .Include(b => b.Author)
            .Include(b => b.Category)
            .Where(b => b.IsPublished && b.IsFeatured)
            .OrderByDescending(b => b.PublishedDate)
            .ToListAsync();
    }

    public async Task<Blog?> GetBlogByIdAsync(int id)
    {
        return await _context.Blogs
            .Include(b => b.Author)
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.BlogId == id);
    }

    public async Task<Blog?> GetBlogByIdWithDetailsAsync(int id)
    {
        return await _context.Blogs
            .Include(b => b.Author)
            .Include(b => b.Category)
            .Include(b => b.Comments.Where(c => c.IsApproved))
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(b => b.BlogId == id);
    }

    public async Task<IEnumerable<Blog>> GetBlogsByCategoryAsync(int categoryId)
    {
        return await _context.Blogs
            .Include(b => b.Author)
            .Include(b => b.Category)
            .Where(b => b.CategoryId == categoryId && b.IsPublished)
            .OrderByDescending(b => b.PublishedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Blog>> SearchBlogsAsync(string searchTerm)
    {
        return await _context.Blogs
            .Include(b => b.Author)
            .Include(b => b.Category)
            .Where(b => b.IsPublished && 
                   (b.Title.Contains(searchTerm) || 
                    b.Content.Contains(searchTerm) || 
                    b.Excerpt.Contains(searchTerm) ||
                    (b.Tags != null && b.Tags.Contains(searchTerm))))
            .OrderByDescending(b => b.PublishedDate)
            .ToListAsync();
    }

    public async Task<Blog> CreateBlogAsync(Blog blog)
    {
        blog.CreatedAt = DateTime.Now;
        blog.UpdatedAt = DateTime.Now;
        blog.ReadTime = await CalculateReadTimeAsync(blog.Content);
        
        _context.Blogs.Add(blog);
        await _context.SaveChangesAsync();
        return blog;
    }

    public async Task<Blog?> UpdateBlogAsync(Blog blog)
    {
        var existingBlog = await _context.Blogs.FindAsync(blog.BlogId);
        if (existingBlog == null) return null;

        existingBlog.Title = blog.Title;
        existingBlog.Content = blog.Content;
        existingBlog.Excerpt = blog.Excerpt;
        existingBlog.FeaturedImage = blog.FeaturedImage;
        existingBlog.CategoryId = blog.CategoryId;
        existingBlog.Tags = blog.Tags;
        existingBlog.IsFeatured = blog.IsFeatured;
        existingBlog.IsPublished = blog.IsPublished;
        existingBlog.UpdatedAt = DateTime.Now;
        existingBlog.ReadTime = await CalculateReadTimeAsync(blog.Content);

        await _context.SaveChangesAsync();
        return existingBlog;
    }

    public async Task<bool> DeleteBlogAsync(int id)
    {
        var blog = await _context.Blogs.FindAsync(id);
        if (blog == null) return false;

        _context.Blogs.Remove(blog);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PublishBlogAsync(int id)
    {
        var blog = await _context.Blogs.FindAsync(id);
        if (blog == null) return false;

        blog.IsPublished = true;
        blog.PublishedDate = DateTime.Now;
        blog.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnpublishBlogAsync(int id)
    {
        var blog = await _context.Blogs.FindAsync(id);
        if (blog == null) return false;

        blog.IsPublished = false;
        blog.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetFeaturedAsync(int id, bool isFeatured)
    {
        var blog = await _context.Blogs.FindAsync(id);
        if (blog == null) return false;

        blog.IsFeatured = isFeatured;
        blog.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IncrementViewCountAsync(int id)
    {
        var blog = await _context.Blogs.FindAsync(id);
        if (blog == null) return false;

        blog.ViewCount++;
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Blog Category operations

    public async Task<IEnumerable<BlogCategory>> GetAllCategoriesAsync()
    {
        return await _context.BlogCategories
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<BlogCategory>> GetActiveCategoriesAsync()
    {
        return await _context.BlogCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<BlogCategory?> GetCategoryByIdAsync(int id)
    {
        return await _context.BlogCategories.FindAsync(id);
    }

    public async Task<BlogCategory> CreateCategoryAsync(BlogCategory category)
    {
        category.CreatedAt = DateTime.Now;
        category.UpdatedAt = DateTime.Now;
        category.Slug = await GenerateSlugAsync(category.Name);

        _context.BlogCategories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<BlogCategory?> UpdateCategoryAsync(BlogCategory category)
    {
        var existingCategory = await _context.BlogCategories.FindAsync(category.CategoryId);
        if (existingCategory == null) return null;

        existingCategory.Name = category.Name;
        existingCategory.Description = category.Description;
        existingCategory.IsActive = category.IsActive;
        existingCategory.UpdatedAt = DateTime.Now;
        existingCategory.Slug = await GenerateSlugAsync(category.Name);

        await _context.SaveChangesAsync();
        return existingCategory;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _context.BlogCategories.FindAsync(id);
        if (category == null) return false;

        // Check if category has blogs
        var hasBlog = await _context.Blogs.AnyAsync(b => b.CategoryId == id);
        if (hasBlog)
        {
            // Deactivate instead of delete if has blogs
            category.IsActive = false;
            await _context.SaveChangesAsync();
        }
        else
        {
            _context.BlogCategories.Remove(category);
            await _context.SaveChangesAsync();
        }
        return true;
    }

    #endregion

    #region Blog Comment operations

    public async Task<IEnumerable<BlogComment>> GetCommentsByBlogIdAsync(int blogId)
    {
        return await _context.BlogComments
            .Include(c => c.User)
            .Where(c => c.BlogId == blogId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<BlogComment>> GetApprovedCommentsByBlogIdAsync(int blogId)
    {
        return await _context.BlogComments
            .Include(c => c.User)
            .Where(c => c.BlogId == blogId && c.IsApproved)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<BlogComment>> GetAllCommentsAsync()
    {
        return await _context.BlogComments
            .Include(c => c.User)
            .Include(c => c.Blog)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<BlogComment?> GetCommentByIdAsync(int commentId)
    {
        return await _context.BlogComments
            .Include(c => c.User)
            .Include(c => c.Blog)
            .FirstOrDefaultAsync(c => c.CommentId == commentId);
    }

    public async Task<BlogComment> CreateCommentAsync(BlogComment comment)
    {
        comment.CreatedAt = DateTime.Now;
        comment.UpdatedAt = DateTime.Now;
        comment.IsApproved = true; // Auto-approve all comments

        _context.BlogComments.Add(comment);
        await _context.SaveChangesAsync();
        
        // Load related data for complete comment object
        await _context.Entry(comment)
            .Reference(c => c.User)
            .LoadAsync();
        await _context.Entry(comment)
            .Reference(c => c.Blog)
            .LoadAsync();
            
        return comment;
    }

    public async Task<bool> ApproveCommentAsync(int commentId)
    {
        var comment = await _context.BlogComments.FindAsync(commentId);
        if (comment == null) return false;

        comment.IsApproved = true;
        comment.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCommentAsync(int commentId)
    {
        var comment = await _context.BlogComments.FindAsync(commentId);
        if (comment == null) return false;

        _context.BlogComments.Remove(comment);
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Helper methods

    public Task<int> CalculateReadTimeAsync(string content)
    {
        // Average reading speed: 200 words per minute
        const int wordsPerMinute = 200;
        
        if (string.IsNullOrEmpty(content)) return Task.FromResult(0);

        // Remove HTML tags and count words
        var plainText = Regex.Replace(content, "<.*?>", string.Empty);
        var wordCount = plainText.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        
        var readTime = Math.Max(1, (int)Math.Ceiling((double)wordCount / wordsPerMinute));
        return Task.FromResult(readTime);
    }

    public Task<string> GenerateSlugAsync(string title)
    {
        if (string.IsNullOrEmpty(title)) return Task.FromResult(string.Empty);

        // Convert to lowercase and replace spaces with hyphens
        var slug = title.ToLower()
            .Replace(" ", "-")
            .Replace("đ", "d")
            .Replace("ă", "a")
            .Replace("â", "a")
            .Replace("ê", "e")
            .Replace("ô", "o")
            .Replace("ơ", "o")
            .Replace("ư", "u")
            .Replace("í", "i")
            .Replace("ì", "i")
            .Replace("ỉ", "i")
            .Replace("ĩ", "i")
            .Replace("ị", "i")
            .Replace("á", "a")
            .Replace("à", "a")
            .Replace("ả", "a")
            .Replace("ã", "a")
            .Replace("ạ", "a")
            .Replace("é", "e")
            .Replace("è", "e")
            .Replace("ẻ", "e")
            .Replace("ẽ", "e")
            .Replace("ẹ", "e")
            .Replace("ó", "o")
            .Replace("ò", "o")
            .Replace("ỏ", "o")
            .Replace("õ", "o")
            .Replace("ọ", "o")
            .Replace("ú", "u")
            .Replace("ù", "u")
            .Replace("ủ", "u")
            .Replace("ũ", "u")
            .Replace("ụ", "u")
            .Replace("ý", "y")
            .Replace("ỳ", "y")
            .Replace("ỷ", "y")
            .Replace("ỹ", "y")
            .Replace("ỵ", "y");

        // Remove special characters except hyphens
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
        
        // Remove duplicate hyphens
        slug = Regex.Replace(slug, @"-+", "-");
        
        // Remove leading and trailing hyphens
        slug = slug.Trim('-');

        return Task.FromResult(slug);
    }

    #endregion
}
