using MonAmour.ViewModels;

namespace MonAmour.Services.Interfaces
{
    public interface IReportService
    {
        // Revenue Reports
        Task<RevenueReportViewModel> GetRevenueReportAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<MonthlyRevenueViewModel>> GetMonthlyRevenueAsync(int year);
        Task<List<DailyRevenueViewModel>> GetDailyRevenueAsync(DateTime fromDate, DateTime toDate);
        Task<decimal> GetTotalRevenueAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<decimal> GetRevenueByStatusAsync(string status, DateTime? fromDate = null, DateTime? toDate = null);

        // User Statistics
        Task<UserStatisticsViewModel> GetUserStatisticsAsync();
        Task<List<UserRegistrationViewModel>> GetUserRegistrationsAsync(DateTime fromDate, DateTime toDate);
        Task<List<UserActivityViewModel>> GetUserActivityAsync(DateTime fromDate, DateTime toDate);
        Task<int> GetActiveUsersCountAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<int> GetNewUsersCountAsync(DateTime fromDate, DateTime toDate);

        // Order Statistics
        Task<OrderStatisticsViewModel> GetOrderStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<OrderStatusDistributionViewModel>> GetOrderStatusDistributionAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<TopSellingProductViewModel>> GetTopSellingProductsAsync(int limit = 10, DateTime? fromDate = null, DateTime? toDate = null);
        Task<decimal> GetAverageOrderValueAsync(DateTime? fromDate = null, DateTime? toDate = null);

        // Product Statistics
        Task<ProductStatisticsViewModel> GetProductStatisticsAsync();
        Task<List<ProductCategoryDistributionViewModel>> GetProductCategoryDistributionAsync();
        Task<List<LowStockProductViewModel>> GetLowStockProductsAsync(int threshold = 10);

        // Booking Statistics
        Task<BookingStatisticsViewModel> GetBookingStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<ConceptPopularityViewModel>> GetConceptPopularityAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<BookingStatusDistributionViewModel>> GetBookingStatusDistributionAsync(DateTime? fromDate = null, DateTime? toDate = null);

        // Partner Statistics
        Task<PartnerStatisticsViewModel> GetPartnerStatisticsAsync();
        Task<List<PartnerPerformanceViewModel>> GetPartnerPerformanceAsync(DateTime? fromDate = null, DateTime? toDate = null);

        // Dashboard Summary
        Task<DashboardSummaryViewModel> GetDashboardSummaryAsync();
        Task<List<RecentActivityViewModel>> GetRecentActivitiesAsync(int limit = 10);
    }
}
