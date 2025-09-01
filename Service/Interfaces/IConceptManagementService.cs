using MonAmourDb_BE.DTOs;

namespace MonAmourDb_BE.Service.Interfaces
{
    public interface IConceptManagementService
    {
        // Concept CRUD
        Task<PaginatedResult<ConceptListDto>> GetConceptsAsync(ConceptFilterDto filter);
        Task<ConceptDetailDto?> GetConceptByIdAsync(int conceptId);
        Task<ConceptDetailDto> CreateConceptAsync(CreateConceptDto createConceptDto);
        Task<ConceptDetailDto?> UpdateConceptAsync(int conceptId, UpdateConceptDto updateConceptDto);
        Task<bool> DeleteConceptAsync(int conceptId);
        Task<bool> ToggleConceptAvailabilityAsync(int conceptId);

        // Concept Images
        Task<List<ConceptImageDto>> GetConceptImagesAsync(int conceptId);
        Task<ConceptImageDto> AddConceptImageAsync(int conceptId, CreateConceptImageDto createImageDto);
        Task<ConceptImageDto?> UpdateConceptImageAsync(int conceptId, int imageId, UpdateConceptImageDto updateImageDto);
        Task<bool> DeleteConceptImageAsync(int conceptId, int imageId);
        Task<bool> SetPrimaryImageAsync(int conceptId, int imageId);
        Task<bool> ReorderImagesAsync(int conceptId, List<int> imageIds);

        // Lookup data
        Task<List<ConceptCategoryDto>> GetConceptCategoriesAsync();
        Task<List<ConceptColorDto>> GetConceptColorsAsync();
        Task<List<ConceptAmbienceDto>> GetConceptAmbiencesAsync();
        Task<List<LocationBasicDto>> GetAvailableLocationsAsync();

        // Statistics
        Task<ConceptStatsDto> GetConceptStatsAsync();
        Task<List<ConceptPopularityDto>> GetPopularConceptsAsync(int limit = 10);
        Task<List<ConceptListDto>> GetRecommendedConceptsAsync(int userId, int limit = 5);

        // Bulk operations
        Task<bool> BulkUpdateAvailabilityAsync(List<int> conceptIds, bool availability);
        Task<bool> BulkUpdateCategoryAsync(List<int> conceptIds, int categoryId);
        Task<bool> BulkDeleteConceptsAsync(List<int> conceptIds);

        // ============ CONCEPT CATEGORY MANAGEMENT ============
        Task<PaginatedResult<ConceptCategoryDto>> GetConceptCategoriesPagedAsync(ConceptCategoryFilterDto filter);
        Task<ConceptCategoryDetailDto?> GetConceptCategoryByIdAsync(int categoryId);
        Task<ConceptCategoryDetailDto> CreateConceptCategoryAsync(CreateConceptCategoryDto createCategoryDto);
        Task<ConceptCategoryDetailDto?> UpdateConceptCategoryAsync(int categoryId, UpdateConceptCategoryDto updateCategoryDto);
        Task<bool> DeleteConceptCategoryAsync(int categoryId);
        Task<bool> ToggleConceptCategoryStatusAsync(int categoryId);

        // ============ CONCEPT COLOR MANAGEMENT ============
        Task<PaginatedResult<ConceptColorDto>> GetConceptColorsPagedAsync(ConceptColorFilterDto filter);
        Task<ConceptColorDetailDto?> GetConceptColorByIdAsync(int colorId);
        Task<ConceptColorDetailDto> CreateConceptColorAsync(CreateConceptColorDto createColorDto);
        Task<ConceptColorDetailDto?> UpdateConceptColorAsync(int colorId, UpdateConceptColorDto updateColorDto);
        Task<bool> DeleteConceptColorAsync(int colorId);

        // ============ CONCEPT AMBIENCE MANAGEMENT ============
        Task<PaginatedResult<ConceptAmbienceDto>> GetConceptAmbiencesPagedAsync(ConceptAmbienceFilterDto filter);
        Task<ConceptAmbienceDetailDto?> GetConceptAmbienceByIdAsync(int ambienceId);
        Task<ConceptAmbienceDetailDto> CreateConceptAmbienceAsync(CreateConceptAmbienceDto createAmbienceDto);
        Task<ConceptAmbienceDetailDto?> UpdateConceptAmbienceAsync(int ambienceId, UpdateConceptAmbienceDto updateAmbienceDto);
        Task<bool> DeleteConceptAmbienceAsync(int ambienceId);
    }
}
