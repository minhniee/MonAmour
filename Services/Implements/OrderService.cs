using Microsoft.EntityFrameworkCore;
using MonAmour.Models;
using MonAmour.Services.Interfaces;
using MonAmour.ViewModels;

namespace MonAmour.Services.Implements
{
    public class OrderService : IOrderService
    {
        private readonly MonAmourDbContext _context;
        private readonly ILogger<OrderService> _logger;

        public OrderService(MonAmourDbContext context, ILogger<OrderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(List<OrderViewModel> orders, int totalCount)> GetOrdersAsync(OrderSearchViewModel searchModel)
        {
            try
            {
                // Lấy tất cả orders (trừ cart) để admin có thể quản lý
                var query = _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.ShippingOption)
                    .Include(o => o.OrderItems)
                    .Include(o => o.PaymentDetails)
                        .ThenInclude(pd => pd.Payment)
                            .ThenInclude(p => p.PaymentMethod)
                    .Where(o => o.Status != "cart") // Chỉ loại bỏ cart, hiển thị tất cả orders khác
                    .AsQueryable();

                _logger.LogInformation($"Total orders in database: {await _context.Orders.CountAsync()}");
                _logger.LogInformation($"Orders after filtering (not cart): {await query.CountAsync()}");

                // Apply filters
                if (!string.IsNullOrEmpty(searchModel.SearchTerm))
                {
                    query = query.Where(o => 
                        o.OrderId.ToString().Contains(searchModel.SearchTerm) ||
                        o.User!.Name!.Contains(searchModel.SearchTerm) ||
                        o.User.Email.Contains(searchModel.SearchTerm) ||
                        o.TrackingNumber!.Contains(searchModel.SearchTerm));
                }

                if (!string.IsNullOrEmpty(searchModel.Status))
                {
                    query = query.Where(o => o.Status == searchModel.Status);
                }

                if (searchModel.UserId.HasValue)
                {
                    query = query.Where(o => o.UserId == searchModel.UserId.Value);
                }

                if (searchModel.FromDate.HasValue)
                {
                    query = query.Where(o => o.CreatedAt >= searchModel.FromDate.Value);
                }

                if (searchModel.ToDate.HasValue)
                {
                    query = query.Where(o => o.CreatedAt <= searchModel.ToDate.Value);
                }

                if (searchModel.HasPayment.HasValue)
                {
                    if (searchModel.HasPayment.Value)
                    {
                        query = query.Where(o => o.PaymentDetails.Any());
                    }
                    else
                    {
                        query = query.Where(o => !o.PaymentDetails.Any());
                    }
                }

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply sorting
                switch (searchModel.SortBy?.ToLower())
                {
                    case "orderid":
                        query = searchModel.SortOrder == "asc" ? query.OrderBy(o => o.OrderId) : query.OrderByDescending(o => o.OrderId);
                        break;
                    case "username":
                        query = searchModel.SortOrder == "asc" ? query.OrderBy(o => o.User!.Name) : query.OrderByDescending(o => o.User!.Name);
                        break;
                    case "totalprice":
                        query = searchModel.SortOrder == "asc" ? query.OrderBy(o => o.TotalPrice) : query.OrderByDescending(o => o.TotalPrice);
                        break;
                    case "status":
                        query = searchModel.SortOrder == "asc" ? query.OrderBy(o => o.Status) : query.OrderByDescending(o => o.Status);
                        break;
                    default:
                        query = searchModel.SortOrder == "asc" ? query.OrderBy(o => o.CreatedAt) : query.OrderByDescending(o => o.CreatedAt);
                        break;
                }

                // Apply pagination
                var orders = await query
                    .Skip((searchModel.Page - 1) * searchModel.PageSize)
                    .Take(searchModel.PageSize)
                    .Select(o => new OrderViewModel
                    {
                        OrderId = o.OrderId,
                        UserId = o.UserId,
                        UserName = o.User != null ? o.User.Name : null,
                        UserEmail = o.User != null ? o.User.Email : null,
                        TotalPrice = o.TotalPrice,
                        ShippingCost = o.ShippingCost,
                        Status = o.Status,
                        ShippingOptionId = o.ShippingOptionId,
                        ShippingOptionName = o.ShippingOption != null ? o.ShippingOption.Name : null,
                        TrackingNumber = o.TrackingNumber,
                        EstimatedDelivery = o.EstimatedDelivery,
                        DeliveredAt = o.DeliveredAt,
                        CreatedAt = o.CreatedAt,
                        UpdatedAt = o.UpdatedAt,
                        ShippingAddress = o.ShippingAddress,
                        ItemCount = o.OrderItems.Count,
                        HasPayment = o.PaymentDetails.Any(),
                        PaymentStatus = o.PaymentDetails.FirstOrDefault() != null && o.PaymentDetails.FirstOrDefault().Payment != null ? o.PaymentDetails.FirstOrDefault().Payment.Status : null,
                        PaymentAmount = o.PaymentDetails.Sum(pd => pd.Amount)
                    })
                    .ToListAsync();

                return (orders, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOrdersAsync");
                return (new List<OrderViewModel>(), 0);
            }
        }

        public async Task<OrderDetailViewModel?> GetOrderByIdAsync(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.ShippingOption)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Include(o => o.PaymentDetails)
                        .ThenInclude(pd => pd.Payment)
                            .ThenInclude(p => p.PaymentMethod)
                    .FirstOrDefaultAsync(o => o.OrderId == id);

                if (order == null) return null;

                        return new OrderDetailViewModel
        {
            OrderId = order.OrderId,
            UserId = order.UserId,
            UserName = order.User?.Name,
            UserEmail = order.User?.Email,
            UserPhone = order.User?.Phone,
            TotalPrice = order.TotalPrice,
            ShippingCost = order.ShippingCost,
            Status = order.Status,
            ShippingOptionId = order.ShippingOptionId,
            ShippingOptionName = order.ShippingOption?.Name,
            TrackingNumber = order.TrackingNumber,
            EstimatedDelivery = order.EstimatedDelivery,
            DeliveredAt = order.DeliveredAt,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            ShippingAddress = order.ShippingAddress,
            OrderItems = order.OrderItems.Select(oi => new OrderItemViewModel
            {
                OrderItemId = oi.OrderItemId,
                OrderId = oi.OrderId,
                ProductId = oi.ProductId,
                ProductName = oi.Product?.Name,
                ProductImage = oi.Product?.ProductImgs?.FirstOrDefault()?.ImgUrl,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                TotalPrice = oi.TotalPrice
            }).ToList(),
            PaymentDetails = order.PaymentDetails.Select(pd => new PaymentDetailViewModel
            {
                PaymentDetailId = pd.PaymentDetailId,
                PaymentId = pd.PaymentId,
                OrderId = pd.OrderId,
                Amount = pd.Amount,
                PaymentMethodName = pd.Payment?.PaymentMethod?.Name,
                PaymentStatus = pd.Payment?.Status,
                CreatedAt = pd.Payment?.CreatedAt,
                ProcessedAt = pd.Payment?.ProcessedAt
            }).ToList()
        };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOrderByIdAsync");
                return null;
            }
        }

