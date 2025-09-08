using Microsoft.AspNetCore.Mvc;
using MonAmour.Services.Interfaces;
using MonAmour.ViewModels;
using MonAmour.Helpers;
using MonAmour.Attributes;
using System.Diagnostics;

namespace MonAmour.Controllers
{
    [AdminOnly]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        private readonly IUserManagementService _userManagementService;
        private readonly ILogger<ReportController> _logger;

        public ReportController(IReportService reportService, IUserManagementService userManagementService, ILogger<ReportController> logger)
        {
            _reportService = reportService;
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
                ViewBag.UserName = "Admin";
                ViewBag.UserEmail = "";
                ViewBag.UserAvatar = "";
            }
        }

        // Revenue Reports
        public async Task<IActionResult> RevenueReport(ReportFilterViewModel? filter)
        {
            try
            {
                // Set common admin ViewBag data
                await SetAdminViewBagAsync();

                filter ??= new ReportFilterViewModel
                {
                    FromDate = DateTime.Now.AddMonths(-12),
                    ToDate = DateTime.Now
                };

                var report = await _reportService.GetRevenueReportAsync(filter.FromDate, filter.ToDate);
                var monthlyData = await _reportService.GetMonthlyRevenueAsync(DateTime.Now.Year);
                var dailyData = await _reportService.GetDailyRevenueAsync(
                    filter.FromDate ?? DateTime.Now.AddDays(-30), 
                    filter.ToDate ?? DateTime.Now);

                // Log data for debugging
                _logger.LogInformation($"Revenue Report - TotalRevenue: {report?.TotalRevenue}, MonthlyData count: {monthlyData?.Count}, DailyData count: {dailyData?.Count}");

                // Ensure we have data
                if (report == null)
                {
                    report = new RevenueReportViewModel();
                }

                ViewBag.Filter = filter;
                ViewBag.MonthlyData = monthlyData ?? new List<MonthlyRevenueViewModel>();
                ViewBag.DailyData = dailyData ?? new List<DailyRevenueViewModel>();
                ViewBag.RevenueReport = report;

                return View(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RevenueReport");
                var emptyReport = new RevenueReportViewModel();
                ViewBag.Filter = new ReportFilterViewModel { FromDate = DateTime.Now.AddMonths(-12), ToDate = DateTime.Now };
                ViewBag.MonthlyData = new List<MonthlyRevenueViewModel>();
                ViewBag.DailyData = new List<DailyRevenueViewModel>();
                ViewBag.RevenueReport = emptyReport;
                return View(emptyReport);
            }
        }

        // User Statistics
        public async Task<IActionResult> UserStatistics(ReportFilterViewModel? filter)
        {
            try
            {
                // Set common admin ViewBag data
                await SetAdminViewBagAsync();

                filter ??= new ReportFilterViewModel
                {
                    FromDate = DateTime.Now.AddDays(-30),
                    ToDate = DateTime.Now
                };

                var statistics = await _reportService.GetUserStatisticsAsync();
                var registrationData = await _reportService.GetUserRegistrationsAsync(
                    filter.FromDate ?? DateTime.Now.AddDays(-30), 
                    filter.ToDate ?? DateTime.Now);
                var activityData = await _reportService.GetUserActivityAsync(
                    filter.FromDate ?? DateTime.Now.AddDays(-30), 
                    filter.ToDate ?? DateTime.Now);

                // Debug logging
                _logger.LogInformation($"UserStatistics - RegistrationData count: {registrationData?.Count ?? 0}");
                _logger.LogInformation($"UserStatistics - ActivityData count: {activityData?.Count ?? 0}");
                if (registrationData?.Any() == true)
                {
                    _logger.LogInformation($"First registration data: Date={registrationData.First().Date}, NewRegistrations={registrationData.First().NewRegistrations}, TotalUsers={registrationData.First().TotalUsers}");
                }

                // Ensure we have data
                if (statistics == null)
                {
                    statistics = new UserStatisticsViewModel();
                }

                ViewBag.Filter = filter;
                ViewBag.RegistrationData = registrationData ?? new List<UserRegistrationViewModel>();
                ViewBag.ActivityData = activityData ?? new List<UserActivityViewModel>();
                ViewBag.UserStats = statistics;

                return View(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UserStatistics");
                return View(new UserStatisticsViewModel());
            }
        }

        // Data Analysis
        public async Task<IActionResult> DataAnalysis(ReportFilterViewModel? filter)
        {
            try
            {
                // Set common admin ViewBag data
                await SetAdminViewBagAsync();

                filter ??= new ReportFilterViewModel
                {
                    FromDate = DateTime.Now.AddMonths(-6),
                    ToDate = DateTime.Now
                };

                var orderStats = await _reportService.GetOrderStatisticsAsync(filter.FromDate, filter.ToDate);
                var productStats = await _reportService.GetProductStatisticsAsync();
                var bookingStats = await _reportService.GetBookingStatisticsAsync(filter.FromDate, filter.ToDate);
                var topProducts = await _reportService.GetTopSellingProductsAsync(10, filter.FromDate, filter.ToDate);
                var conceptPopularity = await _reportService.GetConceptPopularityAsync(filter.FromDate, filter.ToDate);
                var lowStockProducts = await _reportService.GetLowStockProductsAsync(10);

                // Debug logging
                _logger.LogInformation("DataAnalysis - OrderStats StatusDistribution count: {Count}", 
                    orderStats?.StatusDistribution?.Count ?? 0);
                _logger.LogInformation("DataAnalysis - ProductStats CategoryDistribution count: {Count}", 
                    productStats?.CategoryDistribution?.Count ?? 0);
                
                // Log first item details
                if (orderStats?.StatusDistribution?.Any() == true)
                {
                    var firstOrder = orderStats.StatusDistribution.First();
                    _logger.LogInformation("First Order Status: Status={Status}, StatusName={StatusName}, Count={Count}", 
                        firstOrder.Status, firstOrder.StatusName, firstOrder.Count);
                }
                else
                {
                    _logger.LogWarning("OrderStats StatusDistribution is empty or null");
                }
                
                if (productStats?.CategoryDistribution?.Any() == true)
                {
                    var firstCategory = productStats.CategoryDistribution.First();
                    _logger.LogInformation("First Product Category: CategoryName={CategoryName}, ProductCount={ProductCount}", 
                        firstCategory.CategoryName, firstCategory.ProductCount);
                }
                else
                {
                    _logger.LogWarning("ProductStats CategoryDistribution is empty or null");
                }

                // Ensure we have data
                ViewBag.Filter = filter;
                ViewBag.OrderStats = orderStats ?? new OrderStatisticsViewModel();
                ViewBag.ProductStats = productStats ?? new ProductStatisticsViewModel();
                ViewBag.BookingStats = bookingStats ?? new BookingStatisticsViewModel();
                ViewBag.TopSellingProducts = topProducts ?? new List<TopSellingProductViewModel>();
                ViewBag.PopularConcepts = conceptPopularity ?? new List<ConceptPopularityViewModel>();
                ViewBag.LowStockProducts = lowStockProducts ?? new List<LowStockProductViewModel>();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DataAnalysis");
                return View();
            }
        }

        // Partner Performance
        public async Task<IActionResult> PartnerPerformance(ReportFilterViewModel? filter)
        {
            try
            {
                // Set common admin ViewBag data
                await SetAdminViewBagAsync();

                filter ??= new ReportFilterViewModel
                {
                    FromDate = DateTime.Now.AddMonths(-6),
                    ToDate = DateTime.Now
                };

                var partnerStats = await _reportService.GetPartnerStatisticsAsync();
                var performanceData = await _reportService.GetPartnerPerformanceAsync(filter.FromDate, filter.ToDate);

                // Debug logging
                _logger.LogInformation($"PartnerPerformance: Found {performanceData?.Count ?? 0} partners");
                if (performanceData != null && performanceData.Any())
                {
                    _logger.LogInformation($"First partner: {performanceData.First().PartnerName}, Revenue: {performanceData.First().TotalRevenue}");
                }

                // Ensure we have data
                if (partnerStats == null)
                {
                    partnerStats = new PartnerStatisticsViewModel();
                }

                ViewBag.Filter = filter;
                ViewBag.PerformanceData = performanceData ?? new List<PartnerPerformanceViewModel>();
                ViewBag.PartnerStats = partnerStats;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PartnerPerformance");
                return View(new PartnerStatisticsViewModel());
            }
        }

        // Dashboard Summary
        public async Task<IActionResult> DashboardSummary()
        {
            try
            {
                // Set common admin ViewBag data
                await SetAdminViewBagAsync();

                var summary = await _reportService.GetDashboardSummaryAsync();
                return View(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DashboardSummary");
                return View(new DashboardSummaryViewModel());
            }
        }

        // API Endpoints for AJAX calls
        [HttpGet]
        public async Task<IActionResult> GetRevenueData(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var from = fromDate ?? DateTime.Now.AddMonths(-12);
                var to = toDate ?? DateTime.Now;

                var monthlyData = await _reportService.GetMonthlyRevenueAsync(to.Year);
                var dailyData = await _reportService.GetDailyRevenueAsync(from, to);

                return Json(new
                {
                    success = true,
                    monthlyData = monthlyData,
                    dailyData = dailyData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRevenueData");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserData(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var from = fromDate ?? DateTime.Now.AddDays(-30);
                var to = toDate ?? DateTime.Now;

                var registrationData = await _reportService.GetUserRegistrationsAsync(from, to);
                var activityData = await _reportService.GetUserActivityAsync(from, to);

                return Json(new
                {
                    success = true,
                    registrationData = registrationData,
                    activityData = activityData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserData");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderData(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var from = fromDate ?? DateTime.Now.AddMonths(-6);
                var to = toDate ?? DateTime.Now;

                var orderStats = await _reportService.GetOrderStatisticsAsync(from, to);
                var statusDistribution = await _reportService.GetOrderStatusDistributionAsync(from, to);
                var topProducts = await _reportService.GetTopSellingProductsAsync(10, from, to);

                return Json(new
                {
                    success = true,
                    orderStats = orderStats,
                    statusDistribution = statusDistribution,
                    topProducts = topProducts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOrderData");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBookingData(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var from = fromDate ?? DateTime.Now.AddMonths(-6);
                var to = toDate ?? DateTime.Now;

                var bookingStats = await _reportService.GetBookingStatisticsAsync(from, to);
                var statusDistribution = await _reportService.GetBookingStatusDistributionAsync(from, to);
                var conceptPopularity = await _reportService.GetConceptPopularityAsync(from, to);

                return Json(new
                {
                    success = true,
                    bookingStats = bookingStats,
                    statusDistribution = statusDistribution,
                    conceptPopularity = conceptPopularity
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetBookingData");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProductData()
        {
            try
            {
                var productStats = await _reportService.GetProductStatisticsAsync();
                var categoryDistribution = await _reportService.GetProductCategoryDistributionAsync();
                var lowStockProducts = await _reportService.GetLowStockProductsAsync(10);

                return Json(new
                {
                    success = true,
                    productStats = productStats,
                    categoryDistribution = categoryDistribution,
                    lowStockProducts = lowStockProducts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetProductData");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPartnerData(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var from = fromDate ?? DateTime.Now.AddMonths(-6);
                var to = toDate ?? DateTime.Now;

                var partnerStats = await _reportService.GetPartnerStatisticsAsync();
                var performanceData = await _reportService.GetPartnerPerformanceAsync(from, to);

                return Json(new
                {
                    success = true,
                    partnerStats = partnerStats,
                    performanceData = performanceData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPartnerData");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardData()
        {
            try
            {
                var summary = await _reportService.GetDashboardSummaryAsync();
                var recentActivities = await _reportService.GetRecentActivitiesAsync(10);

                return Json(new
                {
                    success = true,
                    summary = summary,
                    recentActivities = recentActivities
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDashboardData");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
