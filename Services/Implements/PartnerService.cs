using Microsoft.EntityFrameworkCore;
using MonAmour.Models;
using MonAmour.Services.Interfaces;
using MonAmour.ViewModels;

namespace MonAmour.Services.Implements
{
    public class PartnerService : IPartnerService
    {
        private readonly MonAmourDbContext _context;
        private readonly ILogger<PartnerService> _logger;

        public PartnerService(MonAmourDbContext context, ILogger<PartnerService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(List<PartnerViewModel> partners, int totalCount)> GetPartnersAsync(PartnerSearchViewModel searchModel)
        {
            try
            {
                var query = _context.Partners
                    .Include(p => p.User)
                    .Include(p => p.Locations)
                    .AsQueryable();

                // Apply search filters
                if (!string.IsNullOrEmpty(searchModel.SearchTerm))
                {
                    var searchTerm = searchModel.SearchTerm.ToLower();
                    query = query.Where(p => 
                        p.Name!.ToLower().Contains(searchTerm) ||
                        p.Email!.ToLower().Contains(searchTerm) ||
                        p.Phone!.ToLower().Contains(searchTerm) ||
                        p.ContactInfo!.ToLower().Contains(searchTerm));
                }

                if (!string.IsNullOrEmpty(searchModel.Status))
                {
                    query = query.Where(p => p.Status == searchModel.Status);
                }

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply sorting
                query = searchModel.SortBy?.ToLower() switch
                {
                    "name" => searchModel.SortOrder == "desc" ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                    "email" => searchModel.SortOrder == "desc" ? query.OrderByDescending(p => p.Email) : query.OrderBy(p => p.Email),
                    "status" => searchModel.SortOrder == "desc" ? query.OrderByDescending(p => p.Status) : query.OrderBy(p => p.Status),
                    "createdat" => searchModel.SortOrder == "desc" ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
                    _ => query.OrderBy(p => p.Name)
                };

                // Apply pagination
                var partners = await query
                    .Skip((searchModel.Page - 1) * searchModel.PageSize)
                    .Take(searchModel.PageSize)
                    .Select(p => new PartnerViewModel
                    {
                        PartnerId = p.PartnerId,
                        Name = p.Name ?? string.Empty,
                        ContactInfo = p.ContactInfo,
                        UserId = p.UserId,
                        Email = p.Email,
                        Phone = p.Phone,
                        //Avatar = p.Avatar,
                        Status = p.Status ?? "Active",
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt,
                        UserName = p.User != null ? p.User.Name : null,
                        LocationCount = p.Locations.Count
                    })
                    .ToListAsync();

                return (partners, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting partners");
                throw;
            }
        }

        public async Task<PartnerDetailViewModel?> GetPartnerByIdAsync(int id)
        {
            try
            {
                var partner = await _context.Partners
                    .Include(p => p.User)
                    .Include(p => p.Locations)
                        .ThenInclude(l => l.Concepts)
                    .FirstOrDefaultAsync(p => p.PartnerId == id);

                if (partner == null) return null;

                return new PartnerDetailViewModel
                {
                    PartnerId = partner.PartnerId,
                    Name = partner.Name ?? string.Empty,
                    ContactInfo = partner.ContactInfo,
                    UserId = partner.UserId,
                    Email = partner.Email,
                    Phone = partner.Phone,
                    //Avatar = partner.Avatar,
                    Status = partner.Status ?? "Active",
                    CreatedAt = partner.CreatedAt,
                    UpdatedAt = partner.UpdatedAt,
                    UserName = partner.User != null ? partner.User.Name : null,
                    Locations = partner.Locations.Select(l => new LocationViewModel
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
                        PartnerName = partner.Name,
                        ConceptCount = l.Concepts.Count
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting partner by id: {Id}", id);
                throw;
            }
        }

        public async Task<bool> CreatePartnerAsync(PartnerCreateViewModel model)
        {
            try
            {
                _logger.LogInformation("Creating partner: Name={Name}, Status={Status}, UserId={UserId}, Email={Email}, Phone={Phone}, Avatar={Avatar}", 
                    model.Name, model.Status, model.UserId, model.Email, model.Phone, model.Avatar);

                var partner = new Partner
                {
                    Name = model.Name,
                    ContactInfo = model.ContactInfo,
                    UserId = model.UserId,
                    Email = model.Email,
                    Phone = model.Phone,
                    //Avatar = model.Avatar,
                    Status = model.Status,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                //_logger.LogInformation("Partner entity created: PartnerId={PartnerId}, Name={Name}, Avatar={Avatar}", 
                //    partner.PartnerId, partner.Name, partner.Avatar);
                _logger.LogInformation("Partner entity created: PartnerId={PartnerId}, Name={Name}, Avatar={Avatar}",
                    partner.PartnerId, partner.Name);

                _context.Partners.Add(partner);
                
                try
                {
                    var result = await _context.SaveChangesAsync();
                    _logger.LogInformation("Partner created successfully. Result: {Result}, PartnerId: {PartnerId}", result, partner.PartnerId);
                    return result > 0;
                }
                catch (Exception saveEx)
                {
                    //_logger.LogError(saveEx, "Error saving partner to database. Partner data: Name={Name}, UserId={UserId}, Avatar={Avatar}", 
                    //    partner.Name, partner.UserId, partner.Avatar);
                    _logger.LogError(saveEx, "Error saving partner to database. Partner data: Name={Name}, UserId={UserId}, Avatar={Avatar}",
                        partner.Name, partner.UserId);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating partner");
                throw;
            }
        }

        public async Task<bool> UpdatePartnerAsync(PartnerEditViewModel model)
        {
            try
            {
                _logger.LogInformation("Updating partner: PartnerId={PartnerId}, Name={Name}, Status={Status}, UserId={UserId}, Email={Email}, Phone={Phone}", 
                    model.PartnerId, model.Name, model.Status, model.UserId, model.Email, model.Phone);

                var partner = await _context.Partners.FindAsync(model.PartnerId);
                if (partner == null) 
                {
                    _logger.LogWarning("Partner not found with ID: {PartnerId}", model.PartnerId);
                    return false;
                }

                partner.Name = model.Name;
                partner.ContactInfo = model.ContactInfo;
                partner.UserId = model.UserId;
                partner.Email = model.Email;
                partner.Phone = model.Phone;
                //partner.Avatar = model.Avatar;
                partner.Status = model.Status;
                partner.UpdatedAt = DateTime.Now;

                _context.Partners.Update(partner);
                var result = await _context.SaveChangesAsync();
                
                _logger.LogInformation("Partner updated successfully. Result: {Result}", result);
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating partner: {Id}", model.PartnerId);
                throw;
            }
        }

        public async Task<bool> DeletePartnerAsync(int id)
        {
            try
            {
                var partner = await _context.Partners
                    .Include(p => p.Locations)
                    .FirstOrDefaultAsync(p => p.PartnerId == id);

                if (partner == null) return false;

                // Check if partner has locations
                if (partner.Locations.Any())
                {
                    return false; // Cannot delete partner with locations
                }

                _context.Partners.Remove(partner);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting partner: {Id}", id);
                throw;
            }
        }

        public async Task<Dictionary<string, int>> GetPartnerStatisticsAsync()
        {
            try
            {
                var stats = new Dictionary<string, int>
                {
                    ["TotalPartners"] = await _context.Partners.CountAsync(),
                    ["ActivePartners"] = await _context.Partners.CountAsync(p => p.Status == "Active"),
                    ["InactivePartners"] = await _context.Partners.CountAsync(p => p.Status == "Inactive"),
                    ["WithLocations"] = await _context.Partners.CountAsync(p => p.Locations.Any())
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting partner statistics");
                throw;
            }
        }

        public async Task<List<PartnerDropdownViewModel>> GetPartnersForDropdownAsync()
        {
            try
            {
                var partners = await _context.Partners
                    .Where(p => p.Status == "Active")
                    .Select(p => new PartnerDropdownViewModel
                    {
                        PartnerId = p.PartnerId,
                        Name = p.Name ?? string.Empty
                    })
                    .ToListAsync();

                return partners;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting partners for dropdown");
                throw;
            }
        }

        public async Task<bool> TogglePartnerStatusAsync(int id, string status)
        {
            try
            {
                var partner = await _context.Partners.FindAsync(id);
                if (partner == null) return false;

                partner.Status = status;
                partner.UpdatedAt = DateTime.Now;

                _context.Partners.Update(partner);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling partner status: {Id}", id);
                throw;
            }
        }
    }
}
