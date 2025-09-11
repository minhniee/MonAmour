using Microsoft.AspNetCore.Mvc;
using MonAmour.AuthViewModel;
using MonAmour.Helpers;
using MonAmour.Services.Interfaces;

namespace MonAmour.Controllers;

// [Authorize]
[NonController]
public class WishlistController : Controller
{
    private readonly IWishListService _wishlistService;
    private readonly ILogger<WishlistController> _logger;

    public WishlistController(IWishListService wishlistService, ILogger<WishlistController> logger)
    {
        _wishlistService = wishlistService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = AuthHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var wishlist = await _wishlistService.GetUserWishListAsync(userId.Value);
            return View(wishlist);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wishlist");
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách yêu thích.";
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet]
    public IActionResult Add()
    {
        var userId = AuthHelper.GetUserId(HttpContext);
        if (userId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var model = new AddToWishListViewModel
        {
            UserId = userId.Value
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(AddToWishListViewModel model)
    {
        try
        {
            var userId = AuthHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Ensure the user can only add to their own wishlist
            model.UserId = userId.Value;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Validate that either ProductId or ConceptId is provided, but not both
            if (model.ProductId == null && model.ConceptId == null)
            {
                ModelState.AddModelError("", "Vui lòng chọn sản phẩm hoặc ý tưởng để thêm vào danh sách yêu thích.");
                return View(model);
            }

            if (model.ProductId != null && model.ConceptId != null)
            {
                ModelState.AddModelError("", "Chỉ có thể thêm một sản phẩm hoặc một ý tưởng tại một thời điểm.");
                return View(model);
            }

            var result = await _wishlistService.AddToWishListAsync(model);
            TempData["SuccessMessage"] = "Đã thêm vào danh sách yêu thích thành công!";
            return RedirectToAction("Index");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already in the wishlist"))
        {
            ModelState.AddModelError("", "Mục này đã có trong danh sách yêu thích của bạn.");
            return View(model);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to wishlist");
            ModelState.AddModelError("", "Có lỗi xảy ra khi thêm vào danh sách yêu thích.");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Remove()
    {
        var userId = AuthHelper.GetUserId(HttpContext);
        if (userId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var model = new RemoveFromWishListViewModel
        {
            UserId = userId.Value
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(RemoveFromWishListViewModel model)
    {
        try
        {
            var userId = AuthHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Ensure the user can only remove from their own wishlist
            model.UserId = userId.Value;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Validate that either ProductId or ConceptId is provided, but not both
            if (model.ProductId == null && model.ConceptId == null)
            {
                ModelState.AddModelError("", "Vui lòng chọn sản phẩm hoặc ý tưởng để xóa khỏi danh sách yêu thích.");
                return View(model);
            }

            if (model.ProductId != null && model.ConceptId != null)
            {
                ModelState.AddModelError("", "Chỉ có thể xóa một sản phẩm hoặc một ý tưởng tại một thời điểm.");
                return View(model);
            }

            var result = await _wishlistService.RemoveFromWishListAsync(model);
            if (result)
            {
                TempData["SuccessMessage"] = "Đã xóa khỏi danh sách yêu thích thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy mục để xóa khỏi danh sách yêu thích.";
            }

            return RedirectToAction("Index");
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing item from wishlist");
            ModelState.AddModelError("", "Có lỗi xảy ra khi xóa khỏi danh sách yêu thích.");
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveItem(int? productId, int? conceptId)
    {
        try
        {
            var userId = AuthHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = new RemoveFromWishListViewModel
            {
                UserId = userId.Value,
                ProductId = productId,
                ConceptId = conceptId
            };

            var result = await _wishlistService.RemoveFromWishListAsync(model);
            if (result)
            {
                TempData["SuccessMessage"] = "Đã xóa khỏi danh sách yêu thích thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy mục để xóa khỏi danh sách yêu thích.";
            }

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing item from wishlist");
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa khỏi danh sách yêu thích.";
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear()
    {
        try
        {
            var userId = AuthHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var result = await _wishlistService.ClearWishListAsync(userId.Value);
            if (result)
            {
                TempData["SuccessMessage"] = "Đã xóa toàn bộ danh sách yêu thích thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa danh sách yêu thích.";
            }

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing wishlist");
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa danh sách yêu thích.";
            return RedirectToAction("Index");
        }
    }
}
