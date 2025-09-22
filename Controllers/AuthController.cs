using Microsoft.AspNetCore.Mvc;
using MonAmour.AuthViewModel;
using MonAmour.Helpers;
using MonAmour.Services.Interfaces;

namespace MonAmour.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var (success, errorMessage) = await _authService.LoginAsync(model);

                if (success)
                {
                    // Check if there's a return URL
                    var returnUrl = HttpContext.Session.GetString("ReturnUrl");
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        HttpContext.Session.Remove("ReturnUrl");
                        return Redirect(returnUrl);
                    }

                    // Role-based redirect
                    var redirectUrl = AuthHelper.GetDefaultRedirectUrl(HttpContext);
                    TempData["SuccessMessage"] = "Đăng nhập thành công!";

                    if (AuthHelper.IsAdmin(HttpContext))
                    {
                        _logger.LogInformation("Admin user logged in: {Email}", model.Email);
                        return Redirect("/Admin/Dashboard"); // Redirect admin to dashboard
                    }
                    else
                    {
                        _logger.LogInformation("Regular user logged in: {Email}", model.Email);
                        return RedirectToAction("Index", "Home"); // Redirect user to home
                    }
                }
                else
                {
                    ModelState.AddModelError("", errorMessage ?? "Đăng nhập thất bại.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", model.Email);
                ModelState.AddModelError("", "Có lỗi xảy ra. Vui lòng thử lại sau.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Signup(SignupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var (success, errorMessage) = await _authService.SignupAsync(model);

                if (success)
                {
                    TempData["SuccessMessage"] = "Đăng ký thành công! Chào mừng bạn đến với trang web của chúng tôiii <3";
                    _logger.LogInformation("User registered successfully: {Email}", model.Email);
                    return View();
                }
                else
                {
                    ModelState.AddModelError("", errorMessage ?? "Đăng ký thất bại.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during signup for email: {Email}", model.Email);
                ModelState.AddModelError("", "Có lỗi xảy ra. Vui lòng thử lại sau. Trân thành cin lỗi bạn về sự bất tiện này");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var (success, errorMessage) = await _authService.ForgotPasswordAsync(model.Email);
                ViewBag.IsSubmitted = true;
                ViewBag.Email = model.Email;

                if (success)
                {
                    ViewBag.SuccessMessage = "Email đặt lại mật khẩu đã được gửi. Vui lòng kiểm tra hộp thư.";
                    _logger.LogInformation("Password reset email sent for: {Email}", model.Email);
                }
                else
                {
                    ViewBag.ErrorMessage = errorMessage ?? "Có lỗi xảy ra. Vui lòng thử lại sau.";
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password for email: {Email}", model.Email);
                ViewBag.IsSubmitted = true;
                ViewBag.Email = model.Email;
                ViewBag.ErrorMessage = "Có lỗi xảy ra. Vui lòng thử lại sau.";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Token không hợp lệ.";
                return RedirectToAction("ForgotPassword");
            }

            try
            {
                var isValid = await _authService.IsTokenValidAsync(token, "reset_password");
                if (!isValid)
                {
                    TempData["ErrorMessage"] = "Link đặt lại mật khẩu đã hết hạn hoặc không hợp lệ.";
                    return RedirectToAction("ForgotPassword");
                }

                var model = new ResetPasswordViewModel { Token = token };
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking reset token: {Token}", token);
                TempData["ErrorMessage"] = "Có lỗi xảy ra. Vui lòng thử lại sau.";
                return RedirectToAction("ForgotPassword");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var (success, errorMessage) = await _authService.ResetPasswordAsync(model);

                if (success)
                {
                    ViewBag.IsSuccess = true;
                    ViewBag.SuccessMessage = "Đặt lại mật khẩu thành công! Bạn có thể đăng nhập với mật khẩu mới.";
                    _logger.LogInformation("Password reset successfully for token: {Token}", model.Token);
                }
                else
                {
                    ViewBag.ErrorMessage = errorMessage ?? "Token không hợp lệ hoặc đã hết hạn. Vui lòng thử lại.";
                }
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset with token: {Token}", model.Token);
                ViewBag.ErrorMessage = "Có lỗi xảy ra. Vui lòng thử lại sau.";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string token, string email)
        {
            var model = new VerifyEmailViewModel { Token = token, Email = email };

            try
            {
                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
                {
                    ViewBag.Status = "error";
                    ViewBag.Message = "Thông tin xác thực không hợp lệ.";
                    return View(model);
                }

                // Kiểm tra email đã được xác thực chưa
                var isVerified = await _authService.IsEmailVerifiedAsync(email);
                if (isVerified)
                {
                    ViewBag.Status = "success";
                    ViewBag.Message = "Email này đã được xác thực trước đó. Bạn có thể đăng nhập vào tài khoản.";
                    return View(model);
                }

                // Kiểm tra token có hợp lệ không
                var isValidToken = await _authService.IsTokenValidAsync(token, "email_verification");
                if (!isValidToken)
                {
                    ViewBag.Status = "expired";
                    ViewBag.Message = "Link xác thực đã hết hạn hoặc không hợp lệ.";
                    return View(model);
                }

                // Xác thực email
                var (success, errorMessage) = await _authService.VerifyEmailAsync(token, email);
                if (success)
                {
                    ViewBag.Status = "success";
                    ViewBag.Message = "Xác thực email thành công! Bạn có thể đăng nhập vào tài khoản.";
                    _logger.LogInformation("Email verified successfully: {Email}", email);
                }
                else
                {
                    ViewBag.Status = "error";
                    ViewBag.Message = errorMessage ?? "Không thể xác thực email. Vui lòng thử lại sau.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email verification for email: {Email}", email);
                ViewBag.Status = "error";
                ViewBag.Message = "Có lỗi xảy ra. Vui lòng thử lại sau.";
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResendVerification(VerifyEmailViewModel model)
        {
            try
            {
                // Kiểm tra email đã được xác thực chưa
                var isVerified = await _authService.IsEmailVerifiedAsync(model.Email);
                if (isVerified)
                {
                    ViewBag.Status = "success";
                    ViewBag.Message = "Email này đã được xác thực trước đó. Bạn có thể đăng nhập vào tài khoản.";
                    return View("VerifyEmail", model);
                }

                var (success, errorMessage) = await _authService.ResendVerificationAsync(model.Email);
                if (success)
                {
                    ViewBag.Status = "resend";
                    ViewBag.Message = "Email xác thực đã được gửi lại. Vui lòng kiểm tra hộp thư.";
                    _logger.LogInformation("Verification email resent for: {Email}", model.Email);
                }
                else
                {
                    ViewBag.Status = "error";
                    ViewBag.Message = errorMessage ?? "Không thể gửi lại email xác thực. Vui lòng thử lại sau.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during resend verification for email: {Email}", model.Email);
                ViewBag.Status = "error";
                ViewBag.Message = "Có lỗi xảy ra. Vui lòng thử lại sau.";
            }

            return View("VerifyEmail", model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _authService.LogoutAsync();

                // Clear session
                AuthHelper.ClearUserSession(HttpContext);

                // Clear remember me cookie
                Response.Cookies.Delete("RememberToken");

                TempData["SuccessMessage"] = "Đăng xuất thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi đăng xuất.";
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
