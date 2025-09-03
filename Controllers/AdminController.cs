using Microsoft.AspNetCore.Mvc;
using MonAmour.Attributes;
using MonAmour.Helpers;
using MonAmour.Services.Interfaces;
using MonAmour.ViewModels;

namespace MonAmour.Controllers
{
    [AdminOnly]
    public class AdminController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IUserManagementService _userManagementService;
        private readonly IProductService _productService;
        private readonly ILogger<AdminController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(IAuthService authService, IUserManagementService userManagementService, IProductService productService, ILogger<AdminController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _authService = authService;
            _userManagementService = userManagementService;
            _productService = productService;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
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

        /// <summary>
        /// Admin Dashboard - chỉ admin mới có thể truy cập
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Dashboard()
        {
            _logger.LogInformation("Admin user accessed dashboard: {UserId}", 
                AuthHelper.GetUserId(HttpContext));

            try
            {
                // Set common admin ViewBag data
                await SetAdminViewBagAsync();

                // Get user statistics
                var userStats = await _userManagementService.GetUserStatisticsAsync();
                ViewBag.UserStats = userStats;

                // Get product statistics
                var productStats = await _productService.GetProductStatisticsAsync();
                ViewBag.ProductStats = productStats;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                await SetAdminViewBagAsync();
                ViewBag.UserStats = new Dictionary<string, int>();
                ViewBag.ProductStats = new Dictionary<string, int>
                {
                    ["TotalProducts"] = 0,
                    ["ActiveProducts"] = 0,
                    ["OutOfStock"] = 0,
                    ["TotalCategories"] = 0
                };
                return View();
            }
        }

        /// <summary>
        /// User Management - quản lý users
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Users(AdminUserViewModel.UserSearchViewModel? searchModel = null)
        {
            try
            {
                // Set common admin ViewBag data
                await SetAdminViewBagAsync();

                searchModel ??= new AdminUserViewModel.UserSearchViewModel();
                var (users, totalCount) = await _userManagementService.GetUsersAsync(searchModel);
                var roles = await _userManagementService.GetAllRolesAsync();
                var statistics = await _userManagementService.GetUserStatisticsAsync();

                ViewBag.Roles = roles ?? new List<AdminUserViewModel.RoleViewModel>();
                ViewBag.Statistics = statistics ?? new Dictionary<string, int>
                {
                    ["TotalUsers"] = 0,
                    ["VerifiedUsers"] = 0,
                    ["ActiveUsers"] = 0,
                    ["AdminUsers"] = 0
                };
                ViewBag.TotalCount = totalCount;
                ViewBag.CurrentPage = searchModel.Page;
                ViewBag.PageSize = searchModel.PageSize;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / searchModel.PageSize);

                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Users action");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách người dùng";
                
                // Set default ViewBag values for error case
                await SetAdminViewBagAsync();
                ViewBag.Roles = new List<AdminUserViewModel.RoleViewModel>();
                ViewBag.Statistics = new Dictionary<string, int>
                {
                    ["TotalUsers"] = 0,
                    ["VerifiedUsers"] = 0,
                    ["ActiveUsers"] = 0,
                    ["AdminUsers"] = 0
                };
                ViewBag.TotalCount = 0;
                ViewBag.CurrentPage = 1;
                ViewBag.PageSize = 10;
                ViewBag.TotalPages = 0;
                
                return View(new List<AdminUserViewModel.UserListViewModel>());
            }
        }

