using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonAmour.Models;
using MonAmour.Services.Interfaces;
using MonAmour.ViewModels;
using MonAmour.Helpers;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;

namespace MonAmour.Controllers
{
    public class BookingController : Controller
    {
        private readonly MonAmourDbContext _db;
        private readonly ICassoService _cassoService;
        private readonly IAuthService _authService;
        private readonly IVietQRService _vietQRService;
        private readonly IConfiguration _config;

        public BookingController(MonAmourDbContext db, ICassoService cassoService, IAuthService authService, IVietQRService vietQRService, IConfiguration config)
        {
            _db = db;
            _cassoService = cassoService;
            _authService = authService;
            _vietQRService = vietQRService;
            _config = config;
        }

        // GET: /Booking/Create/{conceptId}
        [HttpGet]
        public IActionResult Create(int conceptId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var concept = _db.Concepts
                .Include(c => c.ConceptImgs)
                .Include(c => c.Category)
                .Include(c => c.Location)
                .FirstOrDefault(c => c.ConceptId == conceptId);

            if (concept == null)
            {
                return NotFound();
            }

            ViewBag.Concept = concept;
            return View();
        }

        // POST: /Booking/Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBookingRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            try
            {
                var concept = _db.Concepts.FirstOrDefault(c => c.ConceptId == request.ConceptId);
                if (concept == null)
                {
                    return Json(new { success = false, message = "Concept không tồn tại" });
                }

                // Tạo booking mới
                var booking = new Booking
                {
                    UserId = userId,
                    ConceptId = request.ConceptId,
                    BookingDate = DateOnly.FromDateTime(request.BookingDate),
                    BookingTime = TimeOnly.Parse(request.BookingTime),
                    Status = "confirmed", // Chỉ cho phép: completed, cancelled, confirmed
                    PaymentStatus = "pending", // Chỉ cho phép: partial_refund, refunded, paid, pending
                    TotalPrice = concept.Price,
                    CreatedAt = DateTime.Now
                };

                _db.Bookings.Add(booking);
                try
                {
                    await _db.SaveChangesAsync();
                    Console.WriteLine($"Booking saved successfully. BookingId: {booking.BookingId}");
                }
                catch (Exception ex)
                {
                    // Log chi tiết lỗi để debug
                    Console.WriteLine($"Error saving booking: {ex.Message}");
                    Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                    throw;
                }

                // Tạo payment
                var payment = new Payment
                {
                    Amount = concept.Price,
                    Status = "pending",
                    PaymentMethodId = 1, // Casso payment method
                    CreatedAt = DateTime.Now,
                    UserId = userId,
                    PaymentReference = $"BOOKING_{booking.BookingId}_{DateTime.Now:yyyyMMddHHmmss}"
                };

                _db.Payments.Add(payment);
                try
                {
                    await _db.SaveChangesAsync();
                    Console.WriteLine($"Payment saved successfully. PaymentId: {payment.PaymentId}");
                }
                catch (Exception ex)
                {
                    // Log chi tiết lỗi để debug
                    Console.WriteLine($"Error saving payment: {ex.Message}");
                    Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                    throw;
                }

                // Tạo payment detail
                var paymentDetail = new PaymentDetail
                {
                    PaymentId = payment.PaymentId,
                    BookingId = booking.BookingId,
                    Amount = concept.Price
                };
                
                Console.WriteLine($"Creating PaymentDetail with PaymentId: {payment.PaymentId}, BookingId: {booking.BookingId}, Amount: {concept.Price}");

                _db.PaymentDetails.Add(paymentDetail);
                try
                {
                    await _db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // Log chi tiết lỗi để debug
                    Console.WriteLine($"Error saving payment detail: {ex.Message}");
                    Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                    throw;
                }

                // Chuyển đến trang thanh toán
                return Json(new { 
                    success = true, 
                    redirectUrl = Url.Action("Payment", "Booking", new { bookingId = booking.BookingId })
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating booking: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tạo booking" });
            }
        }

        // GET: /Booking/Payment/{bookingId}
        [HttpGet]
        public async Task<IActionResult> Payment(int bookingId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var booking = _db.Bookings
                .Include(b => b.Concept)
                .Include(b => b.Concept.ConceptImgs)
                .Include(b => b.PaymentDetails)
                .ThenInclude(pd => pd.Payment)
                .FirstOrDefault(b => b.BookingId == bookingId && b.UserId == userId);

            if (booking == null)
            {
                return NotFound();
            }

            if (booking.PaymentStatus == "paid")
            {
                return RedirectToAction("Success", "Booking", new { bookingId = bookingId });
            }

            // Tạo mã giao dịch cho Casso (tương tự như CartController)
            var payment = booking.PaymentDetails.FirstOrDefault()?.Payment;
            if (payment != null && string.IsNullOrEmpty(payment.PaymentReference))
            {
                var traceId = $"BOOKING_{bookingId}_{DateTime.Now:yyyyMMddHHmmss}";
                payment.PaymentReference = traceId;
                _db.SaveChanges();
                Console.WriteLine($"Created PaymentReference: {traceId}");
            }

            // Tạo QR code cho thanh toán
            var qrCodeUrl = "";
            if (payment?.PaymentReference != null && booking.TotalPrice.HasValue)
            {
                qrCodeUrl = await GenerateVietQrForBooking(booking.TotalPrice.Value, payment.PaymentReference);
            }

            ViewBag.PaymentReference = payment?.PaymentReference;
            ViewBag.QRCodeUrl = qrCodeUrl;
            Console.WriteLine($"QR Code URL for ViewBag: '{qrCodeUrl}'");
            return View(booking);
        }


        // GET: /Booking/CheckPayment/{bookingId}
        [HttpGet]
        public async Task<IActionResult> CheckPayment(int bookingId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            try
            {
                var booking = _db.Bookings
                    .Include(b => b.PaymentDetails)
                    .ThenInclude(pd => pd.Payment)
                    .FirstOrDefault(b => b.BookingId == bookingId && b.UserId == userId);

                if (booking == null)
                {
                    return Json(new { success = false, message = "Booking không tồn tại" });
                }

                if (booking.PaymentStatus == "paid")
                {
                    return Json(new { success = true, message = "Đã thanh toán", redirectUrl = Url.Action("Success", "Booking", new { bookingId = bookingId }) });
                }

                var payment = booking.PaymentDetails.FirstOrDefault()?.Payment;
                if (payment == null || string.IsNullOrEmpty(payment.PaymentReference))
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin thanh toán" });
                }

                // Sử dụng logic kiểm tra thanh toán từ CartController
                var fromDate = DateTime.Now.AddDays(-7); // Mở rộng lên 7 ngày
                var toDate = DateTime.Now;
                Console.WriteLine($"Querying Casso from {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}");
                var transactions = await _cassoService.GetTransactionsAsync(fromDate, toDate);
                
                Console.WriteLine($"Casso API Response: Code={transactions?.Code}");
                Console.WriteLine($"Casso API Desc: {transactions?.Desc}");
                
                // Xử lý rate limit
                if (transactions?.Code == 429)
                {
                    Console.WriteLine("Casso API Rate Limited - Too many requests");
                    return Json(new { success = false, message = "Đang kiểm tra quá nhiều lần, vui lòng đợi 1 phút rồi thử lại" });
                }
                
                if (transactions?.Data != null)
                {
                    Console.WriteLine($"Casso Data: TotalRecords={transactions.Data.Total}");
                    Console.WriteLine($"Casso Records Count: {transactions.Data.Records?.Count ?? 0}");
                }
                else
                {
                    Console.WriteLine("Casso Data is NULL!");
                }
                
                Console.WriteLine($"Checking payment for booking {bookingId}");
                Console.WriteLine($"Payment Reference: {payment.PaymentReference}");
                Console.WriteLine($"Booking Amount: {booking.TotalPrice}");
                Console.WriteLine($"Expected Amount (VND): {(int)booking.TotalPrice}");
                
                if (transactions?.Data?.Records != null)
                {
                    var recentTransactions = transactions.Data.Records.ToList();
                    Console.WriteLine($"Found {recentTransactions.Count} recent transactions");

                    foreach (var transaction in recentTransactions)
                    {
                        Console.WriteLine($"Transaction: Description='{transaction.Description}', Amount={transaction.Amount}");
                        
                        // Kiểm tra nội dung chuyển khoản có chứa mã booking
                        // VietQR có thể format lại addInfo, nên cần kiểm tra linh hoạt
                        var originalReference = payment.PaymentReference;
                        var flexibleReference = originalReference?.Replace("_", ""); // Bỏ dấu gạch dưới
                        var currentBookingId = booking.BookingId.ToString();
                        
                        var hasReference = transaction.Description != null && (
                            transaction.Description.Contains(originalReference) ||
                            transaction.Description.Contains(flexibleReference) ||
                            transaction.Description.Contains($"BOOKING{currentBookingId}")
                        );
                        var amountMatches = transaction.Amount == (int)booking.TotalPrice;
                        
                        Console.WriteLine($"  - Has Reference: {hasReference} (looking for: '{originalReference}' or '{flexibleReference}' or 'BOOKING{currentBookingId}')");
                        Console.WriteLine($"  - Amount Matches: {amountMatches} (transaction: {transaction.Amount}, expected: {(int)booking.TotalPrice})");
                        
                        if (hasReference && amountMatches)
                        {
                            Console.WriteLine("✅ MATCH FOUND! Processing payment...");
                            
                            try
                            {
                                // Cập nhật trạng thái thanh toán - phải tuân thủ CHECK constraint của DB
                                // Các giá trị hợp lệ theo DB: completed, cancelled, confirmed (tuỳ bảng Payment)
                                payment.Status = "completed";
                                payment.ProcessedAt = DateTime.Now;
                                
                                // Parse TransactionId safely
                                if (long.TryParse(transaction.Tid ?? "0", out long transactionId))
                                {
                                    payment.TransactionId = transactionId;
                                    Console.WriteLine($"Transaction ID parsed: {transactionId}");
                                }
                                else
                                {
                                    Console.WriteLine($"Failed to parse Transaction ID: {transaction.Tid}");
                                    payment.TransactionId = 0;
                                }
                                
                                booking.PaymentStatus = "paid";
                                booking.Status = "confirmed";
                                booking.ConfirmedAt = DateTime.Now;

                                Console.WriteLine("Saving to database...");
                                await _db.SaveChangesAsync();
                                Console.WriteLine("Database saved successfully!");

                                return Json(new { 
                                    success = true, 
                                    message = "Thanh toán thành công!", 
                                    redirectUrl = Url.Action("Success", "Booking", new { bookingId = bookingId })
                                });
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"❌ Error processing payment: {ex.Message}");
                                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                                return Json(new { success = false, message = $"Lỗi khi xử lý thanh toán: {ex.Message}" });
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No transactions found from Casso API");
                }

                Console.WriteLine("No matching transaction found");
                return Json(new { success = false, message = "Chưa có giao dịch nào phù hợp" });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking payment: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra khi kiểm tra thanh toán" });
            }
        }

        // GET: /Booking/Success/{bookingId}
        [HttpGet]
        public IActionResult Success(int bookingId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var booking = _db.Bookings
                .Include(b => b.Concept)
                .Include(b => b.Concept.ConceptImgs)
                .Include(b => b.Concept.Location)
                .FirstOrDefault(b => b.BookingId == bookingId && b.UserId == userId);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: /Booking/MyBookings
        [HttpGet]
        public IActionResult MyBookings()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var bookings = _db.Bookings
                .Include(b => b.Concept)
                .Include(b => b.Concept.ConceptImgs)
                .Include(b => b.Concept.Location)
                .Where(b => b.UserId == userId && b.PaymentStatus == "paid")
                .OrderByDescending(b => b.CreatedAt)
                .ToList();

            return View(bookings);
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

        [HttpGet]
        public async Task<IActionResult> TestCasso()
        {
            try
            {
                var fromDate = DateTime.Now.AddDays(-7);
                var toDate = DateTime.Now;
                Console.WriteLine($"Testing Casso API from {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}");
                
                var transactions = await _cassoService.GetTransactionsAsync(fromDate, toDate);
                
                return Json(new { 
                    success = true, 
                    fromDate = fromDate.ToString("yyyy-MM-dd"),
                    toDate = toDate.ToString("yyyy-MM-dd"),
                    cassResponse = new {
                        code = transactions?.Code,
                        desc = transactions?.Desc,
                        hasData = transactions?.Data != null,
                        totalRecords = transactions?.Data?.Total,
                        recordCount = transactions?.Data?.Records?.Count,
                        records = transactions?.Data?.Records?.Take(3).Select(r => new {
                            id = r.Id,
                            tid = r.Tid,
                            description = r.Description,
                            amount = r.Amount,
                            when = r.When
                        })
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        private async Task<string> GenerateVietQrForBooking(decimal amount, string paymentReference)
        {
            try
            {
                var apiBase = _config["VietQR:ApiBase"] ?? "https://api.vietqr.io";
                var clientId = _config["VietQR:ClientId"];
                var apiKey = _config["VietQR:ApiKey"];
                var acqId = _config["VietQR:AcqId"];
                var accountNo = _config["VietQR:AccountNo"];
                var accountName = _config["VietQR:AccountName"];

                var vndAmount = Math.Max(0, (long)Math.Round(amount, 0, MidpointRounding.AwayFromZero));

                var payload = new
                {
                    accountNo = accountNo,
                    accountName = accountName,
                    acqId = acqId,
                    addInfo = paymentReference,
                    amount = vndAmount,
                    template = _config["VietQR:Template"] ?? "compact"
                };

                using var http = new HttpClient();
                http.BaseAddress = new Uri(apiBase);
                http.DefaultRequestHeaders.Add("x-client-id", clientId);
                http.DefaultRequestHeaders.Add("x-api-key", apiKey);

                var json = JsonSerializer.Serialize(payload);
                Console.WriteLine($"VietQR Payload: {json}");
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await http.PostAsync("/v2/generate", content);
                Console.WriteLine($"VietQR Response Status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"VietQR Response Content: {responseContent}");
                    
                    // Parse response manually để debug
                    using var doc = JsonDocument.Parse(responseContent);
                    if (doc.RootElement.TryGetProperty("data", out var dataElement))
                    {
                        if (dataElement.TryGetProperty("qrCode", out var qrCodeElement))
                        {
                            var qrCode = qrCodeElement.GetString();
                            Console.WriteLine($"QR Code found: {qrCode?.Substring(0, Math.Min(50, qrCode?.Length ?? 0))}...");
                            return qrCode ?? "";
                        }
                        if (dataElement.TryGetProperty("qrDataURL", out var qrDataURLElement))
                        {
                            var qrDataURL = qrDataURLElement.GetString();
                            Console.WriteLine($"QR Data URL found: {qrDataURL}");
                            return qrDataURL ?? "";
                        }
                    }
                    
                    Console.WriteLine("No QR code data found in response");
                    return "";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"VietQR Error: {response.StatusCode} - {errorContent}");
                }

                return "";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VietQR Error: {ex.Message}");
                return "";
            }
        }
    }

    public class VietQRResponse
    {
        public string Code { get; set; } = "";
        public string Desc { get; set; } = "";
        public VietQRData? Data { get; set; }
    }

    public class VietQRData
    {
        public string QrDataURL { get; set; } = "";
        public string QrCode { get; set; } = "";
    }
}
