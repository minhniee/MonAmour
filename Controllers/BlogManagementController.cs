using Microsoft.AspNetCore.Mvc;
using MonAmour.Services.Interfaces;
using MonAmour.ViewModels;
using MonAmour.Attributes;

namespace MonAmour.Controllers
{
    [SessionAuthorize]
    public class BlogManagementController : Controller
    {
        private readonly IBlogManagementService _blogService;
        private readonly ILogger<BlogManagementController> _logger;

        public BlogManagementController(IBlogManagementService blogService, ILogger<BlogManagementController> logger)
        {
            _blogService = blogService;
            _logger = logger;
        }

        #region Blog Actions

        public async Task<IActionResult> Blogs()
        {
            try
            {
                var blogs = await _blogService.GetAllBlogsAsync();
                return View(blogs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Blogs action");
                return View(new List<BlogListViewModel>());
            }
        }

        public async Task<IActionResult> BlogDetail(int id)
        {
            try
            {
                var blog = await _blogService.GetBlogByIdAsync(id);
                if (blog == null)
                {
                    TempData["Error"] = "Không tìm thấy bài viết";
                    return RedirectToAction("Blogs");
                }
                return View(blog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BlogDetail action for ID: {BlogId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải chi tiết bài viết";
                return RedirectToAction("Blogs");
            }
        }

        public async Task<IActionResult> CreateBlog()
        {
            try
            {
                var model = await _blogService.GetCreateBlogViewModelAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateBlog action");
                TempData["Error"] = "Có lỗi xảy ra khi tải form tạo bài viết";
                return RedirectToAction("Blogs");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBlog(BlogCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model = await _blogService.GetCreateBlogViewModelAsync();
                    return View(model);
                }

                var result = await _blogService.CreateBlogAsync(model);
                if (result)
                {
                    TempData["Success"] = "Tạo bài viết thành công";
                    return RedirectToAction("Blogs");
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi tạo bài viết";
                    model = await _blogService.GetCreateBlogViewModelAsync();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating blog");
                TempData["Error"] = "Có lỗi xảy ra khi tạo bài viết";
                model = await _blogService.GetCreateBlogViewModelAsync();
                return View(model);
            }
        }

        public async Task<IActionResult> EditBlog(int id)
        {
            try
            {
                var model = await _blogService.GetEditBlogViewModelAsync(id);
                if (model == null)
                {
                    TempData["Error"] = "Không tìm thấy bài viết";
                    return RedirectToAction("Blogs");
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EditBlog action for ID: {BlogId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải form chỉnh sửa bài viết";
                return RedirectToAction("Blogs");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBlog(BlogEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model = await _blogService.GetEditBlogViewModelAsync(model.BlogId);
                    return View(model);
                }

                var result = await _blogService.UpdateBlogAsync(model);
                if (result)
                {
                    TempData["Success"] = "Cập nhật bài viết thành công";
                    return RedirectToAction("Blogs");
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật bài viết";
                    model = await _blogService.GetEditBlogViewModelAsync(model.BlogId);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating blog: {BlogId}", model.BlogId);
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật bài viết";
                model = await _blogService.GetEditBlogViewModelAsync(model.BlogId);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBlog(int id)
        {
            try
            {
                var result = await _blogService.DeleteBlogAsync(id);
                if (result)
                {
                    TempData["Success"] = "Xóa bài viết thành công";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi xóa bài viết";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting blog: {BlogId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi xóa bài viết";
            }
            return RedirectToAction("Blogs");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleBlogStatus(int id)
        {
            try
            {
                var result = await _blogService.ToggleBlogStatusAsync(id);
                if (result)
                {
                    TempData["Success"] = "Cập nhật trạng thái bài viết thành công";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái bài viết";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling blog status: {BlogId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái bài viết";
            }
            return RedirectToAction("Blogs");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFeaturedStatus(int id)
        {
            try
            {
                var result = await _blogService.ToggleFeaturedStatusAsync(id);
                if (result)
                {
                    TempData["Success"] = "Cập nhật trạng thái nổi bật thành công";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái nổi bật";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling featured status: {BlogId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái nổi bật";
            }
            return RedirectToAction("Blogs");
        }

        [HttpPost]
        public async Task<IActionResult> SearchBlogs(string searchTerm, int? categoryId, bool? isPublished)
        {
            try
            {
                var blogs = await _blogService.SearchBlogsAsync(searchTerm, categoryId, isPublished);
                return PartialView("_BlogListPartial", blogs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching blogs");
                return PartialView("_BlogListPartial", new List<BlogListViewModel>());
            }
        }

        #endregion

        #region Blog Category Actions

        public async Task<IActionResult> BlogCategories()
        {
            try
            {
                var categories = await _blogService.GetAllCategoriesAsync();
                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BlogCategories action");
                return View(new List<BlogCategoryListViewModel>());
            }
        }

        public async Task<IActionResult> CreateCategory()
        {
            try
            {
                var model = await _blogService.GetCreateCategoryViewModelAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateCategory action");
                TempData["Error"] = "Có lỗi xảy ra khi tải form tạo danh mục";
                return RedirectToAction("BlogCategories");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(BlogCategoryCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _blogService.CreateCategoryAsync(model);
                if (result)
                {
                    TempData["Success"] = "Tạo danh mục thành công";
                    return RedirectToAction("BlogCategories");
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi tạo danh mục";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                TempData["Error"] = "Có lỗi xảy ra khi tạo danh mục";
                return View(model);
            }
        }

        public async Task<IActionResult> EditCategory(int id)
        {
            try
            {
                var model = await _blogService.GetEditCategoryViewModelAsync(id);
                if (model == null)
                {
                    TempData["Error"] = "Không tìm thấy danh mục";
                    return RedirectToAction("BlogCategories");
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EditCategory action for ID: {CategoryId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải form chỉnh sửa danh mục";
                return RedirectToAction("BlogCategories");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(BlogCategoryEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _blogService.UpdateCategoryAsync(model);
                if (result)
                {
                    TempData["Success"] = "Cập nhật danh mục thành công";
                    return RedirectToAction("BlogCategories");
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật danh mục";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category: {CategoryId}", model.CategoryId);
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật danh mục";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var result = await _blogService.DeleteCategoryAsync(id);
                if (result)
                {
                    TempData["Success"] = "Xóa danh mục thành công";
                }
                else
                {
                    TempData["Error"] = "Không thể xóa danh mục có chứa bài viết";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category: {CategoryId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi xóa danh mục";
            }
            return RedirectToAction("BlogCategories");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCategoryStatus(int id)
        {
            try
            {
                var result = await _blogService.ToggleCategoryStatusAsync(id);
                if (result)
                {
                    TempData["Success"] = "Cập nhật trạng thái danh mục thành công";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái danh mục";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling category status: {CategoryId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái danh mục";
            }
            return RedirectToAction("BlogCategories");
        }

        #endregion

        #region Blog Comment Actions

        public async Task<IActionResult> BlogComments()
        {
            try
            {
                var comments = await _blogService.GetAllCommentsAsync();
                return View(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BlogComments action");
                return View(new List<BlogCommentListViewModel>());
            }
        }

        public async Task<IActionResult> EditComment(int id)
        {
            try
            {
                var model = await _blogService.GetEditCommentViewModelAsync(id);
                if (model == null)
                {
                    TempData["Error"] = "Không tìm thấy bình luận";
                    return RedirectToAction("BlogComments");
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EditComment action for ID: {CommentId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải form chỉnh sửa bình luận";
                return RedirectToAction("BlogComments");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditComment(BlogCommentEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _blogService.UpdateCommentAsync(model);
                if (result)
                {
                    TempData["Success"] = "Cập nhật bình luận thành công";
                    return RedirectToAction("BlogComments");
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật bình luận";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment: {CommentId}", model.CommentId);
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật bình luận";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int id)
        {
            try
            {
                var result = await _blogService.DeleteCommentAsync(id);
                if (result)
                {
                    TempData["Success"] = "Xóa bình luận thành công";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi xóa bình luận";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment: {CommentId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi xóa bình luận";
            }
            return RedirectToAction("BlogComments");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCommentApproval(int id)
        {
            try
            {
                var result = await _blogService.ToggleCommentApprovalAsync(id);
                if (result)
                {
                    TempData["Success"] = "Cập nhật trạng thái duyệt bình luận thành công";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái duyệt bình luận";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling comment approval: {CommentId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái duyệt bình luận";
            }
            return RedirectToAction("BlogComments");
        }

        [HttpPost]
        public async Task<IActionResult> SearchComments(string searchTerm, bool? isApproved)
        {
            try
            {
                var comments = await _blogService.SearchCommentsAsync(searchTerm, isApproved);
                return PartialView("_CommentListPartial", comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching comments");
                return PartialView("_CommentListPartial", new List<BlogCommentListViewModel>());
            }
        }

        #endregion

        #region Helper Methods

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _blogService.GetAllCategoriesAsync();
                return Json(categories.Select(c => new { 
                    categoryId = c.CategoryId, 
                    name = c.Name 
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return Json(new List<object>());
            }
        }

        #endregion
    }
}
