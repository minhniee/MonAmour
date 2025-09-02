using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonAmour.Models;

namespace MonAmour.Controllers
{
    public class Gift_boxController : Controller
    {
        private readonly MonAmourDbContext _db;

        public Gift_boxController(MonAmourDbContext db)
        {
            _db = db;
        }
        /// <summary>
        /// Display the list of gift boxes with optional filters (category, price, sort)
        /// </summary>
        public IActionResult ListGiftbox(int? categoryId, string? price, string? sort)
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

            var products = query.ToList();

            // Provide data for dropdowns
            ViewBag.Categories = _db.ProductCategories
                .OrderBy(c => c.Name)
                .Select(c => new { c.CategoryId, c.Name })
                .ToList();
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SelectedPrice = price;
            ViewBag.SelectedSort = sort;

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
    }
}
