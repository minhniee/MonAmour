using MonAmour.ViewModels;

namespace MonAmour.Services.Interfaces
{
    public interface ILocationService
    {
        // Location CRUD operations
        Task<(List<LocationViewModel> locations, int totalCount)> GetLocationsAsync(LocationSearchViewModel searchModel);
        Task<LocationDetailViewModel?> GetLocationByIdAsync(int id);
        Task<bool> CreateLocationAsync(LocationCreateViewModel model);
        Task<bool> UpdateLocationAsync(LocationEditViewModel model);
        Task<bool> DeleteLocationAsync(int id);
        
        // Location statistics and utilities
        Task<Dictionary<string, int>> GetLocationStatisticsAsync();
        Task<List<object>> GetLocationsForDropdownAsync();
        Task<List<object>> GetLocationsByPartnerAsync(int partnerId);
        Task<bool> ToggleLocationStatusAsync(int id, string status);
    }
}
