using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonAmour.Helpers;
using MonAmour.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MonAmour.Controllers.Zalopay
{
    [Route("Zalopay")]
    public class ZalopayController : Controller
    {
        private readonly MonAmourDbContext _db;
        private readonly IConfiguration _config;

        public ZalopayController(MonAmourDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        private string AppId => _config["ZaloPay:AppId"] ?? "";
        private string Key1 => _config["ZaloPay:Key1"] ?? "";
        private bool IsSandbox => string.Equals(_config["ZaloPay:Sandbox"], "true", StringComparison.OrdinalIgnoreCase);
        private string CreateOrderUrl => _config["ZaloPay:CreateOrderUrl"] ?? (IsSandbox ? "https://sb-openapi.zalopay.vn/v2/create" : "https://openapi.zalopay.vn/v2/create");

        private static string GetTimeStampMs() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        private static string GenerateAppTransId()
        {
            var now = DateTime.UtcNow;
            var yymmdd = now.ToString("yyMMdd");
            var rand = new Random().Next(100000, 999999);
            return $"{yymmdd}_{rand}";
        }

        private static string HmacSHA256(string key, string data)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
        }

        [HttpPost("CreateOrder")]
        public async Task<IActionResult> CreateOrder()
        {
            int? userId = AuthHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return Unauthorized(new { return_code = -1, return_message = "Unauthorized" });
            }

            var order = await _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == "cart");
            if (order == null || order.OrderItems == null || order.OrderItems.Count == 0)
            {
                return BadRequest(new { return_code = -1, return_message = "Giỏ hàng trống" });
            }

            order.TotalPrice = order.OrderItems
                .Select(i => (decimal?)i.TotalPrice)
                .Sum()
                .GetValueOrDefault(0m);
            await _db.SaveChangesAsync();

            var amount = (long)Math.Max(0, decimal.ToInt64(decimal.Truncate(order.TotalPrice)));
            if (amount <= 0)
            {
                return BadRequest(new { return_code = -1, return_message = "Số tiền không hợp lệ" });
            }

            var app_user = userId.Value.ToString();
            var app_time = GetTimeStampMs();
            var app_trans_id = GenerateAppTransId();

            var redirectUrl = Url.Action("Get", "Redirect", new { }, Request.Scheme);
            var embedDataObj = new { redirecturl = redirectUrl };
            var embed_data = JsonSerializer.Serialize(embedDataObj);

            var item = JsonSerializer.Serialize(new object[] { });
            var description = $"MonAmour - Thanh toan {app_trans_id}";

            var dataToSign = $"{AppId}|{app_trans_id}|{app_user}|{amount}|{app_time}|{embed_data}|{item}";
            var mac = HmacSHA256(Key1, dataToSign);

            var dict = new Dictionary<string, string>
            {
                { "app_id", AppId },
                { "app_trans_id", app_trans_id },
                { "app_user", app_user },
                { "amount", amount.ToString() },
                { "app_time", app_time },
                { "embed_data", embed_data },
                { "item", item },
                { "description", description },
                { "mac", mac }
            };

            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            HttpResponseMessage resp;
            try
            {
                resp = await http.PostAsync(CreateOrderUrl, new FormUrlEncodedContent(dict));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { return_code = -2, return_message = ex.Message });
            }

            var body = await resp.Content.ReadAsStringAsync();
            return Content(body, "application/json");
        }
    }
}
