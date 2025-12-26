using MonAmour.ViewModels;

namespace MonAmour.Services.Interfaces
{
    public interface IOrderService
    {
        // Order CRUD operations (chỉ lấy orders đã thanh toán, không lấy cart)
        Task<(List<OrderViewModel> orders, int totalCount)> GetOrdersAsync(OrderSearchViewModel searchModel);
        Task<OrderDetailViewModel?> GetOrderByIdAsync(int id);
        Task<bool> UpdateOrderAsync(OrderEditViewModel model);
        Task<bool> DeleteOrderAsync(int id);
        Task<bool> UpdateOrderStatusAsync(int orderId, string status);

        // Order statistics and utilities
        Task<Dictionary<string, int>> GetOrderStatisticsAsync();
        Task<List<OrderViewModel>> GetRecentOrdersAsync(int count = 10);
        Task<decimal> GetTotalRevenueAsync(DateTime? fromDate = null, DateTime? toDate = null);

        // Order status management
        Task<List<OrderStatusDropdownViewModel>> GetOrderStatusesAsync();
        Task<bool> IsValidOrderStatusAsync(string status);

        // Shipping and Payment related
        Task<List<ShippingOptionDropdownViewModel>> GetShippingOptionsAsync();
        Task<List<PaymentMethodDropdownViewModel>> GetPaymentMethodsAsync();

        // Order items management
        Task<List<OrderItemViewModel>> GetOrderItemsAsync(int orderId);
        Task<bool> UpdateOrderItemAsync(OrderItemViewModel model);
        Task<bool> DeleteOrderItemAsync(int orderItemId);

        // Payment details management
        Task<List<PaymentDetailViewModel>> GetPaymentDetailsAsync(int orderId);
        Task<bool> AddPaymentDetailAsync(PaymentDetailViewModel model);
        Task<bool> UpdatePaymentDetailAsync(PaymentDetailViewModel model);
        Task<bool> DeletePaymentDetailAsync(int paymentDetailId);

        // Business logic
        Task<bool> CanOrderBeModifiedAsync(int orderId);
        Task<bool> CanOrderBeDeletedAsync(int orderId);
        Task<bool> ProcessOrderPaymentAsync(int orderId, decimal amount, int paymentMethodId);
    }
}