        /// <summary>
        /// Create User - tạo user mới
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> CreateUser()
        {
            try
            {
                await SetAdminViewBagAsync();
                var roles = await _userManagementService.GetAllRolesAsync();
                ViewBag.Roles = roles;
                return View(new AdminUserViewModel.UserCreateViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateUser GET action");
                TempData["Error"] = "Có lỗi xảy ra khi tải trang tạo người dùng";
                return RedirectToAction(nameof(Users));
            }
        }

        /// <summary>
        /// Create User POST - xử lý tạo user
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(IFormFile? AvatarFile, AdminUserViewModel.UserCreateViewModel model)
        {
            try
            {
                _logger.LogInformation("CreateUser POST called with model: Email={Email}, Name={Name}, Phone={Phone}",
                    model.Email, model.Name, model.Phone);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    _logger.LogWarning("ModelState validation failed: {Errors}", string.Join(", ", errors));

                    await SetAdminViewBagAsync();
                    var roles = await _userManagementService.GetAllRolesAsync();
                    ViewBag.Roles = roles;
                    return View(model);
                }

                var adminUserId = AuthHelper.GetUserId(HttpContext);
                if (!adminUserId.HasValue)
                {
                    _logger.LogError("AdminUserId is null");
                    TempData["Error"] = "Không thể xác định người dùng admin";
                    await SetAdminViewBagAsync();
                    var roles = await _userManagementService.GetAllRolesAsync();
                    ViewBag.Roles = roles;
                    return View(model);
                }

                // Check for duplicates before creating
                var (emailExists, phoneExists) = await _userManagementService.CheckDuplicateAsync(model.Email, model.Phone);

                if (emailExists)
                {
                    _logger.LogWarning("Email already exists: {Email}", model.Email);
                    TempData["Error"] = "Email đã tồn tại trong hệ thống";
                    ModelState.AddModelError("Email", "Email đã tồn tại trong hệ thống");
                    await SetAdminViewBagAsync();
                    var roles = await _userManagementService.GetAllRolesAsync();
                    ViewBag.Roles = roles;
                    return View(model);
                }

                if (phoneExists)
                {
                    _logger.LogWarning("Phone already exists: {Phone}", model.Phone);
                    TempData["Error"] = "Số điện thoại đã tồn tại trong hệ thống";
                    ModelState.AddModelError("Phone", "Số điện thoại đã tồn tại trong hệ thống");
                    await SetAdminViewBagAsync();
                    var roles = await _userManagementService.GetAllRolesAsync();
                    ViewBag.Roles = roles;
                    return View(model);
                }

                // Xử lý file upload avatar nếu có
                if (AvatarFile != null && AvatarFile.Length > 0)
                {
                    // Kiểm tra kích thước file (5MB)
                    if (AvatarFile.Length > 5 * 1024 * 1024)
                    {
                        TempData["Error"] = "File avatar quá lớn. Kích thước tối đa là 5MB.";
                        await SetAdminViewBagAsync();
                        var roles = await _userManagementService.GetAllRolesAsync();
                        ViewBag.Roles = roles;
                        return View(model);
                    }

                    // Kiểm tra loại file
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(AvatarFile.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        TempData["Error"] = "Chỉ hỗ trợ file JPG, PNG, GIF cho avatar.";
                        await SetAdminViewBagAsync();
                        var roles = await _userManagementService.GetAllRolesAsync();
                        ViewBag.Roles = roles;
                        return View(model);
                    }

                    // Tạo tên file duy nhất
                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    
                    // Sử dụng đường dẫn tương đối từ thư mục gốc của ứng dụng
                    var uploadPath = Path.Combine(_webHostEnvironment.ContentRootPath, "Imagine", "Avatars");
                    
                    // Tạo thư mục nếu chưa tồn tại
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    var filePath = Path.Combine(uploadPath, fileName);
                    
                    // Lưu file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await AvatarFile.CopyToAsync(stream);
                    }

                    // Cập nhật Avatar URL trong model
                    model.Avatar = $"/Imagine/Avatars/{fileName}";
                }

                var result = await _userManagementService.CreateUserAsync(model, adminUserId.Value);

                if (result)
                {
                    _logger.LogInformation("User created successfully");
                    TempData["Success"] = "Tạo người dùng thành công";
                    return RedirectToAction(nameof(Users));
                }
                else
                {
                    _logger.LogWarning("CreateUserAsync returned false");
                    TempData["Error"] = "Có lỗi xảy ra khi tạo người dùng. Vui lòng kiểm tra lại thông tin.";
                    await SetAdminViewBagAsync();
                    var roles = await _userManagementService.GetAllRolesAsync();
                    ViewBag.Roles = roles;
                    return View(model);
                }
            }
            catch (InvalidOperationException ex) when (ex.Message == "EMAIL_EXISTS")
            {
                TempData["Error"] = "Email đã tồn tại trong hệ thống";
                ModelState.AddModelError("Email", "Email đã tồn tại trong hệ thống");
                await SetAdminViewBagAsync();
                var roles = await _userManagementService.GetAllRolesAsync();
                ViewBag.Roles = roles;
                return View(model);
            }
            catch (InvalidOperationException ex) when (ex.Message == "PHONE_EXISTS")
            {
                TempData["Error"] = "Số điện thoại đã tồn tại trong hệ thống";
                ModelState.AddModelError("Phone", "Số điện thoại đã tồn tại trong hệ thống");
                await SetAdminViewBagAsync();
                var roles = await _userManagementService.GetAllRolesAsync();
                ViewBag.Roles = roles;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateUser POST action");
                TempData["Error"] = $"Có lỗi xảy ra khi tạo người dùng: {ex.Message}";
                await SetAdminViewBagAsync();
                var roles = await _userManagementService.GetAllRolesAsync();
                ViewBag.Roles = roles;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckDuplicate(string email, string phone, int? excludeUserId = null)
        {
            try
            {
                var (emailExists, phoneExists) = await _userManagementService.CheckDuplicateAsync(email, phone, excludeUserId);

                return Json(new
                {
                    emailExists = emailExists,
                    phoneExists = phoneExists,
                    email = email,
                    phone = phone,
                    message = emailExists || phoneExists ? "Có dữ liệu trùng lặp" : "Dữ liệu hợp lệ"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking duplicates: email={Email}, phone={Phone}", email, phone);
                return Json(new { error = ex.Message });
            }
        }


        /// <summary>
        /// Edit User - chỉnh sửa user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditUser(int id)
        {
            try
            {
                await SetAdminViewBagAsync();
                var user = await _userManagementService.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "Không tìm thấy người dùng";
                    return RedirectToAction(nameof(Users));
                }

                var roles = await _userManagementService.GetAllRolesAsync();
                ViewBag.Roles = roles;

                var editModel = new AdminUserViewModel.UserEditViewModel
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    Name = user.Name,
                    Phone = user.Phone,
                    Avatar = user.Avatar,
                    BirthDate = user.BirthDate.HasValue ? new DateTime(user.BirthDate.Value.Year, user.BirthDate.Value.Month, user.BirthDate.Value.Day) : null,
                    Gender = user.Gender,
                    Status = user.Status,
                    Verified = user.Verified ?? false,
                    RoleIds = user.Roles.Select(r => r.RoleId).ToList()
                };

                return View(editModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EditUser GET action for user {UserId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin người dùng";
                return RedirectToAction(nameof(Users));
            }
        }

        /// <summary>
        /// Edit User POST - xử lý chỉnh sửa user
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(IFormFile? AvatarFile, AdminUserViewModel.UserEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var roles = await _userManagementService.GetAllRolesAsync();
                    ViewBag.Roles = roles;
                    return View(model);
                }

                var adminUserId = AuthHelper.GetUserId(HttpContext);
                if (!adminUserId.HasValue)
                {
                    TempData["Error"] = "Không thể xác định người dùng admin";
                    var roles = await _userManagementService.GetAllRolesAsync();
                    ViewBag.Roles = roles;
                    return View(model);
                }

                // Lấy thông tin user hiện tại để giữ avatar cũ nếu không upload mới
                var existingUser = await _userManagementService.GetUserByIdAsync(model.UserId);
                if (existingUser == null)
                {
                    TempData["Error"] = "Không tìm thấy người dùng";
                    var roles = await _userManagementService.GetAllRolesAsync();
                    ViewBag.Roles = roles;
                    return View(model);
                }

                // Xử lý file upload avatar nếu có
                if (AvatarFile != null && AvatarFile.Length > 0)
                {
                    // Kiểm tra kích thước file (5MB)
                    if (AvatarFile.Length > 5 * 1024 * 1024)
                    {
                        TempData["Error"] = "File avatar quá lớn. Kích thước tối đa là 5MB.";
                        var roles = await _userManagementService.GetAllRolesAsync();
                        ViewBag.Roles = roles;
                        return View(model);
                    }

                    // Kiểm tra loại file
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(AvatarFile.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        TempData["Error"] = "Chỉ hỗ trợ file JPG, PNG, GIF cho avatar.";
                        var roles = await _userManagementService.GetAllRolesAsync();
                        ViewBag.Roles = roles;
                        return View(model);
                    }

                    // Tạo tên file duy nhất
                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    
                    // Sử dụng đường dẫn tương đối từ thư mục gốc của ứng dụng
                    var uploadPath = Path.Combine(_webHostEnvironment.ContentRootPath, "Imagine", "Avatars");
                    
                    // Tạo thư mục nếu chưa tồn tại
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    var filePath = Path.Combine(uploadPath, fileName);
                    
                    // Lưu file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await AvatarFile.CopyToAsync(stream);
                    }

                    // Cập nhật Avatar URL trong model
                    model.Avatar = $"/Imagine/Avatars/{fileName}";
                }
                else
                {
                    // Nếu không có file mới, giữ nguyên avatar hiện tại
                    model.Avatar = existingUser.Avatar;
                }

                var result = await _userManagementService.UpdateUserAsync(model, adminUserId.Value);

                if (result)
                {
                    TempData["Success"] = "Cập nhật người dùng thành công";
                    return RedirectToAction(nameof(Users));
                }
                else
                {
                    TempData["Error"] = "Email đã tồn tại hoặc có lỗi xảy ra";
                    var roles = await _userManagementService.GetAllRolesAsync();
                    ViewBag.Roles = roles;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EditUser POST action");
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật người dùng";
                var roles = await _userManagementService.GetAllRolesAsync();
                ViewBag.Roles = roles;
                return View(model);
            }
        }

        /// <summary>
        /// User Detail - chi tiết user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> UserDetail(int id)
        {
            try
            {
                var user = await _userManagementService.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "Không tìm thấy người dùng";
                    return RedirectToAction(nameof(Users));
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UserDetail action for user {UserId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin người dùng";
                return RedirectToAction(nameof(Users));
            }
        }

        /// <summary>
        /// Delete User - xóa user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var adminUserId = AuthHelper.GetUserId(HttpContext);
                if (!adminUserId.HasValue)
                {
                    TempData["Error"] = "Không thể xác định người dùng admin";
                    return RedirectToAction(nameof(Users));
                }
                var result = await _userManagementService.DeleteUserAsync(id, adminUserId.Value);

                if (result)
                {
                    TempData["Success"] = "Xóa người dùng thành công";
                }
                else
                {
                    TempData["Error"] = "Không thể xóa người dùng (có thể là admin hoặc không tồn tại)";
                }

                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteUser action for user {UserId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi xóa người dùng";
                return RedirectToAction(nameof(Users));
            }
        }

        /// <summary>
        /// Change User Password - đổi mật khẩu user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult ChangeUserPassword(int id)
        {
            return View(new AdminUserViewModel.UserChangePasswordViewModel { UserId = id });
        }

        /// <summary>
        /// Change User Password POST - xử lý đổi mật khẩu
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserPassword(AdminUserViewModel.UserChangePasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var adminUserId = AuthHelper.GetUserId(HttpContext);
                if (!adminUserId.HasValue)
                {
                    TempData["Error"] = "Không thể xác định người dùng admin";
                    return View(model);
                }
                var result = await _userManagementService.ChangeUserPasswordAsync(model, adminUserId.Value);

                if (result)
                {
                    TempData["Success"] = "Đổi mật khẩu thành công";
                    return RedirectToAction(nameof(Users));
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy người dùng";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ChangeUserPassword POST action");
                TempData["Error"] = "Có lỗi xảy ra khi đổi mật khẩu";
                return View(model);
            }
        }

