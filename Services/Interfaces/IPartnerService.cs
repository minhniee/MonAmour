using MonAmour.ViewModels;

namespace MonAmour.Services.Interfaces
{
    public interface IPartnerService
    {
        // Partner CRUD operations
        Task<(List<PartnerViewModel> partners, int totalCount)> GetPartnersAsync(PartnerSearchViewModel searchModel);
        Task<PartnerDetailViewModel?> GetPartnerByIdAsync(int id);
        Task<bool> CreatePartnerAsync(PartnerCreateViewModel model);
        Task<bool> UpdatePartnerAsync(PartnerEditViewModel model);
        Task<bool> DeletePartnerAsync(int id);
        
        // Partner statistics and utilities
        Task<Dictionary<string, int>> GetPartnerStatisticsAsync();
        Task<List<PartnerDropdownViewModel>> GetPartnersForDropdownAsync();
        Task<bool> TogglePartnerStatusAsync(int id, string status);
    }
}
