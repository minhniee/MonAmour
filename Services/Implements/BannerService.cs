using Microsoft.EntityFrameworkCore;
using MonAmour.Models;
using MonAmour.Services.Interfaces;
using MonAmour.ViewModels;

namespace MonAmour.Services.Implements
{
    public class BannerManagementService : IBannerService
    {
        private readonly MonAmourDbContext _context;
        private readonly ILogger<BannerManagementService> _logger;
        private readonly IFileUploadService _fileUploadService;

        public BannerManagementService(MonAmourDbContext context, ILogger<BannerManagementService> logger, IFileUploadService fileUploadService)
        {
            _context = context;
            _logger = logger;
            _fileUploadService = fileUploadService;
        }

        #region BannerService Methods

        public async Task<List<BannerServiceListViewModel>> GetAllBannerServicesAsync()
        {
            try
            {
                var banners = await _context.BannerServices
                    .OrderBy(b => b.DisplayOrder)
                    .ThenBy(b => b.CreatedAt)
                    .Select(b => new BannerServiceListViewModel
                    {
                        BannerId = b.BannerId,
                        ImgUrl = b.ImgUrl,
                        IsPrimary = b.IsPrimary,
                        DisplayOrder = b.DisplayOrder,
                        Description = b.Description,
                        IsActive = b.IsActive,
                        CreatedAt = b.CreatedAt,
                        UpdatedAt = b.UpdatedAt
                    })
                    .ToListAsync();

                return banners;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all banner services");
                return new List<BannerServiceListViewModel>();
            }
        }

        public async Task<BannerService?> GetBannerServiceByIdAsync(int id)
        {
            try
            {
                return await _context.BannerServices.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting banner service by ID: {Id}", id);
                return null;
            }
        }