        /// <summary>
        /// Toggle User Verification - bật/tắt xác thực user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserVerification(int id)
        {
            try
            {
                var adminUserId = AuthHelper.GetUserId(HttpContext);
                if (!adminUserId.HasValue)
                {
                    TempData["Error"] = "Không thể xác định người dùng admin";
                    return RedirectToAction(nameof(Users));
                }
                var result = await _userManagementService.ToggleUserVerificationAsync(id, adminUserId.Value);

                if (result)
                {
                    TempData["Success"] = "Cập nhật trạng thái xác thực thành công";
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy người dùng";
                }

                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ToggleUserVerification action for user {UserId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái xác thực";
                return RedirectToAction(nameof(Users));
            }
        }

        /// <summary>
        /// Toggle User Status - thay đổi trạng thái user
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(int id, string status)
        {
            try
            {
                var adminUserId = AuthHelper.GetUserId(HttpContext);
                if (!adminUserId.HasValue)
                {
                    TempData["Error"] = "Không thể xác định người dùng admin";
                    return RedirectToAction(nameof(Users));
                }
                var result = await _userManagementService.ToggleUserStatusAsync(id, status, adminUserId.Value);

                if (result)
                {
                    TempData["Success"] = "Cập nhật trạng thái thành công";
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy người dùng";
                }

                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ToggleUserStatus action for user {UserId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái";
                return RedirectToAction(nameof(Users));
            }
        }

        /// <summary>
        /// System Settings
        /// </summary>
        /// <returns></returns>
        public IActionResult Settings()
        {
            return View();
        }

        /// <summary>
        /// Test Admin Area
        /// </summary>
        /// <returns></returns>
        public IActionResult Test()
        {
            return View();
        }

        /// <summary>
        /// Test Database Connection
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                _logger.LogInformation("TestDatabase called");
                
                // Test roles
                var roles = await _userManagementService.GetAllRolesAsync();
                _logger.LogInformation("Roles count: {Count}", roles.Count);
                
                // Test users count
                var stats = await _userManagementService.GetUserStatisticsAsync();
                _logger.LogInformation("User stats: {Stats}", string.Join(", ", stats.Select(kvp => $"{kvp.Key}={kvp.Value}")));
                
                return Json(new { 
                    Success = true, 
                    RolesCount = roles.Count,
                    Roles = roles.Select(r => new { r.RoleId, r.RoleName }),
                    UserStats = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TestDatabase failed");
                return Json(new { Success = false, Error = ex.Message });
            }
        }

        #region Product Management

        /// <summary>
        /// Product Management - quản lý sản phẩm
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Products(ProductSearchViewModel? searchModel = null)
        {
            try
            {
                await SetAdminViewBagAsync();

                searchModel ??= new ProductSearchViewModel();
                var (products, totalCount) = await _productService.GetProductsAsync(searchModel);
                var categories = await _productService.GetAllCategoriesAsync();
                var statistics = await _productService.GetProductStatisticsAsync();

                ViewBag.Categories = categories;
                ViewBag.Statistics = statistics;
                ViewBag.TotalCount = totalCount;
                ViewBag.CurrentPage = searchModel.Page;
                ViewBag.PageSize = searchModel.PageSize;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / searchModel.PageSize);

                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Products action");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách sản phẩm";
                
                await SetAdminViewBagAsync();
                ViewBag.Categories = new List<ProductCategoryViewModel>();
                ViewBag.Statistics = new Dictionary<string, int>();
                ViewBag.TotalCount = 0;
                ViewBag.CurrentPage = 1;
                ViewBag.PageSize = 10;
                ViewBag.TotalPages = 0;
                
                return View(new List<ProductListViewModel>());
            }
        }

        /// <summary>
        /// Create Product - tạo sản phẩm mới
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> CreateProduct()
        {
            try
            {
                await SetAdminViewBagAsync();
                var categories = await _productService.GetAllCategoriesAsync();
                ViewBag.Categories = categories;
                return View(new ProductCreateViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateProduct GET action");
                TempData["Error"] = "Có lỗi xảy ra khi tải trang tạo sản phẩm";
                return RedirectToAction(nameof(Products));
            }
        }

        /// <summary>
        /// Create Product POST - xử lý tạo sản phẩm
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(ProductCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await SetAdminViewBagAsync();
                    var categories = await _productService.GetAllCategoriesAsync();
                    ViewBag.Categories = categories;
                    return View(model);
                }

                var result = await _productService.CreateProductAsync(model);

                if (result)
                {
                    TempData["Success"] = "Tạo sản phẩm thành công";
                    return RedirectToAction(nameof(Products));
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi tạo sản phẩm";
                    await SetAdminViewBagAsync();
                    var categories = await _productService.GetAllCategoriesAsync();
                    ViewBag.Categories = categories;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateProduct POST action");
                TempData["Error"] = $"Có lỗi xảy ra khi tạo sản phẩm: {ex.Message}";
                await SetAdminViewBagAsync();
                var categories = await _productService.GetAllCategoriesAsync();
                ViewBag.Categories = categories;
                return View(model);
            }
        }

        /// <summary>
        /// Edit Product - chỉnh sửa sản phẩm
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditProduct(int id)
        {
            try
            {
                await SetAdminViewBagAsync();
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Không tìm thấy sản phẩm";
                    return RedirectToAction(nameof(Products));
                }

                var categories = await _productService.GetAllCategoriesAsync();
                ViewBag.Categories = categories;

                var editModel = new ProductEditViewModel
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    CategoryId = product.CategoryId,
                    Description = product.Description,
                    Price = product.Price,
                    Material = product.Material,
                    TargetAudience = product.TargetAudience,
                    StockQuantity = product.StockQuantity,
                    Status = product.Status,
                    CreatedAt = product.CreatedAt
                };

                return View(editModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EditProduct GET action for product {ProductId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin sản phẩm";
                return RedirectToAction(nameof(Products));
            }
        }

        /// <summary>
        /// Edit Product POST - xử lý chỉnh sửa sản phẩm
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(ProductEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var categories = await _productService.GetAllCategoriesAsync();
                    ViewBag.Categories = categories;
                    return View(model);
                }

                var result = await _productService.UpdateProductAsync(model);

                if (result)
                {
                    TempData["Success"] = "Cập nhật sản phẩm thành công";
                    return RedirectToAction(nameof(Products));
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy sản phẩm hoặc có lỗi xảy ra";
                    var categories = await _productService.GetAllCategoriesAsync();
                    ViewBag.Categories = categories;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EditProduct POST action");
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật sản phẩm";
                var categories = await _productService.GetAllCategoriesAsync();
                ViewBag.Categories = categories;
                return View(model);
            }
        }

