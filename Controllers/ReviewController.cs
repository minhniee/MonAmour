using Microsoft.AspNetCore.Mvc;
using MonAmour.Attributes;
using MonAmour.AuthViewModel;
using MonAmour.Helpers;
using MonAmour.Services.Interfaces;

namespace MonAmour.Controllers;

[NonController]
[Authorize]
public class ReviewController : Controller
{
    private readonly IReviewService _reviewService;
    private readonly ILogger<ReviewController> _logger;

    public ReviewController(IReviewService reviewService, ILogger<ReviewController> logger)
    {
        _reviewService = reviewService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? targetType, int? targetId, int page = 1, int pageSize = 10)
    {
        try
        {
            if (string.IsNullOrEmpty(targetType) || !targetId.HasValue)
            {
                // Show user's own reviews if no target specified
                var userId = AuthHelper.GetUserId(HttpContext);
                if (userId == null)
                {
                    return RedirectToAction("Login", "Auth");
                }

                var userReviews = await _reviewService.GetUserReviewsAsync(userId.Value, page, pageSize);
                ViewBag.IsUserReviews = true;
                return View(userReviews);
            }

            if (targetType.ToLower() == "product")
            {
                var productReviews = await _reviewService.GetProductReviewsAsync(targetId.Value, page, pageSize);
                ViewBag.IsUserReviews = false;
                ViewBag.TargetType = "Product";
                ViewBag.TargetId = targetId.Value;
                return View(productReviews);
            }
            else if (targetType.ToLower() == "concept")
            {
                var conceptReviews = await _reviewService.GetConceptReviewsAsync(targetId.Value, page, pageSize);
                ViewBag.IsUserReviews = false;
                ViewBag.TargetType = "Concept";
                ViewBag.TargetId = targetId.Value;
                return View(conceptReviews);
            }
            else
            {
                TempData["ErrorMessage"] = "Loại đối tượng không hợp lệ.";
                return RedirectToAction("Index", "Home");
            }
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reviews");
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách đánh giá.";
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet]
    public IActionResult Create(string? targetType, int? targetId)
    {
        var userId = AuthHelper.GetUserId(HttpContext);
        if (userId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var model = new CreateReviewViewModel
        {
            UserId = userId.Value,
            TargetType = targetType ?? "",
            TargetId = targetId ?? 0
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateReviewViewModel model)
    {
        try
        {
            var userId = AuthHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Ensure the user can only create reviews for themselves
            model.UserId = userId.Value;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if user can review this item
            var canReview = await _reviewService.CanUserReviewAsync(model.UserId, model.TargetType, model.TargetId);
            if (!canReview)
            {
                ModelState.AddModelError("", "Bạn đã đánh giá mục này hoặc mục không tồn tại.");
                return View(model);
            }

            var result = await _reviewService.CreateReviewAsync(model);
            TempData["SuccessMessage"] = "Đánh giá đã được tạo thành công!";
            
            return RedirectToAction("Index", new { targetType = model.TargetType, targetId = model.TargetId });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already reviewed"))
        {
            ModelState.AddModelError("", "Bạn đã đánh giá mục này rồi.");
            return View(model);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating review");
            ModelState.AddModelError("", "Có lỗi xảy ra khi tạo đánh giá.");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Update(int id)
    {
        try
        {
            var userId = AuthHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var review = await _reviewService.GetReviewByIdAsync(id);
            
            // Check if the review belongs to the current user
            if (review.UserId != userId.Value)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền chỉnh sửa đánh giá này.";
                return RedirectToAction("Index");
            }

            var model = new UpdateReviewViewModel
            {
                ReviewId = review.ReviewId,
                Rating = review.Rating,
                Comment = review.Comment
            };

            ViewBag.TargetType = review.TargetType;
            ViewBag.TargetId = review.TargetId;
            ViewBag.TargetName = review.TargetName;

            return View(model);
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting review for update");
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải đánh giá.";
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(UpdateReviewViewModel model, string? targetType, int? targetId)
    {
        try
        {
            var userId = AuthHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                // Reload ViewBag data for the view
                try
                {
                    var review = await _reviewService.GetReviewByIdAsync(model.ReviewId);
                    ViewBag.TargetType = review.TargetType;
                    ViewBag.TargetId = review.TargetId;
                    ViewBag.TargetName = review.TargetName;
                }
                catch
                {
                    // If we can't load the review, redirect to index
                    TempData["ErrorMessage"] = "Không tìm thấy đánh giá.";
                    return RedirectToAction("Index");
                }
                return View(model);
            }

            // Verify ownership
            var existingReview = await _reviewService.GetReviewByIdAsync(model.ReviewId);
            if (existingReview.UserId != userId.Value)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền chỉnh sửa đánh giá này.";
                return RedirectToAction("Index");
            }

            var result = await _reviewService.UpdateReviewAsync(model);
            TempData["SuccessMessage"] = "Đánh giá đã được cập nhật thành công!";
            
            return RedirectToAction("Index", new { targetType = targetType, targetId = targetId });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating review");
            ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật đánh giá.");
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string? targetType, int? targetId)
    {
        try
        {
            var userId = AuthHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var result = await _reviewService.DeleteReviewAsync(id, userId.Value);
            if (result)
            {
                TempData["SuccessMessage"] = "Đánh giá đã được xóa thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy đánh giá hoặc bạn không có quyền xóa.";
            }
            
            return RedirectToAction("Index", new { targetType = targetType, targetId = targetId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting review");
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa đánh giá.";
            return RedirectToAction("Index", new { targetType = targetType, targetId = targetId });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Summary()
    {
        try
        {
            var summary = await _reviewService.GetReviewSummaryAsync();
            return View(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting review summary");
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải tổng quan đánh giá.";
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet]
    public async Task<IActionResult> MyReviews(int page = 1, int pageSize = 10)
    {
        try
        {
            var userId = AuthHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userReviews = await _reviewService.GetUserReviewsAsync(userId.Value, page, pageSize);
            return View("Index", userReviews);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user reviews");
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải đánh giá của bạn.";
            return RedirectToAction("Index", "Home");
        }
    }
}
