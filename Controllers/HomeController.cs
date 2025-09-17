using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonAmour.Services.Interfaces;
using MonAmour.ViewModels;

namespace MonAmour.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBannerService _bannerService;
        private readonly IPartnerService _partnerService;
        private readonly MonAmour.Models.MonAmourDbContext _db;

        public HomeController(IBannerService bannerService, IPartnerService partnerService, MonAmour.Models.MonAmourDbContext db)
        {
            _bannerService = bannerService;
            _partnerService = partnerService;
            _db = db;
        }
        public async Task<IActionResult> Consultation()
        {
            ViewData["Title"] = "Liên hệ tư vấn - MonAmour";
            ViewBag.Categories = await _db.ConceptCategories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();
            ViewBag.Cities = await _db.Locations
                .AsNoTracking()
                .Where(l => l.City != null && l.City != "")
                .Select(l => l.City!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
            return View();
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