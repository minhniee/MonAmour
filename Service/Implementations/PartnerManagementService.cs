using Microsoft.EntityFrameworkCore;
using MonAmourDb_BE.DTOs;
using MonAmourDb_BE.Models;
using MonAmourDb_BE.Service.Interfaces;

namespace MonAmourDb_BE.Service.Implementations
{
    public class PartnerManagementService : IPartnerManagementService
    {
        private readonly MonAmourDbContext _context;

        public PartnerManagementService(MonAmourDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<PartnerListDto>> GetPartnersAsync(PartnerFilterDto filter)
        {
            var query = _context.Partners
                .Include(p => p.User)
                .Include(p => p.Locations)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(p =>
                    (p.Name != null && p.Name.Contains(filter.SearchTerm)) ||
                    (p.Email != null && p.Email.Contains(filter.SearchTerm)) ||
                    (p.Phone != null && p.Phone.Contains(filter.SearchTerm)) ||
                    (p.User != null && p.User.Name != null && p.User.Name.Contains(filter.SearchTerm)));
            }

            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(p => p.Status == filter.Status);
            }

            if (filter.CreatedFrom.HasValue)
            {
                query = query.Where(p => p.CreatedAt >= filter.CreatedFrom.Value);
            }

            if (filter.CreatedTo.HasValue)
            {
                query = query.Where(p => p.CreatedAt <= filter.CreatedTo.Value);
            }

            // Apply sorting
            query = filter.SortBy.ToLower() switch
            {
                "name" => filter.SortOrder.ToLower() == "asc"
                    ? query.OrderBy(p => p.Name)
                    : query.OrderByDescending(p => p.Name),
                "email" => filter.SortOrder.ToLower() == "asc"
                    ? query.OrderBy(p => p.Email)
                    : query.OrderByDescending(p => p.Email),
                "status" => filter.SortOrder.ToLower() == "asc"
                    ? query.OrderBy(p => p.Status)
                    : query.OrderByDescending(p => p.Status),
                _ => filter.SortOrder.ToLower() == "asc"
                    ? query.OrderBy(p => p.CreatedAt)
                    : query.OrderByDescending(p => p.CreatedAt)
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

            var partners = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(p => new PartnerListDto
                {
                    PartnerId = p.PartnerId,
                    Name = p.Name,
                    Email = p.Email,
                    Phone = p.Phone,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    TotalLocations = p.Locations.Count,
                    UserName = p.User != null ? p.User.Name : null
                })
                .ToListAsync();

            return new PaginatedResult<PartnerListDto>
            {
                Data = partners,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = totalPages,
                HasNextPage = filter.Page < totalPages,
                HasPreviousPage = filter.Page > 1
            };
        }

        public async Task<PartnerDetailDto?> GetPartnerByIdAsync(int partnerId)
        {
            var partner = await _context.Partners
                .Include(p => p.User)
                .Include(p => p.Locations)
                .ThenInclude(l => l.Concepts)
                .FirstOrDefaultAsync(p => p.PartnerId == partnerId);

            if (partner == null) return null;

            return new PartnerDetailDto
            {
                PartnerId = partner.PartnerId,
                Name = partner.Name,
                ContactInfo = partner.ContactInfo,
                Email = partner.Email,
                Phone = partner.Phone,
                Status = partner.Status,
                UserId = partner.UserId,
                CreatedAt = partner.CreatedAt,
                UpdatedAt = partner.UpdatedAt,
                TotalLocations = partner.Locations.Count,
                UserName = partner.User?.Name,
                User = partner.User != null ? new UserBasicDto
                {
                    UserId = partner.User.UserId,
                    Email = partner.User.Email,
                    Name = partner.User.Name,
                    Phone = partner.User.Phone
                } : null,
                Locations = partner.Locations.Select(l => new LocationDto
                {
                    LocationId = l.LocationId,
                    Name = l.Name,
                    Address = l.Address,
                    District = l.District,
                    City = l.City,
                    Status = l.Status,
                    GgmapLink = l.GgmapLink,
                    CreatedAt = l.CreatedAt,
                    TotalConcepts = l.Concepts.Count
                }).ToList()
            };
        }

