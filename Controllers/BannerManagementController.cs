using Microsoft.AspNetCore.Mvc;
using MonAmour.Attributes;
using MonAmour.Services.Interfaces;
using MonAmour.ViewModels;
using MonAmour.Helpers;

namespace MonAmour.Controllers
{
    [AdminOnly]
    public class BannerManagementController : Controller
    {
        private readonly IBannerService _bannerService;
        private readonly IUserManagementService _userManagementService;
        private readonly ILogger<BannerManagementController> _logger;

        public BannerManagementController(IBannerService bannerService, IUserManagementService userManagementService, ILogger<BannerManagementController> logger)
        {
            _bannerService = bannerService;
            _userManagementService = userManagementService;
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

        #region BannerService Actions

        [HttpGet]
        public async Task<IActionResult> BannerServices()
        {
            try
            {
                await SetAdminViewBagAsync();
                var banners = await _bannerService.GetAllBannerServicesAsync();
                return View(banners);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading banner services page");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách banner dịch vụ";
                await SetAdminViewBagAsync();
                return View(new List<BannerServiceListViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateBannerService()
        {
            await SetAdminViewBagAsync();
            var model = new BannerServiceCreateViewModel();
            model.DisplayOrder = await _bannerService.GetNextServiceDisplayOrderAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBannerService(BannerServiceCreateViewModel model)
        {
            // Validate display order
            if (await _bannerService.IsServiceDisplayOrderExistsAsync(model.DisplayOrder))
            {
                ModelState.AddModelError("DisplayOrder", "Số thứ tự này đã tồn tại. Vui lòng chọn số khác.");
            }

            if (!ModelState.IsValid)
            {
                await SetAdminViewBagAsync();
                return View(model);
            }

            try
            {
                var result = await _bannerService.CreateBannerServiceAsync(model);
                if (result)
                {
                    TempData["Success"] = "Tạo banner dịch vụ thành công";
                    return RedirectToAction(nameof(BannerServices));
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi tạo banner dịch vụ";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating banner service");
                TempData["Error"] = "Có lỗi xảy ra khi tạo banner dịch vụ";
            }

            await SetAdminViewBagAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditBannerService(int id)
        {
            try
            {
                var banner = await _bannerService.GetBannerServiceByIdAsync(id);
                if (banner == null)
                {
                    TempData["Error"] = "Không tìm thấy banner dịch vụ";
                    return RedirectToAction(nameof(BannerServices));
                }

                var model = new BannerServiceEditViewModel
                {
                    BannerId = banner.BannerId,
                    ImgUrl = banner.ImgUrl,
                    IsPrimary = banner.IsPrimary,
                    DisplayOrder = banner.DisplayOrder,
                    Description = banner.Description,
                    IsActive = banner.IsActive
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading banner service for edit");
                TempData["Error"] = "Có lỗi xảy ra khi tải banner dịch vụ";
                return RedirectToAction(nameof(BannerServices));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBannerService(BannerServiceEditViewModel model)
        {
            // Validate display order (exclude current banner)
            if (await _bannerService.IsServiceDisplayOrderExistsAsync(model.DisplayOrder, model.BannerId))
            {
                ModelState.AddModelError("DisplayOrder", "Số thứ tự này đã tồn tại. Vui lòng chọn số khác.");
            }

            if (!ModelState.IsValid)
            {
                await SetAdminViewBagAsync();
                return View(model);
            }

            try
            {
                var result = await _bannerService.UpdateBannerServiceAsync(model);
                if (result)
                {
                    TempData["Success"] = "Cập nhật banner dịch vụ thành công";
                    return RedirectToAction(nameof(BannerServices));
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật banner dịch vụ";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating banner service");
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật banner dịch vụ";
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBannerService(int id)
        {
            try
            {
                var result = await _bannerService.DeleteBannerServiceAsync(id);
                if (result)
                {
                    TempData["Success"] = "Xóa banner dịch vụ thành công";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi xóa banner dịch vụ";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting banner service");
                TempData["Error"] = "Có lỗi xảy ra khi xóa banner dịch vụ";
            }

            return RedirectToAction(nameof(BannerServices));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleBannerServiceStatus(int id)
        {
            try
            {
                var result = await _bannerService.ToggleBannerServiceStatusAsync(id);
                if (result)
                {
                    TempData["Success"] = "Cập nhật trạng thái banner dịch vụ thành công";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái banner dịch vụ";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling banner service status");
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái banner dịch vụ";
            }

            return RedirectToAction(nameof(BannerServices));
        }

        [HttpPost]
        public async Task<IActionResult> SetPrimaryBannerService(int id)
        {
            try
            {
                var result = await _bannerService.SetPrimaryBannerServiceAsync(id);
                if (result)
                {
                    TempData["Success"] = "Đặt banner dịch vụ chính thành công";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi đặt banner dịch vụ chính";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting primary banner service");
                TempData["Error"] = "Có lỗi xảy ra khi đặt banner dịch vụ chính";
            }

            return RedirectToAction(nameof(BannerServices));
        }

        #endregion

        #region BannerHomepage Actions

        [HttpGet]
        public async Task<IActionResult> BannerHomepages()
        {
            try
            {
                await SetAdminViewBagAsync();
                var banners = await _bannerService.GetAllBannerHomepagesAsync();
                return View(banners);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading banner homepages page");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách banner trang chủ";
                await SetAdminViewBagAsync();
                return View(new List<BannerHomepageListViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateBannerHomepage()
        {
            await SetAdminViewBagAsync();
            var model = new BannerHomepageCreateViewModel();
            model.DisplayOrder = await _bannerService.GetNextDisplayOrderAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBannerHomepage(BannerHomepageCreateViewModel model)
        {
            // Validate display order
            if (await _bannerService.IsDisplayOrderExistsAsync(model.DisplayOrder))
            {
                ModelState.AddModelError("DisplayOrder", "Số thứ tự này đã tồn tại. Vui lòng chọn số khác.");
            }

            if (!ModelState.IsValid)
            {
                await SetAdminViewBagAsync();
                return View(model);
            }

            try
            {
                var result = await _bannerService.CreateBannerHomepageAsync(model);
                if (result)
                {
                    TempData["Success"] = "Tạo banner trang chủ thành công";
                    return RedirectToAction(nameof(BannerHomepages));
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi tạo banner trang chủ";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating banner homepage");
                TempData["Error"] = "Có lỗi xảy ra khi tạo banner trang chủ";
            }

            await SetAdminViewBagAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditBannerHomepage(int id)
        {
            try
            {
                var banner = await _bannerService.GetBannerHomepageByIdAsync(id);
                if (banner == null)
                {
                    TempData["Error"] = "Không tìm thấy banner trang chủ";
                    return RedirectToAction(nameof(BannerHomepages));
                }

                var model = new BannerHomepageEditViewModel
                {
                    BannerId = banner.BannerId,
                    ImgUrl = banner.ImgUrl,
                    IsPrimary = banner.IsPrimary,
                    DisplayOrder = banner.DisplayOrder,
                    Description = banner.Description,
                    IsActive = banner.IsActive
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading banner homepage for edit");
                TempData["Error"] = "Có lỗi xảy ra khi tải banner trang chủ";
                return RedirectToAction(nameof(BannerHomepages));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBannerHomepage(BannerHomepageEditViewModel model)
        {
            // Validate display order (exclude current banner)
            if (await _bannerService.IsDisplayOrderExistsAsync(model.DisplayOrder, model.BannerId))
            {
                ModelState.AddModelError("DisplayOrder", "Số thứ tự này đã tồn tại. Vui lòng chọn số khác.");
            }

            if (!ModelState.IsValid)
            {
                await SetAdminViewBagAsync();
                return View(model);
            }

            try
            {
                var result = await _bannerService.UpdateBannerHomepageAsync(model);
                if (result)
                {
                    TempData["Success"] = "Cập nhật banner trang chủ thành công";
                    return RedirectToAction(nameof(BannerHomepages));
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật banner trang chủ";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating banner homepage");
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật banner trang chủ";
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBannerHomepage(int id)
        {
            try
            {
                var result = await _bannerService.DeleteBannerHomepageAsync(id);
                if (result)
                {
                    TempData["Success"] = "Xóa banner trang chủ thành công";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi xóa banner trang chủ";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting banner homepage");
                TempData["Error"] = "Có lỗi xảy ra khi xóa banner trang chủ";
            }

            return RedirectToAction(nameof(BannerHomepages));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleBannerHomepageStatus(int id)
        {
            try
            {
                var result = await _bannerService.ToggleBannerHomepageStatusAsync(id);
                if (result)
                {
                    TempData["Success"] = "Cập nhật trạng thái banner trang chủ thành công";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái banner trang chủ";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling banner homepage status");
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái banner trang chủ";
            }

            return RedirectToAction(nameof(BannerHomepages));
        }

        [HttpPost]
        public async Task<IActionResult> SetPrimaryBannerHomepage(int id)
        {
            try
            {
                var result = await _bannerService.SetPrimaryBannerHomepageAsync(id);
                if (result)
                {
                    TempData["Success"] = "Đặt banner trang chủ chính thành công";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi đặt banner trang chủ chính";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting primary banner homepage");
                TempData["Error"] = "Có lỗi xảy ra khi đặt banner trang chủ chính";
            }

            return RedirectToAction(nameof(BannerHomepages));
        }

        #endregion

        #region BannerProduct Actions

        [HttpGet]
        public async Task<IActionResult> BannerProducts()
        {
            try
            {
                await SetAdminViewBagAsync();
                var banners = await _bannerService.GetAllBannerProductsAsync();
                return View(banners);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading banner products page");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách banner sản phẩm";
                await SetAdminViewBagAsync();
                return View(new List<BannerProductListViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateBannerProduct()
        {
            await SetAdminViewBagAsync();
            var model = new BannerProductCreateViewModel();
            model.DisplayOrder = await _bannerService.GetNextProductDisplayOrderAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBannerProduct(BannerProductCreateViewModel model)
        {
            // Validate display order
            if (await _bannerService.IsProductDisplayOrderExistsAsync(model.DisplayOrder))
            {
                ModelState.AddModelError("DisplayOrder", "Số thứ tự này đã tồn tại. Vui lòng chọn số khác.");
            }

            if (!ModelState.IsValid)
            {
                await SetAdminViewBagAsync();
                return View(model);
            }

            try
            {
                var result = await _bannerService.CreateBannerProductAsync(model);
                if (result)
                {
                    TempData["Success"] = "Tạo banner sản phẩm thành công";
                    return RedirectToAction(nameof(BannerProducts));
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi tạo banner sản phẩm";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating banner product");
                TempData["Error"] = "Có lỗi xảy ra khi tạo banner sản phẩm";
            }

            await SetAdminViewBagAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditBannerProduct(int id)
        {
            try
            {
                var banner = await _bannerService.GetBannerProductByIdAsync(id);
                if (banner == null)
                {
                    TempData["Error"] = "Không tìm thấy banner sản phẩm";
                    return RedirectToAction(nameof(BannerProducts));
                }

                var model = new BannerProductEditViewModel
                {
                    BannerId = banner.BannerId,
                    ImgUrl = banner.ImgUrl,
                    IsPrimary = banner.IsPrimary,
                    DisplayOrder = banner.DisplayOrder,
                    Description = banner.Description,
                    IsActive = banner.IsActive
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading banner product for edit");
                TempData["Error"] = "Có lỗi xảy ra khi tải banner sản phẩm";
                return RedirectToAction(nameof(BannerProducts));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBannerProduct(BannerProductEditViewModel model)
        {
            // Validate display order (exclude current banner)
            if (await _bannerService.IsProductDisplayOrderExistsAsync(model.DisplayOrder, model.BannerId))
            {
                ModelState.AddModelError("DisplayOrder", "Số thứ tự này đã tồn tại. Vui lòng chọn số khác.");
            }

            if (!ModelState.IsValid)
            {
                await SetAdminViewBagAsync();
                return View(model);
            }

            try
            {
                var result = await _bannerService.UpdateBannerProductAsync(model);
                if (result)
                {
                    TempData["Success"] = "Cập nhật banner sản phẩm thành công";
                    return RedirectToAction(nameof(BannerProducts));
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật banner sản phẩm";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating banner product");
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật banner sản phẩm";
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBannerProduct(int id)
        {
            try
            {
                var result = await _bannerService.DeleteBannerProductAsync(id);
                if (result)
                {
                    TempData["Success"] = "Xóa banner sản phẩm thành công";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi xóa banner sản phẩm";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting banner product");
                TempData["Error"] = "Có lỗi xảy ra khi xóa banner sản phẩm";
            }

            return RedirectToAction(nameof(BannerProducts));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleBannerProductStatus(int id)
        {
            try
            {
                var result = await _bannerService.ToggleBannerProductStatusAsync(id);
                if (result)
                {
                    TempData["Success"] = "Cập nhật trạng thái banner sản phẩm thành công";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái banner sản phẩm";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling banner product status");
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái banner sản phẩm";
            }

            return RedirectToAction(nameof(BannerProducts));
        }

        [HttpPost]
        public async Task<IActionResult> SetPrimaryBannerProduct(int id)
        {
            try
            {
                var result = await _bannerService.SetPrimaryBannerProductAsync(id);
                if (result)
                {
                    TempData["Success"] = "Đặt banner sản phẩm chính thành công";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi đặt banner sản phẩm chính";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting primary banner product");
                TempData["Error"] = "Có lỗi xảy ra khi đặt banner sản phẩm chính";
            }

            return RedirectToAction(nameof(BannerProducts));
        }

        #endregion
    }
}
