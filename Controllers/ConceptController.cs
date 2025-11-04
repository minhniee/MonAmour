using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonAmour.Models;
using MonAmour.Services.Interfaces;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MonAmour.Controllers
{
    public class ConceptController : Controller
    {
        private readonly MonAmourDbContext _db;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        public ConceptController(MonAmourDbContext db, IEmailService emailService, IConfiguration config)
        {
            _db = db;
            _emailService = emailService;
            _config = config;
        }

        public async Task<IActionResult> ListConcept(int? categoryId, int page = 1, string? city = null, string? q = null, string? sortBy = null)
        {
            const int pageSize = 8;
            if (page < 1) page = 1;

            var categories = await _db.ConceptCategories
                .OrderBy(c => c.Name)
                .ToListAsync();

            // Cities for filter
            var cities = await _db.Locations
                .AsNoTracking()
                .Where(l => l.City != null && l.City != "")
                .Select(l => l.City!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            var query = _db.Concepts
                .AsNoTracking()
                .Include(c => c.ConceptImgs)
                .Include(c => c.Category)
                .Include(c => c.Location)
                .Where(c => c.AvailabilityStatus == true);

            if (categoryId.HasValue)
            {
                query = query.Where(c => c.CategoryId == categoryId.Value);
            }

            // Filter by city
            if (!string.IsNullOrWhiteSpace(city))
            {
                query = query.Where(c => c.Location != null && c.Location.City != null && c.Location.City.Contains(city));
            }

            // Search by address/district/city
            if (!string.IsNullOrWhiteSpace(q))
            {
                var keyword = q.Trim();
                query = query.Where(c =>
                    (c.Location != null && (
                        (c.Location.Address ?? "").Contains(keyword) ||
                        (c.Location.District ?? "").Contains(keyword) ||
                        (c.Location.City ?? "").Contains(keyword)
                    )) ||
                    (c.Name != null && c.Name.Contains(keyword))
                );
            }

            // Apply sorting
            switch (sortBy)
            {
                case "price_asc":
                    query = query.OrderBy(c => c.Price ?? decimal.MaxValue).ThenByDescending(c => c.CreatedAt);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(c => c.Price ?? decimal.MinValue).ThenByDescending(c => c.CreatedAt);
                    break;
                case "name_asc":
                    query = query.OrderBy(c => c.Name).ThenByDescending(c => c.CreatedAt);
                    break;
                case "name_desc":
                    query = query.OrderByDescending(c => c.Name).ThenByDescending(c => c.CreatedAt);
                    break;
                default:
                    // Default: Sort by price ascending (lowest to highest)
                    query = query.OrderBy(c => c.Price ?? decimal.MaxValue).ThenByDescending(c => c.CreatedAt);
                    break;
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;
            ViewBag.Cities = cities;
            ViewBag.SelectedCity = city;
            ViewBag.Search = q;
            ViewBag.SortBy = string.IsNullOrEmpty(sortBy) ? "price_asc" : sortBy;

            return View(items);
        }

        public async Task<IActionResult> ConceptDetail(int id)
        {
            var concept = await _db.Concepts
                .Include(c => c.ConceptImgs)
                .Include(c => c.Category)
                .Include(c => c.ConceptColorJunctions)
                    .ThenInclude(ccj => ccj.Color)
                .Include(c => c.Ambience)
                .Include(c => c.Location)
                .FirstOrDefaultAsync(c => c.ConceptId == id);

            if (concept == null)
            {
                return NotFound();
            }

            // Chặn truy cập nếu concept đang không khả dụng
            if (concept.AvailabilityStatus != true)
            {
                TempData["ConceptError"] = "Concept hiện không khả dụng.";
                return RedirectToAction("ListConcept");
            }

            // Lấy các concept liên quan (cùng category)
            var relatedConcepts = await _db.Concepts
                .Include(c => c.ConceptImgs)
                .Include(c => c.Category)
                .Where(c => c.CategoryId == concept.CategoryId && c.ConceptId != id && c.AvailabilityStatus == true)
                .Take(4)
                .ToListAsync();

            ViewBag.RelatedConcepts = relatedConcepts;

            // Dữ liệu cho form liên hệ: danh mục và danh sách thành phố
            ViewBag.Categories = await _db.ConceptCategories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.Cities = await _db.Locations
                .AsNoTracking()
                .Where(l => l.City != null && l.City != "")
                .Select(l => l.City!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return View(concept);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitContact(
            string fullName,
            string email,
            string phone,
            string? currentLocation,
            string? datePurpose,
            string? budget,
            DateTime? eventDate,
            string? eventStartTime,
            string? eventEndTime,
            string? spacePreference,
            string message,
            string? referral)
        {
            // Basic server-side validation
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(message))
            {
                return BadRequest("Vui lòng nhập đầy đủ thông tin bắt buộc.");
            }

            // Build common styles and detail block
            var styles = @"<!DOCTYPE html><html><head><meta charset='utf-8'><style>
			body{font-family:Arial,sans-serif;color:#333} .wrapper{max-width:640px;margin:0 auto;background:#fff;border:1px solid #eee;border-radius:12px;overflow:hidden}
			.header{background:#7f1d1d;color:#fff;padding:24px;text-align:center} .content{padding:24px} .row{margin-bottom:12px}
			.label{color:#7f1d1d;font-weight:bold} .card{background:#faf6f5;border:1px solid #f0e8e7;border-radius:8px;padding:16px}
			.footer{background:#fafafa;color:#666;font-size:12px;padding:16px;text-align:center}
			</style></head><body><div class='wrapper'>";

            var details = new StringBuilder();
            details.Append("<div class='card'>");
            details.Append($"<div class='row'><span class='label'>Họ tên:</span> {fullName}</div>");
            details.Append($"<div class='row'><span class='label'>Email:</span> {email}</div>");
            details.Append($"<div class='row'><span class='label'>SĐT:</span> {phone}</div>");
            details.Append($"<div class='row'><span class='label'>Nơi sống hiện tại:</span> {currentLocation ?? "-"}</div>");
            details.Append($"<div class='row'><span class='label'>Mục đích buổi hẹn hò:</span> {datePurpose ?? "-"}</div>");
            details.Append($"<div class='row'><span class='label'>Ngân sách mong muốn:</span> {budget ?? "-"}</div>");
            details.Append($"<div class='row'><span class='label'>Ngày hẹn hò:</span> {(eventDate.HasValue ? eventDate.Value.ToString("dd/MM/yyyy") : "-")}</div>");
            details.Append($"<div class='row'><span class='label'>Khoảng thời gian:</span> {eventStartTime ?? "-"} đến {eventEndTime ?? "-"}</div>");
            details.Append($"<div class='row'><span class='label'>Không gian mong muốn:</span> {spacePreference ?? "-"}</div>");
            details.Append($"<div class='row'><span class='label'>Kênh biết đến:</span> {referral ?? "-"}</div>");
            details.Append($"<div class='row'><span class='label'>Điều khách hàng muốn MonAmour lưu ý:</span><br>{message}</div>");
            details.Append("</div>");

            var footer = "</div><div class='footer'>© MonAmour</div></div></body></html>";

            // Customer email
            var customerHtml = new StringBuilder();
            customerHtml.Append(styles);
            customerHtml.Append("<div class='header'><h2>MonAmour - Xác nhận yêu cầu tư vấn</h2></div>");
            customerHtml.Append("<div class='content'>");
            customerHtml.Append($"<p>Chào {fullName},</p><p>Mon Amour rất hạnh phúc khi nhận đã được sự tin tưởng từ bạn. Hãy yên tâm, chúng mình sẽ sớm kết nối để mang đến cho bạn sự tư vấn tận tình và những trải nghiệm ngọt ngào nhất 💞✨ ^^</p>");
            customerHtml.Append(details.ToString());
            customerHtml.Append(footer);

            var customerHtmlStr = customerHtml.ToString();
            await _emailService.SendContactConfirmationEmailAsync(email, fullName, customerHtmlStr);

            // Admin email: gửi cho toàn bộ admin trong hệ thống
            // Admin email (khác nội dung):
            var adminHtmlBuilder = new StringBuilder();
            adminHtmlBuilder.Append(styles);
            adminHtmlBuilder.Append("<div class='header'><h2>MonAmour - Yêu cầu tư vấn mới từ khách hàng</h2></div>");
            adminHtmlBuilder.Append("<div class='content'>");
            adminHtmlBuilder.Append("<p>Bạn nhận được một yêu cầu tư vấn mới. Vui lòng kiểm tra thông tin chi tiết bên dưới và chủ động liên hệ khách hàng để xác nhận lịch hẹn.</p>");
            adminHtmlBuilder.Append(details.ToString());
            adminHtmlBuilder.Append("<p style='margin-top:12px;color:#7f1d1d'><strong>Lưu ý:</strong> Không chia sẻ thông tin khách hàng ra ngoài hệ thống.</p>");
            adminHtmlBuilder.Append(footer);
            var adminHtml = adminHtmlBuilder.ToString();
            var adminEmails = await _db.UserRoles
                .AsNoTracking()
                .Include(ur => ur.User)
                .Where(ur => ur.RoleId == 1 && ur.User.Email != null)
                .Select(ur => ur.User.Email!)
                .Distinct()
                .ToListAsync();

            foreach (var adminEmail in adminEmails)
            {
                await _emailService.SendAdminPaymentIssueReportAsync(adminEmail, "Yêu cầu tư vấn mới", adminHtml);
            }

            return Ok(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitChatbotContact(
            int conceptId,
            string fullName,
            string email,
            string phone,
            string? currentLocation,
            string? datePurpose,
            string? budget,
            string? eventDate,
            string? eventStartTime,
            string? eventEndTime,
            string? spacePreference,
            string message,
            string? referral,
            string? conceptImageUrl)
        {
            // Basic server-side validation
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(message))
            {
                return BadRequest(new { success = false, message = "Vui lòng nhập đầy đủ thông tin bắt buộc." });
            }

            // Get concept information
            var concept = await _db.Concepts
                .FirstOrDefaultAsync(c => c.ConceptId == conceptId);

            if (concept == null)
            {
                return NotFound(new { success = false, message = "Concept không tồn tại." });
            }

            // Use concept image from chatbot if provided, otherwise use concept name
            var conceptImage = !string.IsNullOrEmpty(conceptImageUrl) ? conceptImageUrl : "";
            var conceptName = !string.IsNullOrEmpty(concept.Name) ? concept.Name : "Concept đặc biệt";

            // Build email content
            var styles = @"<!DOCTYPE html><html><head><meta charset='utf-8'><style>
			body{font-family:Arial,sans-serif;color:#333} .wrapper{max-width:640px;margin:0 auto;background:#fff;border:1px solid #eee;border-radius:12px;overflow:hidden}
			.header{background:#7f1d1d;color:#fff;padding:24px;text-align:center} .content{padding:24px} .row{margin-bottom:12px}
			.label{color:#7f1d1d;font-weight:bold} .card{background:#faf6f5;border:1px solid #f0e8e7;border-radius:8px;padding:16px}
			.footer{background:#fafafa;color:#666;font-size:12px;padding:16px;text-align:center}
			.concept-image{max-width:100%;border-radius:8px;margin:12px 0}
			</style></head><body><div class='wrapper'>";

            var details = new StringBuilder();
            details.Append("<div class='card'>");
            details.Append($"<h3 style='color:#7f1d1d;margin-top:0;margin-bottom:16px'>Concept đã chọn: {conceptName}</h3>");
            if (!string.IsNullOrEmpty(conceptImage))
            {
                details.Append($"<img src='{conceptImage}' alt='{conceptName}' class='concept-image' />");
            }
            details.Append($"<div class='row'><span class='label'>Giá:</span> {String.Format("{0:N0}₫", concept.Price ?? 0)}</div>");
            details.Append($"<div class='row'><span class='label'>Họ tên:</span> {fullName}</div>");
            details.Append($"<div class='row'><span class='label'>Email:</span> {email}</div>");
            details.Append($"<div class='row'><span class='label'>SĐT:</span> {phone}</div>");
            details.Append($"<div class='row'><span class='label'>Nơi sống hiện tại:</span> {currentLocation ?? "-"}</div>");
            details.Append($"<div class='row'><span class='label'>Mục đích buổi hẹn hò:</span> {datePurpose ?? "-"}</div>");
            details.Append($"<div class='row'><span class='label'>Ngân sách mong muốn:</span> {budget ?? "-"}</div>");
            details.Append($"<div class='row'><span class='label'>Ngày hẹn hò:</span> {eventDate ?? "-"}</div>");
            details.Append($"<div class='row'><span class='label'>Khoảng thời gian:</span> {eventStartTime ?? "-"} đến {eventEndTime ?? "-"}</div>");
            details.Append($"<div class='row'><span class='label'>Không gian mong muốn:</span> {spacePreference ?? "-"}</div>");
            details.Append($"<div class='row'><span class='label'>Kênh biết đến:</span> {referral ?? "-"}</div>");
            details.Append($"<div class='row'><span class='label'>Điều khách hàng muốn MonAmour lưu ý:</span><br>{message}</div>");
            details.Append("</div>");

            var footer = "</div><div class='footer'>© MonAmour</div></div></body></html>";

            // Generate QR code for payment using VietQR API (exactly like Cart/Index.cshtml)
            var apiBase = _config["VietQR:ApiBase"] ?? "https://api.vietqr.io";
            var clientId = _config["VietQR:ClientId"];
            var apiKey = _config["VietQR:ApiKey"];
            var acqId = _config["VietQR:AcqId"];
            var accountNo = _config["VietQR:AccountNo"];
            var accountName = _config["VietQR:AccountName"];
            var template = _config["VietQR:Template"] ?? "compact";

            string qrCodeUrl = "";
            string qrCodeBase64 = "";

            // Try verified API first (exactly like GenerateVietQrVerified in Gift_boxController)
            if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(apiKey) && 
                clientId != "YOUR_CLIENT_ID" && apiKey != "YOUR_API_KEY" &&
                !string.IsNullOrEmpty(acqId) && !string.IsNullOrEmpty(accountNo))
            {
                try
                {
                    var vndAmount = Math.Max(0, (long)Math.Round((concept.Price ?? 0), 0, MidpointRounding.AwayFromZero));
                    var description = $"CONCEPT_{conceptId}_{fullName}";

                    var payload = new
                    {
                        accountNo = accountNo,
                        accountName = accountName,
                        acqId = acqId,
                        addInfo = description,
                        amount = vndAmount,
                        template = template
                    };

                    using var http = new HttpClient();
                    http.BaseAddress = new Uri(apiBase);
                    http.DefaultRequestHeaders.Accept.Clear();
                    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    http.DefaultRequestHeaders.Add("x-client-id", clientId);
                    http.DefaultRequestHeaders.Add("x-api-key", apiKey);

                    var json = JsonSerializer.Serialize(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var resp = await http.PostAsync("/v2/generate", content);
                    var body = await resp.Content.ReadAsStringAsync();

                    if (resp.IsSuccessStatusCode)
                    {
                        using var doc = JsonDocument.Parse(body);
                        var root = doc.RootElement;
                        var code = root.TryGetProperty("code", out var codeEl) ? codeEl.GetString() : null;
                        
                        if (string.Equals(code, "00", StringComparison.OrdinalIgnoreCase))
                        {
                            if (root.TryGetProperty("data", out var dataEl))
                            {
                                // Try to get qrDataURL first (this is what Cart/Index.cshtml uses)
                                if (dataEl.TryGetProperty("qrDataURL", out var qrDataURLEl))
                                {
                                    qrCodeBase64 = qrDataURLEl.GetString() ?? "";
                                }
                                // Fallback to qrCode
                                else if (dataEl.TryGetProperty("qrCode", out var qrCodeEl))
                                {
                                    var qrCode = qrCodeEl.GetString() ?? "";
                                    if (!string.IsNullOrEmpty(qrCode) && !qrCode.StartsWith("data:image"))
                                    {
                                        qrCodeBase64 = $"data:image/png;base64,{qrCode}";
                                    }
                                    else
                                    {
                                        qrCodeBase64 = qrCode;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"VietQR Verified API Error: {ex.Message}");
                }
            }

            // Fallback to URL method if verified API fails (like Cart/Index.cshtml fallback)
            if (string.IsNullOrEmpty(qrCodeBase64))
            {
                try
                {
                    var baseUrl = _config["VietQR:ApiBase"] ?? "https://img.vietqr.io";
                    var bankCode = _config["VietQR:BankCode"] ?? "MB";
                    var vndAmount = Math.Max(0, (long)Math.Round((concept.Price ?? 0), 0, MidpointRounding.AwayFromZero));
                    var description = $"CONCEPT_{conceptId}_{fullName}";
                    var encodedInfo = Uri.EscapeDataString(description);
                    var encodedName = Uri.EscapeDataString(accountName ?? "");
                    
                    qrCodeUrl = $"{baseUrl}/{bankCode}/{accountNo}/{vndAmount}.png?addInfo={encodedInfo}&accountName={encodedName}&template={template}";
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"VietQR URL Fallback Error: {ex.Message}");
                }
            }

            // Customer email
            var customerHtml = new StringBuilder();
            customerHtml.Append(styles);
            customerHtml.Append("<div class='header'><h2>MonAmour - Xác nhận đặt concept</h2></div>");
            customerHtml.Append("<div class='content'>");
            
            // Add MonMon AI logo/branding
            customerHtml.Append(@"
                <div style='text-align:center;margin-bottom:24px;padding:16px;background:#7f1d1d;border-radius:12px;'>
                    <div style='display:inline-flex;align-items:center;gap:12px;'>
                        <div style='width:40px;height:40px;background:#fbf1e6;border-radius:50%;display:flex;align-items:center;justify-content:center;'>
                            <svg xmlns='http://www.w3.org/2000/svg' width='24' height='24' viewBox='0 0 1024 1024' style='fill:#7f1d1d;'>
                                <path d='M738.3 287.6H285.7c-59 0-106.8 47.8-106.8 106.8v303.1c0 59 47.8 106.8 106.8 106.8h81.5v111.1c0 .7.8 1.1 1.4.7l166.9-110.6 41.8-.8h117.4l43.6-.4c59 0 106.8-47.8 106.8-106.8V394.5c0-59-47.8-106.9-106.8-106.9zM351.7 448.2c0-29.5 23.9-53.5 53.5-53.5s53.5 23.9 53.5 53.5-23.9 53.5-53.5 53.5-53.5-23.9-53.5-53.5zm157.9 267.1c-67.8 0-123.8-47.5-132.3-109h264.6c-8.6 61.5-64.5 109-132.3 109zm110-213.7c-29.5 0-53.5-23.9-53.5-53.5s23.9-53.5 53.5-53.5 53.5 23.9 53.5 53.5-23.9 53.5-53.5 53.5zM867.2 644.5V453.1h26.5c19.4 0 35.1 15.7 35.1 35.1v121.1c0 19.4-15.7 35.1-35.1 35.1h-26.5zM95.2 609.4V488.2c0-19.4 15.7-35.1 35.1-35.1h26.5v191.3h-26.5c-19.4 0-35.1-15.7-35.1-35.1zM561.5 149.6c0 23.4-15.6 43.3-36.9 49.7v44.9h-30v-44.9c-21.4-6.5-36.9-26.3-36.9-49.7 0-28.6 23.3-51.9 51.9-51.9s51.9 23.3 51.9 51.9z' />
                            </svg>
                        </div>
                        <h3 style='color:#fbf1e6;font-family:Prata,serif;font-size:1.5rem;margin:0;font-weight:600;'>MonMon AI</h3>
                    </div>
                </div>
            ");
            
            customerHtml.Append($"<p>Chào {fullName},</p>");
            customerHtml.Append($"<p>Mon Amour rất hạnh phúc khi bạn đã chọn concept <strong>{conceptName}</strong>!</p>");
            customerHtml.Append("<p>Yêu cầu đặt concept của bạn đã được ghi nhận qua <strong>MonMon AI</strong> - trợ lý tư vấn thông minh của Mon Amour.</p>");
            customerHtml.Append("<p>Vui lòng quét mã QR bên dưới để thanh toán:</p>");
            
            // Embed QR code image in email using ContentId (cid:qrcode)
            // This will use the attached image instead of base64/URL
            if (!string.IsNullOrEmpty(qrCodeBase64))
            {
                customerHtml.Append($"<div style='text-align:center;margin:20px 0;'><img src='cid:qrcode' alt='QR Code' style='max-width:300px;border:1px solid #ddd;padding:10px;background:#fff;display:block;margin:0 auto;' /></div>");
            }
            else if (!string.IsNullOrEmpty(qrCodeUrl))
            {
                // If only URL available, use URL (should not happen if API works correctly)
                customerHtml.Append($"<div style='text-align:center;margin:20px 0;'><img src='{qrCodeUrl}' alt='QR Code' style='max-width:300px;border:1px solid #ddd;padding:10px;background:#fff;display:block;margin:0 auto;' /></div>");
            }
            else
            {
                customerHtml.Append($"<div style='text-align:center;margin:20px 0;color:#e74c3c;'><p>Không thể tạo mã QR. Vui lòng liên hệ hotline: 0868019255</p></div>");
            }
            
            customerHtml.Append("<p>Sau khi thanh toán, chúng mình sẽ liên hệ với bạn để xác nhận và lên kế hoạch chi tiết.</p>");
            customerHtml.Append(details.ToString());
            customerHtml.Append(footer);

            var customerHtmlStr = customerHtml.ToString();
            
            // Use new method with QR code attachment if available
            if (!string.IsNullOrEmpty(qrCodeBase64))
            {
                await _emailService.SendContactConfirmationEmailWithQrAsync(email, fullName, customerHtmlStr, qrCodeBase64);
            }
            else
            {
                await _emailService.SendContactConfirmationEmailAsync(email, fullName, customerHtmlStr);
            }

            // Admin email
            var adminHtmlBuilder = new StringBuilder();
            adminHtmlBuilder.Append(styles);
            adminHtmlBuilder.Append("<div class='header'><h2>MonAmour - Yêu cầu đặt concept mới từ chatbot</h2></div>");
            adminHtmlBuilder.Append("<div class='content'>");
            adminHtmlBuilder.Append("<p>Bạn nhận được một yêu cầu đặt concept mới từ chatbot. Vui lòng kiểm tra thông tin chi tiết bên dưới và chủ động liên hệ khách hàng để xác nhận.</p>");
            adminHtmlBuilder.Append(details.ToString());
            adminHtmlBuilder.Append("<p style='margin-top:12px;color:#7f1d1d'><strong>Lưu ý:</strong> Không chia sẻ thông tin khách hàng ra ngoài hệ thống.</p>");
            adminHtmlBuilder.Append(footer);
            var adminHtml = adminHtmlBuilder.ToString();

            var adminEmails = await _db.UserRoles
                .AsNoTracking()
                .Include(ur => ur.User)
                .Where(ur => ur.RoleId == 1 && ur.User.Email != null)
                .Select(ur => ur.User.Email!)
                .Distinct()
                .ToListAsync();

            foreach (var adminEmail in adminEmails)
            {
                await _emailService.SendAdminPaymentIssueReportAsync(adminEmail, "Yêu cầu đặt concept mới từ chatbot", adminHtml);
            }

            // Return QR code in the same format as Cart/Index.cshtml expects
            // Prefer qrDataURL (base64) over URL
            return Ok(new { 
                success = true, 
                qrCodeUrl = qrCodeUrl, 
                qrCodeBase64 = qrCodeBase64,
                // Also return in format that Cart/Index.cshtml uses (for compatibility)
                response = !string.IsNullOrEmpty(qrCodeBase64) ? new { 
                    code = "00", 
                    desc = "Success", 
                    data = new { 
                        qrDataURL = qrCodeBase64,
                        qrCode = qrCodeBase64
                    } 
                } : null
            });
        }
    }
}
