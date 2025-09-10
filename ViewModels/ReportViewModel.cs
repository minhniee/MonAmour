using System.ComponentModel.DataAnnotations;

namespace MonAmour.ViewModels
{
    // Revenue Reports
    public class RevenueReportViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal OrderRevenue { get; set; }
        public decimal GrowthRate { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal OrderPercentage { get; set; }
        // Aliases for view compatibility
        public int OrderCount => TotalOrders;
        public List<MonthlyRevenueViewModel> MonthlyData { get; set; } = new List<MonthlyRevenueViewModel>();
    }

    public class MonthlyRevenueViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
        public decimal GrowthRate { get; set; }
    }

    public class DailyRevenueViewModel
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    // User Statistics
    public class UserStatisticsViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewUsers { get; set; }
        public int VerifiedUsers { get; set; }
        public int MaleUsers { get; set; }
        public int FemaleUsers { get; set; }
        public int OtherGenderUsers { get; set; }
        public decimal UserGrowthRate { get; set; }
        public List<UserRegistrationViewModel> RegistrationData { get; set; } = new List<UserRegistrationViewModel>();
        public List<GenderDistributionViewModel> GenderDistribution { get; set; } = new List<GenderDistributionViewModel>();
    }

    public class UserRegistrationViewModel
    {
        public DateTime Date { get; set; }
        public int NewRegistrations { get; set; }
        public int TotalUsers { get; set; }
    }

    public class UserActivityViewModel
    {
        public DateTime Date { get; set; }
        public int ActiveUsers { get; set; }
        public int NewOrders { get; set; }
    }

    public class GenderDistributionViewModel
    {
        public string Gender { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
        public int NewUsers { get; set; }
    }

    // Order Statistics
    public class OrderStatisticsViewModel
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ConfirmedOrders { get; set; }
        public int ShippingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalOrderValue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<OrderStatusDistributionViewModel> StatusDistribution { get; set; } = new List<OrderStatusDistributionViewModel>();
    }

    public class OrderStatusDistributionViewModel
    {
        public string Status { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TopSellingProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AveragePrice { get; set; }
        public string? ProductImage { get; set; }
    }

    // Product Statistics
    public class ProductStatisticsViewModel
    {
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public int LowStockProducts { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public List<ProductCategoryDistributionViewModel> CategoryDistribution { get; set; } = new List<ProductCategoryDistributionViewModel>();
    }

    public class ProductCategoryDistributionViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public decimal TotalValue { get; set; }
        public decimal Percentage { get; set; }
    }

    public class LowStockProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int MinStock { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
    }



    // Partner Statistics
    public class PartnerStatisticsViewModel
    {
        public int TotalPartners { get; set; }
        public int ActivePartners { get; set; }
        public int PendingPartners { get; set; }
        public int InactivePartners { get; set; }
        public int TotalLocations { get; set; }
    }

    // Dashboard Summary
    public class DashboardSummaryViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
        public int TotalPartners { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal DailyRevenue { get; set; }
        public decimal RevenueGrowth { get; set; }
        public int PendingOrders { get; set; }
        public int LowStockProducts { get; set; }
        public List<RecentActivityViewModel> RecentActivities { get; set; } = new List<RecentActivityViewModel>();
    }

    public class RecentActivityViewModel
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal? Amount { get; set; }
    }

    // Chart Data
    public class ChartDataViewModel
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<decimal> Data { get; set; } = new List<decimal>();
        public List<string> Colors { get; set; } = new List<string>();
    }

    // Report Filters
    public class ReportFilterViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Status { get; set; }
        public int? CategoryId { get; set; }
        public int? PartnerId { get; set; }
        public string? ReportType { get; set; }
    }
}
