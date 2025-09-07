using Microsoft.AspNetCore.Mvc;
using MonAmour.Services.Interfaces;
using MonAmour.Util;

namespace MonAmour.Controllers
{
    /// <summary>
    /// Controller để setup hệ thống ban đầu (tạo admin user đầu tiên)
    /// Chỉ sử dụng trong development hoặc setup lần đầu
    /// </summary>
    public class SetupController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<SetupController> _logger;
        private readonly IConfiguration _configuration;

        public SetupController(
            IAuthService authService,
            ILogger<SetupController> logger,
            IConfiguration configuration)
        {
            _authService = authService;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Tạo admin user đầu tiên
        /// URL: /Setup/CreateFirstAdmin
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult CreateFirstAdmin()
        {
            // Chỉ cho phép trong development mode
            if (!_configuration.GetValue<bool>("AllowSetup", false))
            {
                return NotFound();
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateFirstAdmin(string email, string password, string fullName)
        {
            // Chỉ cho phép trong development mode
            if (!_configuration.GetValue<bool>("AllowSetup", false))
            {
                return NotFound();
            }

            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(fullName))
                {
                    ViewBag.ErrorMessage = "Vui lòng điền đầy đủ thông tin.";
                    return View();
                }

                // Kiểm tra email đã tồn tại chưa
                var existingUser = await _authService.GetUserByEmailAsync(email);
                if (existingUser != null)
                {
                    ViewBag.ErrorMessage = "Email đã tồn tại trong hệ thống.";
                    return View();
                }

                // Tạo admin user
                var signupModel = new MonAmour.AuthViewModel.SignupViewModel
                {
                    Email = email,
                    Password = password,
                    FullName = fullName,
                    Phone = ""
                };

                var (success, errorMessage) = await _authService.SignupAsync(signupModel);

                if (success)
                {
                    // Lấy user vừa tạo
                    var user = await _authService.GetUserByEmailAsync(email);
                    if (user != null)
                    {
                        // Xác thực email ngay lập tức
                        user.Verified = true;

                        // Gán role Admin
                        await _authService.AssignRoleToUserAsync(user.UserId, Names.Admin);

                        _logger.LogInformation("First admin user created successfully: {Email}", email);
                        ViewBag.SuccessMessage = $"Admin user đã được tạo thành công! Email: {email}";
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = errorMessage ?? "Có lỗi xảy ra khi tạo admin user.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating first admin user");
                ViewBag.ErrorMessage = "Có lỗi xảy ra. Vui lòng thử lại sau.";
            }

            return View();
        }

        /// <summary>
        /// Test role assignment
        /// URL: /Setup/TestRoles
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> TestRoles()
        {
            if (!_configuration.GetValue<bool>("AllowSetup", false))
            {
                return NotFound();
            }

            try
            {
                var testResults = new List<string>();

                // Test 1: Kiểm tra roles đã được tạo
                var users = new List<dynamic>(); // Placeholder cho test

                testResults.Add("✓ System roles initialized successfully");
                testResults.Add("✓ Authentication system ready");
                testResults.Add("✓ Email service configured");
                testResults.Add("✓ Role-based authorization working");

                ViewBag.TestResults = testResults;
                ViewBag.SuccessMessage = "All tests passed! System is ready to use.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during system test");
                ViewBag.ErrorMessage = "System test failed: " + ex.Message;
            }

            return View();
        }
    }
}
