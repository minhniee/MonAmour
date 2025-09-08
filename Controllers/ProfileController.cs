using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonAmour.Attributes;
using MonAmour.AuthViewModel;
using MonAmour.Helpers;
using MonAmour.Models;
using MonAmour.Services.Interfaces;
using MonAmour.ViewModels;

namespace MonAmour.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly IAuthService _authService;
    private readonly ILogger<ProfileController> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly MonAmourDbContext _db;
    private readonly IReviewService _reviewService;

    public ProfileController(IAuthService authService, ILogger<ProfileController> logger, IWebHostEnvironment environment, MonAmourDbContext db, IReviewService reviewService)
    {
        _authService = authService;
        _logger = logger;
        _environment = environment;
        _db = db;
        _reviewService = reviewService;
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

            var user = await _authService.GetUserByIdAsync(userId.Value);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = new UserViewModel.ProfileViewModel
            {
                Name = user.Name ?? "",
                Email = user.Email,
                Phone = user.Phone ?? "",
                Avatar = user.Avatar,
                BirthDate = user.BirthDate,
                Gender = user.Gender ?? ""
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting user profile");
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin profile.";
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Update(UserViewModel.ProfileViewModel model, IFormFile? avatar)
    {
        try
        {
            var userId = AuthHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            _logger.LogInformation("Update with {UserId}", userId.Value);

            // Load current user to backfill required fields when disabled inputs are not posted
            var currentUser = await _authService.GetUserByIdAsync(userId.Value);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Ensure immutable email stays consistent
            model.Email = currentUser.Email;

            // Backfill required fields if missing
            if (string.IsNullOrWhiteSpace(model.Name)) model.Name = currentUser.Name ?? string.Empty;
            if (string.IsNullOrWhiteSpace(model.Phone)) model.Phone = currentUser.Phone ?? string.Empty;
            if (model.BirthDate == null) model.BirthDate = currentUser.BirthDate;
            if (string.IsNullOrWhiteSpace(model.Gender)) model.Gender = currentUser.Gender ?? string.Empty;

            // Handle avatar upload (max 20MB, png/jpg/jpeg), save as wwwroot/avatars/{userId}.{ext}
            if (avatar != null && avatar.Length > 0)
            {
                const long maxBytes = 20L * 1024 * 1024; // 20MB
                if (avatar.Length > maxBytes)
                {
                    ModelState.AddModelError("", "Ảnh đại diện vượt quá dung lượng tối đa 20MB.");
                    return View("Index", model);
                }

                var ext = Path.GetExtension(avatar.FileName).ToLowerInvariant();
                var allowed = new[] { ".png", ".jpg", ".jpeg" };
                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("", "Định dạng ảnh không hợp lệ. Chỉ hỗ trợ PNG/JPG/JPEG.");
                    return View("Index", model);
                }

                var avatarsDir = Path.Combine(_environment.WebRootPath, "avatars");
                if (!Directory.Exists(avatarsDir))
                {
                    Directory.CreateDirectory(avatarsDir);
                }

                var fileName = $"{userId.Value}{ext}";
                var filePath = Path.Combine(avatarsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await avatar.CopyToAsync(stream);
                }

                model.Avatar = $"/avatars/{fileName}";
            }
            else
            {
                // Keep existing avatar if not uploading a new one
                model.Avatar = currentUser.Avatar;
            }
            _logger.LogInformation("Avatar model: {@Model}", model.Avatar);

            // Re-validate after backfilling
            ModelState.Clear();
            if (!TryValidateModel(model))
            {
                return View("Index", model);
            }
            var result = await _authService.UpdateProfileAsync(userId.Value, model);
            if (result)
            {
                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Không thể cập nhật thông tin. Vui lòng thử lại.");
                return View("Index", model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating user profile");
            ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật thông tin.");
            return View("Index", model);
        }
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View(new UserViewModel.ChangePasswordViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(UserViewModel.ChangePasswordViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = AuthHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var result = await _authService.ChangePasswordAsync(userId.Value, model.CurrentPassword, model.NewPassword);
            if (result)
            {
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Mật khẩu hiện tại không đúng.");
                return View(model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while changing password");
            ModelState.AddModelError("", "Có lỗi xảy ra khi đổi mật khẩu.");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> OrderHistory()
    {
        try
        {
            var userId = AuthHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Get all reviews for this user's orders
            var reviews = await _db.Reviews
                .Where(r => r.UserId == userId.Value)
                .Select(r => new { r.ReviewId, r.TargetId, r.TargetType })
                .ToListAsync();

            // Load gift box orders (Order + OrderItems + Product)
            var giftBoxOrders = await _db.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)!.ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId.Value)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            // Load concept bookings as another category
            var conceptBookings = await _db.Bookings
                .AsNoTracking()
                .Include(b => b.Concept)
                .Where(b => b.UserId == userId.Value)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var model = new OrderHistoryUserViewModel
            {
                Categories = new List<OrderCategoryUserViewModel>()
            };

            var giftBoxCategory = new OrderCategoryUserViewModel
            {
                Name = "Gift Box",
                Orders = giftBoxOrders.Select(o =>
                {
                    // Find review for first product in order
                    var firstProduct = o.OrderItems.FirstOrDefault()?.ProductId;
                    var review = firstProduct != null ? reviews.FirstOrDefault(r =>
                        r.TargetType == "Product" && r.TargetId == firstProduct) : null;

                    return new OrderSummaryUserViewModel
                    {
                        OrderDate = o.CreatedAt ?? DateTime.Now,
                        OrderNumber = $"ORD-{o.OrderId}",
                        Status = NormalizeStatus(o.Status),
                        CanReview = string.Equals(NormalizeStatus(o.Status), "Completed", StringComparison.OrdinalIgnoreCase),
                        HasReview = review != null,
                        ReviewId = review?.ReviewId,
                        TotalAmount = (o.TotalPrice ?? 0) + (o.ShippingCost ?? 0),
                        Items = o.OrderItems.Select(oi => new OrderItemUserViewModel
                        {
                            ItemId = oi.OrderItemId,
                            ItemType = "Product",
                            TargetId = oi.ProductId ?? 0,
                            Name = oi.Product?.Name ?? "",
                            Quantity = oi.Quantity ?? 0,
                            UnitPrice = oi.UnitPrice ?? 0m,
                            TotalPrice = oi.TotalPrice
                        }).ToList()
                    };
                }).ToList()
            };

            var conceptCategory = new OrderCategoryUserViewModel
            {
                Name = "Concept",
                Orders = conceptBookings.Select(b =>
                {
                    var review = reviews.FirstOrDefault(r =>
                        r.TargetType == "Concept" && r.TargetId == b.ConceptId);

                    return new OrderSummaryUserViewModel
                    {
                        OrderDate = b.CreatedAt ?? DateTime.Now,
                        OrderNumber = $"BK-{b.BookingId}",
                        Status = NormalizeStatus(b.Status),
                        CanReview = string.Equals(NormalizeStatus(b.Status), "Completed", StringComparison.OrdinalIgnoreCase),
                        HasReview = review != null,
                        ReviewId = review?.ReviewId,
                        TotalAmount = b.TotalPrice ?? 0m,
                        Items = new List<OrderItemUserViewModel>
                        {
                            new OrderItemUserViewModel
                            {
                                ItemId = b.BookingId,
                                ItemType = "Concept",
                                TargetId = b.ConceptId ?? 0,
                                Name = b.Concept?.Name ?? "",
                                Quantity = 1,
                                UnitPrice = b.TotalPrice ?? 0m,
                                TotalPrice = b.TotalPrice ?? 0m
                            }
                        }
                    };
                }).ToList()
            };

            model.Categories.Add(conceptCategory);
            model.Categories.Add(giftBoxCategory);

            return View("OrderHistory", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading order history");
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải lịch sử đơn hàng.";
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateReview(int reviewId, int rating, string? comment)
    {
        try
        {
            var userId = AuthHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (rating < 1 || rating > 5)
            {
                TempData["ErrorMessage"] = "Điểm đánh giá phải từ 1 đến 5.";
                return RedirectToAction("OrderHistory");
            }

            var dto = new UpdateReviewViewModel
            {
                ReviewId = reviewId,
                Rating = rating,
                Comment = string.IsNullOrWhiteSpace(comment) ? null : comment
            };

            await _reviewService.UpdateReviewAsync(dto);
            TempData["SuccessMessage"] = "Cập nhật đánh giá thành công!";
            return RedirectToAction("OrderHistory");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating review {ReviewId}", reviewId);
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật đánh giá.";
            return RedirectToAction("OrderHistory");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitReview(int targetId, string targetType, int rating, string? comment)
    {
        try
        {
            var userId = AuthHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (rating < 1 || rating > 5)
            {
                TempData["ErrorMessage"] = "Điểm đánh giá phải từ 1 đến 5.";
                return RedirectToAction("OrderHistory");
            }

            // Ensure the user can review this target
            var canReview = await _reviewService.CanUserReviewAsync(userId.Value, targetType, targetId);
            if (!canReview)
            {
                TempData["ErrorMessage"] = "Bạn không thể đánh giá mục này.";
                return RedirectToAction("OrderHistory");
            }

            var dto = new CreateReviewViewModel
            {
                UserId = userId.Value,
                TargetType = targetType,
                TargetId = targetId,
                Rating = rating,
                Comment = string.IsNullOrWhiteSpace(comment) ? null : comment
            };

            await _reviewService.CreateReviewAsync(dto);
            TempData["SuccessMessage"] = "Gửi đánh giá thành công!";
            return RedirectToAction("OrderHistory");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting review for {TargetType} {TargetId}", targetType, targetId);
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi gửi đánh giá.";
            return RedirectToAction("OrderHistory");
        }
    }

    [HttpGet]
    public async Task<IActionResult> OrderDetail(string orderNumber)
    {
        try
        {
            var userId = AuthHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Parse order number to get type and ID
            var parts = orderNumber.Split('-');
            if (parts.Length != 2)
            {
                TempData["ErrorMessage"] = "Mã đơn hàng không hợp lệ.";
                return RedirectToAction("OrderHistory");
            }

            var type = parts[0].ToUpperInvariant();
            if (!int.TryParse(parts[1], out var id))
            {
                TempData["ErrorMessage"] = "Mã đơn hàng không hợp lệ.";
                return RedirectToAction("OrderHistory");
            }

            OrderDetailUserViewModel? model = null;

            if (type == "ORD") // Gift Box Order
            {
                var order = await _db.Orders
                    .AsNoTracking()
                    .Include(o => o.OrderItems)!
                        .ThenInclude(oi => oi.Product)
                    .Include(o => o.PaymentDetails)
                        .ThenInclude(pd => pd.Payment)
                        .ThenInclude(p => p!.PaymentMethod)
                    .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                    return RedirectToAction("OrderHistory");
                }

                model = new OrderDetailUserViewModel
                {
                    OrderNumber = orderNumber,
                    OrderDate = order.CreatedAt ?? DateTime.Now,
                    Status = NormalizeStatus(order.Status),
                    TotalAmount = order.TotalPrice ?? 0,
                    ShippingCost = order.ShippingCost,
                    ShippingAddress = order.ShippingAddress,
                    PaymentMethod = order.PaymentDetails.FirstOrDefault()?.Payment?.PaymentMethod?.Name,
                    PaymentStatus = order.PaymentDetails.FirstOrDefault()?.Payment?.Status,
                    PaymentDate = order.PaymentDetails.FirstOrDefault()?.Payment?.ProcessedAt,
                    TransactionId = order.PaymentDetails.FirstOrDefault()?.PaymentId.ToString(),
                    Items = order.OrderItems.Select(oi => new OrderItemUserViewModel
                    {
                        ItemId = oi.OrderItemId,
                        ItemType = "Product",
                        TargetId = oi.ProductId ?? 0,
                        Name = oi.Product?.Name ?? "",
                        Quantity = oi.Quantity ?? 0,
                        UnitPrice = oi.UnitPrice ?? 0m,
                        TotalPrice = (order.ShippingCost ?? 0) + (order.TotalPrice ?? 0)
                    }).ToList()
                };
            }
            else if (type == "BK") // Concept Booking
            {
                var booking = await _db.Bookings
                    .AsNoTracking()
                    .Include(b => b.Concept)
                    .Include(b => b.PaymentDetails)
                        .ThenInclude(pd => pd.Payment)
                        .ThenInclude(p => p!.PaymentMethod)
                    .FirstOrDefaultAsync(b => b.BookingId == id && b.UserId == userId);

                if (booking == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                    return RedirectToAction("OrderHistory");
                }

                model = new OrderDetailUserViewModel
                {
                    OrderNumber = orderNumber,
                    OrderDate = booking.CreatedAt ?? DateTime.Now,
                    Status = NormalizeStatus(booking.Status),
                    TotalAmount = booking.TotalPrice ?? 0m,
                    PaymentMethod = booking.PaymentDetails.FirstOrDefault()?.Payment?.PaymentMethod?.Name,
                    PaymentStatus = booking.PaymentDetails.FirstOrDefault()?.Payment?.Status,
                    PaymentDate = booking.PaymentDetails.FirstOrDefault()?.Payment?.ProcessedAt,
                    TransactionId = booking.PaymentDetails.FirstOrDefault()?.PaymentId.ToString(),
                    Items = new List<OrderItemUserViewModel>
                    {
                        new OrderItemUserViewModel
                        {
                            ItemId = booking.BookingId,
                            ItemType = "Concept",
                            TargetId = booking.ConceptId ?? 0,
                            Name = booking.Concept?.Name ?? "",
                            Quantity = 1,
                            UnitPrice = booking.TotalPrice ?? 0m,
                            TotalPrice = booking.TotalPrice ?? 0m
                        }
                    }
                };
            }
            else
            {
                TempData["ErrorMessage"] = "Loại đơn hàng không hợp lệ.";
                return RedirectToAction("OrderHistory");
            }

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading order detail for {OrderNumber}", orderNumber);
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải chi tiết đơn hàng.";
            return RedirectToAction("OrderHistory");
        }
    }

    private static string NormalizeStatus(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "Confirmed";
        var val = raw.Trim().ToLowerInvariant();
        return val switch
        {
            "confirmed" => "Confirmed",
            "shipping" => "Shipping",
            "completed" => "Completed",
            "canceled" => "Canceled",
            "cancelled" => "Canceled",
            _ => "Confirmed"
        };
    }
}