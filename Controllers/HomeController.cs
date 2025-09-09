using Microsoft.AspNetCore.Mvc;
using MonAmour.Services.Interfaces;
using MonAmour.ViewModels;
using MonAmour.Services.Interfaces;

namespace MonAmour.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBannerService _bannerService;
        private readonly IPartnerService _partnerService;

        public HomeController(IBannerService bannerService, IPartnerService partnerService)
        {
            _bannerService = bannerService;
            _partnerService = partnerService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Trang chủ - MonAmour";

            var homepageBanners = await _bannerService.GetAllBannerHomepagesAsync();
            var serviceBanners = await _bannerService.GetAllBannerServicesAsync();

            var (partners, _) = await _partnerService.GetPartnersAsync(new PartnerSearchViewModel
            {
                Status = "Active",
                Page = 1,
                PageSize = 50,
                SortBy = "Name",
                SortOrder = "asc"
            });

            var model = new HomeIndexViewModel
            {
                HomepageBanners = homepageBanners.Where(b => b.IsActive).OrderBy(b => b.DisplayOrder).ToList(),
                ServiceBanners = serviceBanners.Where(b => b.IsActive).OrderBy(b => b.DisplayOrder).ToList(),
                Partners = partners
            };

            return View(model);
        }
    }
}