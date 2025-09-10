using MonAmour.ViewModels;

namespace MonAmour.Services.Interfaces
{
    public interface IConceptService
    {
        // Concept CRUD operations
        Task<(List<ConceptViewModel> concepts, int totalCount)> GetConceptsAsync(ConceptSearchViewModel searchModel);
        Task<ConceptDetailViewModel?> GetConceptByIdAsync(int id);
        Task<bool> CreateConceptAsync(ConceptCreateViewModel model);
        Task<bool> UpdateConceptAsync(ConceptEditViewModel model);
        Task<bool> DeleteConceptAsync(int id);
        Task<bool> ToggleConceptStatusAsync(int id, bool status);

        // Concept statistics and utilities
        Task<Dictionary<string, int>> GetConceptStatisticsAsync();
        Task<List<ConceptDropdownViewModel>> GetConceptsForDropdownAsync();
        Task<List<ConceptDropdownViewModel>> SearchConceptsByNameAsync(string searchTerm);

        // Concept Image Management
        Task<List<ConceptImgViewModel>> GetConceptImagesAsync(int conceptId);
        Task<ConceptImgViewModel?> GetConceptImageByIdAsync(int imageId);
        Task<bool> AddConceptImageAsync(ConceptImgViewModel model);
        Task<bool> UpdateConceptImageAsync(ConceptImgViewModel model);
        Task<bool> DeleteConceptImageAsync(int imageId);
        Task<bool> SetPrimaryImageAsync(int conceptId, int imageId);
        Task<int> GetConceptImageCountAsync(int conceptId);
        Task<bool> CanConceptAddMoreImagesAsync(int conceptId);
        Task<List<object>> GetConceptImagesGroupedByConceptAsync();

        // Dropdown data
        Task<List<ConceptCategoryDropdownViewModel>> GetConceptCategoriesForDropdownAsync();
        Task<List<ConceptColorDropdownViewModel>> GetConceptColorsForDropdownAsync();
        Task<List<ConceptAmbienceDropdownViewModel>> GetConceptAmbiencesForDropdownAsync();
        Task<List<LocationDropdownViewModel>> GetLocationsForDropdownAsync();

        // Concept Category Management
        Task<List<ConceptCategoryDropdownViewModel>> GetConceptCategoriesAsync();
        Task<ConceptCategoryDropdownViewModel?> GetConceptCategoryByIdAsync(int id);
        Task<bool> CreateConceptCategoryAsync(ConceptCategoryDropdownViewModel model);
        Task<bool> UpdateConceptCategoryAsync(ConceptCategoryDropdownViewModel model);
        Task<bool> DeleteConceptCategoryAsync(int id);

        // Concept Color Management
        Task<List<ConceptColorDropdownViewModel>> GetConceptColorsAsync();
        Task<ConceptColorDropdownViewModel?> GetConceptColorByIdAsync(int id);
        Task<bool> CreateConceptColorAsync(ConceptColorDropdownViewModel model);
        Task<bool> UpdateConceptColorAsync(ConceptColorDropdownViewModel model);
        Task<bool> DeleteConceptColorAsync(int id);

        // Concept Ambience Management
        Task<List<ConceptAmbienceDropdownViewModel>> GetConceptAmbiencesAsync();
        Task<ConceptAmbienceDropdownViewModel?> GetConceptAmbienceByIdAsync(int id);
        Task<bool> CreateConceptAmbienceAsync(ConceptAmbienceDropdownViewModel model);
        Task<bool> UpdateConceptAmbienceAsync(ConceptAmbienceDropdownViewModel model);
        Task<bool> DeleteConceptAmbienceAsync(int id);
    }

    public class LocationDropdownViewModel
    {
        public int LocationId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
