using Microsoft.AspNetCore.Mvc;
using MonAmour.Attributes;
using MonAmour.AuthViewModel;
using MonAmour.Helpers;
using MonAmour.Services.Interfaces;

namespace MonAmour.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly IAuthService _authService;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(IAuthService authService, ILogger<ProfileController> logger)
    {
        _authService = authService;
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
    public async Task<IActionResult> Update(UserViewModel.ProfileViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            var userId = AuthHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
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