        public async Task<PartnerDetailDto> CreatePartnerAsync(CreatePartnerDto createPartnerDto)
        {
            // Check if email already exists
            if (!string.IsNullOrEmpty(createPartnerDto.Email) &&
                await _context.Partners.AnyAsync(p => p.Email == createPartnerDto.Email))
            {
                throw new InvalidOperationException("Email đã tồn tại trong hệ thống");
            }

            // Check if phone already exists
            if (!string.IsNullOrEmpty(createPartnerDto.Phone) &&
                await _context.Partners.AnyAsync(p => p.Phone == createPartnerDto.Phone))
            {
                throw new InvalidOperationException("Số điện thoại đã tồn tại trong hệ thống");
            }

            // Check if user exists and is not already a partner
            if (createPartnerDto.UserId.HasValue)
            {
                var user = await _context.Users.FindAsync(createPartnerDto.UserId.Value);
                if (user == null)
                {
                    throw new InvalidOperationException("User không tồn tại");
                }

                if (await _context.Partners.AnyAsync(p => p.UserId == createPartnerDto.UserId.Value))
                {
                    throw new InvalidOperationException("User này đã là partner");
                }
            }

            var partner = new Partner
            {
                Name = createPartnerDto.Name,
                ContactInfo = createPartnerDto.ContactInfo,
                Email = createPartnerDto.Email,
                Phone = createPartnerDto.Phone,
                UserId = createPartnerDto.UserId,
                Status = createPartnerDto.Status,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Partners.Add(partner);
            await _context.SaveChangesAsync();

            return await GetPartnerByIdAsync(partner.PartnerId) ??
                throw new InvalidOperationException("Không thể tạo partner");
        }

        public async Task<PartnerDetailDto?> UpdatePartnerAsync(int partnerId, UpdatePartnerDto updatePartnerDto)
        {
            var partner = await _context.Partners.FindAsync(partnerId);
            if (partner == null) return null;

            // Check if email already exists (excluding current partner)
            if (!string.IsNullOrEmpty(updatePartnerDto.Email) &&
                await _context.Partners.AnyAsync(p => p.Email == updatePartnerDto.Email && p.PartnerId != partnerId))
            {
                throw new InvalidOperationException("Email đã tồn tại trong hệ thống");
            }

            // Check if phone already exists (excluding current partner)
            if (!string.IsNullOrEmpty(updatePartnerDto.Phone) &&
                await _context.Partners.AnyAsync(p => p.Phone == updatePartnerDto.Phone && p.PartnerId != partnerId))
            {
                throw new InvalidOperationException("Số điện thoại đã tồn tại trong hệ thống");
            }

            // Check if user exists and is not already a partner (excluding current partner)
            if (updatePartnerDto.UserId.HasValue)
            {
                var user = await _context.Users.FindAsync(updatePartnerDto.UserId.Value);
                if (user == null)
                {
                    throw new InvalidOperationException("User không tồn tại");
                }

                if (await _context.Partners.AnyAsync(p => p.UserId == updatePartnerDto.UserId.Value && p.PartnerId != partnerId))
                {
                    throw new InvalidOperationException("User này đã là partner khác");
                }
            }

            // Update only non-null values
            if (updatePartnerDto.Name != null) partner.Name = updatePartnerDto.Name;
            if (updatePartnerDto.ContactInfo != null) partner.ContactInfo = updatePartnerDto.ContactInfo;
            if (updatePartnerDto.Email != null) partner.Email = updatePartnerDto.Email;
            if (updatePartnerDto.Phone != null) partner.Phone = updatePartnerDto.Phone;
            if (updatePartnerDto.UserId.HasValue) partner.UserId = updatePartnerDto.UserId;
            if (updatePartnerDto.Status != null) partner.Status = updatePartnerDto.Status;

            partner.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return await GetPartnerByIdAsync(partnerId);
        }

        public async Task<bool> DeletePartnerAsync(int partnerId)
        {
            var partner = await _context.Partners
                .Include(p => p.Locations)
                .FirstOrDefaultAsync(p => p.PartnerId == partnerId);

            if (partner == null) return false;

            // Check if partner has active locations
            if (partner.Locations.Any(l => l.Status == "active"))
            {
                throw new InvalidOperationException("Không thể xóa partner có location đang hoạt động");
            }

            // Soft delete - chỉ thay đổi status
            partner.Status = "inactive";
            partner.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> TogglePartnerStatusAsync(int partnerId)
        {
            var partner = await _context.Partners.FindAsync(partnerId);
            if (partner == null) return false;

            partner.Status = partner.Status == "active" ? "inactive" : "active";
            partner.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ApprovePartnerAsync(int partnerId)
        {
            var partner = await _context.Partners.FindAsync(partnerId);
            if (partner == null) return false;

            partner.Status = "active";
            partner.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SuspendPartnerAsync(int partnerId)
        {
            var partner = await _context.Partners.FindAsync(partnerId);
            if (partner == null) return false;

            partner.Status = "suspended";
            partner.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PartnerStatsDto> GetPartnerStatsAsync()
        {
            var totalPartners = await _context.Partners.CountAsync();
            var activePartners = await _context.Partners.CountAsync(p => p.Status == "active");
            var pendingPartners = await _context.Partners.CountAsync(p => p.Status == "pending");
            var inactivePartners = await _context.Partners.CountAsync(p => p.Status == "inactive");
            var suspendedPartners = await _context.Partners.CountAsync(p => p.Status == "suspended");
            var newPartnersThisMonth = await _context.Partners
                .CountAsync(p => p.CreatedAt.HasValue &&
                               p.CreatedAt.Value.Month == DateTime.Now.Month &&
                               p.CreatedAt.Value.Year == DateTime.Now.Year);

            // Partner growth for last 6 months
            var partnerGrowth = await _context.Partners
                .Where(p => p.CreatedAt.HasValue && p.CreatedAt >= DateTime.Now.AddMonths(-6))
                .GroupBy(p => new { p.CreatedAt!.Value.Year, p.CreatedAt.Value.Month })
                .Select(g => new PartnerGrowthDto
                {
                    Month = $"{g.Key.Month:00}/{g.Key.Year}",
                    Count = g.Count()
                })
                .OrderBy(g => g.Month)
                .ToListAsync();

            return new PartnerStatsDto
            {
                TotalPartners = totalPartners,
                ActivePartners = activePartners,
                PendingPartners = pendingPartners,
                InactivePartners = inactivePartners,
                SuspendedPartners = suspendedPartners,
                NewPartnersThisMonth = newPartnersThisMonth,
                PartnerGrowth = partnerGrowth
            };
        }

        public async Task<List<UserBasicDto>> GetAvailableUsersAsync()
        {
            return await _context.Users
                .Where(u => u.Status == "active" &&
                           !_context.Partners.Any(p => p.UserId == u.UserId))
                .Select(u => new UserBasicDto
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    Name = u.Name,
                    Phone = u.Phone
                })
                .ToListAsync();
        }
    }
}
