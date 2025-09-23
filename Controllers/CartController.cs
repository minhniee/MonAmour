using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonAmour.Helpers;
using MonAmour.Models;
using MonAmour.Services.Interfaces;
using System.Security.Claims;
using System.Text.Json;
using MonAmour.AuthViewModel;

namespace MonAmour.Controllers
{
    public class CartController : Controller
    {
        private readonly MonAmourDbContext _db;
        private readonly ICassoService _cassoService;
        private readonly IVietQRService _vietQRService;
        private readonly IConfiguration _config;

        private readonly IEmailService _emailService;
        private readonly IReviewService _reviewService;

        public CartController(MonAmourDbContext db, ICassoService cassoService, IVietQRService vietQRService, IConfiguration configuration, IEmailService emailService, IReviewService reviewService)
        {
            _db = db;
            _cassoService = cassoService;
            _vietQRService = vietQRService;
            _config = configuration;
            _emailService = emailService;
            _reviewService = reviewService;
        }

        // GET: /Cart
        public IActionResult Index()
        {
            int? userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var cart = _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.PaymentDetails)
                    .ThenInclude(pd => pd.Payment)
                .Where(o => o.UserId == userId && o.Status == "cart")
                .AsEnumerable()
                .FirstOrDefault(o => !(o.PaymentDetails?.Any(pd => pd.Payment != null && pd.Payment.Status == "pending") ?? false));

            if (cart == null)
            {
                cart = new Order
                {
                    UserId = userId.Value,
                    Status = "cart",
                    TotalPrice = 0,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _db.Orders.Add(cart);
                _db.SaveChanges();
            }

            return View(cart);
        }

        // POST: /Cart/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(int productId, int quantity = 1)
        {
            if (quantity < 1) quantity = 1;

            int? userId = GetCurrentUserId();
            if (userId == null)
            {
                if (IsAjaxRequest())
                {
                    return Unauthorized(new { success = false, message = "Unauthorized" });
                }
                return RedirectToAction("Login", "Auth");
            }

            var product = _db.Products.FirstOrDefault(p => p.ProductId == productId && p.Status == "active");
            if (product == null)
            {
                if (IsAjaxRequest())
                {
                    return NotFound(new { success = false, message = "Product not found" });
                }
                return NotFound();
            }

            // Check stock availability (chỉ kiểm tra, không giảm stock)
            if (product.StockQuantity.HasValue && product.StockQuantity.Value < quantity)
            {
                if (IsAjaxRequest())
                {
                    return BadRequest(new { success = false, message = $"Số lượng sản phẩm không còn đủ. Hiện tại chỉ còn {product.StockQuantity.Value} sản phẩm trong kho. Vui lòng chọn số lượng phù hợp." });
                }
                TempData["CartError"] = $"Số lượng sản phẩm không còn đủ. Hiện tại chỉ còn {product.StockQuantity.Value} sản phẩm trong kho. Vui lòng chọn số lượng phù hợp.";
                return RedirectToAction("ProductDetail", "Gift_box", new { id = productId });
            }

            var cart = _db.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.PaymentDetails)
                    .ThenInclude(pd => pd.Payment)
                .Where(o => o.UserId == userId && o.Status == "cart")
                .AsEnumerable()
                .FirstOrDefault(o => !(o.PaymentDetails?.Any(pd => pd.Payment != null && pd.Payment.Status == "pending") ?? false));

            if (cart == null)
            {
                cart = new Order
                {
                    UserId = userId.Value,
                    Status = "cart",
                    TotalPrice = 0,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _db.Orders.Add(cart);
                _db.SaveChanges();
            }

            var item = cart.OrderItems.FirstOrDefault(i => i.ProductId == productId);
            int? totalQuantityInCart = quantity;
            if (item != null)
            {
                totalQuantityInCart += item.Quantity;
            }

            // Check total stock availability (existing cart items + new quantity) - chỉ kiểm tra, không giảm stock
            if (product.StockQuantity.HasValue && product.StockQuantity.Value < totalQuantityInCart)
            {
                if (IsAjaxRequest())
                {
                    return BadRequest(new { success = false, message = $"Số lượng sản phẩm không còn đủ. Hiện tại chỉ còn {product.StockQuantity.Value} sản phẩm trong kho. Bạn đã có {item?.Quantity ?? 0} sản phẩm trong giỏ hàng. Vui lòng kiểm tra lại số lượng." });
                }
                TempData["CartError"] = $"Số lượng sản phẩm không còn đủ. Hiện tại chỉ còn {product.StockQuantity.Value} sản phẩm trong kho. Bạn đã có {item?.Quantity ?? 0} sản phẩm trong giỏ hàng. Vui lòng kiểm tra lại số lượng.";
                return RedirectToAction("ProductDetail", "Gift_box", new { id = productId });
            }

            // Kiểm tra tổng số lượng trong tất cả giỏ hàng của user không vượt quá stock
            var totalQuantityInAllCarts = _db.OrderItems
                .Where(i => i.ProductId == productId && i.Order.Status == "cart" && i.Order.UserId == userId)
                .Sum(i => i.Quantity);

            // Tính tổng số lượng sau khi thêm (bao gồm cả item hiện tại nếu có)
            var finalQuantity = totalQuantityInAllCarts + quantity;

            if (product.StockQuantity.HasValue && finalQuantity > product.StockQuantity.Value)
            {
                if (IsAjaxRequest())
                {
                    return BadRequest(new { success = false, message = $"Số lượng sản phẩm không còn đủ. Bạn muốn thêm {quantity} sản phẩm nhưng chỉ còn {product.StockQuantity.Value} sản phẩm trong kho. Vui lòng giảm số lượng hoặc chọn sản phẩm khác." });
                }
                TempData["CartError"] = $"Số lượng sản phẩm không còn đủ. Bạn muốn thêm {quantity} sản phẩm nhưng chỉ còn {product.StockQuantity.Value} sản phẩm trong kho. Vui lòng giảm số lượng hoặc chọn sản phẩm khác.";
                return RedirectToAction("ProductDetail", "Gift_box", new { id = productId });
            }

            if (item == null)
            {
                item = new OrderItem
                {
                    OrderId = cart.OrderId,
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = product.Price ?? 0,
                    TotalPrice = (product.Price ?? 0) * quantity
                };
                _db.OrderItems.Add(item);
            }
            else
            {
                item.Quantity += quantity;
                item.UnitPrice = product.Price.HasValue ? product.Price.Value : item.UnitPrice;
                item.TotalPrice = (decimal)(item.UnitPrice * item.Quantity);
                _db.OrderItems.Update(item);
            }

            // Không giảm stock khi add to cart - chỉ giảm khi thanh toán thành công

            _db.SaveChanges();

            cart.TotalPrice = _db.OrderItems
                .Where(i => i.OrderId == cart.OrderId)
                .Select(i => (decimal?)i.TotalPrice)
                .Sum()
                .GetValueOrDefault(0m);
            cart.UpdatedAt = DateTime.Now;
            _db.Orders.Update(cart);
            _db.SaveChanges();

            if (IsAjaxRequest())
            {
                return Json(new { success = true, message = "Đã thêm vào giỏ hàng", total = cart.TotalPrice, items = cart.OrderItems.Count });
            }

            TempData["CartSuccess"] = "Đã thêm vào giỏ hàng";
            return RedirectToAction("ProductDetail", "Gift_box", new { id = productId, added = true });
        }

