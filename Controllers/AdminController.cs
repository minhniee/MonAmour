using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        private readonly IPartnerService _partnerService;
        private readonly ILocationService _locationService;
        private readonly IConceptService _conceptService;
        private readonly IOrderService _orderService;
        private readonly ILogger<AdminController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ICloudinaryService _cloudinaryService;

        public AdminController(IAuthService authService, IUserManagementService userManagementService, IProductService productService, IPartnerService partnerService, ILocationService locationService, IConceptService conceptService, IOrderService orderService, ILogger<AdminController> logger, IWebHostEnvironment webHostEnvironment, ICloudinaryService cloudinaryService)
        {
            _authService = authService;
            _userManagementService = userManagementService;
            _productService = productService;
            _partnerService = partnerService;
            _locationService = locationService;
            _conceptService = conceptService;
            _orderService = orderService;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _cloudinaryService = cloudinaryService;
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

                // Get concept statistics
                var conceptStats = await _conceptService.GetConceptStatisticsAsync();
                ViewBag.ConceptStats = conceptStats;

                // Get partner statistics
                var partnerStats = await _partnerService.GetPartnerStatisticsAsync();
                ViewBag.PartnerStats = partnerStats;

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
                ViewBag.ConceptStats = new Dictionary<string, int>
                {
                    ["TotalConcepts"] = 0,
                    ["ActiveConcepts"] = 0
                };
                ViewBag.PartnerStats = new Dictionary<string, int>
                {
                    ["TotalPartners"] = 0,
                    ["ActivePartners"] = 0
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
                    try
                    {
                        // Upload to Cloudinary
                        var avatarUrl = await _cloudinaryService.UploadImageAsync(AvatarFile, "avatars");
                        
                        if (!string.IsNullOrEmpty(avatarUrl))
                        {
                            model.Avatar = avatarUrl;
                            _logger.LogInformation("Avatar uploaded successfully to Cloudinary: {Url}", avatarUrl);
                        }
                        else
                        {
                            TempData["Error"] = "Không thể upload avatar. Vui lòng thử lại.";
                            await SetAdminViewBagAsync();
                            var roles = await _userManagementService.GetAllRolesAsync();
                            ViewBag.Roles = roles;
                            return View(model);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading avatar to Cloudinary");
                        TempData["Error"] = "Có lỗi xảy ra khi upload avatar. Vui lòng thử lại.";
                        await SetAdminViewBagAsync();
                        var roles = await _userManagementService.GetAllRolesAsync();
                        ViewBag.Roles = roles;
                        return View(model);
                    }
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
                    try
                    {
                        // Upload to Cloudinary
                        var avatarUrl = await _cloudinaryService.UploadImageAsync(AvatarFile, "avatars");
                        
                        if (!string.IsNullOrEmpty(avatarUrl))
                        {
                            model.Avatar = avatarUrl;
                            _logger.LogInformation("Avatar uploaded successfully to Cloudinary: {Url}", avatarUrl);
                        }
                        else
                        {
                            TempData["Error"] = "Không thể upload avatar. Vui lòng thử lại.";
                            var roles = await _userManagementService.GetAllRolesAsync();
                            ViewBag.Roles = roles;
                            return View(model);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading avatar to Cloudinary");
                        TempData["Error"] = "Có lỗi xảy ra khi upload avatar. Vui lòng thử lại.";
                        var roles = await _userManagementService.GetAllRolesAsync();
                        ViewBag.Roles = roles;
                        return View(model);
                    }
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

                return Json(new
                {
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
        public async Task<IActionResult> ProductCategories(CategorySearchViewModel searchModel)
        {
            try
            {
                await SetAdminViewBagAsync();
                
                // Set default values if not provided
                if (string.IsNullOrEmpty(searchModel.SortBy))
                    searchModel.SortBy = "name";
                if (string.IsNullOrEmpty(searchModel.SortOrder))
                    searchModel.SortOrder = "asc";
                
                var categories = await _productService.GetAllCategoriesAsync();
                
                // Apply search filter
                if (!string.IsNullOrEmpty(searchModel.SearchTerm))
                {
                    var searchTerm = searchModel.SearchTerm.ToLower();
                    categories = categories.Where(c => 
                        c.Name.ToLower().Contains(searchTerm) ||
                        c.CategoryId.ToString().Contains(searchTerm)
                    ).ToList();
                }
                
                // Apply status filter
                if (!string.IsNullOrEmpty(searchModel.Status))
                {
                    switch (searchModel.Status)
                    {
                        case "hasProducts":
                            categories = categories.Where(c => c.ProductCount > 0).ToList();
                            break;
                        case "empty":
                            categories = categories.Where(c => c.ProductCount == 0).ToList();
                            break;
                    }
                }
                
                // Apply sorting
                switch (searchModel.SortBy)
                {
                    case "name":
                        categories = searchModel.SortOrder == "desc" 
                            ? categories.OrderByDescending(c => c.Name).ToList()
                            : categories.OrderBy(c => c.Name).ToList();
                        break;
                    case "productCount":
                        categories = searchModel.SortOrder == "desc"
                            ? categories.OrderByDescending(c => c.ProductCount).ToList()
                            : categories.OrderBy(c => c.ProductCount).ToList();
                        break;
                    case "id":
                        categories = searchModel.SortOrder == "desc"
                            ? categories.OrderByDescending(c => c.CategoryId).ToList()
                            : categories.OrderBy(c => c.CategoryId).ToList();
                        break;
                }
                
                ViewBag.SearchModel = searchModel;
                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProductCategories action");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách danh mục";
                await SetAdminViewBagAsync();
                ViewBag.SearchModel = new CategorySearchViewModel();
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
        public async Task<IActionResult> ProductImages(int? productId = null, string? searchTerm = null)
        {
            try
            {
                await SetAdminViewBagAsync();

                // Lấy danh sách sản phẩm để hiển thị trong dropdown
                var products = await _productService.GetProductsForDropdownAsync();
                ViewBag.Products = products;
                ViewBag.SearchTerm = searchTerm;

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
                else if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    // Tìm kiếm sản phẩm theo tên
                    var searchResults = await _productService.SearchProductsByNameAsync(searchTerm);
                    var searchResultsList = new List<object>();
                    
                    foreach (dynamic product in searchResults)
                    {
                        var images = await _productService.GetProductImagesAsync(product.Value);
                        if (images.Any())
                        {
                            searchResultsList.Add(new
                            {
                                ProductId = product.Value,
                                ProductName = product.Text,
                                Images = images
                            });
                        }
                    }
                    
                    return View(searchResultsList);
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
                ViewBag.SearchTerm = null;
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

        [HttpGet]
        public async Task<IActionResult> SearchProducts(string searchTerm)
        {
            try
            {
                var products = await _productService.SearchProductsByNameAsync(searchTerm);
                return Json(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SearchProducts action for search term: {SearchTerm}", searchTerm);
                return Json(new List<object>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchConcepts(string searchTerm)
        {
            try
            {
                var concepts = await _conceptService.SearchConceptsByNameAsync(searchTerm);
                return Json(concepts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SearchConcepts action for search term: {SearchTerm}", searchTerm);
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
                    try
                    {
                        // Upload to Cloudinary
                        var imageUrl = await _cloudinaryService.UploadImageAsync(ImageFile, "products");
                        
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            model.ImgUrl = imageUrl;
                            _logger.LogInformation("Product image uploaded successfully to Cloudinary: {Url}", imageUrl);
                        }
                        else
                        {
                            return Json(new { success = false, message = "Không thể upload hình ảnh. Vui lòng thử lại." });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading product image to Cloudinary");
                        return Json(new { success = false, message = "Có lỗi xảy ra khi upload hình ảnh. Vui lòng thử lại." });
                    }
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
                // Validate parameters
                if (productId <= 0)
                {
                    return Json(new { success = false, message = "ID sản phẩm không hợp lệ!" });
                }

                if (imageId <= 0)
                {
                    return Json(new { success = false, message = "ID hình ảnh không hợp lệ!" });
                }

                // Check if product exists
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm!" });
                }

                // Check if image exists
                var image = await _productService.GetProductImageByIdAsync(imageId);
                if (image == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hình ảnh!" });
                }

                // Check if image belongs to the product
                if (image.ProductId != productId)
                {
                    return Json(new { success = false, message = "Hình ảnh không thuộc về sản phẩm này!" });
                }

                _logger.LogInformation("Calling SetPrimaryImageAsync for ProductId: {ProductId}, ImageId: {ImageId}", productId, imageId);
                var result = await _productService.SetPrimaryImageAsync(productId, imageId);
                _logger.LogInformation("SetPrimaryImageAsync result: {Result}", result);
                
                if (result)
                {
                    return Json(new { success = true, message = "Đã cập nhật hình ảnh chính thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể cập nhật hình ảnh chính! Có thể không có thay đổi nào được lưu." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SetPrimaryImage action for ProductId: {ProductId}, ImageId: {ImageId}", productId, imageId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật hình ảnh chính: " + ex.Message });
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
                    try
                    {
                        // Delete old image if it exists
                        if (!string.IsNullOrEmpty(existingImage.ImgUrl))
                        {
                            var publicId = _cloudinaryService.ExtractPublicIdFromUrl(existingImage.ImgUrl);
                            if (!string.IsNullOrEmpty(publicId))
                            {
                                await _cloudinaryService.DeleteImageAsync(publicId);
                            }
                        }
                        
                        // Upload new image to Cloudinary
                        var imageUrl = await _cloudinaryService.UploadImageAsync(ImageFile, "products");
                        
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            model.ImgUrl = imageUrl;
                            _logger.LogInformation("Product image updated successfully to Cloudinary: {Url}", imageUrl);
                        }
                        else
                        {
                            return Json(new { success = false, message = "Không thể upload hình ảnh. Vui lòng thử lại." });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading product image to Cloudinary");
                        return Json(new { success = false, message = "Có lỗi xảy ra khi upload hình ảnh. Vui lòng thử lại." });
                    }
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

                return Json(new
                {
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

        #region Partner Management

        /// <summary>
        /// Partners - quản lý đối tác
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Partners(PartnerSearchViewModel searchModel)
        {
            try
            {
                await SetAdminViewBagAsync();
                
                // Set default values if not provided
                if (string.IsNullOrEmpty(searchModel.SortBy))
                    searchModel.SortBy = "name";
                if (string.IsNullOrEmpty(searchModel.SortOrder))
                    searchModel.SortOrder = "asc";
                
                var (partners, totalCount) = await _partnerService.GetPartnersAsync(searchModel);

                ViewBag.TotalCount = totalCount;
                ViewBag.SearchModel = searchModel;

                return View(partners);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Partners action");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách đối tác";
                await SetAdminViewBagAsync();
                return View(new List<PartnerViewModel>());
            }
        }

        /// <summary>
        /// Create Partner - tạo đối tác mới
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> CreatePartner()
        {
            try
            {
                await SetAdminViewBagAsync();
                var model = new PartnerCreateViewModel();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreatePartner GET action");
                TempData["Error"] = "Có lỗi xảy ra khi tải trang tạo đối tác";
                return RedirectToAction(nameof(Partners));
            }
        }

        /// <summary>
        /// Create Partner POST - xử lý tạo đối tác
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePartner(PartnerCreateViewModel model, IFormFile AvatarFile)
        {
            try
            {
                _logger.LogInformation("CreatePartner called with model: Name={Name}, Status={Status}, UserId={UserId}, Email={Email}, Phone={Phone}, ContactInfo={ContactInfo}",
                    model.Name, model.Status, model.UserId, model.Email, model.Phone, model.ContactInfo);

                // Log all form data
                _logger.LogInformation("Form data: {FormData}", string.Join(", ", Request.Form.Select(x => $"{x.Key}={x.Value}")));

                // Handle UserId - convert 0 to null
                if (model.UserId == 0)
                {
                    model.UserId = null;
                    _logger.LogInformation("Converted UserId from 0 to null");
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState is invalid. Errors: {Errors}",
                        string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                    await SetAdminViewBagAsync();
                    return View(model);
                }

                // Handle avatar file upload using Cloudinary
                if (AvatarFile != null && AvatarFile.Length > 0)
                {
                    try
                    {
                        // Upload to Cloudinary
                        var avatarUrl = await _cloudinaryService.UploadImageAsync(AvatarFile, "partners");
                        
                        if (!string.IsNullOrEmpty(avatarUrl))
                        {
                            model.Avatar = avatarUrl;
                            _logger.LogInformation("Partner avatar uploaded successfully to Cloudinary: {Url}", avatarUrl);
                        }
                        else
                        {
                            ModelState.AddModelError("AvatarFile", "Không thể upload avatar. Vui lòng thử lại.");
                            await SetAdminViewBagAsync();
                            return View(model);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading partner avatar to Cloudinary");
                        ModelState.AddModelError("AvatarFile", "Có lỗi xảy ra khi upload avatar. Vui lòng thử lại.");
                        await SetAdminViewBagAsync();
                        return View(model);
                    }
                }

                var result = await _partnerService.CreatePartnerAsync(model);

                if (result)
                {
                    TempData["Success"] = "Tạo đối tác thành công";
                    return RedirectToAction(nameof(Partners));
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi tạo đối tác";
                    await SetAdminViewBagAsync();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreatePartner POST action");
                TempData["Error"] = "Có lỗi xảy ra khi tạo đối tác";
                await SetAdminViewBagAsync();
                return View(model);
            }
        }

        /// <summary>
        /// Edit Partner - chỉnh sửa đối tác
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditPartner(int id)
        {
            try
            {
                await SetAdminViewBagAsync();
                var partner = await _partnerService.GetPartnerByIdAsync(id);
                if (partner == null)
                {
                    TempData["Error"] = "Không tìm thấy đối tác";
                    return RedirectToAction(nameof(Partners));
                }

                var model = new PartnerEditViewModel
                {
                    PartnerId = partner.PartnerId,
                    Name = partner.Name,
                    ContactInfo = partner.ContactInfo,
                    UserId = partner.UserId,
                    Email = partner.Email,
                    Phone = partner.Phone,
                    Avatar = partner.Avatar,
                    Status = partner.Status,
                    CreatedAt = partner.CreatedAt
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EditPartner GET action for partner {PartnerId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin đối tác";
                return RedirectToAction(nameof(Partners));
            }
        }

        /// <summary>
        /// Get Partner for Edit - lấy thông tin đối tác cho modal edit
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetPartnerForEdit(int id)
        {
            try
            {
                var partner = await _partnerService.GetPartnerByIdAsync(id);
                if (partner == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đối tác" });
                }

                var result = new
                {
                    success = true,
                    partner = new
                    {
                        partnerId = partner.PartnerId,
                        name = partner.Name,
                        status = partner.Status,
                        email = partner.Email,
                        phone = partner.Phone,
                        contactInfo = partner.ContactInfo,
                        avatar = partner.Avatar,
                        userId = partner.UserId
                    }
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPartnerForEdit action");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải thông tin đối tác" });
            }
        }

        /// <summary>
        /// Test Database Connection - kiểm tra kết nối database
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> TestDatabaseConnection()
        {
            try
            {
                // Test by getting users and partners
                var users = await _userManagementService.GetAllUsersAsync();
                var partners = await _partnerService.GetPartnersAsync(new PartnerSearchViewModel { PageSize = 1 });

                return Json(new
                {
                    success = true,
                    message = "Database connection successful",
                    partnerCount = partners.totalCount,
                    userCount = users.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database connection test failed");
                return Json(new
                {
                    success = false,
                    message = "Database connection failed: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Test Create Partner - test tạo partner đơn giản
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> TestCreatePartner()
        {
            try
            {
                // Get first user to test with UserId
                var users = await _userManagementService.GetAllUsersAsync();
                var firstUserId = users.FirstOrDefault()?.UserId;

                var testModel = new PartnerCreateViewModel
                {
                    Name = "Test Partner " + DateTime.Now.ToString("HHmmss"),
                    Status = "Active",
                    Email = "test@example.com",
                    Phone = "0123456789",
                    ContactInfo = "Test contact info",
                    UserId = firstUserId, // Test with actual user ID
                    Avatar = null // Test without avatar
                };

                _logger.LogInformation("Testing partner creation with model: {Model}, UserId: {UserId}", testModel.Name, testModel.UserId);

                var result = await _partnerService.CreatePartnerAsync(testModel);

                return Json(new
                {
                    success = result,
                    message = result ? "Partner created successfully" : "Failed to create partner",
                    model = testModel,
                    userId = firstUserId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test partner creation failed");
                return Json(new
                {
                    success = false,
                    message = "Test failed: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Test Create Partner Without User - test tạo partner không có UserId
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> TestCreatePartnerWithoutUser()
        {
            try
            {
                var testModel = new PartnerCreateViewModel
                {
                    Name = "Test Partner No User " + DateTime.Now.ToString("HHmmss"),
                    Status = "Active",
                    Email = "test@example.com",
                    Phone = "0123456789",
                    ContactInfo = "Test contact info",
                    UserId = null, // Test without user ID
                    Avatar = null // Test without avatar
                };

                _logger.LogInformation("Testing partner creation without user: {Model}", testModel.Name);

                var result = await _partnerService.CreatePartnerAsync(testModel);

                return Json(new
                {
                    success = result,
                    message = result ? "Partner created successfully without user" : "Failed to create partner without user",
                    model = testModel
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test partner creation without user failed");
                return Json(new
                {
                    success = false,
                    message = "Test failed: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Get Users for Dropdown - lấy danh sách người dùng cho dropdown
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetUsersForDropdown()
        {
            try
            {
                var users = await _userManagementService.GetAllUsersAsync();
                var result = users.Select(u => new
                {
                    id = u.UserId,
                    name = u.Name,
                    email = u.Email
                }).ToList();

                return Json(new { success = true, users = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUsersForDropdown action");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải danh sách người dùng" });
            }
        }

        /// <summary>
        /// Get Partners for Dropdown - lấy danh sách đối tác cho dropdown
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetPartnersForDropdown()
        {
            try
            {
                var partners = await _partnerService.GetPartnersForDropdownAsync();
                var result = partners.Select(p => new
                {
                    id = p.PartnerId,
                    name = p.Name
                }).ToList();

                return Json(new { success = true, partners = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPartnersForDropdown action");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải danh sách đối tác" });
            }
        }

        /// <summary>
        /// Get Location for Edit - lấy thông tin địa điểm cho edit modal
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetLocationForEdit(int id)
        {
            try
            {
                var location = await _locationService.GetLocationByIdAsync(id);
                if (location == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy địa điểm" });
                }

                var result = new
                {
                    locationId = location.LocationId,
                    name = location.Name,
                    address = location.Address,
                    district = location.District,
                    city = location.City,
                    partnerId = location.PartnerId,
                    ggmapLink = location.GgmapLink,
                    status = location.Status
                };

                return Json(new { success = true, location = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetLocationForEdit action");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải thông tin địa điểm" });
            }
        }

        /// <summary>
        /// Edit Partner POST - xử lý cập nhật đối tác
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPartner(PartnerEditViewModel model, IFormFile AvatarFile)
        {
            try
            {
                _logger.LogInformation("EditPartner called with model: PartnerId={PartnerId}, Name={Name}, Status={Status}, UserId={UserId}, Email={Email}, Phone={Phone}, ContactInfo={ContactInfo}",
                    model.PartnerId, model.Name, model.Status, model.UserId, model.Email, model.Phone, model.ContactInfo);

                // Log all form data
                _logger.LogInformation("Form data: {FormData}", string.Join(", ", Request.Form.Select(x => $"{x.Key}={x.Value}")));

                // Handle UserId - convert 0 to null
                if (model.UserId == 0)
                {
                    model.UserId = null;
                    _logger.LogInformation("Converted UserId from 0 to null");
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState is invalid. Errors: {Errors}",
                        string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                    await SetAdminViewBagAsync();
                    return View(model);
                }

                // Handle avatar file upload using Cloudinary
                if (AvatarFile != null && AvatarFile.Length > 0)
                {
                    try
                    {
                        // Get existing partner to delete old avatar if needed
                        var existingPartner = await _partnerService.GetPartnerByIdAsync(model.PartnerId);
                        
                        // Delete old avatar if it exists
                        if (existingPartner != null && !string.IsNullOrEmpty(existingPartner.Avatar))
                        {
                            var publicId = _cloudinaryService.ExtractPublicIdFromUrl(existingPartner.Avatar);
                            if (!string.IsNullOrEmpty(publicId))
                            {
                                await _cloudinaryService.DeleteImageAsync(publicId);
                            }
                        }
                        
                        // Upload new avatar to Cloudinary
                        var avatarUrl = await _cloudinaryService.UploadImageAsync(AvatarFile, "partners");
                        
                        if (!string.IsNullOrEmpty(avatarUrl))
                        {
                            model.Avatar = avatarUrl;
                            _logger.LogInformation("Partner avatar updated successfully to Cloudinary: {Url}", avatarUrl);
                        }
                        else
                        {
                            ModelState.AddModelError("AvatarFile", "Không thể upload avatar. Vui lòng thử lại.");
                            await SetAdminViewBagAsync();
                            return View(model);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading partner avatar to Cloudinary");
                        ModelState.AddModelError("AvatarFile", "Có lỗi xảy ra khi upload avatar. Vui lòng thử lại.");
                        await SetAdminViewBagAsync();
                        return View(model);
                    }
                }
                else
                {
                    // If no new avatar file, keep existing avatar
                    var existingPartner = await _partnerService.GetPartnerByIdAsync(model.PartnerId);
                    if (existingPartner != null)
                    {
                        model.Avatar = existingPartner.Avatar;
                    }
                }

                var result = await _partnerService.UpdatePartnerAsync(model);

                if (result)
                {
                    TempData["Success"] = "Cập nhật đối tác thành công";
                    return RedirectToAction(nameof(Partners));
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy đối tác hoặc có lỗi xảy ra";
                    await SetAdminViewBagAsync();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EditPartner POST action");
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật đối tác";
                await SetAdminViewBagAsync();
                return View(model);
            }
        }

        /// <summary>
        /// Partner Detail - chi tiết đối tác
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> PartnerDetail(int id)
        {
            try
            {
                await SetAdminViewBagAsync();
                var partner = await _partnerService.GetPartnerByIdAsync(id);
                if (partner == null)
                {
                    TempData["Error"] = "Không tìm thấy đối tác";
                    return RedirectToAction(nameof(Partners));
                }

                return View(partner);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PartnerDetail action for partner {PartnerId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin đối tác";
                return RedirectToAction(nameof(Partners));
            }
        }

        /// <summary>
        /// Delete Partner - xóa đối tác
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePartner(int id)
        {
            try
            {
                var result = await _partnerService.DeletePartnerAsync(id);

                if (result)
                {
                    TempData["Success"] = "Xóa đối tác thành công";
                }
                else
                {
                    TempData["Error"] = "Không thể xóa đối tác (có thể có địa điểm liên kết)";
                }

                return RedirectToAction(nameof(Partners));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeletePartner action for partner {PartnerId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi xóa đối tác";
                return RedirectToAction(nameof(Partners));
            }
        }

        /// <summary>
        /// Toggle Partner Status - thay đổi trạng thái đối tác
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePartnerStatus(int id, string status)
        {
            try
            {
                var result = await _partnerService.TogglePartnerStatusAsync(id, status);

                if (result)
                {
                    TempData["Success"] = "Cập nhật trạng thái đối tác thành công";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái";
                }

                return RedirectToAction(nameof(Partners));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TogglePartnerStatus action for partner {PartnerId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái đối tác";
                return RedirectToAction(nameof(Partners));
            }
        }

        #endregion

        #region Location Management

        /// <summary>
        /// Locations - quản lý địa điểm
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Locations(LocationSearchViewModel searchModel)
        {
            try
            {
                await SetAdminViewBagAsync();
                
                // Initialize searchModel if null
                if (searchModel == null)
                    searchModel = new LocationSearchViewModel();
                
                // Set default values if not provided
                if (string.IsNullOrEmpty(searchModel.SortBy))
                    searchModel.SortBy = "name";
                if (string.IsNullOrEmpty(searchModel.SortOrder))
                    searchModel.SortOrder = "asc";
                
                var (locations, totalCount) = await _locationService.GetLocationsAsync(searchModel);

                ViewBag.TotalCount = totalCount;
                ViewBag.SearchModel = searchModel;

                return View(locations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Locations action");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách địa điểm";
                await SetAdminViewBagAsync();
                ViewBag.SearchModel = new LocationSearchViewModel();
                return View(new List<LocationViewModel>());
            }
        }

        /// <summary>
        /// Create Location - tạo địa điểm mới
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> CreateLocation()
        {
            try
            {
                await SetAdminViewBagAsync();
                var model = new LocationCreateViewModel();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateLocation GET action");
                TempData["Error"] = "Có lỗi xảy ra khi tải trang tạo địa điểm";
                return RedirectToAction(nameof(Locations));
            }
        }

        /// <summary>
        /// Create Location POST - xử lý tạo địa điểm
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLocation(LocationCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await SetAdminViewBagAsync();
                    return View(model);
                }

                var result = await _locationService.CreateLocationAsync(model);

                if (result)
                {
                    TempData["Success"] = "Tạo địa điểm thành công";
                    return RedirectToAction(nameof(Locations));
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi tạo địa điểm";
                    await SetAdminViewBagAsync();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateLocation POST action");
                TempData["Error"] = "Có lỗi xảy ra khi tạo địa điểm";
                await SetAdminViewBagAsync();
                return View(model);
            }
        }

        /// <summary>
        /// Edit Location - chỉnh sửa địa điểm
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditLocation(int id)
        {
            try
            {
                await SetAdminViewBagAsync();
                var location = await _locationService.GetLocationByIdAsync(id);
                if (location == null)
                {
                    TempData["Error"] = "Không tìm thấy địa điểm";
                    return RedirectToAction(nameof(Locations));
                }

                var model = new LocationEditViewModel
                {
                    LocationId = location.LocationId,
                    Name = location.Name,
                    Address = location.Address,
                    District = location.District,
                    City = location.City,
                    Status = location.Status,
                    PartnerId = location.PartnerId,
                    GgmapLink = location.GgmapLink,
                    CreatedAt = location.CreatedAt
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EditLocation GET action for location {LocationId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin địa điểm";
                return RedirectToAction(nameof(Locations));
            }
        }

        /// <summary>
        /// Edit Location POST - xử lý cập nhật địa điểm
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLocation(LocationEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await SetAdminViewBagAsync();
                    return View(model);
                }

                var result = await _locationService.UpdateLocationAsync(model);

                if (result)
                {
                    TempData["Success"] = "Cập nhật địa điểm thành công";
                    return RedirectToAction(nameof(Locations));
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy địa điểm hoặc có lỗi xảy ra";
                    await SetAdminViewBagAsync();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EditLocation POST action");
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật địa điểm";
                await SetAdminViewBagAsync();
                return View(model);
            }
        }

        /// <summary>
        /// Location Detail - chi tiết địa điểm
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> LocationDetail(int id)
        {
            try
            {
                await SetAdminViewBagAsync();
                var location = await _locationService.GetLocationByIdAsync(id);
                if (location == null)
                {
                    TempData["Error"] = "Không tìm thấy địa điểm";
                    return RedirectToAction(nameof(Locations));
                }

                return View(location);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LocationDetail action for location {LocationId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin địa điểm";
                return RedirectToAction(nameof(Locations));
            }
        }

        /// <summary>
        /// Delete Location - xóa địa điểm
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            try
            {
                var result = await _locationService.DeleteLocationAsync(id);

                if (result)
                {
                    TempData["Success"] = "Xóa địa điểm thành công";
                }
                else
                {
                    TempData["Error"] = "Không thể xóa địa điểm (có thể có concept liên kết)";
                }

                return RedirectToAction(nameof(Locations));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteLocation action for location {LocationId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi xóa địa điểm";
                return RedirectToAction(nameof(Locations));
            }
        }

        /// <summary>
        /// Toggle Location Status - thay đổi trạng thái địa điểm
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLocationStatus(int id, string status)
        {
            try
            {
                var result = await _locationService.ToggleLocationStatusAsync(id, status);

                if (result)
                {
                    TempData["Success"] = "Cập nhật trạng thái địa điểm thành công";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái";
                }

                return RedirectToAction(nameof(Locations));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ToggleLocationStatus action for location {LocationId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái địa điểm";
                return RedirectToAction(nameof(Locations));
            }
        }

        #endregion

        #region Concept Management

        /// <summary>
        /// Concepts - quản lý concept
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Concepts(ConceptSearchViewModel searchModel)
        {
            try
            {
                await SetAdminViewBagAsync();
                
                // Initialize searchModel if null
                if (searchModel == null)
                    searchModel = new ConceptSearchViewModel();
                
                // Set default values if not provided
                if (string.IsNullOrEmpty(searchModel.SortBy))
                    searchModel.SortBy = "name";
                if (string.IsNullOrEmpty(searchModel.SortOrder))
                    searchModel.SortOrder = "asc";
                if (searchModel.PageSize == 0)
                    searchModel.PageSize = 50; // Default page size
                
                var (concepts, totalCount) = await _conceptService.GetConceptsAsync(searchModel);

                ViewBag.TotalCount = totalCount;
                ViewBag.SearchModel = searchModel;

                return View(concepts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Concepts action");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách concept";
                await SetAdminViewBagAsync();
                return View(new List<ConceptViewModel>());
            }
        }

        /// <summary>
        /// Create Concept - tạo concept mới
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> CreateConcept()
        {
            try
            {
                await SetAdminViewBagAsync();
                var model = new ConceptCreateViewModel();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateConcept GET action");
                TempData["Error"] = "Có lỗi xảy ra khi tải trang tạo concept";
                return RedirectToAction(nameof(Concepts));
            }
        }

        /// <summary>
        /// Create Concept POST - xử lý tạo concept
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateConcept(ConceptCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await SetAdminViewBagAsync();
                    return View(model);
                }

                var result = await _conceptService.CreateConceptAsync(model);

                if (result)
                {
                    TempData["Success"] = "Tạo concept thành công";
                    return RedirectToAction(nameof(Concepts));
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi tạo concept";
                    await SetAdminViewBagAsync();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateConcept POST action");
                TempData["Error"] = "Có lỗi xảy ra khi tạo concept";
                await SetAdminViewBagAsync();
                return View(model);
            }
        }

        /// <summary>
        /// Edit Concept - chỉnh sửa concept
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditConcept(int id)
        {
            try
            {
                await SetAdminViewBagAsync();
                var concept = await _conceptService.GetConceptByIdAsync(id);
                if (concept == null)
                {
                    TempData["Error"] = "Không tìm thấy concept";
                    return RedirectToAction(nameof(Concepts));
                }

                var model = new ConceptEditViewModel
                {
                    ConceptId = concept.ConceptId,
                    Name = concept.Name,
                    Description = concept.Description,
                    Price = concept.Price,
                    LocationId = concept.LocationId,
                    ColorIds = concept.ColorIds,
                    CategoryId = concept.CategoryId,
                    AmbienceId = concept.AmbienceId,
                    PreparationTime = concept.PreparationTime,
                    AvailabilityStatus = concept.AvailabilityStatus,
                    CreatedAt = concept.CreatedAt
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EditConcept GET action for concept {ConceptId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin concept";
                return RedirectToAction(nameof(Concepts));
            }
        }

        /// <summary>
        /// Edit Concept POST - xử lý cập nhật concept
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditConcept(ConceptEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await SetAdminViewBagAsync();
                    return View(model);
                }

                var result = await _conceptService.UpdateConceptAsync(model);

                if (result)
                {
                    TempData["Success"] = "Cập nhật concept thành công";
                    return RedirectToAction(nameof(Concepts));
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy concept hoặc có lỗi xảy ra";
                    await SetAdminViewBagAsync();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EditConcept POST action");
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật concept";
                await SetAdminViewBagAsync();
                return View(model);
            }
        }

        /// <summary>
        /// Concept Detail - chi tiết concept
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> ConceptDetail(int id)
        {
            try
            {
                await SetAdminViewBagAsync();
                var concept = await _conceptService.GetConceptByIdAsync(id);
                if (concept == null)
                {
                    TempData["Error"] = "Không tìm thấy concept";
                    return RedirectToAction(nameof(Concepts));
                }

                return View(concept);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ConceptDetail action for concept {ConceptId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin concept";
                return RedirectToAction(nameof(Concepts));
            }
        }

        /// <summary>
        /// Delete Concept - xóa concept
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConcept(int id)
        {
            try
            {
                var result = await _conceptService.DeleteConceptAsync(id);

                if (result)
                {
                    TempData["Success"] = "Xóa concept thành công";
                }
                else
                {
                    TempData["Error"] = "Không thể xóa concept (có thể có booking liên kết)";
                }

                return RedirectToAction(nameof(Concepts));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteConcept action for concept {ConceptId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi xóa concept";
                return RedirectToAction(nameof(Concepts));
            }
        }

        /// <summary>
        /// Toggle Concept Status - thay đổi trạng thái concept
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleConceptStatus(int id, bool status)
        {
            try
            {
                var result = await _conceptService.ToggleConceptStatusAsync(id, status);

                if (result)
                {
                    TempData["Success"] = "Cập nhật trạng thái concept thành công";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái";
                }

                return RedirectToAction(nameof(Concepts));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ToggleConceptStatus action for concept {ConceptId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái concept";
                return RedirectToAction(nameof(Concepts));
            }
        }

        /// <summary>
        /// Get Concept Dropdown Data - lấy dữ liệu dropdown cho concept
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetConceptDropdownData()
        {
            try
            {
                var categories = await _conceptService.GetConceptCategoriesForDropdownAsync();
                var colors = await _conceptService.GetConceptColorsForDropdownAsync();
                var ambiences = await _conceptService.GetConceptAmbiencesForDropdownAsync();
                var locations = await _conceptService.GetLocationsForDropdownAsync();

                return Json(new
                {
                    success = true,
                    categories = categories,
                    colors = colors,
                    ambiences = ambiences,
                    locations = locations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetConceptDropdownData action");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải dữ liệu dropdown" });
            }
        }

        #endregion

        #region Concept Category Management

        /// <summary>
        /// Concept Categories List - danh sách danh mục concept
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ConceptCategories(ConceptCategorySearchViewModel searchModel)
        {
            try
            {
                await SetAdminViewBagAsync();
                
                // Initialize searchModel if null
                if (searchModel == null)
                    searchModel = new ConceptCategorySearchViewModel();
                
                // Set default values if not provided
                if (string.IsNullOrEmpty(searchModel.SortBy))
                    searchModel.SortBy = "name";
                if (string.IsNullOrEmpty(searchModel.SortOrder))
                    searchModel.SortOrder = "asc";
                
                var categories = await _conceptService.GetConceptCategoriesAsync(searchModel);
                ViewBag.SearchModel = searchModel;
                
                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ConceptCategories action");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách danh mục concept";
                await SetAdminViewBagAsync();
                ViewBag.SearchModel = new ConceptCategorySearchViewModel();
                return View(new List<ConceptCategoryDropdownViewModel>());
            }
        }

        /// <summary>
        /// Create Concept Category - tạo danh mục concept mới
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> CreateConceptCategory()
        {
            await SetAdminViewBagAsync();
            return View();
        }

        /// <summary>
        /// Create Concept Category POST - xử lý tạo danh mục concept
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateConceptCategory(ConceptCategoryDropdownViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _conceptService.CreateConceptCategoryAsync(model);
                if (result)
                {
                    TempData["SuccessMessage"] = "Tạo danh mục concept thành công!";
                    return RedirectToAction("ConceptCategories");
                }
                else
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo danh mục concept";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateConceptCategory POST action");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo danh mục concept";
                return View(model);
            }
        }

        /// <summary>
        /// Edit Concept Category - chỉnh sửa danh mục concept
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> EditConceptCategory(int id)
        {
            try
            {
                await SetAdminViewBagAsync();
                var category = await _conceptService.GetConceptCategoryByIdAsync(id);
                if (category == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy danh mục concept";
                    return RedirectToAction("ConceptCategories");
                }

                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EditConceptCategory action");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin danh mục concept";
                return RedirectToAction("ConceptCategories");
            }
        }

        /// <summary>
        /// Edit Concept Category POST - xử lý chỉnh sửa danh mục concept
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditConceptCategory(ConceptCategoryDropdownViewModel model)
        {
            try
            {
                _logger.LogInformation("EditConceptCategory POST called with CategoryId: {CategoryId}, Name: {Name}, Description: {Description}, IsActive: {IsActive}", 
                    model.CategoryId, model.Name, model.Description, model.IsActive);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    _logger.LogWarning("ModelState validation failed: {Errors}", string.Join(", ", errors));
                    await SetAdminViewBagAsync();
                    return View(model);
                }

                var result = await _conceptService.UpdateConceptCategoryAsync(model);
                if (result)
                {
                    _logger.LogInformation("Concept category updated successfully: {CategoryId}", model.CategoryId);
                    TempData["SuccessMessage"] = "Cập nhật danh mục concept thành công!";
                    return RedirectToAction("ConceptCategories");
                }
                else
                {
                    _logger.LogWarning("Failed to update concept category: {CategoryId}", model.CategoryId);
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật danh mục concept";
                    await SetAdminViewBagAsync();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EditConceptCategory POST action for CategoryId: {CategoryId}", model.CategoryId);
                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi cập nhật danh mục concept: {ex.Message}";
                await SetAdminViewBagAsync();
                return View(model);
            }
        }

        /// <summary>
        /// Delete Concept Category - xóa danh mục concept
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConceptCategory(int id)
        {
            try
            {
                var result = await _conceptService.DeleteConceptCategoryAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Xóa danh mục concept thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xóa danh mục concept này vì đang được sử dụng";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteConceptCategory action");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa danh mục concept";
            }

            return RedirectToAction("ConceptCategories");
        }

        #endregion

        #region Concept Color Management

        /// <summary>
        /// Concept Colors List - danh sách màu sắc concept
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ConceptColors(ConceptColorSearchViewModel searchModel)
        {
            try
            {
                await SetAdminViewBagAsync();
                
                // Initialize searchModel if null
                if (searchModel == null)
                    searchModel = new ConceptColorSearchViewModel();
                
                // Set default values if not provided
                if (string.IsNullOrEmpty(searchModel.SortBy))
                    searchModel.SortBy = "name";
                if (string.IsNullOrEmpty(searchModel.SortOrder))
                    searchModel.SortOrder = "asc";
                
                var colors = await _conceptService.GetConceptColorsAsync(searchModel);
                ViewBag.SearchModel = searchModel;
                
                return View(colors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ConceptColors action");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách màu sắc concept";
                await SetAdminViewBagAsync();
                ViewBag.SearchModel = new ConceptColorSearchViewModel();
                return View(new List<ConceptColorDropdownViewModel>());
            }
        }

        /// <summary>
        /// Create Concept Color - tạo màu sắc concept mới
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> CreateConceptColor()
        {
            await SetAdminViewBagAsync();
            return View();
        }

        /// <summary>
        /// Create Concept Color POST - xử lý tạo màu sắc concept
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateConceptColor(ConceptColorDropdownViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _conceptService.CreateConceptColorAsync(model);
                if (result)
                {
                    TempData["SuccessMessage"] = "Tạo màu sắc concept thành công!";
                    return RedirectToAction("ConceptColors");
                }
                else
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo màu sắc concept";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateConceptColor POST action");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo màu sắc concept";
                return View(model);
            }
        }

        /// <summary>
        /// Edit Concept Color - chỉnh sửa màu sắc concept
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> EditConceptColor(int id)
        {
            try
            {
                await SetAdminViewBagAsync();
                var color = await _conceptService.GetConceptColorByIdAsync(id);
                if (color == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy màu sắc concept";
                    return RedirectToAction("ConceptColors");
                }

                return View(color);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EditConceptColor action");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin màu sắc concept";
                return RedirectToAction("ConceptColors");
            }
        }

        /// <summary>
        /// Edit Concept Color POST - xử lý chỉnh sửa màu sắc concept
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditConceptColor(ConceptColorDropdownViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _conceptService.UpdateConceptColorAsync(model);
                if (result)
                {
                    TempData["SuccessMessage"] = "Cập nhật màu sắc concept thành công!";
                    return RedirectToAction("ConceptColors");
                }
                else
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật màu sắc concept";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EditConceptColor POST action");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật màu sắc concept";
                return View(model);
            }
        }

        /// <summary>
        /// Delete Concept Color - xóa màu sắc concept
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConceptColor(int id)
        {
            try
            {
                var result = await _conceptService.DeleteConceptColorAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Xóa màu sắc concept thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xóa màu sắc concept này vì đang được sử dụng";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteConceptColor action");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa màu sắc concept";
            }

            return RedirectToAction("ConceptColors");
        }

        #endregion

        #region Concept Ambience Management

        /// <summary>
        /// Concept Ambiences List - danh sách không gian concept
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ConceptAmbiences(ConceptAmbienceSearchViewModel searchModel)
        {
            try
            {
                await SetAdminViewBagAsync();
                
                // Initialize searchModel if null
                if (searchModel == null)
                    searchModel = new ConceptAmbienceSearchViewModel();
                
                // Set default values if not provided
                if (string.IsNullOrEmpty(searchModel.SortBy))
                    searchModel.SortBy = "name";
                if (string.IsNullOrEmpty(searchModel.SortOrder))
                    searchModel.SortOrder = "asc";
                
                var ambiences = await _conceptService.GetConceptAmbiencesAsync(searchModel);
                ViewBag.SearchModel = searchModel;
                
                return View(ambiences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ConceptAmbiences action");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách không gian concept";
                await SetAdminViewBagAsync();
                ViewBag.SearchModel = new ConceptAmbienceSearchViewModel();
                return View(new List<ConceptAmbienceDropdownViewModel>());
            }
        }

        /// <summary>
        /// Create Concept Ambience - tạo không gian concept mới
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> CreateConceptAmbience()
        {
            await SetAdminViewBagAsync();
            return View();
        }

        /// <summary>
        /// Create Concept Ambience POST - xử lý tạo không gian concept
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateConceptAmbience(ConceptAmbienceDropdownViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _conceptService.CreateConceptAmbienceAsync(model);
                if (result)
                {
                    TempData["SuccessMessage"] = "Tạo không gian concept thành công!";
                    return RedirectToAction("ConceptAmbiences");
                }
                else
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo không gian concept";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateConceptAmbience POST action");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo không gian concept";
                return View(model);
            }
        }

        /// <summary>
        /// Edit Concept Ambience - chỉnh sửa không gian concept
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> EditConceptAmbience(int id)
        {
            try
            {
                await SetAdminViewBagAsync();
                var ambience = await _conceptService.GetConceptAmbienceByIdAsync(id);
                if (ambience == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy không gian concept";
                    return RedirectToAction("ConceptAmbiences");
                }

                return View(ambience);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EditConceptAmbience action");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin không gian concept";
                return RedirectToAction("ConceptAmbiences");
            }
        }

        /// <summary>
        /// Edit Concept Ambience POST - xử lý chỉnh sửa không gian concept
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditConceptAmbience(ConceptAmbienceDropdownViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _conceptService.UpdateConceptAmbienceAsync(model);
                if (result)
                {
                    TempData["SuccessMessage"] = "Cập nhật không gian concept thành công!";
                    return RedirectToAction("ConceptAmbiences");
                }
                else
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật không gian concept";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EditConceptAmbience POST action");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật không gian concept";
                return View(model);
            }
        }

        /// <summary>
        /// Delete Concept Ambience - xóa không gian concept
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConceptAmbience(int id)
        {
            try
            {
                var result = await _conceptService.DeleteConceptAmbienceAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Xóa không gian concept thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xóa không gian concept này vì đang được sử dụng";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteConceptAmbience action");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa không gian concept";
            }

            return RedirectToAction("ConceptAmbiences");
        }

        #endregion

        #region Concept Image Management

        [HttpGet]
        public async Task<IActionResult> ConceptImages(int? conceptId = null, string? searchTerm = null)
        {
            try
            {
                await SetAdminViewBagAsync();
                var concepts = await _conceptService.GetConceptsForDropdownAsync();
                ViewBag.Concepts = concepts.Select(c => new SelectListItem
                {
                    Value = c.ConceptId.ToString(),
                    Text = c.Name
                }).ToList();
                ViewBag.SearchTerm = searchTerm;

                List<object> conceptImagesGrouped = new List<object>();

                if (conceptId.HasValue)
                {
                    // Lấy hình ảnh của concept cụ thể
                    var images = await _conceptService.GetConceptImagesAsync(conceptId.Value);
                    var concept = concepts.FirstOrDefault(c => c.ConceptId == conceptId.Value);
                    if (concept != null)
                    {
                        conceptImagesGrouped.Add(new
                        {
                            ConceptId = concept.ConceptId,
                            ConceptName = concept.Name,
                            Images = images
                        });
                    }
                    ViewBag.SelectedConceptId = conceptId;
                }
                else if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    // Tìm kiếm concept theo tên
                    var searchResults = await _conceptService.SearchConceptsByNameAsync(searchTerm);
                    
                    foreach (var concept in searchResults)
                    {
                        var images = await _conceptService.GetConceptImagesAsync(concept.ConceptId);
                        if (images.Any())
                        {
                            conceptImagesGrouped.Add(new
                            {
                                ConceptId = concept.ConceptId,
                                ConceptName = concept.Name,
                                Images = images
                            });
                        }
                    }
                }
                else
                {
                    // Lấy tất cả concept (kể cả chưa có hình ảnh)
                    foreach (var concept in concepts)
                    {
                        var images = await _conceptService.GetConceptImagesAsync(concept.ConceptId);
                        conceptImagesGrouped.Add(new
                        {
                            ConceptId = concept.ConceptId,
                            ConceptName = concept.Name,
                            Images = images
                        });
                    }
                }

                return View(conceptImagesGrouped);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ConceptImages action");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách hình ảnh concept";
                await SetAdminViewBagAsync();
                return View(new List<object>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddConceptImage(ConceptImgViewModel model, IFormFile imageFile)
        {
            try
            {
                if (imageFile == null || imageFile.Length == 0)
                {
                    return Json(new { success = false, message = "Vui lòng chọn file hình ảnh!" });
                }

                // Check if the specific position already has an image
                var existingImages = await _conceptService.GetConceptImagesAsync(model.ConceptId);
                var existingImageAtPosition = existingImages.FirstOrDefault(img => img.DisplayOrder == model.DisplayOrder);
                
                if (existingImageAtPosition != null)
                {
                    return Json(new { success = false, message = $"Vị trí {model.DisplayOrder} đã có hình ảnh! Vui lòng sử dụng chức năng chỉnh sửa để thay thế." });
                }

                try
                {
                    // Upload to Cloudinary
                    var imageUrl = await _cloudinaryService.UploadImageAsync(imageFile, "concepts");
                    
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        model.ImgUrl = imageUrl;
                        _logger.LogInformation("Concept image uploaded successfully to Cloudinary: {Url}", imageUrl);
                    }
                    else
                    {
                        return Json(new { success = false, message = "Không thể upload hình ảnh. Vui lòng thử lại." });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading concept image to Cloudinary");
                    return Json(new { success = false, message = "Có lỗi xảy ra khi upload hình ảnh. Vui lòng thử lại." });
                }

                // Set default values
                if (string.IsNullOrEmpty(model.ImgName))
                {
                    model.ImgName = "Hình ảnh concept";
                }
                if (string.IsNullOrEmpty(model.AltText))
                {
                    model.AltText = "Hình ảnh concept";
                }
                if (model.DisplayOrder == 0)
                {
                    model.DisplayOrder = 1;
                }

                var result = await _conceptService.AddConceptImageAsync(model);
                if (result)
                {
                    return Json(new { success = true, message = "Thêm hình ảnh concept thành công!" });
                }
                else
                {
                    // If database operation failed, try to delete the uploaded image from Cloudinary
                    if (!string.IsNullOrEmpty(model.ImgUrl))
                    {
                        var publicId = _cloudinaryService.ExtractPublicIdFromUrl(model.ImgUrl);
                        if (!string.IsNullOrEmpty(publicId))
                        {
                            await _cloudinaryService.DeleteImageAsync(publicId);
                        }
                    }
                    return Json(new { success = false, message = "Không thể thêm hình ảnh concept!" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddConceptImage action");
                return Json(new { success = false, message = "Có lỗi xảy ra khi thêm hình ảnh concept!" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateConceptImage(ConceptImgViewModel model, IFormFile? imageFile)
        {
            try
            {
                var existingImage = await _conceptService.GetConceptImageByIdAsync(model.ImgId);
                if (existingImage == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hình ảnh!" });
                }

                // If new image file is provided
                if (imageFile != null && imageFile.Length > 0)
                {
                    try
                    {
                        // Delete old image if it exists
                        if (!string.IsNullOrEmpty(existingImage.ImgUrl))
                        {
                            var publicId = _cloudinaryService.ExtractPublicIdFromUrl(existingImage.ImgUrl);
                            if (!string.IsNullOrEmpty(publicId))
                            {
                                await _cloudinaryService.DeleteImageAsync(publicId);
                            }
                        }
                        
                        // Upload new image to Cloudinary
                        var imageUrl = await _cloudinaryService.UploadImageAsync(imageFile, "concepts");
                        
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            model.ImgUrl = imageUrl;
                            _logger.LogInformation("Concept image updated successfully to Cloudinary: {Url}", imageUrl);
                        }
                        else
                        {
                            return Json(new { success = false, message = "Không thể upload hình ảnh. Vui lòng thử lại." });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading concept image to Cloudinary");
                        return Json(new { success = false, message = "Có lỗi xảy ra khi upload hình ảnh. Vui lòng thử lại." });
                    }
                }
                else
                {
                    // Keep existing image URL
                    model.ImgUrl = existingImage.ImgUrl;
                }

                var result = await _conceptService.UpdateConceptImageAsync(model);
                if (result)
                {
                    return Json(new { success = true, message = "Cập nhật hình ảnh concept thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể cập nhật hình ảnh concept!" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateConceptImage action");
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật hình ảnh concept!" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConceptImage(int imageId)
        {
            try
            {
                var existingImage = await _conceptService.GetConceptImageByIdAsync(imageId);
                if (existingImage == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hình ảnh!" });
                }

                var result = await _conceptService.DeleteConceptImageAsync(imageId);
                if (result)
                {
                    // Delete image from Cloudinary
                    if (!string.IsNullOrEmpty(existingImage.ImgUrl))
                    {
                        var publicId = _cloudinaryService.ExtractPublicIdFromUrl(existingImage.ImgUrl);
                        if (!string.IsNullOrEmpty(publicId))
                        {
                            await _cloudinaryService.DeleteImageAsync(publicId);
                        }
                    }

                    return Json(new { success = true, message = "Xóa hình ảnh concept thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể xóa hình ảnh concept!" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteConceptImage action");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa hình ảnh concept!" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetPrimaryConceptImage(int conceptId, int imageId)
        {
            try
            {
                var result = await _conceptService.SetPrimaryImageAsync(conceptId, imageId);
                if (result)
                {
                    return Json(new { success = true, message = "Đặt hình ảnh chính thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể đặt hình ảnh chính!" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SetPrimaryConceptImage action");
                return Json(new { success = false, message = "Có lỗi xảy ra khi đặt hình ảnh chính!" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetConceptImageCount(int conceptId)
        {
            try
            {
                var images = await _conceptService.GetConceptImagesAsync(conceptId);
                var count = images.Count;
                var occupiedPositions = images.Select(img => img.DisplayOrder).ToList();
                var availablePositions = new List<int>();
                
                // Check which positions (1-6) are available
                for (int i = 1; i <= 6; i++)
                {
                    if (!occupiedPositions.Contains(i))
                    {
                        availablePositions.Add(i);
                    }
                }
                
                var canAddMore = availablePositions.Any();

                return Json(new
                {
                    success = true,
                    count = count,
                    canAddMore = canAddMore,
                    occupiedPositions = occupiedPositions,
                    availablePositions = availablePositions,
                    message = canAddMore ? $"Có thể thêm hình ảnh vào {availablePositions.Count} vị trí còn trống" : "Tất cả 6 vị trí đã có hình ảnh"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetConceptImageCount action");
                return Json(new { success = false, message = "Có lỗi xảy ra khi kiểm tra số lượng hình ảnh!" });
            }
        }

        #endregion

        #region Order Management

        [HttpGet]
        public async Task<IActionResult> Orders(OrderSearchViewModel? searchModel)
        {
            try
            {
                await SetAdminViewBagAsync();

                searchModel ??= new OrderSearchViewModel();
                
                // Set a large page size for admin to see all orders
                if (searchModel.PageSize == 10) // Only override if using default
                {
                    searchModel.PageSize = 1000; // Show up to 1000 orders
                }

                var (orders, totalCount) = await _orderService.GetOrdersAsync(searchModel);
                var statistics = await _orderService.GetOrderStatisticsAsync();
                var orderStatuses = await _orderService.GetOrderStatusesAsync();

                // Helper: resolve payment status from Payment table (preferred),
                // fall back to any existing Order.PaymentStatus property if present
                ViewBag.GetPaymentStatus = (Func<dynamic, string>)(o =>
                {
                    try
                    {
                        // Try common casings first
                        var s = (string)(o?.Payment?.Status ?? o?.payment?.status ?? o?.PaymentStatus ?? o?.paymentStatus ?? string.Empty);
                        return s ?? string.Empty;
                    }
                    catch
                    {
                        return string.Empty;
                    }
                });

                // Provide a simple translator for payment status labels in views
                ViewBag.TranslatePaymentStatus = (Func<string, string>)(code =>
                {
                    if (string.IsNullOrWhiteSpace(code)) return string.Empty;
                    var v = code.Trim();
                    return string.Equals(v, "pending", StringComparison.OrdinalIgnoreCase)
                        ? "Đang xử lí"
                        : v; // giữ nguyên các trạng thái khác
                });

                ViewBag.Orders = orders;
                ViewBag.TotalCount = totalCount;
                ViewBag.Statistics = statistics;
                ViewBag.OrderStatuses = orderStatuses;
                ViewBag.SearchModel = searchModel;

                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Orders action");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách đơn hàng";
                return View(new List<OrderViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> OrderDetail(int id)
        {
            try
            {
                await SetAdminViewBagAsync();

                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng";
                    return RedirectToAction("Orders");
                }

                var orderStatuses = await _orderService.GetOrderStatusesAsync();

                // Same payment status translator for detail view
                ViewBag.GetPaymentStatus = (Func<dynamic, string>)(o =>
                {
                    try
                    {
                        var s = (string)(o?.Payment?.Status ?? o?.payment?.status ?? o?.PaymentStatus ?? o?.paymentStatus ?? string.Empty);
                        return s ?? string.Empty;
                    }
                    catch
                    {
                        return string.Empty;
                    }
                });
                ViewBag.TranslatePaymentStatus = (Func<string, string>)(code =>
                {
                    if (string.IsNullOrWhiteSpace(code)) return string.Empty;
                    var v = code.Trim();
                    return string.Equals(v, "pending", StringComparison.OrdinalIgnoreCase)
                        ? "Đang xử lí"
                        : v;
                });
                var shippingOptions = await _orderService.GetShippingOptionsAsync();

                ViewBag.OrderStatuses = orderStatuses;
                ViewBag.ShippingOptions = shippingOptions;

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OrderDetail action");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải chi tiết đơn hàng";
                return RedirectToAction("Orders");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrder(OrderEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Dữ liệu không hợp lệ";
                    return RedirectToAction("OrderDetail", new { id = model.OrderId });
                }

                var result = await _orderService.UpdateOrderAsync(model);
                if (result)
                {
                    TempData["SuccessMessage"] = "Cập nhật đơn hàng thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể cập nhật đơn hàng";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateOrder action");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật đơn hàng";
            }

            return RedirectToAction("OrderDetail", new { id = model.OrderId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status, string returnUrl = null)
        {
            try
            {
                var result = await _orderService.UpdateOrderStatusAsync(orderId, status);
                if (result)
                {
                    TempData["SuccessMessage"] = "Cập nhật trạng thái đơn hàng thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể cập nhật trạng thái đơn hàng";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateOrderStatus action");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật trạng thái đơn hàng";
            }

            // Redirect về trang gốc hoặc Orders mặc định
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Orders");
        }


        [HttpGet]
        public async Task<IActionResult> GetOrderStatistics()
        {
            try
            {
                var statistics = await _orderService.GetOrderStatisticsAsync();
                var totalRevenue = await _orderService.GetTotalRevenueAsync();
                var recentOrders = await _orderService.GetRecentOrdersAsync(5);

                return Json(new
                {
                    success = true,
                    statistics = statistics,
                    totalRevenue = totalRevenue,
                    recentOrders = recentOrders
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOrderStatistics action");
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy thống kê đơn hàng" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(int orderId, decimal amount, int paymentMethodId)
        {
            try
            {
                var result = await _orderService.ProcessOrderPaymentAsync(orderId, amount, paymentMethodId);
                if (result)
                {
                    return Json(new { success = true, message = "Xử lý thanh toán thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể xử lý thanh toán" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProcessPayment action");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xử lý thanh toán" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(int orderId, string returnUrl = null)
        {
            try
            {
                _logger.LogInformation($"AdminController.CancelOrder called for OrderId: {orderId}");
                
                var result = await _orderService.CancelOrderAsync(orderId);
                if (result)
                {
                    TempData["SuccessMessage"] = $"Hủy đơn hàng #{orderId} thành công!";
                    _logger.LogInformation($"Successfully cancelled order {orderId}");
                }
                else
                {
                    TempData["ErrorMessage"] = $"Không thể hủy đơn hàng #{orderId}. Đơn hàng có thể đã được xác nhận, đang giao hàng hoặc đã hoàn thành.";
                    _logger.LogWarning($"Failed to cancel order {orderId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in CancelOrder action for OrderId: {orderId}");
                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi hủy đơn hàng #{orderId}";
            }

            // Redirect về trang gốc hoặc Orders mặc định
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Orders");
        }

        #endregion

    }
}