        public async Task<bool> UpdateOrderAsync(OrderEditViewModel model)
        {
            try
            {
                var order = await _context.Orders.FindAsync(model.OrderId);
                if (order == null) return false;

                order.Status = model.Status;
                order.EstimatedDelivery = model.EstimatedDelivery;
                order.DeliveredAt = model.DeliveredAt;
                order.ShippingOptionId = model.ShippingOptionId;
                order.ShippingAddress = model.ShippingAddress;
                order.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateOrderAsync");
                return false;
            }
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null) return false;

                // Chỉ cho phép xóa orders ở trạng thái cart hoặc confirmed
                if (order.Status == "shipping" || order.Status == "completed")
                {
                    return false;
                }

                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteOrderAsync");
                return false;
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null) return false;

                if (!await IsValidOrderStatusAsync(status))
                {
                    return false;
                }

                order.Status = status;
                order.UpdatedAt = DateTime.Now;

                // Nếu chuyển sang completed, set DeliveredAt
                if (status == "completed" && !order.DeliveredAt.HasValue)
                {
                    order.DeliveredAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateOrderStatusAsync");
                return false;
            }
        }

        public async Task<Dictionary<string, int>> GetOrderStatisticsAsync()
        {
            try
            {
                var stats = await _context.Orders
                    .Where(o => o.Status != "cart") // Hiển thị thống kê cho tất cả orders (trừ cart)
                    .GroupBy(o => o.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Status ?? "unknown", x => x.Count);

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOrderStatisticsAsync");
                return new Dictionary<string, int>();
            }
        }

        public async Task<List<OrderViewModel>> GetRecentOrdersAsync(int count = 10)
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.ShippingOption)
                    .Include(o => o.OrderItems)
                    .Include(o => o.PaymentDetails)
                        .ThenInclude(pd => pd.Payment)
                            .ThenInclude(p => p.PaymentMethod)
                    .Where(o => o.Status != "cart") // Hiển thị tất cả orders (trừ cart)
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(count)
                    .Select(o => new OrderViewModel
                    {
                        OrderId = o.OrderId,
                        UserId = o.UserId,
                        UserName = o.User != null ? o.User.Name : null,
                        UserEmail = o.User != null ? o.User.Email : null,
                        TotalPrice = o.TotalPrice,
                        Status = o.Status,
                        ShippingOptionName = o.ShippingOption != null ? o.ShippingOption.Name : null,
                        TrackingNumber = o.TrackingNumber,
                        CreatedAt = o.CreatedAt,
                        ShippingAddress = o.ShippingAddress,
                        ItemCount = o.OrderItems.Count,
                        HasPayment = o.PaymentDetails.Any(),
                        PaymentStatus = o.PaymentDetails.FirstOrDefault() != null && o.PaymentDetails.FirstOrDefault().Payment != null ? o.PaymentDetails.FirstOrDefault().Payment.Status : null
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRecentOrdersAsync");
                return new List<OrderViewModel>();
            }
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.Orders
                    .Where(o => o.Status == "completed" && o.PaymentDetails.Any());

                if (fromDate.HasValue)
                {
                    query = query.Where(o => o.CreatedAt >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(o => o.CreatedAt <= toDate.Value);
                }

                return await query.SumAsync(o => o.TotalPrice ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetTotalRevenueAsync");
                return 0;
            }
        }

        public async Task<List<OrderStatusDropdownViewModel>> GetOrderStatusesAsync()
        {
            return new List<OrderStatusDropdownViewModel>
            {
                new() { Value = "pending", Text = "Chờ xử lý" },
                new() { Value = "confirmed", Text = "Đã xác nhận" },
                new() { Value = "shipping", Text = "Đang giao hàng" },
                new() { Value = "completed", Text = "Hoàn thành" },
                new() { Value = "cancelled", Text = "Đã hủy" }
            };
        }

        public async Task<bool> IsValidOrderStatusAsync(string status)
        {
            var validStatuses = new[] { "cart", "pending", "confirmed", "shipping", "completed", "cancelled" };
            return validStatuses.Contains(status);
        }

        public async Task<List<ShippingOptionDropdownViewModel>> GetShippingOptionsAsync()
        {
            try
            {
                return await _context.ShippingOptions
                    .Select(so => new ShippingOptionDropdownViewModel
                    {
                        ShippingOptionId = so.ShippingOptionId,
                        Name = so.Name ?? "Unknown"
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetShippingOptionsAsync");
                return new List<ShippingOptionDropdownViewModel>();
            }
        }

        public async Task<List<PaymentMethodDropdownViewModel>> GetPaymentMethodsAsync()
        {
            try
            {
                return await _context.PaymentMethods
                    .Select(pm => new PaymentMethodDropdownViewModel
                    {
                        PaymentMethodId = pm.PaymentMethodId,
                        Name = pm.Name ?? "Unknown"
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPaymentMethodsAsync");
                return new List<PaymentMethodDropdownViewModel>();
            }
        }

        public async Task<List<OrderItemViewModel>> GetOrderItemsAsync(int orderId)
        {
            try
            {
                return await _context.OrderItems
                    .Include(oi => oi.Product)
                    .Where(oi => oi.OrderId == orderId)
                    .Select(oi => new OrderItemViewModel
                    {
                        OrderItemId = oi.OrderItemId,
                        OrderId = oi.OrderId,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product != null ? oi.Product.Name : null,
                        ProductImage = oi.Product != null && oi.Product.ProductImgs != null && oi.Product.ProductImgs.Any() ? oi.Product.ProductImgs.FirstOrDefault().ImgUrl : null,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOrderItemsAsync");
                return new List<OrderItemViewModel>();
            }
        }

        public async Task<bool> UpdateOrderItemAsync(OrderItemViewModel model)
        {
            try
            {
                var orderItem = await _context.OrderItems.FindAsync(model.OrderItemId);
                if (orderItem == null) return false;

                orderItem.Quantity = model.Quantity;
                orderItem.UnitPrice = model.UnitPrice;
                orderItem.TotalPrice = model.TotalPrice;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateOrderItemAsync");
                return false;
            }
        }

        public async Task<bool> DeleteOrderItemAsync(int orderItemId)
        {
            try
            {
                var orderItem = await _context.OrderItems.FindAsync(orderItemId);
                if (orderItem == null) return false;

                _context.OrderItems.Remove(orderItem);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteOrderItemAsync");
                return false;
            }
        }

        public async Task<List<PaymentDetailViewModel>> GetPaymentDetailsAsync(int orderId)
        {
            try
            {
                return await _context.PaymentDetails
                    .Include(pd => pd.Payment)
                        .ThenInclude(p => p.PaymentMethod)
                    .Where(pd => pd.OrderId == orderId)
                    .Select(pd => new PaymentDetailViewModel
                    {
                        PaymentDetailId = pd.PaymentDetailId,
                        PaymentId = pd.PaymentId,
                        OrderId = pd.OrderId,
                        Amount = pd.Amount,
                        PaymentMethodName = pd.Payment != null && pd.Payment.PaymentMethod != null ? pd.Payment.PaymentMethod.Name : null,
                        PaymentStatus = pd.Payment != null ? pd.Payment.Status : null,
                        CreatedAt = pd.Payment != null ? pd.Payment.CreatedAt : null,
                        ProcessedAt = pd.Payment != null ? pd.Payment.ProcessedAt : null
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPaymentDetailsAsync");
                return new List<PaymentDetailViewModel>();
            }
        }

        public async Task<bool> AddPaymentDetailAsync(PaymentDetailViewModel model)
        {
            try
            {
                var payment = new Payment
                {
                    Amount = model.Amount,
                    Status = model.PaymentStatus ?? "pending",
                    PaymentMethodId = _context.PaymentMethods.FirstOrDefault()?.PaymentMethodId,
                    CreatedAt = DateTime.Now
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                var paymentDetail = new PaymentDetail
                {
                    PaymentId = payment.PaymentId,
                    OrderId = model.OrderId,
                    Amount = model.Amount
                };

                _context.PaymentDetails.Add(paymentDetail);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddPaymentDetailAsync");
                return false;
            }
        }

        public async Task<bool> UpdatePaymentDetailAsync(PaymentDetailViewModel model)
        {
            try
            {
                var paymentDetail = await _context.PaymentDetails
                    .Include(pd => pd.Payment)
                    .FirstOrDefaultAsync(pd => pd.PaymentDetailId == model.PaymentDetailId);

                if (paymentDetail == null) return false;

                paymentDetail.Amount = model.Amount;
                if (paymentDetail.Payment != null)
                {
                    paymentDetail.Payment.Status = model.PaymentStatus ?? paymentDetail.Payment.Status;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdatePaymentDetailAsync");
                return false;
            }
        }

        public async Task<bool> DeletePaymentDetailAsync(int paymentDetailId)
        {
            try
            {
                var paymentDetail = await _context.PaymentDetails
                    .Include(pd => pd.Payment)
                    .FirstOrDefaultAsync(pd => pd.PaymentDetailId == paymentDetailId);

                if (paymentDetail == null) return false;

                _context.PaymentDetails.Remove(paymentDetail);
                if (paymentDetail.Payment != null)
                {
                    _context.Payments.Remove(paymentDetail.Payment);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeletePaymentDetailAsync");
                return false;
            }
        }

        public async Task<bool> CanOrderBeModifiedAsync(int orderId)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                return order != null && order.Status != "completed";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CanOrderBeModifiedAsync");
                return false;
            }
        }

        public async Task<bool> CanOrderBeDeletedAsync(int orderId)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                return order != null && (order.Status == "cart" || order.Status == "confirmed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CanOrderBeDeletedAsync");
                return false;
            }
        }

        public async Task<bool> ProcessOrderPaymentAsync(int orderId, decimal amount, int paymentMethodId)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null) return false;

                // Tạo payment
                var payment = new Payment
                {
                    Amount = amount,
                    Status = "completed",
                    PaymentMethodId = paymentMethodId,
                    CreatedAt = DateTime.Now,
                    ProcessedAt = DateTime.Now
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // Tạo payment detail
                var paymentDetail = new PaymentDetail
                {
                    PaymentId = payment.PaymentId,
                    OrderId = orderId,
                    Amount = amount
                };

                _context.PaymentDetails.Add(paymentDetail);

                // Cập nhật trạng thái order
                if (order.Status == "cart")
                {
                    order.Status = "confirmed";
                    order.UpdatedAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProcessOrderPaymentAsync");
                return false;
            }
        }
    }
}