        // POST: /Cart/SubmitReview - Chỉ cho phép khách đã mua hàng và đơn ở trạng thái completed/confirmed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview([FromForm] int targetId, [FromForm] string targetType, [FromForm] int rating, [FromForm] string? comment)
        {
            int? userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            targetType = (targetType ?? string.Empty).Trim();
            if (!string.Equals(targetType, "Product", StringComparison.OrdinalIgnoreCase) && !string.Equals(targetType, "Concept", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Loại mục đánh giá không hợp lệ" });
            }

            if (rating < 1 || rating > 5)
            {
                return Json(new { success = false, message = "Điểm đánh giá phải từ 1 đến 5" });
            }

            // Xác thực: user đã mua sản phẩm này và đơn đã hoàn tất/đã xác nhận
            bool purchased = false;
            if (string.Equals(targetType, "Product", StringComparison.OrdinalIgnoreCase))
            {
                purchased = await _db.OrderItems
                    .Include(oi => oi.Order)
                    .AsNoTracking()
                    .AnyAsync(oi => oi.ProductId == targetId
                                    && oi.Order.UserId == userId
                                    && (oi.Order.Status == "completed" || oi.Order.Status == "confirmed"));
            }
            else
            {
                // Nếu có Concept review thì xác thực tương tự theo domain của bạn
                purchased = true; // cho phép tạm nếu business không yêu cầu
            }

            if (!purchased)
            {
                return Json(new { success = false, message = "Bạn chỉ có thể đánh giá sản phẩm đã mua" });
            }

            // Không cho đánh giá trùng
            if (await _reviewService.HasUserReviewedAsync(userId.Value, "Product", targetId))
            {
                return Json(new { success = false, message = "Bạn đã đánh giá sản phẩm này rồi" });
            }

            var dto = new CreateReviewViewModel
            {
                UserId = userId.Value,
                TargetType = "Product",
                TargetId = targetId,
                Rating = rating,
                Comment = string.IsNullOrWhiteSpace(comment) ? null : comment!.Trim()
            };

            try
            {
                var created = await _reviewService.CreateReviewAsync(dto);
                return Json(new { success = true, message = "Cảm ơn bạn đã đánh giá!", reviewId = created.ReviewId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Không thể lưu đánh giá: " + ex.Message });
            }
        }

        // POST: /Cart/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(int itemId, int quantity)
        {
            if (quantity < 1) quantity = 1;
            var item = _db.OrderItems.Include(i => i.Order).Include(i => i.Product).FirstOrDefault(i => i.OrderItemId == itemId);
            if (item == null || item.Order == null || item.Order.Status != "cart")
            {
                return NotFound();
            }

            var oldQuantity = item.Quantity;
            var quantityDifference = quantity - oldQuantity;

            // Check stock availability for the difference (chỉ kiểm tra, không giảm stock)
            if (quantityDifference > 0 && item.Product.StockQuantity.HasValue && item.Product.StockQuantity.Value < quantityDifference)
            {
                TempData["CartError"] = $"Số lượng sản phẩm không còn đủ. Hiện tại chỉ còn {item.Product.StockQuantity.Value} sản phẩm trong kho. Vui lòng chọn số lượng phù hợp.";
                return RedirectToAction("Index");
            }

            // Kiểm tra tổng số lượng trong giỏ hàng không vượt quá stock (sau khi cập nhật)
            var totalQuantityInCart = _db.OrderItems
                .Where(i => i.ProductId == item.ProductId && i.Order.Status == "cart")
                .Sum(i => i.Quantity) - oldQuantity + quantity; // Tính lại với số lượng mới

            if (item.Product.StockQuantity.HasValue && totalQuantityInCart > item.Product.StockQuantity.Value)
            {
                TempData["CartError"] = $"Số lượng sản phẩm không còn đủ. Sản phẩm nhưng chỉ còn {item.Product.StockQuantity.Value} sản phẩm trong kho. Vui lòng giảm số lượng.";
                return RedirectToAction("Index");
            }

            // Update cart item
            item.Quantity = quantity;
            item.TotalPrice = (decimal)(item.UnitPrice * item.Quantity);
            _db.OrderItems.Update(item);

            // Không giảm stock khi update cart - chỉ giảm khi thanh toán thành công

            // Persist item change to ensure subsequent sum reads latest value
            _db.SaveChanges();

            item.Order.TotalPrice = _db.OrderItems
                .Where(i => i.OrderId == item.OrderId)
                .Select(i => (decimal?)i.TotalPrice)
                .Sum()
                .GetValueOrDefault(0m);
            item.Order.UpdatedAt = DateTime.Now;
            _db.Orders.Update(item.Order);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        // POST: /Cart/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int itemId)
        {
            var item = _db.OrderItems.Include(i => i.Order).Include(i => i.Product).FirstOrDefault(i => i.OrderItemId == itemId);
            if (item == null || item.Order == null || item.Order.Status != "cart")
            {
                return NotFound();
            }

            // Không cần restore stock vì chưa giảm stock khi add to cart

            var order = item.Order; // reuse the already-tracked Order to avoid double-tracking
            _db.OrderItems.Remove(item);
            _db.SaveChanges();

            // Ensure latest items are loaded for total recalculation
            _db.Entry(order).Collection(o => o.OrderItems).Load();
            order.TotalPrice = order.OrderItems
                .Select(i => (decimal?)i.TotalPrice)
                .Sum()
                .GetValueOrDefault(0m);
            order.UpdatedAt = DateTime.Now;
            // No need to call Update on a tracked entity
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        // POST: /Cart/SaveAddress - Save phone and address for current cart
        [HttpPost]
        public async Task<IActionResult> SaveAddress([FromForm] string address, [FromForm] string? phone)
        {
            int? userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            if (string.IsNullOrWhiteSpace(address))
            {
                return Json(new { success = false, message = "Vui lòng nhập địa chỉ giao hàng" });
            }
            // Phone validation theo đầu số Việt Nam
            var phoneStr = (phone ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(phoneStr) || !IsValidVietnamesePhone(phoneStr))
            {
                return Json(new { success = false, message = "Vui lòng nhập số điện thoại 10 số hợp lệ theo đầu số Việt Nam" });
            }

            var candidateCarts = await _db.Orders
                .Include(o => o.PaymentDetails)
                    .ThenInclude(pd => pd.Payment)
                .Where(o => o.UserId == userId && o.Status == "cart")
                .ToListAsync();
            var cart = candidateCarts
                .FirstOrDefault(o => !(o.PaymentDetails?.Any(pd => pd.Payment != null && pd.Payment.Status == "pending") ?? false));
            if (cart == null)
            {
                return Json(new { success = false, message = "Không có giỏ hàng" });
            }

            cart.ShippingAddress = $"Phone: {phoneStr} | Address: {address.Trim()}";
            cart.UpdatedAt = DateTime.Now;
            _db.Orders.Update(cart);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Đã lưu địa chỉ giao hàng" });
        }

        // POST: /Cart/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout()
        {
            int? userId = GetCurrentUserId();
            if (userId == null)
            {
                if (IsAjaxRequest()) return Unauthorized(new { success = false, message = "Unauthorized" });
                return RedirectToAction("Login", "Auth");
            }

            var order = _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.PaymentDetails)
                    .ThenInclude(pd => pd.Payment)
                .Where(o => o.UserId == userId && o.Status == "cart")
                .AsEnumerable()
                .FirstOrDefault(o => !(o.PaymentDetails?.Any(pd => pd.Payment != null && pd.Payment.Status == "pending") ?? false));

            if (order == null || !order.OrderItems.Any())
            {
                if (IsAjaxRequest()) return BadRequest(new { success = false, message = "Giỏ hàng trống" });
                TempData["CartError"] = "Giỏ hàng trống";
                return RedirectToAction("Index");
            }

            // Recalculate totals to be safe
            order.TotalPrice = order.OrderItems
                .Select(i => (decimal?)i.TotalPrice)
                .Sum()
                .GetValueOrDefault(0m);

            // Kiểm tra stock trước khi tạo payment
            var insufficientStockItems = new List<string>();
            foreach (var item in order.OrderItems)
            {
                if (item.Product.StockQuantity.HasValue)
                {
                    if (item.Product.StockQuantity.Value < item.Quantity)
                    {
                        insufficientStockItems.Add($"{item.Product.Name} (cần {item.Quantity}, còn {item.Product.StockQuantity.Value})");
                    }
                }
            }

            if (insufficientStockItems.Any())
            {
                var message = "Một số sản phẩm trong giỏ hàng đã hết hàng:\n" + string.Join("\n", insufficientStockItems) + "\n\nVui lòng kiểm tra lại giỏ hàng và điều chỉnh số lượng phù hợp.";
                if (IsAjaxRequest()) return BadRequest(new { success = false, message = message });
                TempData["CartError"] = message;
                return RedirectToAction("Index");
            }

            // Giảm stock cho tất cả sản phẩm
            foreach (var item in order.OrderItems)
            {
                if (item.Product.StockQuantity.HasValue)
                {
                    item.Product.StockQuantity -= item.Quantity;
                    _db.Products.Update(item.Product);
                }
            }

            // Create a payment with status pending -> completed (for demo)
            var paymentMethod = _db.PaymentMethods.FirstOrDefault();
            var todayRef = DateTime.Now.ToString("yyyyMMdd");
            var paymentReferenceCheckout = $"ORDER{order.OrderId}_{order.UserId}_{todayRef}";
            var payment = new Payment
            {
                Amount = order.TotalPrice,
                Status = "completed",
                PaymentMethodId = paymentMethod?.PaymentMethodId,
                PaymentReference = paymentReferenceCheckout,
                CreatedAt = DateTime.Now,
                ProcessedAt = DateTime.Now
            };
            _db.Payments.Add(payment);
            _db.SaveChanges();

            var paymentDetail = new PaymentDetail
            {
                PaymentId = payment.PaymentId,
                OrderId = order.OrderId,
                Amount = order.TotalPrice
            };
            _db.PaymentDetails.Add(paymentDetail);

            // Update order status
            order.Status = "confirmed";
            order.UpdatedAt = DateTime.Now;
            _db.Orders.Update(order);
            _db.SaveChanges();

            if (IsAjaxRequest())
            {
                return Json(new { success = true, message = "Thanh toán thành công", orderId = order.OrderId, total = order.TotalPrice });
            }

            TempData["CartSuccess"] = "Thanh toán thành công";
            return RedirectToAction("Index");
        }


        private int? GetCurrentUserId()
        {
            // Prefer session-based auth to be consistent with the app's AuthHelper
            var sessionUserId = AuthHelper.GetUserId(HttpContext);
            if (sessionUserId.HasValue) return sessionUserId.Value;

            // Fallback to claims if needed
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(id, out int userId))
            {
                return userId;
            }
            return null;
        }

        // POST: /Cart/CheckCassoPayment
        [HttpPost]
        public async Task<IActionResult> CheckCassoPayment()
        {
            int? userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            try
            {
                // Track failed check attempts in session to enable fallback flow
                const string attemptKey = "CassoCheckAttempts";
                int attempts = 0;
                if (HttpContext.Session != null)
                {
                    var raw = HttpContext.Session.GetString(attemptKey);
                    _ = int.TryParse(raw, out attempts);
                }
                // Lấy đơn hàng giỏ hiện tại để tạo mã tham chiếu theo đơn
                var cartOrderRef = "";
                var cartOrder = await _db.Orders.FirstOrDefaultAsync(o => o.UserId == userId && o.Status == "cart");
                if (cartOrder == null)
                {
                    return Json(new { success = false, message = "Không có giỏ hàng" });
                }
                if (string.IsNullOrWhiteSpace(cartOrder.ShippingAddress) || !cartOrder.ShippingAddress.Contains("Phone:"))
                {
                    return Json(new { success = false, message = "Vui lòng nhập số điện thoại và địa chỉ giao hàng trước khi thanh toán" });
                }
                if (cartOrder != null)
                {
                    var today = DateTime.Now.ToString("yyyyMMdd");
                    cartOrderRef = $"ORDER{cartOrder.OrderId}_{userId}_{today}";
                }

                // Check for new transactions in the last 24 hours (giảm thời gian kiểm tra vì có webhook)
                var from = DateTime.UtcNow.AddHours(-24);
                var to = DateTime.UtcNow;

                var response = await _cassoService.GetTransactionsAsync(from, to);

                var debugInfo = new List<string>();
                debugInfo.Add($"Casso API Response: Code={response.Code}, HasData={response.Data != null}");
                debugInfo.Add($"Response Description: {response.Desc}");
                debugInfo.Add($"Response Message: {response.Desc ?? "No message"}");
                if (response.Data != null)
                {
                    debugInfo.Add($"Total transactions found: {response.Data.Records?.Count ?? 0}");
                    if (response.Data.Records != null && response.Data.Records.Any())
                    {
                        debugInfo.Add($"First transaction: {JsonSerializer.Serialize(response.Data.Records.First())}");
                    }
                }
                else
                {
                    debugInfo.Add("No data returned from Casso API");
                }

                if (response.Code != 0 || response.Data == null)
                {
                    var errorMessage = response.Code == 401 ? "API key không hợp lệ hoặc hết hạn" :
                                     response.Code == 403 ? "Không có quyền truy cập API" :
                                     response.Code == 404 ? "API endpoint không tồn tại" :
                                     response.Code == 429 ? "Hệ thống đang giới hạn tần suất. Vui lòng thử lại sau ít phút Ngân hàng có thể đang gặp sự cố. Bạn có thể bấm 'Báo lỗi thanh toán' để thông báo lỗi tới hệ thống. Xin lỗi quý khách vì sự bất tiện này!" :
                                     response.Code == 500 ? "Lỗi server" :
                                     $"Vui lòng chờ 1-2 phút và thử lại";

                    // Count as a failed attempt too so users can access the fallback after 5 tries
                    attempts++;
                    HttpContext.Session?.SetString(attemptKey, attempts.ToString());
                    var extra = attempts >= 5 && response.Code != 429 ? "\nNgân hàng có thể đang gặp sự cố. Bạn có thể bấm 'Báo lỗi thanh toán' để thông báo lỗi tới hệ thống." : "";
                    return Json(new { success = false, message = errorMessage + extra, showReport = attempts >= 5, attempts, debug = debugInfo });
                }

                // First, check if the cart already has a completed payment with a non-null TransactionId
                var hasCompletedPayment = await _db.PaymentDetails
                    .Include(pd => pd.Payment)
                    .AnyAsync(pd => pd.OrderId == cartOrder.OrderId && pd.Payment != null && pd.Payment.Status == "completed" && pd.Payment.TransactionId != null);

                if (hasCompletedPayment)
                {
                    // Stock processing and redirect if not already confirmed
                    var stockResultExisting = await ProcessStockAfterPayment(cartOrder.OrderId);
                    if (!stockResultExisting.Success)
                    {
                        return Json(new { success = false, message = stockResultExisting.Message, debug = debugInfo });
                    }
                    HttpContext.Session?.SetString(attemptKey, "0");
                    return Json(new
                    {
                        success = true,
                        message = "Đơn hàng đã được thanh toán và xác nhận",
                        redirectUrl = Url.Action("BillDetail", "Cart", new { orderId = cartOrder.OrderId }),
                        debug = debugInfo
                    });
                }

                var processedCount = 0;

                foreach (var transaction in response.Data.Records)
                {
                    // Debug: Log transaction details
                    var transactionText = $"Transaction: Description='{transaction.Description}', Reference='{transaction.Reference}', Ref='{transaction.Ref}', Amount={transaction.Amount}";
                    debugInfo.Add(transactionText);

                    // Chỉ kiểm tra theo mã tham chiếu đơn hàng để tránh trùng lặp
                    // Build flexible variants to match banks that strip separators or user id
                    var today = DateTime.Now.ToString("yyyyMMdd");
                    var variants = new List<string>();
                    if (!string.IsNullOrWhiteSpace(cartOrderRef))
                    {
                        variants.Add(cartOrderRef); // ORDER{orderId}_{userId}_{yyyyMMdd}
                        variants.Add(cartOrderRef.Replace("_", "")); // ORDER{orderId}{userId}{yyyyMMdd}
                    }
                    // Without user id
                    variants.Add($"ORDER{cartOrder.OrderId}_{today}");
                    variants.Add($"ORDER{cartOrder.OrderId}{today}");
                    // Fallback to just order id
                    variants.Add($"ORDER{cartOrder.OrderId}");

                    bool hasOrderRef = variants.Any(v =>
                        (transaction.Description?.Contains(v) == true) ||
                        (transaction.Reference?.Contains(v) == true) ||
                        (transaction.Ref?.Contains(v) == true)
                    );

                    debugInfo.Add($"OrderRefVariants: [" + string.Join(", ", variants) + "]");
                    debugInfo.Add($"HasOrderRef: {hasOrderRef}");

                    if (hasOrderRef)
                    {
                        var success = await _cassoService.ProcessPaymentAsync(transaction, userId.Value);
                        if (success) processedCount++;
                        debugInfo.Add($"Processed transaction: {success}");
                    }
                }

                if (processedCount > 0)
                {
                    // Kiểm tra và giảm stock khi thanh toán thành công
                    var stockResult = await ProcessStockAfterPayment(cartOrder.OrderId);
                    if (!stockResult.Success)
                    {
                        return Json(new { success = false, message = stockResult.Message, debug = debugInfo });
                    }
                    // Reset failed attempts
                    HttpContext.Session?.SetString(attemptKey, "0");
                    return Json(new
                    {
                        success = true,
                        message = $"Đã xử lý {processedCount} giao dịch thành công",
                        redirectUrl = Url.Action("BillDetail", "Cart", new { orderId = cartOrder.OrderId }),
                        debug = debugInfo
                    });
                }
                else
                {
                    // Increment failed attempts and suggest fallback after threshold
                    attempts++;
                    HttpContext.Session?.SetString(attemptKey, attempts.ToString());
                    var baseMsg = "Chưa có giao dịch nào phù hợp";
                    var extra = attempts >= 5 ? "\nNgân hàng có thể đang gặp sự cố. Bạn có thể bấm 'Báo lỗi thanh toán' để gửi mã chuyển khoản, hệ thống sẽ tạo đơn hàng ở trạng thái 'pending' để admin xác minh." : "";
                    return Json(new { success = false, message = baseMsg + extra, showReport = attempts >= 5, attempts, debug = debugInfo });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi kiểm tra thanh toán: " + ex.Message });
            }
        }

        // POST: /Cart/ReportPaymentIssue - Tạo đơn hàng pending với mã giao dịch khách hàng cung cấp
        [HttpPost]
        public async Task<IActionResult> ReportPaymentIssue([FromForm] string transferCode, [FromForm] string? contactName, [FromForm] string? contactPhone, [FromForm] string? contactEmail)
        {
            int? userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            transferCode = (transferCode ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(transferCode))
            {
                return Json(new { success = false, message = "Vui lòng nhập mã giao dịch/FT hoặc nội dung chuyển khoản" });
            }

            // Validate contact info: name required, phone required (VN 10 digits), email optional but must be valid if provided
            contactName = (contactName ?? string.Empty).Trim();
            contactPhone = (contactPhone ?? string.Empty).Trim();
            contactEmail = (contactEmail ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(contactName))
            {
                return Json(new { success = false, message = "Vui lòng nhập họ tên để chúng tôi liên hệ hỗ trợ" });
            }
            if (string.IsNullOrWhiteSpace(contactPhone) || !IsValidVietnamesePhone(contactPhone))
            {
                return Json(new { success = false, message = "Vui lòng nhập số điện thoại 10 số hợp lệ theo đầu số Việt Nam" });
            }
            if (!string.IsNullOrWhiteSpace(contactEmail))
            {
                try
                {
                    var _ = new System.Net.Mail.MailAddress(contactEmail);
                }
                catch
                {
                    return Json(new { success = false, message = "Email không hợp lệ" });
                }
            }

            // Lấy giỏ hàng hiện tại
            var order = await _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == "cart");

            if (order == null || !order.OrderItems.Any())
            {
                return Json(new { success = false, message = "Giỏ hàng trống hoặc không tồn tại" });
            }

            if (string.IsNullOrWhiteSpace(order.ShippingAddress) || !order.ShippingAddress.Contains("Phone:"))
            {
                return Json(new { success = false, message = "Vui lòng nhập số điện thoại và địa chỉ giao hàng trước" });
            }

            // Không giảm stock. Chỉ tạo payment pending và chuyển order sang pending_review
            // Recalculate total to be safe
            order.TotalPrice = order.OrderItems
                .Select(i => (decimal?)i.TotalPrice)
                .Sum()
                .GetValueOrDefault(0m);

            var paymentMethod = _db.PaymentMethods.FirstOrDefault();
            var today = DateTime.Now.ToString("yyyyMMdd");
            var orderRef = $"ORDER{order.OrderId}_{userId}_{today}";
            var payment = new Payment
            {
                Amount = order.TotalPrice,
                Status = "pending",
                PaymentMethodId = paymentMethod?.PaymentMethodId,
                PaymentReference = orderRef,
                CreatedAt = DateTime.Now
            };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            var paymentDetail = new PaymentDetail
            {
                PaymentId = payment.PaymentId,
                OrderId = order.OrderId,
                Amount = order.TotalPrice,
            };
            _db.PaymentDetails.Add(paymentDetail);

            // Đánh dấu đơn hàng ở trạng thái chờ xử lý để admin xác minh
            order.Status = "pending";
            order.TrackingNumber = transferCode; // lưu mã giao dịch khách
            order.UpdatedAt = DateTime.Now;
            _db.Orders.Update(order);
            await _db.SaveChangesAsync();

            // Gửi email báo cáo tới admin
            try
            {
                var adminEmails = await _db.UserRoles
                    .Where(ur => ur.RoleId == 1)
                    .Select(ur => ur.User.Email)
                    .Distinct()
                    .Where(e => e != null && e != "")
                    .ToListAsync();

                if (adminEmails.Any())
                {
                    var subject = $"[MonAmour] Báo lỗi thanh toán - Order #{order.OrderId}";
                    var htmlBody = $@"
                        <html><body style='font-family: Arial, sans-serif; color:#333'>
                          <h2>📣 Báo lỗi thanh toán</h2>
                          <p><strong>Order ID:</strong> {order.OrderId}</p>
                          <p><strong>User ID:</strong> {order.UserId}</p>
                          <p><strong>Số tiền:</strong> {(order.TotalPrice ?? 0m):N0}₫</p>
                          <p><strong>Nội dung chuyển khoản (TrackingNumber):</strong> {System.Net.WebUtility.HtmlEncode(order.TrackingNumber)}</p>
                          <p><strong>PaymentReference (dự kiến):</strong> {System.Net.WebUtility.HtmlEncode(orderRef)}</p>
                          <h3>Thông tin liên hệ khách hàng</h3>
                          <p><strong>Tên:</strong> {System.Net.WebUtility.HtmlEncode(contactName ?? "")}</p>
                          <p><strong>Điện thoại:</strong> {System.Net.WebUtility.HtmlEncode(contactPhone ?? "")}</p>
                          <p><strong>Email:</strong> {System.Net.WebUtility.HtmlEncode(contactEmail ?? "")}</p>
                          <p><strong>Thời gian báo cáo:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
                          <hr/>
                          <p>Vui lòng kiểm tra giao dịch và xác minh thủ công trong Casso/Ngân hàng.</p>
                        </body></html>";

                    foreach (var adminEmail in adminEmails)
                    {
                        await _emailService.SendAdminPaymentIssueReportAsync(adminEmail!, subject, htmlBody);
                    }
                }
            }
            catch { /* Không để lỗi email chặn flow của khách */ }

            // Reset failed attempts
            HttpContext.Session?.SetString("CassoCheckAttempts", "0");

            return Json(new
            {
                success = true,
                message = "Đã ghi nhận sự cố thanh toán. Chúng tôi sẽ liên hệ qua thông tin bạn cung cấp để xác minh.",
                redirectUrl = Url.Action("OrderHistory", "Cart")
            });
        }

        // GET: /Cart/GetPaymentStatus - Kiểm tra trạng thái thanh toán realtime
        [HttpGet]
        public async Task<IActionResult> GetPaymentStatus()
        {
            int? userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            try
            {
                var cartOrder = await _db.Orders
                    .Include(o => o.PaymentDetails)
                        .ThenInclude(pd => pd.Payment)
                    .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == "cart");

                if (cartOrder == null)
                {
                    return Json(new
                    {
                        success = true,
                        hasCart = false,
                        isPaid = false,
                        amount = 0m,
                        message = "Không có giỏ hàng"
                    });
                }

                var isPaid = cartOrder.PaymentDetails.Any(pd => pd.Payment?.Status == "completed");
                var amount = cartOrder.TotalPrice;

                return Json(new
                {
                    success = true,
                    hasCart = true,
                    isPaid = isPaid,
                    amount = amount,
                    orderId = cartOrder.OrderId,
                    message = isPaid ? "Đã thanh toán thành công" : "Chưa thanh toán"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi kiểm tra trạng thái: " + ex.Message });
            }
        }

        // GET: /Cart/GetPaymentInstructions
        [HttpGet]
        public IActionResult GetPaymentInstructions()
        {
            int? userId = GetCurrentUserId();
            if (userId == null)
            {
                if (IsAjaxRequest()) return Unauthorized(new { success = false, message = "Unauthorized" });
                return RedirectToAction("Login", "Auth");
            }

            var cart = _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.UserId == userId && o.Status == "cart");


            if (cart == null)
            {
                return Json(new { success = false, message = "Không có giỏ hàng" });
            }

            var amount = cart.TotalPrice;
            // Nội dung chuyển khoản dùng mã tham chiếu theo đơn hàng
            // Để ổn định khi khớp giao dịch, dùng dạng ngắn: ORDER{orderId}
            var today = DateTime.Now.ToString("yyyyMMdd");
            var paymentReference = $"ORDER{cart.OrderId}_{userId}_{today}";
            var transferContent = paymentReference;

            // Tạo QR code cho thanh toán
            var qrCodeUrl = _vietQRService.GetQRCodeUrl(amount ?? 0, transferContent);

            var instructions = new
            {
                success = true,
                amount = amount,
                userId = userId,
                transferContent = transferContent,
                qrCodeUrl = qrCodeUrl,
                instructions = new[]
                {
                    $"Chuyển khoản chính xác số tiền: {amount:N0}₫",
                    $"Nội dung chuyển khoản: {transferContent}",
                    "Quét mã QR bằng ứng dụng ngân hàng để thanh toán nhanh",
                    "Sau khi chuyển khoản, nhấn nút 'Kiểm tra thanh toán' để xác nhận",
                    "Hệ thống sẽ tự động xác minh và cập nhật trạng thái đơn hàng"
                }
            };

            return Json(instructions);
        }


        // Test method để debug regex
        [HttpGet("test-regex")]
        public IActionResult TestRegex(string text = "UserID1 Ma giao dich Trace091476 Trace 091476")
        {
            var patterns = new[]
            {
                @"UserID[:\s]*(\d+)",  // UserID:123, UserID 123, hoặc UserID123
                @"User[:\s]*(\d+)",    // User:123, User 123, hoặc User123
                @"(\d+)$"              // Số ở cuối chuỗi
            };

            var results = new List<object>();
            foreach (var pattern in patterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(text, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                results.Add(new { pattern, success = match.Success, value = match.Success ? match.Groups[1].Value : null });
            }

            return Json(new { text, results });
        }

        // Test method để debug Casso API
        [HttpGet("test-casso")]
        public async Task<IActionResult> TestCasso()
        {
            try
            {
                var from = DateTime.UtcNow.AddDays(-7);
                var to = DateTime.UtcNow;

                var response = await _cassoService.GetTransactionsAsync(from, to);

                return Json(new
                {
                    success = true,
                    code = response.Code,
                    hasData = response.Data != null,
                    recordCount = response.Data?.Records?.Count ?? 0,
                    records = response.Data?.Records?.Take(5).Select(t => new
                    {
                        description = t.Description,
                        reference = t.Reference,
                        ref_field = t.Ref,
                        amount = t.Amount,
                        when = t.When
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // Test method để thử các cách xác thực khác nhau
        [HttpGet("test-casso-auth")]
        public async Task<IActionResult> TestCassoAuth()
        {
            var results = new List<object>();
            var apiKey = _config["Casso:ApiKey"];
            var from = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");
            var to = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var url = $"https://oauth.casso.vn/v2/transactions?from={from}&to={to}&page=1&pageSize=100";

            using var httpClient = new HttpClient();

            // Test 1: Authorization Apikey (Casso format)
            try
            {
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Apikey {apiKey}");
                var response1 = await httpClient.GetAsync(url);
                var content1 = await response1.Content.ReadAsStringAsync();
                results.Add(new { method = "Authorization Apikey", status = (int)response1.StatusCode, content = content1.Substring(0, Math.Min(200, content1.Length)) });
            }
            catch (Exception ex)
            {
                results.Add(new { method = "Authorization Apikey", error = ex.Message });
            }

            // Test 2: Authorization Bearer
            try
            {
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                var response2 = await httpClient.GetAsync(url);
                var content2 = await response2.Content.ReadAsStringAsync();
                results.Add(new { method = "Authorization Bearer", status = (int)response2.StatusCode, content = content2.Substring(0, Math.Min(200, content2.Length)) });
            }
            catch (Exception ex)
            {
                results.Add(new { method = "Authorization Bearer", error = ex.Message });
            }

            // Test 3: X-API-KEY
            try
            {
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
                var response3 = await httpClient.GetAsync(url);
                var content3 = await response3.Content.ReadAsStringAsync();
                results.Add(new { method = "X-API-KEY", status = (int)response3.StatusCode, content = content3.Substring(0, Math.Min(200, content3.Length)) });
            }
            catch (Exception ex)
            {
                results.Add(new { method = "X-API-KEY", error = ex.Message });
            }

            // Test 3: API key in URL
            try
            {
                httpClient.DefaultRequestHeaders.Clear();
                var urlWithKey = $"{url}&api_key={apiKey}";
                var response3 = await httpClient.GetAsync(urlWithKey);
                var content3 = await response3.Content.ReadAsStringAsync();
                results.Add(new { method = "API key in URL", status = (int)response3.StatusCode, content = content3.Substring(0, Math.Min(200, content3.Length)) });
            }
            catch (Exception ex)
            {
                results.Add(new { method = "API key in URL", error = ex.Message });
            }

            return Json(new { results });
        }


        // GET: /Cart/TestCassoResponse - Test Casso API response
        [HttpGet]
        public async Task<IActionResult> TestCassoResponse()
        {
            try
            {
                var from = DateTime.Now.AddDays(-1);
                var to = DateTime.Now;

                var result = await _cassoService.GetTransactionsAsync(from, to);

                return Json(new
                {
                    success = true,
                    code = result.Code,
                    desc = result.Desc,
                    data = result.Data,
                    hasData = result.Data != null,
                    recordCount = result.Data?.Records?.Count ?? 0
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }


        // GET: /Cart/TestCassoDirect - Test Casso API directly with HttpClient
        [HttpGet]
        public async Task<IActionResult> TestCassoDirect()
        {
            try
            {
                using var httpClient = new HttpClient();
                var apiKey = _config["Casso:ApiKey"];
                var apiBase = _config["Casso:ApiBase"];

                httpClient.BaseAddress = new Uri(apiBase);
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Apikey {apiKey}");

                var from = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                var to = DateTime.Now.ToString("yyyy-MM-dd");
                var url = $"/v2/transactions?from={from}&to={to}&page=1&pageSize=100";

                var response = await httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                return Json(new
                {
                    success = response.IsSuccessStatusCode,
                    statusCode = (int)response.StatusCode,
                    statusText = response.StatusCode.ToString(),
                    content = content,
                    headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value))
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        // Debug endpoint để kiểm tra session
        [HttpGet("debug-session")]
        public IActionResult DebugSession()
        {
            var debugInfo = new
            {
                hasSession = HttpContext.Session != null,
                sessionId = HttpContext.Session?.Id,
                isAuthenticated = AuthHelper.IsAuthenticated(HttpContext),
                userId = AuthHelper.GetUserId(HttpContext),
                userEmail = AuthHelper.GetUserEmail(HttpContext),
                cookies = Request.Cookies.Select(c => new { c.Key, c.Value }).ToList(),
                headers = Request.Headers.Select(h => new { h.Key, Value = string.Join(", ", h.Value) }).ToList()
            };

            return Json(debugInfo);
        }

        // Test endpoint đơn giản để kiểm tra session
        [HttpGet("test-session")]
        public IActionResult TestSession()
        {
            try
            {
                var sessionId = HttpContext.Session?.Id;
                var hasSession = HttpContext.Session != null;
                var isAuthenticated = AuthHelper.IsAuthenticated(HttpContext);
                var userId = AuthHelper.GetUserId(HttpContext);

                return Json(new
                {
                    success = true,
                    sessionId = sessionId,
                    hasSession = hasSession,
                    isAuthenticated = isAuthenticated,
                    userId = userId,
                    message = "Session test completed"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        // GET: /Cart/OrderHistory - Lịch sử đơn hàng
        [HttpGet]
        public IActionResult OrderHistory()
        {
            int? userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var orders = _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.ProductImgs)
                .Include(o => o.PaymentDetails)
                .ThenInclude(pd => pd.Payment)
                // Hiển thị cả đơn đang chờ xác minh (đang ở trạng thái cart nhưng có Payment pending) và đơn đã hủy
                .Where(o => o.UserId == userId && (
                    o.Status != "cart" ||
                    o.Status == "cancelled" ||
                    _db.PaymentDetails.Any(pd => pd.OrderId == o.OrderId && pd.Payment != null && pd.Payment.Status == "pending")
                ))
                .AsNoTracking()
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            // Lấy danh sách sản phẩm user đã đánh giá để disable nút Đánh giá
            var reviewedProductIds = _db.Reviews
                .Where(r => r.UserId == userId && r.TargetType == "Product")
                .Select(r => r.TargetId)
                .ToList();
            ViewBag.ReviewedProductIds = reviewedProductIds;

            return View(orders);
        }

        // GET: /Cart/BillDetail/{orderId} - Chi tiết hóa đơn
        [HttpGet]
        public IActionResult BillDetail(int orderId)
        {
            int? userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var order = _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.ProductImgs)
                .Include(o => o.PaymentDetails)
                .ThenInclude(pd => pd.Payment)
                .Include(o => o.User)
                .FirstOrDefault(o => o.OrderId == orderId && o.UserId == userId && o.Status != "cart");

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // Xử lý stock sau khi thanh toán thành công
        private async Task<(bool Success, string Message)> ProcessStockAfterPayment(int orderId)
        {
            try
            {
                var order = await _db.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    return (false, "Không tìm thấy đơn hàng");
                }

                var insufficientStockItems = new List<string>();

                // Kiểm tra stock cho từng sản phẩm
                foreach (var item in order.OrderItems)
                {
                    if (item.Product.StockQuantity.HasValue)
                    {
                        if (item.Product.StockQuantity.Value < item.Quantity)
                        {
                            insufficientStockItems.Add($"{item.Product.Name} (cần {item.Quantity}, còn {item.Product.StockQuantity.Value})");
                        }
                    }
                }

                // Nếu có sản phẩm không đủ stock, hủy thanh toán
                if (insufficientStockItems.Any())
                {
                    // Revert payment status
                    var paymentDetails = await _db.PaymentDetails
                        .Include(pd => pd.Payment)
                        .Where(pd => pd.OrderId == orderId)
                        .ToListAsync();

                    foreach (var paymentDetail in paymentDetails)
                    {
                        if (paymentDetail.Payment != null)
                        {
                            paymentDetail.Payment.Status = "failed";
                            _db.Payments.Update(paymentDetail.Payment);
                        }
                    }

                    // Revert order status
                    order.Status = "cart";
                    _db.Orders.Update(order);
                    await _db.SaveChangesAsync();

                    var message = "Một số sản phẩm trong giỏ hàng đã hết hàng:\n" + string.Join("\n", insufficientStockItems) + "\n\nĐơn hàng đã được hủy. Vui lòng kiểm tra lại giỏ hàng và điều chỉnh số lượng phù hợp.";
                    return (false, message);
                }

                // Giảm stock cho tất cả sản phẩm
                foreach (var item in order.OrderItems)
                {
                    if (item.Product.StockQuantity.HasValue)
                    {
                        item.Product.StockQuantity -= item.Quantity;
                        _db.Products.Update(item.Product);
                    }
                }

                // Cập nhật trạng thái đơn hàng thành confirmed
                order.Status = "confirmed";
                order.UpdatedAt = DateTime.Now;
                _db.Orders.Update(order);

                await _db.SaveChangesAsync();

                return (true, "Thanh toán thành công! Đơn hàng đã được xác nhận và kho hàng đã được cập nhật.");
            }
            catch (Exception ex)
            {
                return (false, "Đã xảy ra lỗi khi xử lý kho hàng. Vui lòng thử lại sau.");
            }
        }

        private bool IsAjaxRequest()
        {
            return string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsValidVietnamesePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            // Loại bỏ khoảng trắng và ký tự đặc biệt
            phone = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

            // Kiểm tra độ dài (chỉ 10 số)
            if (phone.Length != 10)
                return false;

            // Phải bắt đầu bằng 0
            if (!phone.StartsWith("0"))
                return false;

            // Kiểm tra đầu số Việt Nam hợp lệ
            var validPrefixes = new[]
            {
                "032", "033", "034", "035", "036", "037", "038", "039", // Viettel
                "070", "076", "077", "078", "079", // Mobifone
                "081", "082", "083", "084", "085", // Vinaphone
                "056", "058", // Vietnamobile
                "059", // Gmobile
                "020", "021", "022", "023", "024", "025", "026", "027", "028", "029", // Landline
                "030", "031", "040", "041", "042", "043", "044", "045", "046", "047", "048", "049", // Landline
                "050", "051", "052", "053", "054", "055", "057", // Landline
                "060", "061", "062", "063", "064", "065", "066", "067", "068", "069", // Landline
                "090", "091", "092", "093", "094", "095", "096", "097", "098", "099", // Mobile
                "086", "087", "088", "089" // Mobile
            };

            // Kiểm tra 3 số đầu
            var prefix = phone.Substring(0, 3);
            return validPrefixes.Contains(prefix);
        }
    }
}

