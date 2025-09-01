using Microsoft.EntityFrameworkCore;
using MonAmourDb_BE.DTOs;
using MonAmourDb_BE.Models;
using MonAmourDb_BE.Service.Interfaces;

namespace MonAmourDb_BE.Service.Implementations
{
    public class ConceptManagementService : IConceptManagementService
    {
        private readonly MonAmourDbContext _context;

        public ConceptManagementService(MonAmourDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<ConceptListDto>> GetConceptsAsync(ConceptFilterDto filter)
        {
            var query = _context.Concepts
                .Include(c => c.Location)
                .Include(c => c.Category)
                .Include(c => c.Color)
                .Include(c => c.Ambience)
                .Include(c => c.ConceptImgs)
                .Include(c => c.Bookings)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(c => c.Name!.Contains(filter.SearchTerm) ||
                                       c.Description!.Contains(filter.SearchTerm));
            }

            if (filter.LocationId.HasValue)
                query = query.Where(c => c.LocationId == filter.LocationId);

            if (filter.CategoryId.HasValue)
                query = query.Where(c => c.CategoryId == filter.CategoryId);

            if (filter.ColorId.HasValue)
                query = query.Where(c => c.ColorId == filter.ColorId);

            if (filter.AmbienceId.HasValue)
                query = query.Where(c => c.AmbienceId == filter.AmbienceId);

            if (filter.AvailabilityStatus.HasValue)
                query = query.Where(c => c.AvailabilityStatus == filter.AvailabilityStatus);

            if (filter.MinPrice.HasValue)
                query = query.Where(c => c.Price >= filter.MinPrice);

            if (filter.MaxPrice.HasValue)
                query = query.Where(c => c.Price <= filter.MaxPrice);

            if (filter.CreatedFrom.HasValue)
                query = query.Where(c => c.CreatedAt >= filter.CreatedFrom);

            if (filter.CreatedTo.HasValue)
                query = query.Where(c => c.CreatedAt <= filter.CreatedTo);

            // Apply sorting
            query = filter.SortBy.ToLower() switch
            {
                "name" => filter.SortOrder.ToLower() == "asc" ?
                    query.OrderBy(c => c.Name) : query.OrderByDescending(c => c.Name),
                "price" => filter.SortOrder.ToLower() == "asc" ?
                    query.OrderBy(c => c.Price) : query.OrderByDescending(c => c.Price),
                "createdat" => filter.SortOrder.ToLower() == "asc" ?
                    query.OrderBy(c => c.CreatedAt) : query.OrderByDescending(c => c.CreatedAt),
                _ => query.OrderByDescending(c => c.CreatedAt)
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

            var concepts = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(c => new ConceptListDto
                {
                    ConceptId = c.ConceptId,
                    Name = c.Name,
                    Description = c.Description,
                    Price = c.Price,
                    PreparationTime = c.PreparationTime,
                    AvailabilityStatus = c.AvailabilityStatus,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    LocationName = c.Location!.Name,
                    CategoryName = c.Category!.Name,
                    ColorName = c.Color!.Name,
                    AmbienceName = c.Ambience!.Name,
                    PrimaryImageUrl = c.ConceptImgs
                        .Where(img => img.IsPrimary == true)
                        .Select(img => img.ImgUrl)
                        .FirstOrDefault(),
                    TotalImages = c.ConceptImgs.Count,
                    TotalBookings = c.Bookings.Count
                })
                .ToListAsync();

            return new PaginatedResult<ConceptListDto>
            {
                Data = concepts,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = totalPages,
                HasNextPage = filter.Page < totalPages,
                HasPreviousPage = filter.Page > 1
            };
        }

        public async Task<ConceptDetailDto?> GetConceptByIdAsync(int conceptId)
        {
            var concept = await _context.Concepts
                .Include(c => c.Location)
                .Include(c => c.Category)
                .Include(c => c.Color)
                .Include(c => c.Ambience)
                .Include(c => c.ConceptImgs)
                .Include(c => c.Bookings)
                    .ThenInclude(b => b.User)
                .FirstOrDefaultAsync(c => c.ConceptId == conceptId);

            if (concept == null) return null;

            return new ConceptDetailDto
            {
                ConceptId = concept.ConceptId,
                Name = concept.Name,
                Description = concept.Description,
                Price = concept.Price,
                LocationId = concept.LocationId,
                ColorId = concept.ColorId,
                CategoryId = concept.CategoryId,
                AmbienceId = concept.AmbienceId,
                PreparationTime = concept.PreparationTime,
                AvailabilityStatus = concept.AvailabilityStatus,
                CreatedAt = concept.CreatedAt,
                UpdatedAt = concept.UpdatedAt,
                LocationName = concept.Location?.Name,
                CategoryName = concept.Category?.Name,
                ColorName = concept.Color?.Name,
                AmbienceName = concept.Ambience?.Name,
                Location = concept.Location != null ? new LocationBasicDto
                {
                    LocationId = concept.Location.LocationId,
                    Name = concept.Location.Name,
                    Address = concept.Location.Address,
                    District = concept.Location.District,
                    City = concept.Location.City,
                    Status = concept.Location.Status
                } : null,
                Category = concept.Category != null ? new ConceptCategoryDto
                {
                    CategoryId = concept.Category.CategoryId,
                    Name = concept.Category.Name,
                    Description = concept.Category.Description,
                    IsActive = concept.Category.IsActive
                } : null,
                Color = concept.Color != null ? new ConceptColorDto
                {
                    ColorId = concept.Color.ColorId,
                    Name = concept.Color.Name,
                    Code = concept.Color.Code
                } : null,
                Ambience = concept.Ambience != null ? new ConceptAmbienceDto
                {
                    AmbienceId = concept.Ambience.AmbienceId,
                    Name = concept.Ambience.Name
                } : null,
                Images = concept.ConceptImgs
                    .OrderBy(img => img.DisplayOrder)
                    .Select(img => new ConceptImageDto
                    {
                        ImgId = img.ImgId,
                        ConceptId = img.ConceptId,
                        ImgUrl = img.ImgUrl,
                        ImgName = img.ImgName,
                        AltText = img.AltText,
                        IsPrimary = img.IsPrimary,
                        DisplayOrder = img.DisplayOrder,
                        CreatedAt = img.CreatedAt
                    }).ToList(),
                RecentBookings = concept.Bookings
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(5)
                    .Select(b => new BookingBasicDto
                    {
                        BookingId = b.BookingId,
                        BookingDate = b.BookingDate,
                        BookingTime = b.BookingTime,
                        Status = b.Status,
                        TotalPrice = b.TotalPrice,
                        UserName = b.User?.Name,
                        UserEmail = b.User?.Email
                    }).ToList(),
                PrimaryImageUrl = concept.ConceptImgs
                    .Where(img => img.IsPrimary == true)
                    .Select(img => img.ImgUrl)
                    .FirstOrDefault(),
                TotalImages = concept.ConceptImgs.Count,
                TotalBookings = concept.Bookings.Count
            };
        }

        public async Task<ConceptDetailDto> CreateConceptAsync(CreateConceptDto createConceptDto)
        {
            // Validate location exists
            var locationExists = await _context.Locations
                .AnyAsync(l => l.LocationId == createConceptDto.LocationId);
            if (!locationExists)
                throw new ArgumentException("Vị trí không tồn tại");

            var concept = new Concept
            {
                Name = createConceptDto.Name,
                Description = createConceptDto.Description,
                Price = createConceptDto.Price,
                LocationId = createConceptDto.LocationId,
                ColorId = createConceptDto.ColorId,
                CategoryId = createConceptDto.CategoryId,
                AmbienceId = createConceptDto.AmbienceId,
                PreparationTime = createConceptDto.PreparationTime,
                AvailabilityStatus = createConceptDto.AvailabilityStatus,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Concepts.Add(concept);
            await _context.SaveChangesAsync();

            return (await GetConceptByIdAsync(concept.ConceptId))!;
        }

        public async Task<ConceptDetailDto?> UpdateConceptAsync(int conceptId, UpdateConceptDto updateConceptDto)
        {
            var concept = await _context.Concepts.FindAsync(conceptId);
            if (concept == null) return null;

            // Update only provided fields
            if (!string.IsNullOrEmpty(updateConceptDto.Name))
                concept.Name = updateConceptDto.Name;

            if (updateConceptDto.Description != null)
                concept.Description = updateConceptDto.Description;

            if (updateConceptDto.Price.HasValue)
                concept.Price = updateConceptDto.Price;

            if (updateConceptDto.LocationId.HasValue)
            {
                var locationExists = await _context.Locations
                    .AnyAsync(l => l.LocationId == updateConceptDto.LocationId);
                if (!locationExists)
                    throw new ArgumentException("Vị trí không tồn tại");
                concept.LocationId = updateConceptDto.LocationId;
            }

            if (updateConceptDto.ColorId.HasValue)
                concept.ColorId = updateConceptDto.ColorId;

            if (updateConceptDto.CategoryId.HasValue)
                concept.CategoryId = updateConceptDto.CategoryId;

            if (updateConceptDto.AmbienceId.HasValue)
                concept.AmbienceId = updateConceptDto.AmbienceId;

            if (updateConceptDto.PreparationTime.HasValue)
                concept.PreparationTime = updateConceptDto.PreparationTime;

            if (updateConceptDto.AvailabilityStatus.HasValue)
                concept.AvailabilityStatus = updateConceptDto.AvailabilityStatus;

            concept.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return await GetConceptByIdAsync(conceptId);
        }

        public async Task<bool> DeleteConceptAsync(int conceptId)
        {
            var concept = await _context.Concepts
                .Include(c => c.Bookings)
                .Include(c => c.ConceptImgs)
                .FirstOrDefaultAsync(c => c.ConceptId == conceptId);

            if (concept == null) return false;

            // Check if concept has bookings
            if (concept.Bookings.Any())
                throw new InvalidOperationException("Không thể xóa concept đã có booking");

            // Remove images first
            _context.ConceptImgs.RemoveRange(concept.ConceptImgs);

            // Remove concept
            _context.Concepts.Remove(concept);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleConceptAvailabilityAsync(int conceptId)
        {
            var concept = await _context.Concepts.FindAsync(conceptId);
            if (concept == null) return false;

            concept.AvailabilityStatus = !concept.AvailabilityStatus;
            concept.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        // Image Management Methods
        public async Task<List<ConceptImageDto>> GetConceptImagesAsync(int conceptId)
        {
            return await _context.ConceptImgs
                .Where(img => img.ConceptId == conceptId)
                .OrderBy(img => img.DisplayOrder)
                .Select(img => new ConceptImageDto
                {
                    ImgId = img.ImgId,
                    ConceptId = img.ConceptId,
                    ImgUrl = img.ImgUrl,
                    ImgName = img.ImgName,
                    AltText = img.AltText,
                    IsPrimary = img.IsPrimary,
                    DisplayOrder = img.DisplayOrder,
                    CreatedAt = img.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<ConceptImageDto> AddConceptImageAsync(int conceptId, CreateConceptImageDto createImageDto)
        {
            // Verify concept exists
            var conceptExists = await _context.Concepts.AnyAsync(c => c.ConceptId == conceptId);
            if (!conceptExists)
                throw new ArgumentException("Concept không tồn tại");

            // If this is set as primary, unset other primary images
            if (createImageDto.IsPrimary)
            {
                await _context.ConceptImgs
                    .Where(img => img.ConceptId == conceptId && img.IsPrimary == true)
                    .ExecuteUpdateAsync(img => img.SetProperty(i => i.IsPrimary, false));
            }

            var conceptImg = new ConceptImg
            {
                ConceptId = conceptId,
                ImgUrl = createImageDto.ImgUrl,
                ImgName = createImageDto.ImgName,
                AltText = createImageDto.AltText,
                IsPrimary = createImageDto.IsPrimary,
                DisplayOrder = createImageDto.DisplayOrder,
                CreatedAt = DateTime.Now
            };

            _context.ConceptImgs.Add(conceptImg);
            await _context.SaveChangesAsync();

            return new ConceptImageDto
            {
                ImgId = conceptImg.ImgId,
                ConceptId = conceptImg.ConceptId,
                ImgUrl = conceptImg.ImgUrl,
                ImgName = conceptImg.ImgName,
                AltText = conceptImg.AltText,
                IsPrimary = conceptImg.IsPrimary,
                DisplayOrder = conceptImg.DisplayOrder,
                CreatedAt = conceptImg.CreatedAt
            };
        }

        public async Task<ConceptImageDto?> UpdateConceptImageAsync(int conceptId, int imageId, UpdateConceptImageDto updateImageDto)
        {
            var image = await _context.ConceptImgs
                .FirstOrDefaultAsync(img => img.ImgId == imageId && img.ConceptId == conceptId);

            if (image == null) return null;

            // If setting as primary, unset other primary images
            if (updateImageDto.IsPrimary == true)
            {
                await _context.ConceptImgs
                    .Where(img => img.ConceptId == conceptId && img.ImgId != imageId && img.IsPrimary == true)
                    .ExecuteUpdateAsync(img => img.SetProperty(i => i.IsPrimary, false));
            }

            // Update fields
            if (!string.IsNullOrEmpty(updateImageDto.ImgUrl))
                image.ImgUrl = updateImageDto.ImgUrl;

            if (updateImageDto.ImgName != null)
                image.ImgName = updateImageDto.ImgName;

            if (updateImageDto.AltText != null)
                image.AltText = updateImageDto.AltText;

            if (updateImageDto.IsPrimary.HasValue)
                image.IsPrimary = updateImageDto.IsPrimary;

            if (updateImageDto.DisplayOrder.HasValue)
                image.DisplayOrder = updateImageDto.DisplayOrder;

            await _context.SaveChangesAsync();

            return new ConceptImageDto
            {
                ImgId = image.ImgId,
                ConceptId = image.ConceptId,
                ImgUrl = image.ImgUrl,
                ImgName = image.ImgName,
                AltText = image.AltText,
                IsPrimary = image.IsPrimary,
                DisplayOrder = image.DisplayOrder,
                CreatedAt = image.CreatedAt
            };
        }

        public async Task<bool> DeleteConceptImageAsync(int conceptId, int imageId)
        {
            var image = await _context.ConceptImgs
                .FirstOrDefaultAsync(img => img.ImgId == imageId && img.ConceptId == conceptId);

            if (image == null) return false;

            _context.ConceptImgs.Remove(image);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetPrimaryImageAsync(int conceptId, int imageId)
        {
            var image = await _context.ConceptImgs
                .FirstOrDefaultAsync(img => img.ImgId == imageId && img.ConceptId == conceptId);

            if (image == null) return false;

            // Unset all primary images for this concept
            await _context.ConceptImgs
                .Where(img => img.ConceptId == conceptId && img.IsPrimary == true)
                .ExecuteUpdateAsync(img => img.SetProperty(i => i.IsPrimary, false));

            // Set this image as primary
            image.IsPrimary = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReorderImagesAsync(int conceptId, List<int> imageIds)
        {
            var images = await _context.ConceptImgs
                .Where(img => img.ConceptId == conceptId && imageIds.Contains(img.ImgId))
                .ToListAsync();

            if (images.Count != imageIds.Count) return false;

            // Update display order based on position in list
            for (int i = 0; i < imageIds.Count; i++)
            {
                var image = images.FirstOrDefault(img => img.ImgId == imageIds[i]);
                if (image != null)
                {
                    image.DisplayOrder = i + 1;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // Lookup Data Methods
        public async Task<List<ConceptCategoryDto>> GetConceptCategoriesAsync()
        {
            return await _context.ConceptCategories
                .Where(c => c.IsActive == true)
                .Select(c => new ConceptCategoryDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Description = c.Description,
                    IsActive = c.IsActive
                })
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<List<ConceptColorDto>> GetConceptColorsAsync()
        {
            return await _context.ConceptColors
                .Select(c => new ConceptColorDto
                {
                    ColorId = c.ColorId,
                    Name = c.Name,
                    Code = c.Code
                })
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<List<ConceptAmbienceDto>> GetConceptAmbiencesAsync()
        {
            return await _context.ConceptAmbiences
                .Select(a => new ConceptAmbienceDto
                {
                    AmbienceId = a.AmbienceId,
                    Name = a.Name
                })
                .OrderBy(a => a.Name)
                .ToListAsync();
        }

        public async Task<List<LocationBasicDto>> GetAvailableLocationsAsync()
        {
            return await _context.Locations
                .Where(l => l.Status == "active")
                .Select(l => new LocationBasicDto
                {
                    LocationId = l.LocationId,
                    Name = l.Name,
                    Address = l.Address,
                    District = l.District,
                    City = l.City,
                    Status = l.Status
                })
                .OrderBy(l => l.Name)
                .ToListAsync();
        }

        // Statistics Methods
        public async Task<ConceptStatsDto> GetConceptStatsAsync()
        {
            var totalConcepts = await _context.Concepts.CountAsync();
            var availableConcepts = await _context.Concepts.CountAsync(c => c.AvailabilityStatus == true);
            var unavailableConcepts = totalConcepts - availableConcepts;

            var thisMonth = DateTime.Now.Date.AddDays(-DateTime.Now.Day + 1);
            var newConceptsThisMonth = await _context.Concepts
                .CountAsync(c => c.CreatedAt >= thisMonth);

            var averagePrice = await _context.Concepts
                .Where(c => c.Price.HasValue)
                .AverageAsync(c => c.Price) ?? 0;

            var totalRevenue = await _context.Bookings
                .Where(b => b.Status == "completed")
                .SumAsync(b => b.TotalPrice) ?? 0;

            var categoryStats = await _context.ConceptCategories
                .Select(cat => new ConceptCategoryStatsDto
                {
                    CategoryName = cat.Name ?? "Unknown",
                    ConceptCount = cat.Concepts.Count,
                    BookingCount = cat.Concepts.SelectMany(c => c.Bookings).Count(),
                    Revenue = cat.Concepts
                        .SelectMany(c => c.Bookings)
                        .Where(b => b.Status == "completed")
                        .Sum(b => b.TotalPrice) ?? 0
                })
                .ToListAsync();

            var popularConcepts = await GetPopularConceptsAsync(5);

            return new ConceptStatsDto
            {
                TotalConcepts = totalConcepts,
                AvailableConcepts = availableConcepts,
                UnavailableConcepts = unavailableConcepts,
                NewConceptsThisMonth = newConceptsThisMonth,
                AveragePrice = averagePrice,
                TotalRevenue = totalRevenue,
                CategoryStats = categoryStats,
                PopularConcepts = popularConcepts
            };
        }

        public async Task<List<ConceptPopularityDto>> GetPopularConceptsAsync(int limit = 10)
        {
            return await _context.Concepts
                .Include(c => c.Bookings)
                .Include(c => c.ConceptImgs)
                .Select(c => new ConceptPopularityDto
                {
                    ConceptId = c.ConceptId,
                    ConceptName = c.Name ?? "Unknown",
                    BookingCount = c.Bookings.Count,
                    Revenue = c.Bookings
                        .Where(b => b.Status == "completed")
                        .Sum(b => b.TotalPrice) ?? 0,
                    PrimaryImageUrl = c.ConceptImgs
                        .Where(img => img.IsPrimary == true)
                        .Select(img => img.ImgUrl)
                        .FirstOrDefault()
                })
                .OrderByDescending(c => c.BookingCount)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<ConceptListDto>> GetRecommendedConceptsAsync(int userId, int limit = 5)
        {
            // Simple recommendation based on user's booking history
            var userBookedCategories = await _context.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.Concept)
                .ThenInclude(c => c!.Category)
                .Select(b => b.Concept!.CategoryId)
                .Distinct()
                .ToListAsync();

            var query = _context.Concepts
                .Include(c => c.Location)
                .Include(c => c.Category)
                .Include(c => c.Color)
                .Include(c => c.Ambience)
                .Include(c => c.ConceptImgs)
                .Include(c => c.Bookings)
                .Where(c => c.AvailabilityStatus == true);

            if (userBookedCategories.Any())
            {
                query = query.Where(c => userBookedCategories.Contains(c.CategoryId));
            }

            return await query
                .OrderByDescending(c => c.Bookings.Count)
                .Take(limit)
                .Select(c => new ConceptListDto
                {
                    ConceptId = c.ConceptId,
                    Name = c.Name,
                    Description = c.Description,
                    Price = c.Price,
                    PreparationTime = c.PreparationTime,
                    AvailabilityStatus = c.AvailabilityStatus,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    LocationName = c.Location!.Name,
                    CategoryName = c.Category!.Name,
                    ColorName = c.Color!.Name,
                    AmbienceName = c.Ambience!.Name,
                    PrimaryImageUrl = c.ConceptImgs
                        .Where(img => img.IsPrimary == true)
                        .Select(img => img.ImgUrl)
                        .FirstOrDefault(),
                    TotalImages = c.ConceptImgs.Count,
                    TotalBookings = c.Bookings.Count
                })
                .ToListAsync();
        }

        // Bulk Operations
        public async Task<bool> BulkUpdateAvailabilityAsync(List<int> conceptIds, bool availability)
        {
            await _context.Concepts
                .Where(c => conceptIds.Contains(c.ConceptId))
                .ExecuteUpdateAsync(c => c
                    .SetProperty(x => x.AvailabilityStatus, availability)
                    .SetProperty(x => x.UpdatedAt, DateTime.Now));

            return true;
        }

        public async Task<bool> BulkUpdateCategoryAsync(List<int> conceptIds, int categoryId)
        {
            // Verify category exists
            var categoryExists = await _context.ConceptCategories
                .AnyAsync(c => c.CategoryId == categoryId);
            if (!categoryExists)
                throw new ArgumentException("Category không tồn tại");

            await _context.Concepts
                .Where(c => conceptIds.Contains(c.ConceptId))
                .ExecuteUpdateAsync(c => c
                    .SetProperty(x => x.CategoryId, categoryId)
                    .SetProperty(x => x.UpdatedAt, DateTime.Now));

            return true;
        }

        public async Task<bool> BulkDeleteConceptsAsync(List<int> conceptIds)
        {
            // Check if any concepts have bookings
            var conceptsWithBookings = await _context.Concepts
                .Where(c => conceptIds.Contains(c.ConceptId))
                .Include(c => c.Bookings)
                .Where(c => c.Bookings.Any())
                .Select(c => c.Name)
                .ToListAsync();

            if (conceptsWithBookings.Any())
                throw new InvalidOperationException($"Không thể xóa các concept sau vì đã có booking: {string.Join(", ", conceptsWithBookings)}");

            // Delete images first
            await _context.ConceptImgs
                .Where(img => conceptIds.Contains(img.ConceptId ?? 0))
                .ExecuteDeleteAsync();

            // Delete concepts
            await _context.Concepts
                .Where(c => conceptIds.Contains(c.ConceptId))
                .ExecuteDeleteAsync();

            return true;
        }

        // ============ CONCEPT CATEGORY MANAGEMENT ============
        public async Task<PaginatedResult<ConceptCategoryDto>> GetConceptCategoriesPagedAsync(ConceptCategoryFilterDto filter)
        {
            var query = _context.ConceptCategories.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(c => c.Name!.Contains(filter.SearchTerm) ||
                                       c.Description!.Contains(filter.SearchTerm));
            }

            if (filter.IsActive.HasValue)
                query = query.Where(c => c.IsActive == filter.IsActive);

            // Apply sorting
            query = filter.SortBy.ToLower() switch
            {
                "name" => filter.SortOrder.ToLower() == "asc" ?
                    query.OrderBy(c => c.Name) : query.OrderByDescending(c => c.Name),
                "description" => filter.SortOrder.ToLower() == "asc" ?
                    query.OrderBy(c => c.Description) : query.OrderByDescending(c => c.Description),
                _ => query.OrderBy(c => c.Name)
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

            var categories = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(c => new ConceptCategoryDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Description = c.Description,
                    IsActive = c.IsActive
                })
                .ToListAsync();

            return new PaginatedResult<ConceptCategoryDto>
            {
                Data = categories,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = totalPages,
                HasNextPage = filter.Page < totalPages,
                HasPreviousPage = filter.Page > 1
            };
        }

        public async Task<ConceptCategoryDetailDto?> GetConceptCategoryByIdAsync(int categoryId)
        {
            var category = await _context.ConceptCategories
                .Include(c => c.Concepts)
                    .ThenInclude(concept => concept.Bookings)
                .Include(c => c.Concepts)
                    .ThenInclude(concept => concept.ConceptImgs)
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            if (category == null) return null;

            var totalRevenue = category.Concepts
                .SelectMany(c => c.Bookings)
                .Where(b => b.Status == "completed")
                .Sum(b => b.TotalPrice) ?? 0;

            return new ConceptCategoryDetailDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                ConceptCount = category.Concepts.Count,
                BookingCount = category.Concepts.SelectMany(c => c.Bookings).Count(),
                TotalRevenue = totalRevenue,
                RecentConcepts = category.Concepts
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(5)
                    .Select(c => new ConceptListDto
                    {
                        ConceptId = c.ConceptId,
                        Name = c.Name,
                        Description = c.Description,
                        Price = c.Price,
                        AvailabilityStatus = c.AvailabilityStatus,
                        CreatedAt = c.CreatedAt,
                        PrimaryImageUrl = c.ConceptImgs
                            .Where(img => img.IsPrimary == true)
                            .Select(img => img.ImgUrl)
                            .FirstOrDefault(),
                        TotalBookings = c.Bookings.Count
                    }).ToList()
            };
        }

        public async Task<ConceptCategoryDetailDto> CreateConceptCategoryAsync(CreateConceptCategoryDto createCategoryDto)
        {
            // Check if name already exists
            var existingCategory = await _context.ConceptCategories
                .AnyAsync(c => c.Name == createCategoryDto.Name);
            if (existingCategory)
                throw new ArgumentException("Tên category đã tồn tại");

            var category = new ConceptCategory
            {
                Name = createCategoryDto.Name,
                Description = createCategoryDto.Description,
                IsActive = createCategoryDto.IsActive
            };

            _context.ConceptCategories.Add(category);
            await _context.SaveChangesAsync();

            return (await GetConceptCategoryByIdAsync(category.CategoryId))!;
        }

        public async Task<ConceptCategoryDetailDto?> UpdateConceptCategoryAsync(int categoryId, UpdateConceptCategoryDto updateCategoryDto)
        {
            var category = await _context.ConceptCategories.FindAsync(categoryId);
            if (category == null) return null;

            // Check name uniqueness if updating name
            if (!string.IsNullOrEmpty(updateCategoryDto.Name) && updateCategoryDto.Name != category.Name)
            {
                var existingCategory = await _context.ConceptCategories
                    .AnyAsync(c => c.Name == updateCategoryDto.Name && c.CategoryId != categoryId);
                if (existingCategory)
                    throw new ArgumentException("Tên category đã tồn tại");
            }

            // Update fields
            if (!string.IsNullOrEmpty(updateCategoryDto.Name))
                category.Name = updateCategoryDto.Name;

            if (updateCategoryDto.Description != null)
                category.Description = updateCategoryDto.Description;

            if (updateCategoryDto.IsActive.HasValue)
                category.IsActive = updateCategoryDto.IsActive;

            await _context.SaveChangesAsync();
            return await GetConceptCategoryByIdAsync(categoryId);
        }

        public async Task<bool> DeleteConceptCategoryAsync(int categoryId)
        {
            var category = await _context.ConceptCategories
                .Include(c => c.Concepts)
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            if (category == null) return false;

            // Check if category has concepts
            if (category.Concepts.Any())
                throw new InvalidOperationException("Không thể xóa category đã có concepts");

            _context.ConceptCategories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleConceptCategoryStatusAsync(int categoryId)
        {
            var category = await _context.ConceptCategories.FindAsync(categoryId);
            if (category == null) return false;

            category.IsActive = !category.IsActive;
            await _context.SaveChangesAsync();
            return true;
        }

        // ============ CONCEPT COLOR MANAGEMENT ============
        public async Task<PaginatedResult<ConceptColorDto>> GetConceptColorsPagedAsync(ConceptColorFilterDto filter)
        {
            var query = _context.ConceptColors.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(c => c.Name!.Contains(filter.SearchTerm) ||
                                       c.Code!.Contains(filter.SearchTerm));
            }

            // Apply sorting
            query = filter.SortBy.ToLower() switch
            {
                "name" => filter.SortOrder.ToLower() == "asc" ?
                    query.OrderBy(c => c.Name) : query.OrderByDescending(c => c.Name),
                "code" => filter.SortOrder.ToLower() == "asc" ?
                    query.OrderBy(c => c.Code) : query.OrderByDescending(c => c.Code),
                _ => query.OrderBy(c => c.Name)
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

            var colors = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(c => new ConceptColorDto
                {
                    ColorId = c.ColorId,
                    Name = c.Name,
                    Code = c.Code
                })
                .ToListAsync();

            return new PaginatedResult<ConceptColorDto>
            {
                Data = colors,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = totalPages,
                HasNextPage = filter.Page < totalPages,
                HasPreviousPage = filter.Page > 1
            };
        }

        public async Task<ConceptColorDetailDto?> GetConceptColorByIdAsync(int colorId)
        {
            var color = await _context.ConceptColors
                .Include(c => c.Concepts)
                    .ThenInclude(concept => concept.Bookings)
                .Include(c => c.Concepts)
                    .ThenInclude(concept => concept.ConceptImgs)
                .FirstOrDefaultAsync(c => c.ColorId == colorId);

            if (color == null) return null;

            var totalRevenue = color.Concepts
                .SelectMany(c => c.Bookings)
                .Where(b => b.Status == "completed")
                .Sum(b => b.TotalPrice) ?? 0;

            return new ConceptColorDetailDto
            {
                ColorId = color.ColorId,
                Name = color.Name,
                Code = color.Code,
                ConceptCount = color.Concepts.Count,
                BookingCount = color.Concepts.SelectMany(c => c.Bookings).Count(),
                TotalRevenue = totalRevenue,
                RecentConcepts = color.Concepts
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(5)
                    .Select(c => new ConceptListDto
                    {
                        ConceptId = c.ConceptId,
                        Name = c.Name,
                        Description = c.Description,
                        Price = c.Price,
                        AvailabilityStatus = c.AvailabilityStatus,
                        CreatedAt = c.CreatedAt,
                        PrimaryImageUrl = c.ConceptImgs
                            .Where(img => img.IsPrimary == true)
                            .Select(img => img.ImgUrl)
                            .FirstOrDefault(),
                        TotalBookings = c.Bookings.Count
                    }).ToList()
            };
        }

        public async Task<ConceptColorDetailDto> CreateConceptColorAsync(CreateConceptColorDto createColorDto)
        {
            // Check if name or code already exists
            var existingColor = await _context.ConceptColors
                .AnyAsync(c => c.Name == createColorDto.Name || c.Code == createColorDto.Code);
            if (existingColor)
                throw new ArgumentException("Tên màu hoặc mã màu đã tồn tại");

            var color = new ConceptColor
            {
                Name = createColorDto.Name,
                Code = createColorDto.Code
            };

            _context.ConceptColors.Add(color);
            await _context.SaveChangesAsync();

            return (await GetConceptColorByIdAsync(color.ColorId))!;
        }

        public async Task<ConceptColorDetailDto?> UpdateConceptColorAsync(int colorId, UpdateConceptColorDto updateColorDto)
        {
            var color = await _context.ConceptColors.FindAsync(colorId);
            if (color == null) return null;

            // Check uniqueness if updating name or code
            if (!string.IsNullOrEmpty(updateColorDto.Name) && updateColorDto.Name != color.Name)
            {
                var existingName = await _context.ConceptColors
                    .AnyAsync(c => c.Name == updateColorDto.Name && c.ColorId != colorId);
                if (existingName)
                    throw new ArgumentException("Tên màu đã tồn tại");
            }

            if (!string.IsNullOrEmpty(updateColorDto.Code) && updateColorDto.Code != color.Code)
            {
                var existingCode = await _context.ConceptColors
                    .AnyAsync(c => c.Code == updateColorDto.Code && c.ColorId != colorId);
                if (existingCode)
                    throw new ArgumentException("Mã màu đã tồn tại");
            }

            // Update fields
            if (!string.IsNullOrEmpty(updateColorDto.Name))
                color.Name = updateColorDto.Name;

            if (!string.IsNullOrEmpty(updateColorDto.Code))
                color.Code = updateColorDto.Code;

            await _context.SaveChangesAsync();
            return await GetConceptColorByIdAsync(colorId);
        }

        public async Task<bool> DeleteConceptColorAsync(int colorId)
        {
            var color = await _context.ConceptColors
                .Include(c => c.Concepts)
                .FirstOrDefaultAsync(c => c.ColorId == colorId);

            if (color == null) return false;

            // Check if color has concepts
            if (color.Concepts.Any())
                throw new InvalidOperationException("Không thể xóa màu đã có concepts");

            _context.ConceptColors.Remove(color);
            await _context.SaveChangesAsync();
            return true;
        }

        // ============ CONCEPT AMBIENCE MANAGEMENT ============
        public async Task<PaginatedResult<ConceptAmbienceDto>> GetConceptAmbiencesPagedAsync(ConceptAmbienceFilterDto filter)
        {
            var query = _context.ConceptAmbiences.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(a => a.Name!.Contains(filter.SearchTerm));
            }

            // Apply sorting
            query = filter.SortBy.ToLower() switch
            {
                "name" => filter.SortOrder.ToLower() == "asc" ?
                    query.OrderBy(a => a.Name) : query.OrderByDescending(a => a.Name),
                _ => query.OrderBy(a => a.Name)
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

            var ambiences = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(a => new ConceptAmbienceDto
                {
                    AmbienceId = a.AmbienceId,
                    Name = a.Name
                })
                .ToListAsync();

            return new PaginatedResult<ConceptAmbienceDto>
            {
                Data = ambiences,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = totalPages,
                HasNextPage = filter.Page < totalPages,
                HasPreviousPage = filter.Page > 1
            };
        }

        public async Task<ConceptAmbienceDetailDto?> GetConceptAmbienceByIdAsync(int ambienceId)
        {
            var ambience = await _context.ConceptAmbiences
                .Include(a => a.Concepts)
                    .ThenInclude(concept => concept.Bookings)
                .Include(a => a.Concepts)
                    .ThenInclude(concept => concept.ConceptImgs)
                .FirstOrDefaultAsync(a => a.AmbienceId == ambienceId);

            if (ambience == null) return null;

            var totalRevenue = ambience.Concepts
                .SelectMany(c => c.Bookings)
                .Where(b => b.Status == "completed")
                .Sum(b => b.TotalPrice) ?? 0;

            return new ConceptAmbienceDetailDto
            {
                AmbienceId = ambience.AmbienceId,
                Name = ambience.Name,
                ConceptCount = ambience.Concepts.Count,
                BookingCount = ambience.Concepts.SelectMany(c => c.Bookings).Count(),
                TotalRevenue = totalRevenue,
                RecentConcepts = ambience.Concepts
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(5)
                    .Select(c => new ConceptListDto
                    {
                        ConceptId = c.ConceptId,
                        Name = c.Name,
                        Description = c.Description,
                        Price = c.Price,
                        AvailabilityStatus = c.AvailabilityStatus,
                        CreatedAt = c.CreatedAt,
                        PrimaryImageUrl = c.ConceptImgs
                            .Where(img => img.IsPrimary == true)
                            .Select(img => img.ImgUrl)
                            .FirstOrDefault(),
                        TotalBookings = c.Bookings.Count
                    }).ToList()
            };
        }

        public async Task<ConceptAmbienceDetailDto> CreateConceptAmbienceAsync(CreateConceptAmbienceDto createAmbienceDto)
        {
            // Check if name already exists
            var existingAmbience = await _context.ConceptAmbiences
                .AnyAsync(a => a.Name == createAmbienceDto.Name);
            if (existingAmbience)
                throw new ArgumentException("Tên ambience đã tồn tại");

            var ambience = new ConceptAmbience
            {
                Name = createAmbienceDto.Name
            };

            _context.ConceptAmbiences.Add(ambience);
            await _context.SaveChangesAsync();

            return (await GetConceptAmbienceByIdAsync(ambience.AmbienceId))!;
        }

        public async Task<ConceptAmbienceDetailDto?> UpdateConceptAmbienceAsync(int ambienceId, UpdateConceptAmbienceDto updateAmbienceDto)
        {
            var ambience = await _context.ConceptAmbiences.FindAsync(ambienceId);
            if (ambience == null) return null;

            // Check name uniqueness if updating name
            if (!string.IsNullOrEmpty(updateAmbienceDto.Name) && updateAmbienceDto.Name != ambience.Name)
            {
                var existingAmbience = await _context.ConceptAmbiences
                    .AnyAsync(a => a.Name == updateAmbienceDto.Name && a.AmbienceId != ambienceId);
                if (existingAmbience)
                    throw new ArgumentException("Tên ambience đã tồn tại");
            }

            // Update fields
            if (!string.IsNullOrEmpty(updateAmbienceDto.Name))
                ambience.Name = updateAmbienceDto.Name;

            await _context.SaveChangesAsync();
            return await GetConceptAmbienceByIdAsync(ambienceId);
        }

        public async Task<bool> DeleteConceptAmbienceAsync(int ambienceId)
        {
            var ambience = await _context.ConceptAmbiences
                .Include(a => a.Concepts)
                .FirstOrDefaultAsync(a => a.AmbienceId == ambienceId);

            if (ambience == null) return false;

            // Check if ambience has concepts
            if (ambience.Concepts.Any())
                throw new InvalidOperationException("Không thể xóa ambience đã có concepts");

            _context.ConceptAmbiences.Remove(ambience);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

