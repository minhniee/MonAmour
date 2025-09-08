using Microsoft.EntityFrameworkCore;
using MonAmour.Models;
using MonAmour.Services.Interfaces;
using MonAmour.ViewModels;

namespace MonAmour.Services.Implements
{
    public class LocationService : ILocationService
    {
        private readonly MonAmourDbContext _context;
        private readonly ILogger<LocationService> _logger;

        public LocationService(MonAmourDbContext context, ILogger<LocationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(List<LocationViewModel> locations, int totalCount)> GetLocationsAsync(LocationSearchViewModel searchModel)
        {
            try
            {
                var query = _context.Locations
                    .Include(l => l.Partner)
                    .Include(l => l.Concepts)
                    .AsQueryable();

                // Apply search filters
                if (!string.IsNullOrEmpty(searchModel.SearchTerm))
                {
                    var searchTerm = searchModel.SearchTerm.ToLower();
                    query = query.Where(l => 
                        l.Name!.ToLower().Contains(searchTerm) ||
                        l.Address!.ToLower().Contains(searchTerm) ||
                        l.District!.ToLower().Contains(searchTerm) ||
                        l.City!.ToLower().Contains(searchTerm) ||
                        l.Partner!.Name!.ToLower().Contains(searchTerm));
                }

                if (!string.IsNullOrEmpty(searchModel.Status))
                {
                    query = query.Where(l => l.Status == searchModel.Status);
                }

                if (searchModel.PartnerId.HasValue)
                {
                    query = query.Where(l => l.PartnerId == searchModel.PartnerId.Value);
                }

                if (!string.IsNullOrEmpty(searchModel.City))
                {
                    query = query.Where(l => l.City == searchModel.City);
                }

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply sorting
                query = searchModel.SortBy?.ToLower() switch
                {
                    "name" => searchModel.SortOrder == "desc" ? query.OrderByDescending(l => l.Name) : query.OrderBy(l => l.Name),
                    "address" => searchModel.SortOrder == "desc" ? query.OrderByDescending(l => l.Address) : query.OrderBy(l => l.Address),
                    "city" => searchModel.SortOrder == "desc" ? query.OrderByDescending(l => l.City) : query.OrderBy(l => l.City),
                    "status" => searchModel.SortOrder == "desc" ? query.OrderByDescending(l => l.Status) : query.OrderBy(l => l.Status),
                    "createdat" => searchModel.SortOrder == "desc" ? query.OrderByDescending(l => l.CreatedAt) : query.OrderBy(l => l.CreatedAt),
                    _ => query.OrderBy(l => l.Name)
                };

                // Apply pagination
                var locations = await query
                    .Skip((searchModel.Page - 1) * searchModel.PageSize)
                    .Take(searchModel.PageSize)
                    .Select(l => new LocationViewModel
                    {
                        LocationId = l.LocationId,
                        Name = l.Name ?? string.Empty,
                        Address = l.Address ?? string.Empty,
                        District = l.District,
                        City = l.City,
                        Status = l.Status ?? "Active",
                        PartnerId = l.PartnerId,
                        GgmapLink = l.GgmapLink,
                        CreatedAt = l.CreatedAt,
                        UpdatedAt = l.UpdatedAt,
                        PartnerName = l.Partner != null ? l.Partner.Name : null,
                        ConceptCount = l.Concepts.Count
                    })
                    .ToListAsync();

                return (locations, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting locations");
                throw;
            }
        }

        public async Task<LocationDetailViewModel?> GetLocationByIdAsync(int id)
        {
            try
            {
                var location = await _context.Locations
                    .Include(l => l.Partner)
                    .Include(l => l.Concepts)
                    .FirstOrDefaultAsync(l => l.LocationId == id);

                if (location == null) return null;

                return new LocationDetailViewModel
                {
                    LocationId = location.LocationId,
                    Name = location.Name ?? string.Empty,
                    Address = location.Address ?? string.Empty,
                    District = location.District,
                    City = location.City,
                    Status = location.Status ?? "Active",
                    PartnerId = location.PartnerId,
                    GgmapLink = location.GgmapLink,
                    CreatedAt = location.CreatedAt,
                    UpdatedAt = location.UpdatedAt,
                    PartnerName = location.Partner != null ? location.Partner.Name : null,
                    Concepts = location.Concepts.Select(c => new LocationConceptViewModel
                    {
                        ConceptId = c.ConceptId,
                        Name = c.Name ?? string.Empty,
                        Description = c.Description,
                        Status = c.AvailabilityStatus == true ? "Active" : "Inactive",
                        CreatedAt = c.CreatedAt
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting location by id: {Id}", id);
                throw;
            }
        }

        public async Task<bool> CreateLocationAsync(LocationCreateViewModel model)
        {
            try
            {
                var location = new Location
                {
                    Name = model.Name,
                    Address = model.Address,
                    District = model.District,
                    City = model.City,
                    Status = model.Status,
                    PartnerId = model.PartnerId,
                    GgmapLink = model.GgmapLink,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Locations.Add(location);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating location");
                throw;
            }
        }

        public async Task<bool> UpdateLocationAsync(LocationEditViewModel model)
        {
            try
            {
                var location = await _context.Locations.FindAsync(model.LocationId);
                if (location == null) return false;

                location.Name = model.Name;
                location.Address = model.Address;
                location.District = model.District;
                location.City = model.City;
                location.Status = model.Status;
                location.PartnerId = model.PartnerId;
                location.GgmapLink = model.GgmapLink;
                location.UpdatedAt = DateTime.Now;

                _context.Locations.Update(location);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating location: {Id}", model.LocationId);
                throw;
            }
        }

        public async Task<bool> DeleteLocationAsync(int id)
        {
            try
            {
                var location = await _context.Locations
                    .Include(l => l.Concepts)
                    .FirstOrDefaultAsync(l => l.LocationId == id);

                if (location == null) return false;

                // Check if location has concepts
                if (location.Concepts.Any())
                {
                    return false; // Cannot delete location with concepts
                }

                _context.Locations.Remove(location);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting location: {Id}", id);
                throw;
            }
        }

        public async Task<Dictionary<string, int>> GetLocationStatisticsAsync()
        {
            try
            {
                var stats = new Dictionary<string, int>
                {
                    ["Total"] = await _context.Locations.CountAsync(),
                    ["Active"] = await _context.Locations.CountAsync(l => l.Status == "Active"),
                    ["Inactive"] = await _context.Locations.CountAsync(l => l.Status == "Inactive"),
                    ["WithConcepts"] = await _context.Locations.CountAsync(l => l.Concepts.Any())
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting location statistics");
                throw;
            }
        }

        public async Task<List<object>> GetLocationsForDropdownAsync()
        {
            try
            {
                var locations = await _context.Locations
                    .Where(l => l.Status == "Active")
                    .Select(l => new
                    {
                        locationId = l.LocationId,
                        name = l.Name,
                        address = l.Address,
                        city = l.City
                    })
                    .ToListAsync();

                return locations.Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting locations for dropdown");
                throw;
            }
        }

        public async Task<List<object>> GetLocationsByPartnerAsync(int partnerId)
        {
            try
            {
                var locations = await _context.Locations
                    .Where(l => l.PartnerId == partnerId && l.Status == "Active")
                    .Select(l => new
                    {
                        locationId = l.LocationId,
                        name = l.Name,
                        address = l.Address,
                        city = l.City
                    })
                    .ToListAsync();

                return locations.Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting locations by partner: {PartnerId}", partnerId);
                throw;
            }
        }

        public async Task<bool> ToggleLocationStatusAsync(int id, string status)
        {
            try
            {
                var location = await _context.Locations.FindAsync(id);
                if (location == null) return false;

                location.Status = status;
                location.UpdatedAt = DateTime.Now;

                _context.Locations.Update(location);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling location status: {Id}", id);
                throw;
            }
        }
    }
}
