using Microsoft.AspNetCore.Mvc;
using MonAmour.Attributes;
using MonAmour.Helpers;
using MonAmour.Models;
using MonAmour.Services.Interfaces;
using MonAmour.ViewModels;

namespace MonAmour.Controllers
{
    [AdminOnly]
    public class AdminController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IUserManagementService _userManagementService;
        private readonly IBlogService _blogService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IAuthService authService, IUserManagementService userManagementService, IBlogService blogService, ILogger<AdminController> logger)
        {
            _authService = authService;
            _userManagementService = userManagementService;
            _blogService = blogService;
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
                ViewBag.UserStats = userStats ?? new Dictionary<string, int>
                {
                    ["TotalUsers"] = 0,
                    ["VerifiedUsers"] = 0,
                    ["AdminUsers"] = 0,
                    ["ActiveUsers"] = 0
                };

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                await SetAdminViewBagAsync();
                ViewBag.UserStats = new Dictionary<string, int>
                {
                    ["TotalUsers"] = 0,
                    ["VerifiedUsers"] = 0,
                    ["AdminUsers"] = 0,
                    ["ActiveUsers"] = 0
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
        public async Task<IActionResult> CreateUser(AdminUserViewModel.UserCreateViewModel model)
        {
            try
            {
                _logger.LogInformation("CreateUser POST called with model: Email={Email}, Name={Name}, Phone={Phone}", 
                    model.Email, model.Name, model.Phone);
                
                _logger.LogInformation("ModelState.IsValid: {IsValid}", ModelState.IsValid);
                
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
                _logger.LogInformation("AdminUserId: {AdminUserId}", adminUserId);
                
                if (!adminUserId.HasValue)
                {
                    _logger.LogError("AdminUserId is null");
                    TempData["Error"] = "Không thể xác định người dùng admin";
                    await SetAdminViewBagAsync();
                    var roles = await _userManagementService.GetAllRolesAsync();
                    ViewBag.Roles = roles;
                    return View(model);
                }
                
                _logger.LogInformation("Calling CreateUserAsync with adminUserId: {AdminUserId}", adminUserId.Value);
                var result = await _userManagementService.CreateUserAsync(model, adminUserId.Value);
                _logger.LogInformation("CreateUserAsync result: {Result}", result);

                if (result)
                {
                    _logger.LogInformation("User created successfully");
                    TempData["Success"] = "Tạo người dùng thành công";
                    return RedirectToAction(nameof(Users));
                }
                else
                {
                    _logger.LogWarning("CreateUserAsync returned false - email might already exist");
                    TempData["Error"] = "Email đã tồn tại hoặc có lỗi xảy ra";
                    await SetAdminViewBagAsync();
                    var roles = await _userManagementService.GetAllRolesAsync();
                    ViewBag.Roles = roles;
                    return View(model);
                }
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
        public async Task<IActionResult> EditUser(AdminUserViewModel.UserEditViewModel model)
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

        /// <summary>
        /// Comments Management - quản lý bình luận
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Comments()
        {
            try
            {
                await SetAdminViewBagAsync();
                
                var comments = await _blogService.GetAllCommentsAsync();
                return View(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading comments management page");
                await SetAdminViewBagAsync();
                return View(new List<BlogComment>());
            }
        }

        /// <summary>
        /// Delete Comment - xóa bình luận
        /// </summary>
        /// <param name="commentId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            try
            {
                var success = await _blogService.DeleteCommentAsync(commentId);
                if (success)
                {
                    TempData["Success"] = "Bình luận đã được xóa thành công!";
                }
                else
                {
                    TempData["Error"] = "Không thể xóa bình luận!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment {CommentId}", commentId);
                TempData["Error"] = "Có lỗi xảy ra khi xóa bình luận!";
            }

            return RedirectToAction("Comments");
        }

        /// <summary>
        /// Approve Comment - duyệt bình luận
        /// </summary>
        /// <param name="commentId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ApproveComment(int commentId)
        {
            try
            {
                var success = await _blogService.ApproveCommentAsync(commentId);
                if (success)
                {
                    TempData["Success"] = "Bình luận đã được duyệt!";
                }
                else
                {
                    TempData["Error"] = "Không thể duyệt bình luận!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving comment {CommentId}", commentId);
                TempData["Error"] = "Có lỗi xảy ra khi duyệt bình luận!";
            }

            return RedirectToAction("Comments");
        }
    }
}
