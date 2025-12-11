using Microsoft.AspNetCore.Mvc;
using MonAmour.Services.Interfaces;
using MonAmour.ViewModels;

namespace MonAmour.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBannerService _bannerService;
        private readonly IPartnerService _partnerService;
        private readonly MonAmour.Models.MonAmourDbContext _db;
        private readonly IConfiguration _config;

        public HomeController(IBannerService bannerService, IPartnerService partnerService, MonAmour.Models.MonAmourDbContext db, IConfiguration config)
        {
            _bannerService = bannerService;
            _partnerService = partnerService;
            _db = db;
            _config = config;
        }
        public async Task<IActionResult> Consultation()
        {
            List<string> locations = new List<string> {
            "Ba Đình",
            "Hoàn Kiếm",
            "Hai Bà Trưng",
            "Đống Đa",
            "Cầu Giấy",
            "Tây Hồ",
            "Thanh Xuân",
            "Hoàng Mai",
            "Long Biên",
            "Bắc Từ Liêm",
            "Nam Từ Liêm",
            "Hà Đông"
        };
            List<string> categories = new List<string> {
            "Sinh nhật", "Kỷ niệm", "Hẹn hò lần đầu", "Hẹn hò bình thường"
        };
            ViewData["Title"] = "Liên hệ tư vấn - MonAmour";
            //ViewBag.Categories = await _db.ConceptCategories
            //    .AsNoTracking()
            //    .OrderBy(c => c.Name)
            //    .ToListAsync();
            ViewBag.Categories = categories;
            //ViewBag.Cities = await _db.Locations
            //    .AsNoTracking()
            //    .Where(l => l.City != null && l.City != "")
            //    .Select(l => l.City!)
            //    .Distinct()
            //    .OrderBy(c => c)
            //    .ToListAsync();
            ViewBag.Cities = locations;
            return View();
        }
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Trang chủ - MonAmour";

            try
            {
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
            catch (Exception ex)
            {
                // Log the error and return a simple view
                Console.WriteLine($"Database connection error: {ex.Message}");
                var model = new HomeIndexViewModel
                {
                    HomepageBanners = new List<BannerHomepageListViewModel>(),
                    ServiceBanners = new List<BannerServiceListViewModel>(),
                    Partners = new List<PartnerViewModel>()
                };

                return View(model);
            }
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Content("Test endpoint is working!");
        }

        [HttpGet("health")]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                // Test database connection
                var bannerCount = await _bannerService.GetAllBannerHomepagesAsync();
                var partnerResult = await _partnerService.GetPartnersAsync(new PartnerSearchViewModel
                {
                    Page = 1,
                    PageSize = 1
                });

                return Json(new
                {
                    status = "healthy",
                    database = "connected",
                    timestamp = DateTime.UtcNow,
                    bannerCount = bannerCount.Count,
                    partnerCount = partnerResult.partners.Count,
                    totalPartners = partnerResult.totalCount
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = "unhealthy",
                    database = "disconnected",
                    error = ex.Message,
                    innerException = ex.InnerException?.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// API endpoint để lấy chatbot API keys
        /// GET /api/chatbot/config
        /// </summary>
        [HttpGet("api/chatbot/config")]
        public IActionResult GetChatbotConfig()
        {
            return Json(new
            {
                geminiApiKey = _config["Chatbot:GeminiApiKey"],
                openAIApiKey = _config["Chatbot:OpenAIApiKey"]
            });
        }
    }
}