using Microsoft.EntityFrameworkCore;
using MonAmour.Models;
using MonAmour.Services.Interfaces;
using MonAmour.ViewModels;

namespace MonAmour.Services.Implements
{
    public class BlogManagementService : IBlogManagementService
    {
        private readonly MonAmourDbContext _context;
        private readonly ILogger<BlogManagementService> _logger;
        private readonly IFileUploadService _fileUploadService;

        public BlogManagementService(MonAmourDbContext context, ILogger<BlogManagementService> logger, IFileUploadService fileUploadService)
        {
            _context = context;
            _logger = logger;
            _fileUploadService = fileUploadService;
        }

        #region Blog Methods

        public async Task<List<BlogListViewModel>> GetAllBlogsAsync()
        {
            try
            {
                var blogs = await _context.Blogs
                    .Where(b => b.IsDeleted != true) // Only get non-deleted blogs
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();

                var result = new List<BlogListViewModel>();
                
                foreach (var blog in blogs)
                {
                    // Load author separately
                    var author = blog.AuthorId.HasValue ? 
                        await _context.Users.FindAsync(blog.AuthorId.Value) : null;
                    
                    // Load category separately  
                    var category = blog.CategoryId.HasValue ?
                        await _context.BlogCategories.FindAsync(blog.CategoryId.Value) : null;
                    
                    // Count comments separately
                    var commentCount = await _context.BlogComments
                        .CountAsync(c => c.BlogId == blog.BlogId);
                    
                    result.Add(new BlogListViewModel
                    {
                        BlogId = blog.BlogId,
                        Title = blog.Title,
                        Excerpt = blog.Excerpt,
                        FeaturedImage = blog.FeaturedImage,
                        AuthorName = author?.Name,
                        CategoryName = category?.Name,
                        PublishedDate = blog.PublishedDate,
                        IsFeatured = blog.IsFeatured,
                        IsPublished = blog.IsPublished,
                        ViewCount = blog.ViewCount,
                        ReadTime = blog.ReadTime,
                        CreatedAt = blog.CreatedAt,
                        CommentCount = commentCount
                    });
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all blogs");
                return new List<BlogListViewModel>();
            }
        }

        public async Task<BlogDetailViewModel?> GetBlogByIdAsync(int id)
        {
            try
            {
                var blog = await _context.Blogs
                    .FirstOrDefaultAsync(b => b.BlogId == id);

                if (blog == null) return null;

                // Load related data separately
                var author = blog.AuthorId.HasValue ? 
                    await _context.Users.FindAsync(blog.AuthorId.Value) : null;
                var category = blog.CategoryId.HasValue ?
                    await _context.BlogCategories.FindAsync(blog.CategoryId.Value) : null;
                var comments = await _context.BlogComments
                    .Where(c => c.BlogId == blog.BlogId)
                    .ToListAsync();

                return new BlogDetailViewModel
                {
                    BlogId = blog.BlogId,
                    Title = blog.Title,
                    Content = blog.Content,
                    Excerpt = blog.Excerpt,
                    FeaturedImage = blog.FeaturedImage,
                    AuthorName = author?.Name,
                    CategoryName = category?.Name,
                    Tags = blog.Tags,
                    PublishedDate = blog.PublishedDate,
                    IsFeatured = blog.IsFeatured,
                    IsPublished = blog.IsPublished,
                    ReadTime = blog.ReadTime,
                    ViewCount = blog.ViewCount,
                    CreatedAt = blog.CreatedAt,
                    UpdatedAt = blog.UpdatedAt,
                    Comments = comments
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blog by ID: {BlogId}", id);
                return null;
            }
        }

        public async Task<BlogCreateViewModel> GetCreateBlogViewModelAsync()
        {
            try
            {
                var categories = await _context.BlogCategories
                    .Where(c => c.IsActive == true)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                var authors = await _context.Users
                    .Where(u => u.Status == "active")
                    .OrderBy(u => u.Name)
                    .ToListAsync();

                return new BlogCreateViewModel
                {
                    Categories = categories,
                    Authors = authors,
                    PublishedDate = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting create blog view model");
                return new BlogCreateViewModel();
            }
        }

        public async Task<BlogEditViewModel?> GetEditBlogViewModelAsync(int id)
        {
            try
            {
                var blog = await _context.Blogs.FindAsync(id);
                if (blog == null) return null;

                var categories = await _context.BlogCategories
                    .Where(c => c.IsActive == true)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                var authors = await _context.Users
                    .Where(u => u.Status == "active")
                    .OrderBy(u => u.Name)
                    .ToListAsync();

                return new BlogEditViewModel
                {
                    BlogId = blog.BlogId,
                    Title = blog.Title,
                    Content = blog.Content,
                    Excerpt = blog.Excerpt,
                    FeaturedImage = blog.FeaturedImage,
                    AuthorId = blog.AuthorId,
                    CategoryId = blog.CategoryId,
                    Tags = blog.Tags,
                    PublishedDate = blog.PublishedDate,
                    IsFeatured = blog.IsFeatured ?? false,
                    IsPublished = blog.IsPublished ?? false,
                    ReadTime = blog.ReadTime,
                    Categories = categories,
                    Authors = authors
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting edit blog view model for ID: {BlogId}", id);
                return null;
            }
        }

        public async Task<bool> CreateBlogAsync(BlogCreateViewModel model)
        {
            try
            {
                string? featuredImage = null;
                
                if (model.ImageFile != null)
                {
                    featuredImage = await _fileUploadService.UploadBlogImageAsync(model.ImageFile);
                    if (string.IsNullOrEmpty(featuredImage))
                    {
                        _logger.LogError("Failed to upload blog image");
                        return false;
                    }
                }
                else if (!string.IsNullOrEmpty(model.FeaturedImage))
                {
                    featuredImage = model.FeaturedImage;
                }

                var blog = new Blog
                {
                    Title = model.Title,
                    Content = model.Content,
                    Excerpt = model.Excerpt,
                    FeaturedImage = featuredImage,
                    AuthorId = model.AuthorId,
                    CategoryId = model.CategoryId,
                    Tags = model.Tags,
                    PublishedDate = model.PublishedDate,
                    IsFeatured = model.IsFeatured,
                    IsPublished = model.IsPublished,
                    ReadTime = model.ReadTime,
                    ViewCount = 0,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Blogs.Add(blog);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating blog");
                return false;
            }
        }

        public async Task<bool> UpdateBlogAsync(BlogEditViewModel model)
        {
            try
            {
                var blog = await _context.Blogs.FindAsync(model.BlogId);
                if (blog == null) return false;

                // Handle image update
                if (model.ImageFile != null)
                {
                    var newImageUrl = await _fileUploadService.UpdateBlogImageAsync(model.ImageFile, blog.FeaturedImage);
                    if (!string.IsNullOrEmpty(newImageUrl))
                    {
                        blog.FeaturedImage = newImageUrl;
                    }
                }
                else if (!string.IsNullOrEmpty(model.FeaturedImage))
                {
                    blog.FeaturedImage = model.FeaturedImage;
                }
                // If no new image and no FeaturedImage provided, keep existing FeaturedImage

                blog.Title = model.Title;
                blog.Content = model.Content;
                blog.Excerpt = model.Excerpt;
                blog.AuthorId = model.AuthorId;
                blog.CategoryId = model.CategoryId;
                blog.Tags = model.Tags;
                blog.PublishedDate = model.PublishedDate;
                blog.IsFeatured = model.IsFeatured;
                blog.IsPublished = model.IsPublished;
                blog.ReadTime = model.ReadTime;
                blog.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating blog: {BlogId}", model.BlogId);
                return false;
            }
        }

        public async Task<bool> DeleteBlogAsync(int id)
        {
            try
            {
                var blog = await _context.Blogs.FindAsync(id);
                if (blog == null) return false;

                // Soft delete - mark as deleted instead of removing from database
                blog.IsDeleted = true;
                blog.DeletedAt = DateTime.Now;
                blog.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting blog: {BlogId}", id);
                return false;
            }
        }

        public async Task<bool> ToggleBlogStatusAsync(int id)
        {
            try
            {
                var blog = await _context.Blogs.FindAsync(id);
                if (blog == null) return false;

                blog.IsPublished = !blog.IsPublished;
                blog.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling blog status: {BlogId}", id);
                return false;
            }
        }

        public async Task<bool> ToggleFeaturedStatusAsync(int id)
        {
            try
            {
                var blog = await _context.Blogs.FindAsync(id);
                if (blog == null) return false;

                blog.IsFeatured = !blog.IsFeatured;
                blog.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling featured status: {BlogId}", id);
                return false;
            }
        }

        public async Task<List<BlogListViewModel>> SearchBlogsAsync(string searchTerm, int? categoryId, bool? isPublished)
        {
            try
            {
                var query = _context.Blogs
                    .Where(b => b.IsDeleted != true) // Only get non-deleted blogs
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(b => b.Title.Contains(searchTerm) || 
                                           (b.Excerpt != null && b.Excerpt.Contains(searchTerm)) ||
                                           (b.Tags != null && b.Tags.Contains(searchTerm)));
                }

                if (categoryId.HasValue)
                {
                    query = query.Where(b => b.CategoryId == categoryId.Value);
                }

                if (isPublished.HasValue)
                {
                    query = query.Where(b => b.IsPublished == isPublished.Value);
                }

                var blogs = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();

                return blogs.Select(b => new BlogListViewModel
                {
                    BlogId = b.BlogId,
                    Title = b.Title,
                    Excerpt = b.Excerpt,
                    FeaturedImage = b.FeaturedImage,
                    AuthorName = b.Author?.Name,
                    CategoryName = b.Category?.Name,
                    PublishedDate = b.PublishedDate,
                    IsFeatured = b.IsFeatured,
                    IsPublished = b.IsPublished,
                    ViewCount = b.ViewCount,
                    ReadTime = b.ReadTime,
                    CreatedAt = b.CreatedAt,
                    CommentCount = b.Comments.Count
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching blogs");
                return new List<BlogListViewModel>();
            }
        }

        #endregion

        #region Blog Category Methods

        public async Task<List<BlogCategoryListViewModel>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _context.BlogCategories
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                var result = new List<BlogCategoryListViewModel>();
                
                foreach (var category in categories)
                {
                    // Count blogs for this category (excluding deleted blogs)
                    var blogCount = await _context.Blogs
                        .CountAsync(b => b.CategoryId == category.CategoryId && b.IsDeleted != true);
                    
                    result.Add(new BlogCategoryListViewModel
                    {
                        CategoryId = category.CategoryId,
                        Name = category.Name,
                        Description = category.Description,
                        Slug = category.Slug,
                        IsActive = category.IsActive,
                        CreatedAt = category.CreatedAt,
                        BlogCount = blogCount
                    });
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories");
                return new List<BlogCategoryListViewModel>();
            }
        }

        public async Task<BlogCategory?> GetCategoryByIdAsync(int id)
        {
            try
            {
                return await _context.BlogCategories.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category by ID: {CategoryId}", id);
                return null;
            }
        }

        public async Task<BlogCategoryCreateViewModel> GetCreateCategoryViewModelAsync()
        {
            return new BlogCategoryCreateViewModel();
        }

        public async Task<BlogCategoryEditViewModel?> GetEditCategoryViewModelAsync(int id)
        {
            try
            {
                var category = await _context.BlogCategories.FindAsync(id);
                if (category == null) return null;

                return new BlogCategoryEditViewModel
                {
                    CategoryId = category.CategoryId,
                    Name = category.Name,
                    Description = category.Description,
                    Slug = category.Slug,
                    IsActive = category.IsActive ?? true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting edit category view model for ID: {CategoryId}", id);
                return null;
            }
        }

        public async Task<bool> CreateCategoryAsync(BlogCategoryCreateViewModel model)
        {
            try
            {
                var category = new BlogCategory
                {
                    Name = model.Name,
                    Description = model.Description,
                    Slug = model.Slug,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.BlogCategories.Add(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return false;
            }
        }

        public async Task<bool> UpdateCategoryAsync(BlogCategoryEditViewModel model)
        {
            try
            {
                var category = await _context.BlogCategories.FindAsync(model.CategoryId);
                if (category == null) return false;

                category.Name = model.Name;
                category.Description = model.Description;
                category.Slug = model.Slug;
                category.IsActive = model.IsActive;
                category.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category: {CategoryId}", model.CategoryId);
                return false;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _context.BlogCategories.FindAsync(id);
                if (category == null) return false;

                // Check if category has blogs
                var hasBlogs = await _context.Blogs.AnyAsync(b => b.CategoryId == id);
                if (hasBlogs)
                {
                    return false; // Cannot delete category with blogs
                }

                _context.BlogCategories.Remove(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category: {CategoryId}", id);
                return false;
            }
        }

        public async Task<bool> ToggleCategoryStatusAsync(int id)
        {
            try
            {
                var category = await _context.BlogCategories.FindAsync(id);
                if (category == null) return false;

                category.IsActive = !category.IsActive;
                category.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling category status: {CategoryId}", id);
                return false;
            }
        }

        #endregion

        #region Blog Comment Methods

        public async Task<List<BlogCommentListViewModel>> GetAllCommentsAsync()
        {
            try
            {
                var comments = await _context.BlogComments
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                var result = new List<BlogCommentListViewModel>();
                
                foreach (var comment in comments)
                {
                    var blog = await _context.Blogs.FindAsync(comment.BlogId);
                    var user = comment.UserId.HasValue ? 
                        await _context.Users.FindAsync(comment.UserId.Value) : null;
                    
                    result.Add(new BlogCommentListViewModel
                    {
                        CommentId = comment.CommentId,
                        BlogTitle = blog?.Title ?? "Unknown Blog",
                        AuthorName = comment.AuthorName,
                        AuthorEmail = comment.AuthorEmail,
                        Content = comment.Content,
                        IsApproved = comment.IsApproved,
                        CreatedAt = comment.CreatedAt,
                        UserName = user?.Name
                    });
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all comments");
                return new List<BlogCommentListViewModel>();
            }
        }

        public async Task<BlogComment?> GetCommentByIdAsync(int id)
        {
            try
            {
                return await _context.BlogComments
                    .FirstOrDefaultAsync(c => c.CommentId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comment by ID: {CommentId}", id);
                return null;
            }
        }

        public async Task<BlogCommentEditViewModel?> GetEditCommentViewModelAsync(int id)
        {
            try
            {
                var comment = await _context.BlogComments
                    .FirstOrDefaultAsync(c => c.CommentId == id);

                if (comment == null) return null;

                return new BlogCommentEditViewModel
                {
                    CommentId = comment.CommentId,
                    BlogTitle = comment.Blog.Title,
                    AuthorName = comment.AuthorName,
                    AuthorEmail = comment.AuthorEmail,
                    Content = comment.Content,
                    IsApproved = comment.IsApproved ?? false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting edit comment view model for ID: {CommentId}", id);
                return null;
            }
        }

        public async Task<bool> UpdateCommentAsync(BlogCommentEditViewModel model)
        {
            try
            {
                var comment = await _context.BlogComments.FindAsync(model.CommentId);
                if (comment == null) return false;

                comment.AuthorName = model.AuthorName;
                comment.AuthorEmail = model.AuthorEmail;
                comment.Content = model.Content;
                comment.IsApproved = model.IsApproved;
                comment.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment: {CommentId}", model.CommentId);
                return false;
            }
        }

        public async Task<bool> DeleteCommentAsync(int id)
        {
            try
            {
                var comment = await _context.BlogComments.FindAsync(id);
                if (comment == null) return false;

                _context.BlogComments.Remove(comment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment: {CommentId}", id);
                return false;
            }
        }

        public async Task<bool> ToggleCommentApprovalAsync(int id)
        {
            try
            {
                var comment = await _context.BlogComments.FindAsync(id);
                if (comment == null) return false;

                comment.IsApproved = !comment.IsApproved;
                comment.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling comment approval: {CommentId}", id);
                return false;
            }
        }

        public async Task<List<BlogCommentListViewModel>> SearchCommentsAsync(string searchTerm, bool? isApproved)
        {
            try
            {
                var query = _context.BlogComments.AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(c => c.Content.Contains(searchTerm) ||
                                           (c.AuthorName != null && c.AuthorName.Contains(searchTerm)) ||
                                           (c.AuthorEmail != null && c.AuthorEmail.Contains(searchTerm)) ||
                                           c.Blog.Title.Contains(searchTerm));
                }

                if (isApproved.HasValue)
                {
                    query = query.Where(c => c.IsApproved == isApproved.Value);
                }

                var comments = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();

                return comments.Select(c => new BlogCommentListViewModel
                {
                    CommentId = c.CommentId,
                    BlogTitle = c.Blog.Title,
                    AuthorName = c.AuthorName,
                    AuthorEmail = c.AuthorEmail,
                    Content = c.Content,
                    IsApproved = c.IsApproved,
                    CreatedAt = c.CreatedAt,
                    UserName = c.User?.Name
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching comments");
                return new List<BlogCommentListViewModel>();
            }
        }

        #endregion
    }
}
