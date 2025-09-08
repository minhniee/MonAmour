using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonAmour.Helpers;
using MonAmour.Models;
using MonAmour.Services.Interfaces;
using System.Security.Claims;
using System.Text.Json;

namespace MonAmour.Controllers
{
    public class CartController : Controller
    {
        private readonly MonAmourDbContext _db;
        private readonly ICassoService _cassoService;
        private readonly IVietQRService _vietQRService;
        private readonly IConfiguration _config;

        public CartController(MonAmourDbContext db, ICassoService cassoService, IVietQRService vietQRService, IConfiguration configuration)
        {
            _db = db;
            _cassoService = cassoService;
            _vietQRService = vietQRService;
            _config = configuration;
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
                .FirstOrDefault(o => o.UserId == userId && o.Status == "cart");

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

            // Check stock availability
            if (product.StockQuantity.HasValue && product.StockQuantity.Value < quantity)
            {
                if (IsAjaxRequest())
                {
                    return BadRequest(new { success = false, message = $"Chỉ còn {product.StockQuantity.Value} sản phẩm trong kho" });
                }
                TempData["CartError"] = $"Chỉ còn {product.StockQuantity.Value} sản phẩm trong kho";
                return RedirectToAction("ProductDetail", "Gift_box", new { id = productId });
            }

            var cart = _db.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.UserId == userId && o.Status == "cart");

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

            // Check total stock availability (existing cart items + new quantity)
            if (product.StockQuantity.HasValue && product.StockQuantity.Value < totalQuantityInCart)
            {
                if (IsAjaxRequest())
                {
                    return BadRequest(new { success = false, message = $"Chỉ còn {product.StockQuantity.Value} sản phẩm trong kho. Đã có {item?.Quantity ?? 0} trong giỏ hàng." });
                }
                TempData["CartError"] = $"Chỉ còn {product.StockQuantity.Value} sản phẩm trong kho. Đã có {item?.Quantity ?? 0} trong giỏ hàng.";
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

            // Update product stock
            if (product.StockQuantity.HasValue)
            {
                product.StockQuantity -= quantity;
                _db.Products.Update(product);
            }

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

            // Check stock availability for the difference
            if (quantityDifference > 0 && item.Product.StockQuantity.HasValue && item.Product.StockQuantity.Value < quantityDifference)
            {
                TempData["CartError"] = $"Chỉ còn {item.Product.StockQuantity.Value} sản phẩm trong kho";
                return RedirectToAction("Index");
            }

            // Update cart item
            item.Quantity = quantity;
            item.TotalPrice = (decimal)(item.UnitPrice * item.Quantity);
            _db.OrderItems.Update(item);

            // Update product stock
            if (item.Product.StockQuantity.HasValue)
            {
                item.Product.StockQuantity -= quantityDifference;
                _db.Products.Update(item.Product);
            }

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

            // Restore stock quantity
            if (item.Product.StockQuantity.HasValue)
            {
                item.Product.StockQuantity += item.Quantity;
                _db.Products.Update(item.Product);
            }

            var orderId = item.OrderId;
            _db.OrderItems.Remove(item);
            _db.SaveChanges();

            var order = _db.Orders.Include(o => o.OrderItems).FirstOrDefault(o => o.OrderId == orderId);
            if (order != null)
            {
                order.TotalPrice = order.OrderItems
                    .Select(i => (decimal?)i.TotalPrice)
                    .Sum()
                    .GetValueOrDefault(0m);
                order.UpdatedAt = DateTime.Now;
                _db.Orders.Update(order);
                _db.SaveChanges();
            }

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
            // Basic phone validation (10-11 digits, starts with 0)
            var phoneStr = (phone ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(phoneStr) || !System.Text.RegularExpressions.Regex.IsMatch(phoneStr, "^0[0-9]{9,10}$"))
            {
                return Json(new { success = false, message = "Vui lòng nhập số điện thoại hợp lệ (10-11 số, bắt đầu bằng 0)" });
            }

            var cart = await _db.Orders.FirstOrDefaultAsync(o => o.UserId == userId && o.Status == "cart");
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
                .FirstOrDefault(o => o.UserId == userId && o.Status == "cart");

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

            // Create a payment with status pending -> completed (for demo)
            var paymentMethod = _db.PaymentMethods.FirstOrDefault();
            var payment = new Payment
            {
                Amount = order.TotalPrice,
                Status = "completed",
                PaymentMethodId = paymentMethod?.PaymentMethodId,
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
                    cartOrderRef = $"ORDER{cartOrder.OrderId}";
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
                                     response.Code == 500 ? "Lỗi server Casso" :
                                     $"Lỗi API Casso (Code: {response.Code})";

                    return Json(new { success = false, message = errorMessage, debug = debugInfo });
                }

                var processedCount = 0;

                foreach (var transaction in response.Data.Records)
                {
                    // Debug: Log transaction details
                    var transactionText = $"Transaction: Description='{transaction.Description}', Reference='{transaction.Reference}', Ref='{transaction.Ref}', Amount={transaction.Amount}";
                    debugInfo.Add(transactionText);

                    // Kiểm tra theo mã tham chiếu đơn hàng hoặc theo UserID (tương thích cũ)
                    var hasOrderRef = !string.IsNullOrWhiteSpace(cartOrderRef) && (transaction.Description?.Contains(cartOrderRef) == true || transaction.Reference?.Contains(cartOrderRef) == true || transaction.Ref?.Contains(cartOrderRef) == true);
                    var transactionUserId = ExtractUserIdFromTransaction(transaction);
                    debugInfo.Add($"HasOrderRef: {hasOrderRef}, OrderRef: {cartOrderRef}, Extracted UserID: {transactionUserId}, Current UserID: {userId}");

                    if (hasOrderRef || transactionUserId == userId)
                    {
                        var success = await _cassoService.ProcessPaymentAsync(transaction, userId.Value);
                        if (success) processedCount++;
                        debugInfo.Add($"Processed transaction: {success}");
                    }
                }

                if (processedCount > 0)
                {
                    return Json(new { success = true, message = $"Đã xử lý {processedCount} giao dịch thành công", debug = debugInfo });
                }
                else
                {
                    return Json(new { success = false, message = "Chưa có giao dịch nào phù hợp", debug = debugInfo });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi kiểm tra thanh toán: " + ex.Message });
            }
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
            // Nội dung chuyển khoản dùng mã tham chiếu theo đơn hàng, tương tự booking
            // Để ổn định khi khớp giao dịch, dùng dạng ngắn: ORDER{orderId}
            var paymentReference = $"ORDER{cart.OrderId}";
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

        private int? ExtractUserIdFromTransaction(CassoTransaction transaction)
        {
            // Try to extract user ID from various fields
            var textToSearch = $"{transaction.Description} {transaction.Reference} {transaction.Ref}";

            // Look for patterns like "UserID123" hoặc "UserID:123" (hỗ trợ cả 2 format)
            var patterns = new[]
            {
                @"UserID[:\s]*(\d+)",  // UserID:123, UserID 123, hoặc UserID123
                @"User[:\s]*(\d+)",    // User:123, User 123, hoặc User123
                @"(\d+)$"              // Số ở cuối chuỗi
            };

            foreach (var pattern in patterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(textToSearch, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (match.Success && int.TryParse(match.Groups[1].Value, out var userId))
                {
                    return userId;
                }
            }

            return null;
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
                .Where(o => o.UserId == userId && o.Status != "cart")
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            return View(orders);
        }

        private bool IsAjaxRequest()
        {
            return string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        }
    }
}