        /// <summary>
        /// Product Detail - chi tiết sản phẩm
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> ProductDetail(int id)
        {
            try
            {
                await SetAdminViewBagAsync();
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Không tìm thấy sản phẩm";
                    return RedirectToAction(nameof(Products));
                }

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProductDetail action for product {ProductId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin sản phẩm";
                return RedirectToAction(nameof(Products));
            }
        }

        /// <summary>
        /// Delete Product - xóa sản phẩm
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(id);

                if (result)
                {
                    TempData["Success"] = "Xóa sản phẩm thành công";
                }
                else
                {
                    TempData["Error"] = "Không thể xóa sản phẩm (có thể không tồn tại)";
                }

                return RedirectToAction(nameof(Products));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteProduct action for product {ProductId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi xóa sản phẩm";
                return RedirectToAction(nameof(Products));
            }
        }

        /// <summary>
        /// Toggle Product Status - thay đổi trạng thái sản phẩm
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleProductStatus(int id, string status)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Không tìm thấy sản phẩm";
                    return RedirectToAction(nameof(Products));
                }

                var editModel = new ProductEditViewModel
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    CategoryId = product.CategoryId,
                    Description = product.Description,
                    Price = product.Price,
                    Material = product.Material,
                    TargetAudience = product.TargetAudience,
                    StockQuantity = product.StockQuantity,
                    Status = status,
                    CreatedAt = product.CreatedAt
                };

                var result = await _productService.UpdateProductAsync(editModel);

                if (result)
                {
                    TempData["Success"] = "Cập nhật trạng thái sản phẩm thành công";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái";
                }

                return RedirectToAction(nameof(Products));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ToggleProductStatus action for product {ProductId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái sản phẩm";
                return RedirectToAction(nameof(Products));
            }
        }

        #endregion

        #region Product Category Management

        /// <summary>
        /// Product Categories - quản lý danh mục sản phẩm
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> ProductCategories()
        {
            try
            {
                await SetAdminViewBagAsync();
                var categories = await _productService.GetAllCategoriesAsync();
                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProductCategories action");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách danh mục";
                await SetAdminViewBagAsync();
                return View(new List<ProductCategoryViewModel>());
            }
        }

        /// <summary>
        /// Create Category - tạo danh mục mới
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> CreateCategory()
        {
            try
            {
                await SetAdminViewBagAsync();
                return View(new ProductCategoryViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateCategory GET action");
                TempData["Error"] = "Có lỗi xảy ra khi tải trang tạo danh mục";
                return RedirectToAction(nameof(ProductCategories));
            }
        }

        /// <summary>
        /// Create Category POST - xử lý tạo danh mục
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(ProductCategoryViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await SetAdminViewBagAsync();
                    return View(model);
                }

                var result = await _productService.CreateCategoryAsync(model);

                if (result)
                {
                    TempData["Success"] = "Tạo danh mục thành công";
                    return RedirectToAction(nameof(ProductCategories));
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi tạo danh mục";
                    await SetAdminViewBagAsync();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateCategory POST action");
                TempData["Error"] = $"Có lỗi xảy ra khi tạo danh mục: {ex.Message}";
                await SetAdminViewBagAsync();
                return View(model);
            }
        }

        /// <summary>
        /// Edit Category - chỉnh sửa danh mục
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditCategory(int id)
        {
            try
            {
                await SetAdminViewBagAsync();
                var category = await _productService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    TempData["Error"] = "Không tìm thấy danh mục";
                    return RedirectToAction(nameof(ProductCategories));
                }

                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EditCategory GET action for category {CategoryId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin danh mục";
                return RedirectToAction(nameof(ProductCategories));
            }
        }

        /// <summary>
        /// Edit Category POST - xử lý chỉnh sửa danh mục
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(ProductCategoryViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await SetAdminViewBagAsync();
                    return View(model);
                }

                var result = await _productService.UpdateCategoryAsync(model);

                if (result)
                {
                    TempData["Success"] = "Cập nhật danh mục thành công";
                    return RedirectToAction(nameof(ProductCategories));
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy danh mục hoặc có lỗi xảy ra";
                    await SetAdminViewBagAsync();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EditCategory POST action");
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật danh mục";
                await SetAdminViewBagAsync();
                return View(model);
            }
        }

        /// <summary>
        /// Delete Category - xóa danh mục
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var result = await _productService.DeleteCategoryAsync(id);

                if (result)
                {
                    TempData["Success"] = "Xóa danh mục thành công";
                }
                else
                {
                    TempData["Error"] = "Không thể xóa danh mục (có thể có sản phẩm trong danh mục này)";
                }

                return RedirectToAction(nameof(ProductCategories));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteCategory action for category {CategoryId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi xóa danh mục";
                return RedirectToAction(nameof(ProductCategories));
            }
        }

        /// <summary>
        /// Delete Category With Products - xóa danh mục và xử lý sản phẩm
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reassignToCategoryId"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategoryWithProducts(int id, int? reassignToCategoryId = null)
        {
            try
            {
                _logger.LogInformation("DeleteCategoryWithProducts called for category {CategoryId}, reassignTo: {ReassignToCategoryId}", id, reassignToCategoryId);
                
                var result = await _productService.DeleteCategoryWithProductsAsync(id, reassignToCategoryId);

                if (result)
                {
                    TempData["Success"] = "Xóa danh mục thành công";
                }
                else
                {
                    TempData["Error"] = "Không thể xóa danh mục (có thể không tồn tại)";
                }

                return RedirectToAction(nameof(ProductCategories));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteCategoryWithProducts action for category {CategoryId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi xóa danh mục";
                return RedirectToAction(nameof(ProductCategories));
            }
        }

        /// <summary>
        /// Get Categories For Reassignment - lấy danh sách danh mục để chuyển sản phẩm
        /// </summary>
        /// <param name="excludeCategoryId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetCategoriesForReassignment(int excludeCategoryId)
        {
            try
            {
                var categories = await _productService.GetCategoriesForReassignmentAsync(excludeCategoryId);
                return Json(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCategoriesForReassignment action");
                return Json(new List<object>());
            }
        }

        #endregion

        #region Product Image Management

        /// <summary>
        /// Product Images - quản lý hình ảnh sản phẩm
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> ProductImages(int? productId = null)
        {
            try
            {
                await SetAdminViewBagAsync();
                
                // Lấy danh sách sản phẩm để hiển thị trong dropdown
                var products = await _productService.GetProductsForDropdownAsync();
                ViewBag.Products = products;
                
                if (productId.HasValue)
                {
                    // Lấy hình ảnh của sản phẩm cụ thể
                    var images = await _productService.GetProductImagesAsync(productId.Value);
                    var product = await _productService.GetProductByIdAsync(productId.Value);
                    ViewBag.SelectedProductId = productId.Value;
                    
                    var groupedImages = new List<object>
                    {
                        new
                        {
                            ProductId = product.ProductId,
                            ProductName = product.Name,
                            Images = images
                        }
                    };
                    
                    return View(groupedImages);
                }
                else
                {
                    // Lấy tất cả sản phẩm có hình ảnh, nhóm theo sản phẩm
                    var groupedImages = await _productService.GetProductImagesGroupedByProductAsync();
                    ViewBag.SelectedProductId = null;
                    
                    return View(groupedImages);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProductImages action");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách hình ảnh";
                await SetAdminViewBagAsync();
                ViewBag.Products = new List<object>();
                ViewBag.SelectedProductId = null;
                return View(new List<object>());
            }
        }

        /// <summary>
        /// Get Products for Dropdown - lấy danh sách sản phẩm cho dropdown
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetProductsForDropdown()
        {
            try
            {
                var products = await _productService.GetProductsForDropdownAsync();
                return Json(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetProductsForDropdown action");
                return Json(new List<object>());
            }
        }

        /// <summary>
        /// Get Product Images - lấy danh sách hình ảnh sản phẩm
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetProductImages(int productId)
        {
            try
            {
                var images = await _productService.GetProductImagesAsync(productId);
                return Json(images);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetProductImages action for product {ProductId}", productId);
                return Json(new List<ProductImgViewModel>());
            }
        }

        /// <summary>
        /// Add Product Image - thêm hình ảnh sản phẩm
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddProductImage(IFormFile? ImageFile, ProductImgViewModel model)
        {
            try
            {
                // Bỏ qua ModelState validation cho file upload, chỉ kiểm tra các trường cần thiết
                if (model.ProductId <= 0)
                {
                    return Json(new { success = false, message = "Vui lòng chọn sản phẩm!" });
                }

                // Kiểm tra giới hạn 3 hình ảnh
                var canAddMore = await _productService.CanProductAddMoreImagesAsync(model.ProductId);
                if (!canAddMore)
                {
                    return Json(new { success = false, message = "Sản phẩm này đã đạt giới hạn 3 hình ảnh. Không thể thêm hình ảnh mới." });
                }

                // Xử lý file upload nếu có
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    // Kiểm tra kích thước file (5MB)
                    if (ImageFile.Length > 5 * 1024 * 1024)
                    {
                        return Json(new { success = false, message = "File quá lớn. Kích thước tối đa là 5MB." });
                    }

                    // Kiểm tra loại file
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return Json(new { success = false, message = "Chỉ hỗ trợ file JPG, PNG, GIF." });
                    }

                                         // Tạo tên file duy nhất
                     var fileName = $"{Guid.NewGuid()}{fileExtension}";
                     
                     // Sử dụng đường dẫn tương đối từ thư mục gốc của ứng dụng
                     var uploadPath = Path.Combine(_webHostEnvironment.ContentRootPath, "Imagine", "IMGProduct");
                     
                     // Tạo thư mục nếu chưa tồn tại
                     if (!Directory.Exists(uploadPath))
                     {
                         Directory.CreateDirectory(uploadPath);
                     }

                     var filePath = Path.Combine(uploadPath, fileName);
                     
                     // Lưu file
                     using (var stream = new FileStream(filePath, FileMode.Create))
                     {
                         await ImageFile.CopyToAsync(stream);
                     }

                     // Cập nhật URL trong model
                     model.ImgUrl = $"/Imagine/IMGProduct/{fileName}";
                }
                else if (string.IsNullOrEmpty(model.ImgUrl))
                {
                    return Json(new { success = false, message = "Vui lòng chọn file hình ảnh hoặc nhập URL!" });
                }

                // Kiểm tra các trường khác
                if (string.IsNullOrEmpty(model.ImgName))
                {
                    model.ImgName = "Hình ảnh sản phẩm";
                }
                
                if (string.IsNullOrEmpty(model.AltText))
                {
                    model.AltText = "Hình ảnh sản phẩm";
                }
                
                // Đảm bảo DisplayOrder có giá trị
                if (model.DisplayOrder <= 0)
                {
                    model.DisplayOrder = 1;
                }

                var result = await _productService.AddProductImageAsync(model);
                if (result)
                {
                    return Json(new { success = true, message = "Thêm hình ảnh thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể thêm hình ảnh. Có thể sản phẩm đã đạt giới hạn 3 hình ảnh." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddProductImage action");
                return Json(new { success = false, message = "Có lỗi xảy ra khi thêm hình ảnh" });
            }
        }

        /// <summary>
        /// Set Primary Image - đặt hình ảnh chính
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="imageId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SetPrimaryImage(int productId, int imageId)
        {
            try
            {
                var result = await _productService.SetPrimaryImageAsync(productId, imageId);
                return Json(new { success = result, message = result ? "Đã cập nhật hình ảnh chính" : "Có lỗi xảy ra" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SetPrimaryImage action");
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật hình ảnh chính" });
            }
        }

        /// <summary>
        /// Update Product Image - cập nhật hình ảnh sản phẩm
        /// </summary>
        /// <param name="ImageFile"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpdateProductImage(IFormFile? ImageFile, ProductImgViewModel model)
        {
            try
            {
                // Bỏ qua ModelState validation cho file upload, chỉ kiểm tra các trường cần thiết
                if (model.ImgId <= 0)
                {
                    return Json(new { success = false, message = "ID hình ảnh không hợp lệ!" });
                }

                // Lấy thông tin hình ảnh hiện tại từ database
                var existingImage = await _productService.GetProductImageByIdAsync(model.ImgId);
                if (existingImage == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hình ảnh!" });
                }

                // Xử lý file upload nếu có
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    // Kiểm tra kích thước file (5MB)
                    if (ImageFile.Length > 5 * 1024 * 1024)
                    {
                        return Json(new { success = false, message = "File quá lớn. Kích thước tối đa là 5MB." });
                    }

                    // Kiểm tra loại file
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return Json(new { success = false, message = "Chỉ hỗ trợ file JPG, PNG, GIF." });
                    }

                    // Tạo tên file duy nhất
                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    
                    // Sử dụng đường dẫn tương đối từ thư mục gốc của ứng dụng
                    var uploadPath = Path.Combine(_webHostEnvironment.ContentRootPath, "Imagine", "IMGProduct");
                    
                    // Tạo thư mục nếu chưa tồn tại
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    var filePath = Path.Combine(uploadPath, fileName);
                    
                    // Lưu file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    // Cập nhật URL trong model
                    model.ImgUrl = $"/Imagine/IMGProduct/{fileName}";
                }
                else
                {
                    // Nếu không có file mới, giữ nguyên URL hiện tại
                    model.ImgUrl = existingImage.ImgUrl;
                }

                var result = await _productService.UpdateProductImageAsync(model);
                return Json(new { success = result, message = result ? "Cập nhật hình ảnh thành công" : "Có lỗi xảy ra" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateProductImage action");
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật hình ảnh" });
            }
        }

        /// <summary>
        /// Get Product Image Count - lấy số lượng hình ảnh của sản phẩm
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetProductImageCount(int productId)
        {
            try
            {
                var imageCount = await _productService.GetProductImageCountAsync(productId);
                var canAddMore = await _productService.CanProductAddMoreImagesAsync(productId);
                
                return Json(new { 
                    success = true, 
                    imageCount = imageCount, 
                    canAddMore = canAddMore,
                    maxImages = 3,
                    message = canAddMore ? $"Có thể thêm {3 - imageCount} hình ảnh nữa" : "Đã đạt giới hạn 3 hình ảnh"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetProductImageCount action for product {ProductId}", productId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi kiểm tra số lượng hình ảnh" });
            }
        }

        /// <summary>
        /// Delete Product Image - xóa hình ảnh sản phẩm
        /// </summary>
        /// <param name="imageId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> DeleteProductImage(int imageId)
        {
            try
            {
                var result = await _productService.DeleteProductImageAsync(imageId);
                return Json(new { success = result, message = result ? "Đã xóa hình ảnh" : "Có lỗi xảy ra" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteProductImage action");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa hình ảnh" });
            }
        }

        #endregion
    }
}
