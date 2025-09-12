using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonAmour.Models;
using MonAmour.Services.Interfaces;
using System.Text;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MonAmour.Controllers
{
	public class ConceptController : Controller
	{
		private readonly MonAmourDbContext _db;
		private readonly IEmailService _emailService;

		public ConceptController(MonAmourDbContext db, IEmailService emailService)
		{
			_db = db;
			_emailService = emailService;
		}

		public async Task<IActionResult> ListConcept(int? categoryId, int page = 1, string? city = null, string? q = null)
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

			query = query.OrderByDescending(c => c.CreatedAt).ThenByDescending(c => c.ConceptId);

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
			string? location,
			string? eventType,
			string? budget,
			DateTime? eventDate,
			string? eventTime,
			string? message,
			string? referral)
		{
			// Basic server-side validation
			if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(phone))
			{
				return BadRequest("Vui lòng nhập đầy đủ Họ tên, Email và Số điện thoại.");
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
			details.Append($"<div class='row'><span class='label'>Nơi tổ chức:</span> {location}</div>");
			details.Append($"<div class='row'><span class='label'>Loại hình:</span> {eventType}</div>");
			details.Append($"<div class='row'><span class='label'>Ngân sách:</span> {budget}</div>");
			details.Append($"<div class='row'><span class='label'>Ngày tổ chức:</span> {(eventDate.HasValue ? eventDate.Value.ToString("dd/MM/yyyy") : "-")}</div>");
			details.Append($"<div class='row'><span class='label'>Thời gian:</span> {eventTime}</div>");
			details.Append($"<div class='row'><span class='label'>Kênh biết đến:</span> {referral}</div>");
			if (!string.IsNullOrWhiteSpace(message))
			{
				details.Append($"<div class='row'><span class='label'>Lời nhắn:</span><br>{message}</div>");
			}
			details.Append("</div>");

			var footer = "</div><div class='footer'>© MonAmour</div></div></body></html>";

			// Customer email
			var customerHtml = new StringBuilder();
			customerHtml.Append(styles);
			customerHtml.Append("<div class='header'><h2>MonAmour - Xác nhận yêu cầu tư vấn</h2></div>");
			customerHtml.Append("<div class='content'>");
			customerHtml.Append($"<p>Chào {fullName},</p><p>Chúng tôi đã nhận được yêu cầu tư vấn của bạn. Nhân viên sẽ liên hệ lại trong thời gian sớm nhất.</p>");
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
	}
}