        public async Task<bool> CreateBannerServiceAsync(BannerServiceCreateViewModel model)
        {
            try
            {
                string? imageUrl = null;
                
                if (model.ImageFile != null)
                {
                    imageUrl = await _fileUploadService.UploadBannerImageAsync(model.ImageFile, "service");
                    if (string.IsNullOrEmpty(imageUrl))
                    {
                        _logger.LogError("Failed to upload banner service image");
                        return false;
                    }
                }
                else if (!string.IsNullOrEmpty(model.ImgUrl))
                {
                    imageUrl = model.ImgUrl;
                }
                else
                {
                    _logger.LogError("No image provided for banner service");
                    return false;
                }

                var banner = new BannerService
                {
                    ImgUrl = imageUrl,
                    IsPrimary = model.IsPrimary,
                    DisplayOrder = model.DisplayOrder,
                    Description = model.Description,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                // If this is set as primary, unset other primary banners
                if (model.IsPrimary)
                {
                    var existingPrimary = await _context.BannerServices
                        .Where(b => b.IsPrimary && b.IsActive)
                        .FirstOrDefaultAsync();
                    
                    if (existingPrimary != null)
                    {
                        existingPrimary.IsPrimary = false;
                        existingPrimary.UpdatedAt = DateTime.Now;
                    }
                }

                _context.BannerServices.Add(banner);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating banner service");
                return false;
            }
        }

        public async Task<bool> UpdateBannerServiceAsync(BannerServiceEditViewModel model)
        {
            try
            {
                var banner = await _context.BannerServices.FindAsync(model.BannerId);
                if (banner == null) return false;

                // Handle image update
                if (model.ImageFile != null)
                {
                    var newImageUrl = await _fileUploadService.UpdateBannerImageAsync(model.ImageFile, banner.ImgUrl, "service");
                    if (!string.IsNullOrEmpty(newImageUrl))
                    {
                        banner.ImgUrl = newImageUrl;
                    }
                }
                else if (!string.IsNullOrEmpty(model.ImgUrl))
                {
                    banner.ImgUrl = model.ImgUrl;
                }

                banner.IsPrimary = model.IsPrimary;
                banner.DisplayOrder = model.DisplayOrder;
                banner.Description = model.Description;
                banner.IsActive = model.IsActive;
                banner.UpdatedAt = DateTime.Now;

                // If this is set as primary, unset other primary banners
                if (model.IsPrimary)
                {
                    var existingPrimary = await _context.BannerServices
                        .Where(b => b.IsPrimary && b.IsActive && b.BannerId != model.BannerId)
                        .FirstOrDefaultAsync();
                    
                    if (existingPrimary != null)
                    {
                        existingPrimary.IsPrimary = false;
                        existingPrimary.UpdatedAt = DateTime.Now;
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating banner service");
                return false;
            }
        }

        public async Task<bool> DeleteBannerServiceAsync(int id)
        {
            try
            {
                var banner = await _context.BannerServices.FindAsync(id);
                if (banner == null) return false;

                // Delete associated image file
                await _fileUploadService.DeleteBannerImageAsync(banner.ImgUrl);

                _context.BannerServices.Remove(banner);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting banner service");
                return false;
            }
        }

        public async Task<bool> ToggleBannerServiceStatusAsync(int id)
        {
            try
            {
                var banner = await _context.BannerServices.FindAsync(id);
                if (banner == null) return false;

                banner.IsActive = !banner.IsActive;
                banner.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling banner service status");
                return false;
            }
        }

        public async Task<bool> SetPrimaryBannerServiceAsync(int id)
        {
            try
            {
                var banner = await _context.BannerServices.FindAsync(id);
                if (banner == null) return false;

                // Unset all other primary banners
                var existingPrimary = await _context.BannerServices
                    .Where(b => b.IsPrimary && b.IsActive)
                    .ToListAsync();
                
                foreach (var primary in existingPrimary)
                {
                    primary.IsPrimary = false;
                    primary.UpdatedAt = DateTime.Now;
                }

                // Set this banner as primary
                banner.IsPrimary = true;
                banner.IsActive = true;
                banner.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting primary banner service");
                return false;
            }
        }

        #endregion

        #region BannerHomepage Methods

        public async Task<List<BannerHomepageListViewModel>> GetAllBannerHomepagesAsync()
        {
            try
            {
                var banners = await _context.BannerHomepages
                    .OrderBy(b => b.DisplayOrder)
                    .ThenBy(b => b.CreatedAt)
                    .Select(b => new BannerHomepageListViewModel
                    {
                        BannerId = b.BannerId,
                        ImgUrl = b.ImgUrl,
                        IsPrimary = b.IsPrimary,
                        DisplayOrder = b.DisplayOrder,
                        Description = b.Description,
                        IsActive = b.IsActive,
                        CreatedAt = b.CreatedAt,
                        UpdatedAt = b.UpdatedAt
                    })
                    .ToListAsync();

                return banners;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all banner homepages");
                return new List<BannerHomepageListViewModel>();
            }
        }

        public async Task<BannerHomepage?> GetBannerHomepageByIdAsync(int id)
        {
            try
            {
                return await _context.BannerHomepages.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting banner homepage by ID: {Id}", id);
                return null;
            }
        }

        public async Task<bool> CreateBannerHomepageAsync(BannerHomepageCreateViewModel model)
        {
            try
            {
                string? imageUrl = null;
                
                if (model.ImageFile != null)
                {
                    imageUrl = await _fileUploadService.UploadBannerImageAsync(model.ImageFile, "homepage");
                    if (string.IsNullOrEmpty(imageUrl))
                    {
                        _logger.LogError("Failed to upload banner homepage image");
                        return false;
                    }
                }
                else if (!string.IsNullOrEmpty(model.ImgUrl))
                {
                    imageUrl = model.ImgUrl;
                }
                else
                {
                    _logger.LogError("No image provided for banner homepage");
                    return false;
                }

                var banner = new BannerHomepage
                {
                    ImgUrl = imageUrl,
                    IsPrimary = model.IsPrimary,
                    DisplayOrder = model.DisplayOrder,
                    Description = model.Description,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                // If this is set as primary, unset other primary banners
                if (model.IsPrimary)
                {
                    var existingPrimary = await _context.BannerHomepages
                        .Where(b => b.IsPrimary && b.IsActive)
                        .FirstOrDefaultAsync();
                    
                    if (existingPrimary != null)
                    {
                        existingPrimary.IsPrimary = false;
                        existingPrimary.UpdatedAt = DateTime.Now;
                    }
                }

                _context.BannerHomepages.Add(banner);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating banner homepage");
                return false;
            }
        }

        public async Task<bool> UpdateBannerHomepageAsync(BannerHomepageEditViewModel model)
        {
            try
            {
                var banner = await _context.BannerHomepages.FindAsync(model.BannerId);
                if (banner == null) return false;

                // Handle image update
                if (model.ImageFile != null)
                {
                    var newImageUrl = await _fileUploadService.UpdateBannerImageAsync(model.ImageFile, banner.ImgUrl, "homepage");
                    if (!string.IsNullOrEmpty(newImageUrl))
                    {
                        banner.ImgUrl = newImageUrl;
                    }
                }
                else if (!string.IsNullOrEmpty(model.ImgUrl))
                {
                    banner.ImgUrl = model.ImgUrl;
                }
                // If no new image and no ImgUrl provided, keep existing ImgUrl

                banner.IsPrimary = model.IsPrimary;
                banner.DisplayOrder = model.DisplayOrder;
                banner.Description = model.Description;
                banner.IsActive = model.IsActive;
                banner.UpdatedAt = DateTime.Now;

                // If this is set as primary, unset other primary banners
                if (model.IsPrimary)
                {
                    var existingPrimary = await _context.BannerHomepages
                        .Where(b => b.IsPrimary && b.IsActive && b.BannerId != model.BannerId)
                        .FirstOrDefaultAsync();
                    
                    if (existingPrimary != null)
                    {
                        existingPrimary.IsPrimary = false;
                        existingPrimary.UpdatedAt = DateTime.Now;
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating banner homepage");
                return false;
            }
        }

        public async Task<bool> DeleteBannerHomepageAsync(int id)
        {
            try
            {
                var banner = await _context.BannerHomepages.FindAsync(id);
                if (banner == null) return false;

                _context.BannerHomepages.Remove(banner);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting banner homepage");
                return false;
            }
        }

        public async Task<bool> ToggleBannerHomepageStatusAsync(int id)
        {
            try
            {
                var banner = await _context.BannerHomepages.FindAsync(id);
                if (banner == null) return false;

                banner.IsActive = !banner.IsActive;
                banner.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling banner homepage status");
                return false;
            }
        }

        public async Task<bool> SetPrimaryBannerHomepageAsync(int id)
        {
            try
            {
                var banner = await _context.BannerHomepages.FindAsync(id);
                if (banner == null) return false;

                // Unset all other primary banners
                var existingPrimary = await _context.BannerHomepages
                    .Where(b => b.IsPrimary && b.IsActive)
                    .ToListAsync();
                
                foreach (var primary in existingPrimary)
                {
                    primary.IsPrimary = false;
                    primary.UpdatedAt = DateTime.Now;
                }

                // Set this banner as primary
                banner.IsPrimary = true;
                banner.IsActive = true;
                banner.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting primary banner homepage");
                return false;
            }
        }

        #endregion

        #region BannerProduct Methods

        public async Task<List<BannerProductListViewModel>> GetAllBannerProductsAsync()
        {
            try
            {
                var banners = await _context.BannerProducts
                    .OrderBy(b => b.DisplayOrder)
                    .ThenBy(b => b.CreatedAt)
                    .Select(b => new BannerProductListViewModel
                    {
                        BannerId = b.BannerId,
                        ImgUrl = b.ImgUrl,
                        IsPrimary = b.IsPrimary,
                        DisplayOrder = b.DisplayOrder,
                        Description = b.Description,
                        IsActive = b.IsActive,
                        CreatedAt = b.CreatedAt,
                        UpdatedAt = b.UpdatedAt
                    })
                    .ToListAsync();

                return banners;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all banner products");
                return new List<BannerProductListViewModel>();
            }
        }

        public async Task<BannerProduct?> GetBannerProductByIdAsync(int id)
        {
            try
            {
                return await _context.BannerProducts.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting banner product by ID: {Id}", id);
                return null;
            }
        }

        public async Task<bool> CreateBannerProductAsync(BannerProductCreateViewModel model)
        {
            try
            {
                string? imageUrl = null;
                
                if (model.ImageFile != null)
                {
                    imageUrl = await _fileUploadService.UploadBannerImageAsync(model.ImageFile, "product");
                    if (string.IsNullOrEmpty(imageUrl))
                    {
                        _logger.LogError("Failed to upload banner product image");
                        return false;
                    }
                }
                else if (!string.IsNullOrEmpty(model.ImgUrl))
                {
                    imageUrl = model.ImgUrl;
                }
                else
                {
                    _logger.LogError("No image provided for banner product");
                    return false;
                }

                var banner = new BannerProduct
                {
                    ImgUrl = imageUrl,
                    IsPrimary = model.IsPrimary,
                    DisplayOrder = model.DisplayOrder,
                    Description = model.Description,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                // If this is set as primary, unset other primary banners
                if (model.IsPrimary)
                {
                    var existingPrimary = await _context.BannerProducts
                        .Where(b => b.IsPrimary && b.IsActive)
                        .FirstOrDefaultAsync();
                    
                    if (existingPrimary != null)
                    {
                        existingPrimary.IsPrimary = false;
                        existingPrimary.UpdatedAt = DateTime.Now;
                    }
                }

                _context.BannerProducts.Add(banner);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating banner product");
                return false;
            }
        }

        public async Task<bool> UpdateBannerProductAsync(BannerProductEditViewModel model)
        {
            try
            {
                var banner = await _context.BannerProducts.FindAsync(model.BannerId);
                if (banner == null) return false;

                // Handle image update
                if (model.ImageFile != null)
                {
                    var newImageUrl = await _fileUploadService.UpdateBannerImageAsync(model.ImageFile, banner.ImgUrl, "product");
                    if (!string.IsNullOrEmpty(newImageUrl))
                    {
                        banner.ImgUrl = newImageUrl;
                    }
                }
                else if (!string.IsNullOrEmpty(model.ImgUrl))
                {
                    banner.ImgUrl = model.ImgUrl;
                }
                // If no new image and no ImgUrl provided, keep existing ImgUrl

                banner.IsPrimary = model.IsPrimary;
                banner.DisplayOrder = model.DisplayOrder;
                banner.Description = model.Description;
                banner.IsActive = model.IsActive;
                banner.UpdatedAt = DateTime.Now;

                // If this is set as primary, unset other primary banners
                if (model.IsPrimary)
                {
                    var existingPrimary = await _context.BannerProducts
                        .Where(b => b.IsPrimary && b.IsActive && b.BannerId != model.BannerId)
                        .FirstOrDefaultAsync();
                    
                    if (existingPrimary != null)
                    {
                        existingPrimary.IsPrimary = false;
                        existingPrimary.UpdatedAt = DateTime.Now;
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating banner product");
                return false;
            }
        }

        public async Task<bool> DeleteBannerProductAsync(int id)
        {
            try
            {
                var banner = await _context.BannerProducts.FindAsync(id);
                if (banner == null) return false;

                // Delete associated image file
                await _fileUploadService.DeleteBannerImageAsync(banner.ImgUrl);

                _context.BannerProducts.Remove(banner);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting banner product");
                return false;
            }
        }

        public async Task<bool> ToggleBannerProductStatusAsync(int id)
        {
            try
            {
                var banner = await _context.BannerProducts.FindAsync(id);
                if (banner == null) return false;

                banner.IsActive = !banner.IsActive;
                banner.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling banner product status");
                return false;
            }
        }

        public async Task<bool> SetPrimaryBannerProductAsync(int id)
        {
            try
            {
                var banner = await _context.BannerProducts.FindAsync(id);
                if (banner == null) return false;

                // Unset all other primary banners
                var existingPrimary = await _context.BannerProducts
                    .Where(b => b.IsPrimary && b.IsActive)
                    .ToListAsync();
                
                foreach (var primary in existingPrimary)
                {
                    primary.IsPrimary = false;
                    primary.UpdatedAt = DateTime.Now;
                }

                // Set this banner as primary
                banner.IsPrimary = true;
                banner.IsActive = true;
                banner.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting primary banner product");
                return false;
            }
        }

        #endregion
    }
}
