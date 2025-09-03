using Microsoft.AspNetCore.Mvc;
using ZaloPay.Helper.Crypto;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using MonAmour.Helpers;

namespace MonAmour.Controllers.Zalopay
{
	[Route("Zalopay/Redirect")]
	public class RedirectController : Controller
	{
		private readonly Models.MonAmourDbContext _db;
		private readonly IWebHostEnvironment _env;
		private readonly IConfiguration _config;
		
		public RedirectController(Models.MonAmourDbContext db, IWebHostEnvironment env, IConfiguration config)
		{
			_db = db;
			_env = env;
			_config = config;
		}

		private string Key2 => _config["ZaloPay:Key2"] ?? "";

		[HttpGet]
		public IActionResult Get()
		{
			var data = Request.Query;

			bool hasChecksum = data.ContainsKey("appid") && data.ContainsKey("apptransid") && data.ContainsKey("pmcid")
				&& data.ContainsKey("bankcode") && data.ContainsKey("amount") && data.ContainsKey("discountamount")
				&& data.ContainsKey("status") && data.ContainsKey("checksum");

			bool successStatus = (data.ContainsKey("status") && data["status"] == "1")
				|| (data.ContainsKey("return_code") && data["return_code"] == "1");

			bool success = successStatus;

			if (_env.IsProduction() && hasChecksum && successStatus)
			{
				var checksumData = data["appid"] + "|" + data["apptransid"] + "|" + data["pmcid"] + "|" +
					data["bankcode"] + "|" + data["amount"] + "|" + data["discountamount"] + "|" + data["status"];
				var computed = HmacHelper.Compute(ZaloPayHMAC.HMACSHA256, Key2, checksumData);
				var received = data["checksum"];
				success = successStatus && computed.Equals(received);
			}

			if (success)
			{
				decimal? amount = null;
				if (data.ContainsKey("amount") && decimal.TryParse(data["amount"], out var amt)) amount = amt;
				return RedirectToAction("FinalizeZp", "Cart", new { amount });
			}

			TempData["CartError"] = "Thanh toán thất bại";
			return RedirectToAction("Index", "Cart");
		}
	}
}
