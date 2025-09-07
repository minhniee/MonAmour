using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonAmour.Models;
using MonAmour.Helpers;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MonAmour.Controllers
{
    public class Gift_boxController : Controller
    {
        private readonly MonAmourDbContext _db;
        private readonly IConfiguration _config;

        public Gift_boxController(MonAmourDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }
        /// <summary>
        /// Display the list of gift boxes with optional filters (category, price, sort)
        /// </summary>
        public IActionResult ListGiftbox(int? categoryId, string? price, string? sort, int page = 1)
        {
            ViewData["Title"] = "Gift Box Collection - MonAmour";

            // Prepare base query
            IQueryable<Product> query = _db.Products
                .Include(p => p.ProductImgs)
                .Include(p => p.Category)
                .Where(p => p.Status == "active");

            // Category filter
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Price filter (assuming VND). Map dropdown values to ranges
            // Supported values: under50, 50-100, 100-200, above200 (in USD equivalent) or
            // under500k, 500k-1m, 1m-2m, above2m (VND). We'll support both for flexibility.
            if (!string.IsNullOrWhiteSpace(price))
            {
                var v = price.Trim().ToLowerInvariant();
                switch (v)
                {
                    case "under50":
                        query = query.Where(p => p.Price != null && p.Price < 50_000); // 50k VND
                        break;
                    case "50-100":
                        query = query.Where(p => p.Price != null && p.Price >= 50_000 && p.Price <= 100_000);
                        break;
                    case "100-200":
                        query = query.Where(p => p.Price != null && p.Price >= 100_000 && p.Price <= 200_000);
                        break;
                    case "above200":
                        query = query.Where(p => p.Price != null && p.Price > 200_000);
                        break;
                    case "under500k":
                        query = query.Where(p => p.Price != null && p.Price < 500_000);
                        break;
                    case "500k-1m":
                        query = query.Where(p => p.Price != null && p.Price >= 500_000 && p.Price <= 1_000_000);
                        break;
                    case "1m-2m":
                        query = query.Where(p => p.Price != null && p.Price >= 1_000_000 && p.Price <= 2_000_000);
                        break;
                    case "above2m":
                        query = query.Where(p => p.Price != null && p.Price > 2_000_000);
                        break;
                }
            }

            // Sorting
            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            // Pagination
            const int pageSize = 8;
            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var currentPage = page < 1 ? 1 : (page > totalPages && totalPages > 0 ? totalPages : page);

            var products = query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Provide data for dropdowns
            ViewBag.Categories = _db.ProductCategories
                .OrderBy(c => c.Name)
                .Select(c => new { c.CategoryId, c.Name })
                .ToList();
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SelectedPrice = price;
            ViewBag.SelectedSort = sort;
            ViewBag.Page = currentPage;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;

            // Top 4 best-selling products (exclude cart orders)
            try
            {
                var topIds = _db.OrderItems
                    .Include(oi => oi.Order)
                    .Where(oi => oi.Order != null && oi.Order.Status != "cart")
                    .GroupBy(oi => oi.ProductId)
                    .Select(g => new { ProductId = g.Key, Total = g.Sum(x => (int?)x.Quantity) ?? 0 })
                    .OrderByDescending(x => x.Total)
                    .Take(4)
                    .Select(x => x.ProductId)
                    .ToList();

                var topProducts = _db.Products
                    .Include(p => p.ProductImgs)
                    .Where(p => topIds.Contains(p.ProductId))
                    .AsEnumerable()
                    .OrderBy(p => topIds.IndexOf(p.ProductId))
                    .ToList();

                if (topProducts == null || topProducts.Count == 0)
                {
                    topProducts = products.Take(4).ToList();
                }

                ViewBag.TopProducts = topProducts;
            }
            catch
            {
                ViewBag.TopProducts = products.Take(4).ToList();
            }

            return View(products);
        }

        /// <summary>
        /// Display product detail page
        /// </summary>
        public IActionResult ProductDetail(int id)
        {
            ViewData["Title"] = "Product Detail - MonAmour";

            var product = _db.Products
                .Include(p => p.ProductImgs)
                .Include(p => p.Category)
                .FirstOrDefault(p => p.ProductId == id && p.Status == "active");

            if (product == null)
            {
                return NotFound();
            }

            // Load related products in the same category (exclude current)
            var related = _db.Products
                .Include(p => p.ProductImgs)
                .Where(p => p.Status == "active" && p.ProductId != id && p.CategoryId == product.CategoryId)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            ViewBag.RelatedProducts = related;

            return View(product);
        }

        /// <summary>
        /// Generate VietQR image URL for payment in VND using img.vietqr.io.
        /// Returns JSON { success, url } that FE can use to render an <img>.
        /// </summary>
        [HttpGet]
        public IActionResult GenerateVietQr(decimal amount, string? note)
        {
            // Lấy UserID từ session
            var userId = AuthHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            try
            {
                var baseUrl = _config["VietQR:BaseUrl"] ?? "https://img.vietqr.io";
                var bankCode = _config["VietQR:BankCode"] ?? "VCB";
                var accountNo = _config["VietQR:AccountNo"] ?? string.Empty;
                var accountName = _config["VietQR:AccountName"] ?? string.Empty;
                var template = _config["VietQR:Template"] ?? "compact";

                if (string.IsNullOrWhiteSpace(accountNo))
                {
                    return Json(new { success = false, message = "Missing VietQR account configuration" });
                }

                // VietQR expects integer amount in VND
                var vndAmount = Math.Max(0, (long)Math.Round(amount, 0, MidpointRounding.AwayFromZero));
                // Nội dung chuyển khoản: ưu tiên tham số note (ví dụ: ORDER{orderId}), fallback UserID
                var info = string.IsNullOrWhiteSpace(note) ? $"UserID:{userId}" : note;

                var encodedInfo = Uri.EscapeDataString(info);
                var encodedName = Uri.EscapeDataString(accountName);

                var url = $"{baseUrl}/{bankCode}/{accountNo}/{vndAmount}.png?addInfo={encodedInfo}&accountName={encodedName}&template={template}";

                return Json(new { success = true, url });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Generate VERIFIED VietQR using official API (api.vietqr.io) with client credentials.
        /// Returns JSON { success, data } where data contains QR code URL (base64/png) and text.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GenerateVietQrVerified([FromForm] decimal amount, [FromForm] string? note)
        {
            // Lấy UserID từ session
            var userId = AuthHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            var apiBase = _config["VietQR:ApiBase"] ?? "https://api.vietqr.io";
            var clientId = _config["VietQR:ClientId"];
            var apiKey = _config["VietQR:ApiKey"];
            var acqId = _config["VietQR:AcqId"]; // numeric bank id
            var accountNo = _config["VietQR:AccountNo"];
            var accountName = _config["VietQR:AccountName"];

            if (string.IsNullOrWhiteSpace(clientId) || clientId == "YOUR_CLIENT_ID" || string.IsNullOrWhiteSpace(apiKey) || apiKey == "YOUR_API_KEY")
            {
                return Json(new { success = false, message = "VietQR API credentials are not configured." });
            }
            if (string.IsNullOrWhiteSpace(acqId) || string.IsNullOrWhiteSpace(accountNo))
            {
                return Json(new { success = false, message = "VietQR account is not configured." });
            }

            var vndAmount = Math.Max(0, (long)Math.Round(amount, 0, MidpointRounding.AwayFromZero));
            // Nội dung chuyển khoản: ưu tiên tham số note (ví dụ: ORDER{orderId}), fallback UserID
            var description = string.IsNullOrWhiteSpace(note) ? $"UserID:{userId}" : note;

            var payload = new
            {
                accountNo = accountNo,
                accountName = accountName,
                acqId = acqId,
                addInfo = description,
                amount = vndAmount,
                template = _config["VietQR:Template"] ?? "compact"
            };

            try
            {
                using var http = new HttpClient();
                http.BaseAddress = new Uri(apiBase);
                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                http.DefaultRequestHeaders.Add("x-client-id", clientId!);
                http.DefaultRequestHeaders.Add("x-api-key", apiKey!);

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var resp = await http.PostAsync("/v2/generate", content);
                var body = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    return Json(new { success = false, status = (int)resp.StatusCode, message = body });
                }

                // VietQR typical response: { code: "00", desc: "Success", data: { qrDataURL, qrCode, ... } }
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                var code = root.TryGetProperty("code", out var codeEl) ? codeEl.GetString() : null;
                var data = root.TryGetProperty("data", out var dataEl) ? dataEl.ToString() : null;

                var ok = string.Equals(code, "00", StringComparison.OrdinalIgnoreCase);
                return Json(new { success = ok, response = JsonSerializer.Deserialize<object>(body) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
