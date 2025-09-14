using MonAmour.Models;
using MonAmour.ViewModels;

namespace MonAmour.Services.Interfaces
{
    public interface IBlogManagementService
    {
        // Blog methods
        Task<List<BlogListViewModel>> GetAllBlogsAsync();
        Task<BlogDetailViewModel?> GetBlogByIdAsync(int id);
        Task<BlogCreateViewModel> GetCreateBlogViewModelAsync();
        Task<BlogEditViewModel?> GetEditBlogViewModelAsync(int id);
        Task<bool> CreateBlogAsync(BlogCreateViewModel model);
        Task<bool> UpdateBlogAsync(BlogEditViewModel model);
        Task<bool> DeleteBlogAsync(int id);
        Task<bool> ToggleBlogStatusAsync(int id);
        Task<bool> ToggleFeaturedStatusAsync(int id);
        Task<List<BlogListViewModel>> SearchBlogsAsync(string searchTerm, int? categoryId, bool? isPublished);

        // Blog Category methods
        Task<List<BlogCategoryListViewModel>> GetAllCategoriesAsync();
        Task<BlogCategory?> GetCategoryByIdAsync(int id);
        Task<BlogCategoryCreateViewModel> GetCreateCategoryViewModelAsync();
        Task<BlogCategoryEditViewModel?> GetEditCategoryViewModelAsync(int id);
        Task<bool> CreateCategoryAsync(BlogCategoryCreateViewModel model);
        Task<bool> UpdateCategoryAsync(BlogCategoryEditViewModel model);
        Task<bool> DeleteCategoryAsync(int id);
        Task<bool> ToggleCategoryStatusAsync(int id);
        Task<List<BlogCategoryListViewModel>> SearchCategoriesAsync(string searchTerm, bool? isActive);

        // Blog Comment methods
        Task<List<BlogCommentListViewModel>> GetAllCommentsAsync();
        Task<BlogComment?> GetCommentByIdAsync(int id);
        Task<BlogCommentEditViewModel?> GetEditCommentViewModelAsync(int id);
        Task<bool> UpdateCommentAsync(BlogCommentEditViewModel model);
        Task<bool> DeleteCommentAsync(int id);
        Task<bool> ToggleCommentApprovalAsync(int id);
        Task<List<BlogCommentListViewModel>> SearchCommentsAsync(string searchTerm, bool? isApproved);
    }
}
