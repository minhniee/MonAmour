using Microsoft.EntityFrameworkCore;
using MonAmour.Models;
using MonAmour.Services.Interfaces;
using MonAmour.ViewModels;

namespace MonAmour.Services.Implements
{
    public class ConceptService : IConceptService
    {
        private readonly MonAmourDbContext _context;
        private readonly ILogger<ConceptService> _logger;

        public ConceptService(MonAmourDbContext context, ILogger<ConceptService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(List<ConceptViewModel> concepts, int totalCount)> GetConceptsAsync(ConceptSearchViewModel searchModel)
        {
            try
            {
                var query = _context.Concepts
                    .Include(c => c.Location)
                    .Include(c => c.Category)
                    .Include(c => c.ConceptColorJunctions)
                        .ThenInclude(ccj => ccj.Color)
                    .Include(c => c.Ambience)
                    .Include(c => c.ConceptImgs)
                    .Include(c => c.Bookings)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(searchModel.SearchTerm))
                {
                    query = query.Where(c => c.Name!.Contains(searchModel.SearchTerm) ||
                                           c.Description!.Contains(searchModel.SearchTerm));
                }

                if (searchModel.LocationId.HasValue)
                {
                    query = query.Where(c => c.LocationId == searchModel.LocationId.Value);
                }

                if (searchModel.CategoryId.HasValue)
                {
                    query = query.Where(c => c.CategoryId == searchModel.CategoryId.Value);
                }

                if (searchModel.ColorIds != null && searchModel.ColorIds.Any())
                {
                    query = query.Where(c => c.ConceptColorJunctions.Any(ccj => searchModel.ColorIds.Contains(ccj.ColorId)));
                }

                if (searchModel.AmbienceId.HasValue)
                {
                    query = query.Where(c => c.AmbienceId == searchModel.AmbienceId.Value);
                }

                if (searchModel.AvailabilityStatus.HasValue)
                {
                    query = query.Where(c => c.AvailabilityStatus == searchModel.AvailabilityStatus.Value);
                }

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply sorting
                query = searchModel.SortBy?.ToLower() switch
                {
                    "name" => searchModel.SortOrder == "desc" ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
                    "price" => searchModel.SortOrder == "desc" ? query.OrderByDescending(c => c.Price) : query.OrderBy(c => c.Price),
                    "location" => searchModel.SortOrder == "desc" ? query.OrderByDescending(c => c.Location!.Name) : query.OrderBy(c => c.Location!.Name),
                    "category" => searchModel.SortOrder == "desc" ? query.OrderByDescending(c => c.Category!.Name) : query.OrderBy(c => c.Category!.Name),
                    "createdat" => searchModel.SortOrder == "desc" ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
                    _ => query.OrderBy(c => c.Name)
                };

                // Apply pagination
                var concepts = await query
                    .Skip((searchModel.Page - 1) * searchModel.PageSize)
                    .Take(searchModel.PageSize)
                    .Select(c => new ConceptViewModel
                    {
                        ConceptId = c.ConceptId,
                        Name = c.Name ?? string.Empty,
                        Description = c.Description,
                        Price = c.Price,
                        LocationId = c.LocationId,
                        ColorIds = c.ConceptColorJunctions.Select(ccj => ccj.Color.ColorId).ToList(),
                        CategoryId = c.CategoryId,
                        AmbienceId = c.AmbienceId,
                        PreparationTime = c.PreparationTime,
                        AvailabilityStatus = c.AvailabilityStatus ?? false,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        LocationName = c.Location != null ? c.Location.Name : null,
                        ColorNames = c.ConceptColorJunctions.Select(ccj => ccj.Color.Name ?? "").ToList(),
                        CategoryName = c.Category != null ? c.Category.Name : null,
                        AmbienceName = c.Ambience != null ? c.Ambience.Name : null,
                        ImageCount = c.ConceptImgs.Count,
                        BookingCount = c.Bookings.Count
                    })
                    .ToListAsync();

                return (concepts, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting concepts");
                throw;
            }
        }

        public async Task<ConceptDetailViewModel?> GetConceptByIdAsync(int id)
        {
            try
            {
                var concept = await _context.Concepts
                    .Include(c => c.Location)
                    .Include(c => c.Category)
                    .Include(c => c.ConceptColorJunctions)
                        .ThenInclude(ccj => ccj.Color)
                    .Include(c => c.Ambience)
                    .Include(c => c.ConceptImgs)
                    .Include(c => c.Bookings)
                    .FirstOrDefaultAsync(c => c.ConceptId == id);

                if (concept == null) return null;

                return new ConceptDetailViewModel
                {
                    ConceptId = concept.ConceptId,
                    Name = concept.Name ?? string.Empty,
                    Description = concept.Description,
                    Price = concept.Price,
                    LocationId = concept.LocationId,
                    ColorIds = concept.ConceptColorJunctions.Select(ccj => ccj.Color.ColorId).ToList(),
                    CategoryId = concept.CategoryId,
                    AmbienceId = concept.AmbienceId,
                    PreparationTime = concept.PreparationTime,
                    AvailabilityStatus = concept.AvailabilityStatus ?? false,
                    CreatedAt = concept.CreatedAt,
                    UpdatedAt = concept.UpdatedAt,
                    LocationName = concept.Location != null ? concept.Location.Name : null,
                    ColorNames = concept.ConceptColorJunctions.Select(ccj => ccj.Color.Name ?? "").ToList(),
                    CategoryName = concept.Category != null ? concept.Category.Name : null,
                    AmbienceName = concept.Ambience != null ? concept.Ambience.Name : null,
                    Images = concept.ConceptImgs.Select(img => new ConceptImgViewModel
                    {
                        ImgId = img.ImgId,
                        ConceptId = img.ConceptId ?? 0,
                        ImgUrl = img.ImgUrl,
                        ImgName = img.ImgName,
                        AltText = img.AltText,
                        IsPrimary = img.IsPrimary ?? false,
                        DisplayOrder = img.DisplayOrder ?? 0,
                        CreatedAt = img.CreatedAt
                    }).ToList(),
                    Bookings = concept.Bookings.Select(b => new BookingViewModel
                    {
                        BookingId = b.BookingId,
                        UserId = b.UserId ?? 0,
                        ConceptId = b.ConceptId ?? 0,
                        BookingDate = b.BookingDate ?? DateOnly.MinValue,
                        BookingTime = b.BookingTime ?? TimeOnly.MinValue,
                        TotalPrice = b.TotalPrice ?? 0,
                        Status = b.Status ?? string.Empty,
                        PaymentStatus = b.PaymentStatus ?? string.Empty,
                        CreatedAt = b.CreatedAt,
                        ConfirmedAt = b.ConfirmedAt,
                        CancelledAt = b.CancelledAt
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting concept by id: {Id}", id);
                throw;
            }
        }

        public async Task<bool> CreateConceptAsync(ConceptCreateViewModel model)
        {
            try
            {
                _logger.LogInformation("Creating concept: Name={Name}, Price={Price}, LocationId={LocationId}", 
                    model.Name, model.Price, model.LocationId);

                var concept = new Concept
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    LocationId = model.LocationId,
                    CategoryId = model.CategoryId,
                    AmbienceId = model.AmbienceId,
                    PreparationTime = model.PreparationTime,
                    AvailabilityStatus = model.AvailabilityStatus,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _logger.LogInformation("Concept entity created: ConceptId={ConceptId}, Name={Name}", 
                    concept.ConceptId, concept.Name);

                _context.Concepts.Add(concept);
                
                try
                {
                    var result = await _context.SaveChangesAsync();
                    
                    // Add color junctions after concept is saved
                    if (model.ColorIds != null && model.ColorIds.Any())
                    {
                        foreach (var colorId in model.ColorIds)
                        {
                            var colorJunction = new ConceptColorJunction
                            {
                                ConceptId = concept.ConceptId,
                                ColorId = colorId
                            };
                            _context.ConceptColorJunctions.Add(colorJunction);
                        }
                        await _context.SaveChangesAsync();
                    }
                    
                    _logger.LogInformation("Concept created successfully. Result: {Result}, ConceptId: {ConceptId}", result, concept.ConceptId);
                    return result > 0;
                }
                catch (Exception saveEx)
                {
                    _logger.LogError(saveEx, "Error saving concept to database. Concept data: Name={Name}, LocationId={LocationId}", 
                        concept.Name, concept.LocationId);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating concept");
                throw;
            }
        }

        public async Task<bool> UpdateConceptAsync(ConceptEditViewModel model)
        {
            try
            {
                _logger.LogInformation("Updating concept: ConceptId={ConceptId}, Name={Name}, Price={Price}", 
                    model.ConceptId, model.Name, model.Price);

                var concept = await _context.Concepts.FindAsync(model.ConceptId);
                if (concept == null)
                {
                    _logger.LogWarning("Concept not found: {ConceptId}", model.ConceptId);
                    return false;
                }

                concept.Name = model.Name;
                concept.Description = model.Description;
                concept.Price = model.Price;
                concept.LocationId = model.LocationId;
                concept.CategoryId = model.CategoryId;
                concept.AmbienceId = model.AmbienceId;
                concept.PreparationTime = model.PreparationTime;
                concept.AvailabilityStatus = model.AvailabilityStatus;
                concept.UpdatedAt = DateTime.Now;

                // Update color junctions
                if (model.ColorIds != null)
                {
                    // Remove existing color junctions
                    var existingJunctions = await _context.ConceptColorJunctions
                        .Where(ccj => ccj.ConceptId == concept.ConceptId)
                        .ToListAsync();
                    _context.ConceptColorJunctions.RemoveRange(existingJunctions);

                    // Add new color junctions
                    foreach (var colorId in model.ColorIds)
                    {
                        var colorJunction = new ConceptColorJunction
                        {
                            ConceptId = concept.ConceptId,
                            ColorId = colorId
                        };
                        _context.ConceptColorJunctions.Add(colorJunction);
                    }
                }

                var result = await _context.SaveChangesAsync();
                _logger.LogInformation("Concept updated successfully. Result: {Result}", result);
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating concept");
                throw;
            }
        }

        public async Task<bool> DeleteConceptAsync(int id)
        {
            try
            {
                var concept = await _context.Concepts.FindAsync(id);
                if (concept == null) return false;

                _context.Concepts.Remove(concept);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting concept");
                throw;
            }
        }

        public async Task<bool> ToggleConceptStatusAsync(int id, bool status)
        {
            try
            {
                var concept = await _context.Concepts.FindAsync(id);
                if (concept == null) return false;

                concept.AvailabilityStatus = status;
                concept.UpdatedAt = DateTime.Now;

                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling concept status");
                throw;
            }
        }

        public async Task<Dictionary<string, int>> GetConceptStatisticsAsync()
        {
            try
            {
                var totalConcepts = await _context.Concepts.CountAsync();
                var activeConcepts = await _context.Concepts.CountAsync(c => c.AvailabilityStatus == true);
                var inactiveConcepts = await _context.Concepts.CountAsync(c => c.AvailabilityStatus == false);
                var conceptsWithImages = await _context.Concepts.CountAsync(c => c.ConceptImgs.Any());
                var conceptsWithBookings = await _context.Concepts.CountAsync(c => c.Bookings.Any());

                return new Dictionary<string, int>
                {
                    ["TotalConcepts"] = totalConcepts,
                    ["ActiveConcepts"] = activeConcepts,
                    ["InactiveConcepts"] = inactiveConcepts,
                    ["ConceptsWithImages"] = conceptsWithImages,
                    ["ConceptsWithBookings"] = conceptsWithBookings
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting concept statistics");
                throw;
            }
        }

        public async Task<List<ConceptDropdownViewModel>> GetConceptsForDropdownAsync()
        {
            try
            {
                var concepts = await _context.Concepts
                    .Where(c => c.AvailabilityStatus == true)
                    .Select(c => new ConceptDropdownViewModel
                    {
                        ConceptId = c.ConceptId,
                        Name = c.Name ?? string.Empty
                    })
                    .ToListAsync();

                return concepts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting concepts for dropdown");
                throw;
            }
        }

        // Concept Image Management
        public async Task<List<ConceptImgViewModel>> GetConceptImagesAsync(int conceptId)
        {
            try
            {
                var images = await _context.ConceptImgs
                    .Where(img => img.ConceptId == conceptId)
                    .OrderBy(img => img.DisplayOrder)
                    .Select(img => new ConceptImgViewModel
                    {
                        ImgId = img.ImgId,
                        ConceptId = img.ConceptId ?? 0,
                        ImgUrl = img.ImgUrl,
                        ImgName = img.ImgName,
                        AltText = img.AltText,
                        IsPrimary = img.IsPrimary ?? false,
                        DisplayOrder = img.DisplayOrder ?? 0,
                        CreatedAt = img.CreatedAt
                    })
                    .ToListAsync();

                return images;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting concept images for concept {ConceptId}", conceptId);
                throw;
            }
        }

        public async Task<ConceptImgViewModel?> GetConceptImageByIdAsync(int imageId)
        {
            try
            {
                var image = await _context.ConceptImgs.FindAsync(imageId);
                if (image == null) return null;

                return new ConceptImgViewModel
                {
                    ImgId = image.ImgId,
                    ConceptId = image.ConceptId ?? 0,
                    ImgUrl = image.ImgUrl,
                    ImgName = image.ImgName,
                    AltText = image.AltText,
                    IsPrimary = image.IsPrimary ?? false,
                    DisplayOrder = image.DisplayOrder ?? 0,
                    CreatedAt = image.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting concept image by id: {ImageId}", imageId);
                throw;
            }
        }

        public async Task<bool> AddConceptImageAsync(ConceptImgViewModel model)
        {
            try
            {
                // Check if concept can add more images (limit to 6)
                var canAddMore = await CanConceptAddMoreImagesAsync(model.ConceptId);
                if (!canAddMore)
                {
                    return false;
                }

                // Auto-set IsPrimary and AltText based on DisplayOrder
                bool isPrimary = model.DisplayOrder == 1;
                string altText = model.AltText;
                
                if (string.IsNullOrEmpty(altText))
                {
                    if (model.DisplayOrder == 1)
                    {
                        altText = "Ảnh chính của concept";
                    }
                    else if (model.DisplayOrder >= 2 && model.DisplayOrder <= 3)
                    {
                        altText = "Banner concept";
                    }
                    else
                    {
                        altText = "Mô tả chi tiết về concept";
                    }
                }

                var image = new ConceptImg
                {
                    ConceptId = model.ConceptId,
                    ImgUrl = model.ImgUrl,
                    ImgName = model.ImgName,
                    AltText = altText,
                    IsPrimary = isPrimary,
                    DisplayOrder = model.DisplayOrder,
                    CreatedAt = DateTime.Now
                };

                _context.ConceptImgs.Add(image);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding concept image");
                throw;
            }
        }

        public async Task<bool> UpdateConceptImageAsync(ConceptImgViewModel model)
        {
            try
            {
                var image = await _context.ConceptImgs.FindAsync(model.ImgId);
                if (image == null) return false;

                // Auto-set IsPrimary and AltText based on DisplayOrder
                bool isPrimary = model.DisplayOrder == 1;
                string altText = model.AltText;
                
                if (string.IsNullOrEmpty(altText))
                {
                    if (model.DisplayOrder == 1)
                    {
                        altText = "Ảnh chính của concept";
                    }
                    else if (model.DisplayOrder >= 2 && model.DisplayOrder <= 3)
                    {
                        altText = "Banner concept";
                    }
                    else
                    {
                        altText = "Mô tả chi tiết về concept";
                    }
                }

                image.ImgUrl = model.ImgUrl;
                image.ImgName = model.ImgName;
                image.AltText = altText;
                image.IsPrimary = isPrimary;
                image.DisplayOrder = model.DisplayOrder;

                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating concept image");
                throw;
            }
        }

        public async Task<bool> DeleteConceptImageAsync(int imageId)
        {
            try
            {
                var image = await _context.ConceptImgs.FindAsync(imageId);
                if (image == null) return false;

                _context.ConceptImgs.Remove(image);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting concept image");
                throw;
            }
        }

        public async Task<bool> SetPrimaryImageAsync(int conceptId, int imageId)
        {
            try
            {
                // Remove primary flag from all images of this concept
                var conceptImages = await _context.ConceptImgs
                    .Where(img => img.ConceptId == conceptId)
                    .ToListAsync();

                foreach (var img in conceptImages)
                {
                    img.IsPrimary = false;
                }

                // Set the specified image as primary
                var primaryImage = conceptImages.FirstOrDefault(img => img.ImgId == imageId);
                if (primaryImage != null)
                {
                    primaryImage.IsPrimary = true;
                }

                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting primary image");
                throw;
            }
        }

        public async Task<int> GetConceptImageCountAsync(int conceptId)
        {
            try
            {
                return await _context.ConceptImgs.CountAsync(img => img.ConceptId == conceptId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting concept image count");
                throw;
            }
        }

        public async Task<bool> CanConceptAddMoreImagesAsync(int conceptId)
        {
            try
            {
                var imageCount = await GetConceptImageCountAsync(conceptId);
                return imageCount < 6; // Limit to 6 images per concept
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if concept can add more images");
                throw;
            }
        }

        public async Task<List<object>> GetConceptImagesGroupedByConceptAsync()
        {
            try
            {
                var concepts = await _context.Concepts
                    .Include(c => c.ConceptImgs)
                    .Where(c => c.ConceptImgs.Any())
                    .Select(c => new
                    {
                        ConceptId = c.ConceptId,
                        ConceptName = c.Name,
                        Images = c.ConceptImgs.Select(img => new ConceptImgViewModel
                        {
                            ImgId = img.ImgId,
                            ConceptId = img.ConceptId ?? 0,
                            ImgUrl = img.ImgUrl,
                            ImgName = img.ImgName,
                            AltText = img.AltText,
                            IsPrimary = img.IsPrimary ?? false,
                            DisplayOrder = img.DisplayOrder ?? 0,
                            CreatedAt = img.CreatedAt
                        }).ToList()
                    })
                    .ToListAsync();

                return concepts.Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting concept images grouped by concept");
                throw;
            }
        }

        // Dropdown data methods
        public async Task<List<ConceptCategoryDropdownViewModel>> GetConceptCategoriesForDropdownAsync()
        {
            try
            {
                var categories = await _context.ConceptCategories
                    .Where(c => c.IsActive == true)
                    .Select(c => new ConceptCategoryDropdownViewModel
                    {
                        CategoryId = c.CategoryId,
                        Name = c.Name ?? string.Empty
                    })
                    .ToListAsync();

                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting concept categories for dropdown");
                throw;
            }
        }

        public async Task<List<ConceptColorDropdownViewModel>> GetConceptColorsForDropdownAsync()
        {
            try
            {
                var colors = await _context.ConceptColors
                    .Select(c => new ConceptColorDropdownViewModel
                    {
                        ColorId = c.ColorId,
                        Name = c.Name ?? string.Empty,
                        Code = c.Code
                    })
                    .ToListAsync();

                return colors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting concept colors for dropdown");
                throw;
            }
        }

        public async Task<List<ConceptAmbienceDropdownViewModel>> GetConceptAmbiencesForDropdownAsync()
        {
            try
            {
                var ambiences = await _context.ConceptAmbiences
                    .Select(a => new ConceptAmbienceDropdownViewModel
                    {
                        AmbienceId = a.AmbienceId,
                        Name = a.Name ?? string.Empty
                    })
                    .ToListAsync();

                return ambiences;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting concept ambiences for dropdown");
                throw;
            }
        }

        public async Task<List<LocationDropdownViewModel>> GetLocationsForDropdownAsync()
        {
            try
            {
                var locations = await _context.Locations
                    .Where(l => l.Status == "Active")
                    .Select(l => new LocationDropdownViewModel
                    {
                        LocationId = l.LocationId,
                        Name = l.Name ?? string.Empty
                    })
                    .ToListAsync();

                return locations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting locations for dropdown");
                throw;
            }
        }

        #region Concept Category Management

        public async Task<List<ConceptCategoryDropdownViewModel>> GetConceptCategoriesAsync()
        {
            try
            {
                var categories = await _context.ConceptCategories
                    .OrderBy(c => c.Name)
                    .Select(c => new ConceptCategoryDropdownViewModel
                    {
                        CategoryId = c.CategoryId,
                        Name = c.Name ?? string.Empty,
                        Description = c.Description,
                        IsActive = c.IsActive ?? true
                    })
                    .ToListAsync();

                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting concept categories");
                throw;
            }
        }

        public async Task<ConceptCategoryDropdownViewModel?> GetConceptCategoryByIdAsync(int id)
        {
            try
            {
                var category = await _context.ConceptCategories
                    .Where(c => c.CategoryId == id)
                    .Select(c => new ConceptCategoryDropdownViewModel
                    {
                        CategoryId = c.CategoryId,
                        Name = c.Name ?? string.Empty,
                        Description = c.Description,
                        IsActive = c.IsActive ?? true
                    })
                    .FirstOrDefaultAsync();

                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting concept category by id: {CategoryId}", id);
                throw;
            }
        }

        public async Task<bool> CreateConceptCategoryAsync(ConceptCategoryDropdownViewModel model)
        {
            try
            {
                var category = new ConceptCategory
                {
                    Name = model.Name,
                    Description = model.Description,
                    IsActive = model.IsActive
                };

                _context.ConceptCategories.Add(category);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating concept category");
                throw;
            }
        }

        public async Task<bool> UpdateConceptCategoryAsync(ConceptCategoryDropdownViewModel model)
        {
            try
            {
                var category = await _context.ConceptCategories.FindAsync(model.CategoryId);
                if (category == null) return false;

                category.Name = model.Name;
                category.Description = model.Description;
                category.IsActive = model.IsActive;

                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating concept category");
                throw;
            }
        }

        public async Task<bool> DeleteConceptCategoryAsync(int id)
        {
            try
            {
                // Check if category is being used by any concepts
                var isUsed = await _context.Concepts.AnyAsync(c => c.CategoryId == id);
                if (isUsed)
                {
                    return false; // Cannot delete if in use
                }

                var category = await _context.ConceptCategories.FindAsync(id);
                if (category == null) return false;

                _context.ConceptCategories.Remove(category);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting concept category");
                throw;
            }
        }

        #endregion

        #region Concept Color Management

        public async Task<List<ConceptColorDropdownViewModel>> GetConceptColorsAsync()
        {
            try
            {
                var colors = await _context.ConceptColors
                    .OrderBy(c => c.Name)
                    .Select(c => new ConceptColorDropdownViewModel
                    {
                        ColorId = c.ColorId,
                        Name = c.Name ?? string.Empty,
                        Code = c.Code
                    })
                    .ToListAsync();

                return colors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting concept colors");
                throw;
            }
        }

        public async Task<ConceptColorDropdownViewModel?> GetConceptColorByIdAsync(int id)
        {
            try
            {
                var color = await _context.ConceptColors
                    .Where(c => c.ColorId == id)
                    .Select(c => new ConceptColorDropdownViewModel
                    {
                        ColorId = c.ColorId,
                        Name = c.Name ?? string.Empty,
                        Code = c.Code
                    })
                    .FirstOrDefaultAsync();

                return color;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting concept color by id: {ColorId}", id);
                throw;
            }
        }

        public async Task<bool> CreateConceptColorAsync(ConceptColorDropdownViewModel model)
        {
            try
            {
                var color = new ConceptColor
                {
                    Name = model.Name,
                    Code = model.Code
                };

                _context.ConceptColors.Add(color);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating concept color");
                throw;
            }
        }

        public async Task<bool> UpdateConceptColorAsync(ConceptColorDropdownViewModel model)
        {
            try
            {
                var color = await _context.ConceptColors.FindAsync(model.ColorId);
                if (color == null) return false;

                color.Name = model.Name;
                color.Code = model.Code;

                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating concept color");
                throw;
            }
        }

        public async Task<bool> DeleteConceptColorAsync(int id)
        {
            try
            {
                // Check if color is being used by any concepts
                var isUsed = await _context.ConceptColorJunctions.AnyAsync(ccj => ccj.ColorId == id);
                if (isUsed)
                {
                    return false; // Cannot delete if in use
                }

                var color = await _context.ConceptColors.FindAsync(id);
                if (color == null) return false;

                _context.ConceptColors.Remove(color);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting concept color");
                throw;
            }
        }

        #endregion

        #region Concept Ambience Management

        public async Task<List<ConceptAmbienceDropdownViewModel>> GetConceptAmbiencesAsync()
        {
            try
            {
                var ambiences = await _context.ConceptAmbiences
                    .OrderBy(a => a.Name)
                    .Select(a => new ConceptAmbienceDropdownViewModel
                    {
                        AmbienceId = a.AmbienceId,
                        Name = a.Name ?? string.Empty
                    })
                    .ToListAsync();

                return ambiences;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting concept ambiences");
                throw;
            }
        }

        public async Task<ConceptAmbienceDropdownViewModel?> GetConceptAmbienceByIdAsync(int id)
        {
            try
            {
                var ambience = await _context.ConceptAmbiences
                    .Where(a => a.AmbienceId == id)
                    .Select(a => new ConceptAmbienceDropdownViewModel
                    {
                        AmbienceId = a.AmbienceId,
                        Name = a.Name ?? string.Empty
                    })
                    .FirstOrDefaultAsync();

                return ambience;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting concept ambience by id: {AmbienceId}", id);
                throw;
            }
        }

        public async Task<bool> CreateConceptAmbienceAsync(ConceptAmbienceDropdownViewModel model)
        {
            try
            {
                var ambience = new ConceptAmbience
                {
                    Name = model.Name
                };

                _context.ConceptAmbiences.Add(ambience);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating concept ambience");
                throw;
            }
        }

        public async Task<bool> UpdateConceptAmbienceAsync(ConceptAmbienceDropdownViewModel model)
        {
            try
            {
                var ambience = await _context.ConceptAmbiences.FindAsync(model.AmbienceId);
                if (ambience == null) return false;

                ambience.Name = model.Name;

                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating concept ambience");
                throw;
            }
        }

        public async Task<bool> DeleteConceptAmbienceAsync(int id)
        {
            try
            {
                // Check if ambience is being used by any concepts
                var isUsed = await _context.Concepts.AnyAsync(c => c.AmbienceId == id);
                if (isUsed)
                {
                    return false; // Cannot delete if in use
                }

                var ambience = await _context.ConceptAmbiences.FindAsync(id);
                if (ambience == null) return false;

                _context.ConceptAmbiences.Remove(ambience);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting concept ambience");
                throw;
            }
        }

        #endregion
    }
}
