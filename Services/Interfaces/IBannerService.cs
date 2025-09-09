using MonAmour.Models;
using MonAmour.ViewModels;

namespace MonAmour.Services.Interfaces
{
    public interface IBannerService
    {
        // BannerService methods
        Task<List<BannerServiceListViewModel>> GetAllBannerServicesAsync();
        Task<BannerService?> GetBannerServiceByIdAsync(int id);
        Task<bool> CreateBannerServiceAsync(BannerServiceCreateViewModel model);
        Task<bool> UpdateBannerServiceAsync(BannerServiceEditViewModel model);
        Task<bool> DeleteBannerServiceAsync(int id);
        Task<bool> ToggleBannerServiceStatusAsync(int id);
        Task<bool> SetPrimaryBannerServiceAsync(int id);
        Task<bool> IsServiceDisplayOrderExistsAsync(int displayOrder, int? excludeId = null);
        Task<int> GetNextServiceDisplayOrderAsync();

        // BannerHomepage methods
        Task<List<BannerHomepageListViewModel>> GetAllBannerHomepagesAsync();
        Task<BannerHomepage?> GetBannerHomepageByIdAsync(int id);
        Task<bool> CreateBannerHomepageAsync(BannerHomepageCreateViewModel model);
        Task<bool> UpdateBannerHomepageAsync(BannerHomepageEditViewModel model);
        Task<bool> DeleteBannerHomepageAsync(int id);
        Task<bool> ToggleBannerHomepageStatusAsync(int id);
        Task<bool> SetPrimaryBannerHomepageAsync(int id);
        Task<bool> IsDisplayOrderExistsAsync(int displayOrder, int? excludeId = null);
        Task<int> GetNextDisplayOrderAsync();

        // BannerProduct methods
        Task<List<BannerProductListViewModel>> GetAllBannerProductsAsync();
        Task<BannerProduct?> GetBannerProductByIdAsync(int id);
        Task<bool> CreateBannerProductAsync(BannerProductCreateViewModel model);
        Task<bool> UpdateBannerProductAsync(BannerProductEditViewModel model);
        Task<bool> DeleteBannerProductAsync(int id);
        Task<bool> ToggleBannerProductStatusAsync(int id);
        Task<bool> SetPrimaryBannerProductAsync(int id);
        Task<bool> IsProductDisplayOrderExistsAsync(int displayOrder, int? excludeId = null);
        Task<int> GetNextProductDisplayOrderAsync();
    }
}
