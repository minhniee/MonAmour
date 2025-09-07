using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonAmour.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MonAmour.Controllers
{
	public class ConceptController : Controller
	{
		private readonly MonAmourDbContext _db;

		public ConceptController(MonAmourDbContext db)
		{
			_db = db;
		}

		public async Task<IActionResult> ListConcept(int? categoryId, int page = 1)
		{
			const int pageSize = 8;
			if (page < 1) page = 1;

			var categories = await _db.ConceptCategories
				.OrderBy(c => c.Name)
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

			return View(items);
		}

		public async Task<IActionResult> ConceptDetail(int id)
		{
			var concept = await _db.Concepts
				.Include(c => c.ConceptImgs)
				.Include(c => c.Category)
				.Include(c => c.Color)
				.Include(c => c.Ambience)
				.Include(c => c.Location)
				.FirstOrDefaultAsync(c => c.ConceptId == id);

			if (concept == null)
			{
				return NotFound();
			}

			// Lấy các concept liên quan (cùng category)
			var relatedConcepts = await _db.Concepts
				.Include(c => c.ConceptImgs)
				.Include(c => c.Category)
				.Where(c => c.CategoryId == concept.CategoryId && c.ConceptId != id && c.AvailabilityStatus == true)
				.Take(4)
				.ToListAsync();

			ViewBag.RelatedConcepts = relatedConcepts;

			return View(concept);
		}
	}
}
