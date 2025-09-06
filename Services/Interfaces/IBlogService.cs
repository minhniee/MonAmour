using MonAmour.Models;

namespace MonAmour.Services.Interfaces;

public interface IBlogService
{
    // Blog CRUD operations
    Task<IEnumerable<Blog>> GetAllBlogsAsync();
    Task<IEnumerable<Blog>> GetPublishedBlogsAsync();
    Task<IEnumerable<Blog>> GetFeaturedBlogsAsync();
    Task<Blog?> GetBlogByIdAsync(int id);
    Task<Blog?> GetBlogByIdWithDetailsAsync(int id);
    Task<IEnumerable<Blog>> GetBlogsByCategoryAsync(int categoryId);
    Task<IEnumerable<Blog>> SearchBlogsAsync(string searchTerm);
    Task<Blog> CreateBlogAsync(Blog blog);
    Task<Blog?> UpdateBlogAsync(Blog blog);
    Task<bool> DeleteBlogAsync(int id);
    Task<bool> PublishBlogAsync(int id);
    Task<bool> UnpublishBlogAsync(int id);
    Task<bool> SetFeaturedAsync(int id, bool isFeatured);
    Task<bool> IncrementViewCountAsync(int id);

    // Blog Category operations
    Task<IEnumerable<BlogCategory>> GetAllCategoriesAsync();
    Task<IEnumerable<BlogCategory>> GetActiveCategoriesAsync();
    Task<BlogCategory?> GetCategoryByIdAsync(int id);
    Task<BlogCategory> CreateCategoryAsync(BlogCategory category);
    Task<BlogCategory?> UpdateCategoryAsync(BlogCategory category);
    Task<bool> DeleteCategoryAsync(int id);

    // Blog Comment operations
    Task<IEnumerable<BlogComment>> GetCommentsByBlogIdAsync(int blogId);
    Task<IEnumerable<BlogComment>> GetApprovedCommentsByBlogIdAsync(int blogId);
    Task<IEnumerable<BlogComment>> GetAllCommentsAsync(); // For admin
    Task<BlogComment> CreateCommentAsync(BlogComment comment);
    Task<BlogComment?> GetCommentByIdAsync(int commentId);
    Task<bool> ApproveCommentAsync(int commentId);
    Task<bool> DeleteCommentAsync(int commentId);

    // Helper methods
    Task<int> CalculateReadTimeAsync(string content);
    Task<string> GenerateSlugAsync(string title);
}
