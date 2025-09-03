using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonAmour.Models;
using MonAmour.Helpers;

namespace MonAmour.Controllers
{
    public class CartController : Controller
    {
        private readonly MonAmourDbContext _db;

        public CartController(MonAmourDbContext db)
        {
            _db = db;
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
                item.TotalPrice = item.UnitPrice * item.Quantity;
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
            item.TotalPrice = item.UnitPrice * item.Quantity;
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

        // GET: /Cart/FinalizeZp
        [HttpGet]
        public IActionResult FinalizeZp(decimal? amount = null)
        {
            int? userId = GetCurrentUserId();

            var orderQuery = _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.Status == "cart");

            if (userId != null)
            {
                orderQuery = orderQuery.Where(o => o.UserId == userId);
            }

            var order = orderQuery
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefault();

            if (order == null || order.OrderItems == null || !order.OrderItems.Any())
            {
                TempData["CartError"] = "Giỏ hàng trống";
                return RedirectToAction("Index");
            }

            order.TotalPrice = order.OrderItems
                .Select(i => (decimal?)i.TotalPrice)
                .Sum()
                .GetValueOrDefault(0m);

            // If amount is provided from gateway, prefer it
            var paidAmount = amount ?? order.TotalPrice.GetValueOrDefault(0m);

            var paymentMethod = _db.PaymentMethods.FirstOrDefault();
            var payment = new Payment
            {
                Amount = paidAmount,
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
                Amount = paidAmount
            };
            _db.PaymentDetails.Add(paymentDetail);
            _db.SaveChanges();

            order.Status = "confirmed";
            order.UpdatedAt = DateTime.Now;
            _db.Orders.Update(order);
            _db.SaveChanges();

            TempData["CartSuccess"] = "Thanh toán thành công";
            return RedirectToAction("Index", new { paid = true });
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

        private bool IsAjaxRequest()
        {
            return string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        }
    }
}


