using Microsoft.AspNetCore.Mvc;
using MonAmour.Services.Interfaces;
using MonAmour.ViewModels;
using MonAmour.Attributes;
using MonAmour.Helpers;

namespace MonAmour.Controllers
{
    [AdminOnly]
    public class BlogManagementController : Controller
    {
        private readonly IBlogManagementService _blogService;
        private readonly IBlogService _blogHardDeleteService;
        private readonly IUserManagementService _userManagementService;
        private readonly ILogger<BlogManagementController> _logger;

        public BlogManagementController(IBlogManagementService blogService, IBlogService blogHardDeleteService, IUserManagementService userManagementService, ILogger<BlogManagementController> logger)
        {
            _blogService = blogService;
            _blogHardDeleteService = blogHardDeleteService;
            _userManagementService = userManagementService;
            _logger = logger;
        }

        /// <summary>
        /// Helper method to set common ViewBag data for admin pages
        /// </summary>
        private async Task SetAdminViewBagAsync()
        {
            try
            {
                var currentUserId = AuthHelper.GetUserId(HttpContext);
                var currentUser = await _userManagementService.GetUserByIdAsync(currentUserId.Value);

                ViewBag.UserName = AuthHelper.GetUserName(HttpContext);
                ViewBag.UserEmail = AuthHelper.GetUserEmail(HttpContext);
                ViewBag.UserAvatar = currentUser?.Avatar;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting admin ViewBag data");
                ViewBag.UserName = AuthHelper.GetUserName(HttpContext);
                ViewBag.UserEmail = AuthHelper.GetUserEmail(HttpContext);
                ViewBag.UserAvatar = null;
            }
        }

        #region Blog Actions

        public async Task<IActionResult> Blogs()
        {
            try
            {
                await SetAdminViewBagAsync();
                var blogs = await _blogService.GetAllBlogsAsync();
                return View(blogs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Blogs action");
                await SetAdminViewBagAsync();
                return View(new List<BlogListViewModel>());
            }
        }

        public async Task<IActionResult> BlogDetail(int id)
        {
            try
            {
                await SetAdminViewBagAsync();
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
                await SetAdminViewBagAsync();
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
                await SetAdminViewBagAsync();
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
                var result = await _blogHardDeleteService.DeleteBlogAsync(id);
                if (result)
                {
                    return Json(new { success = true, message = "Xóa bài viết thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra khi xóa bài viết" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting blog: {BlogId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa bài viết" });
            }
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
                    return Json(new { success = true, message = "Cập nhật trạng thái bài viết thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật trạng thái bài viết" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling blog status: {BlogId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật trạng thái bài viết" });
            }
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
                    return Json(new { success = true, message = "Cập nhật trạng thái nổi bật thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật trạng thái nổi bật" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling featured status: {BlogId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật trạng thái nổi bật" });
            }
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
                await SetAdminViewBagAsync();
                var categories = await _blogService.GetAllCategoriesAsync();
                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BlogCategories action");
                await SetAdminViewBagAsync();
                return View(new List<BlogCategoryListViewModel>());
            }
        }

        public async Task<IActionResult> CreateCategory()
        {
            try
            {
                await SetAdminViewBagAsync();
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
                    return Json(new { success = true, message = "Xóa danh mục thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể xóa danh mục có chứa bài viết" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category: {CategoryId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa danh mục" });
            }
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
                    return Json(new { success = true, message = "Cập nhật trạng thái danh mục thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật trạng thái danh mục" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling category status: {CategoryId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật trạng thái danh mục" });
            }
        }

        #endregion

        #region Blog Comment Actions

        public async Task<IActionResult> BlogComments()
        {
            try
            {
                await SetAdminViewBagAsync();
                var comments = await _blogService.GetAllCommentsAsync();
                return View(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BlogComments action");
                await SetAdminViewBagAsync();
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
                    return Json(new { success = true, message = "Xóa bình luận thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra khi xóa bình luận" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment: {CommentId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa bình luận" });
            }
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
                    return Json(new { success = true, message = "Cập nhật trạng thái duyệt bình luận thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật trạng thái duyệt bình luận" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling comment approval: {CommentId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật trạng thái duyệt bình luận" });
            }
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
