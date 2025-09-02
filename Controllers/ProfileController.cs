using Microsoft.AspNetCore.Mvc;
using MonAmour.Attributes;
using Microsoft.AspNetCore.Hosting;
using MonAmour.AuthViewModel;
using MonAmour.Helpers;
using MonAmour.Services.Interfaces;

namespace MonAmour.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly IAuthService _authService;
    private readonly ILogger<ProfileController> _logger;
    private readonly IWebHostEnvironment _environment;

    public ProfileController(IAuthService authService, ILogger<ProfileController> logger, IWebHostEnvironment environment)
    {
        _authService = authService;
        _logger = logger;
        _environment = environment;
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
}
