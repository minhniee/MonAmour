using System.ComponentModel.DataAnnotations;

namespace MonAmour.ViewModels
{
    public class ConceptSearchViewModel
    {
        public string? SearchTerm { get; set; }
        public string? Status { get; set; } // "available", "unavailable"
        public int? LocationId { get; set; }
        public int? CategoryId { get; set; }
        public int? AmbienceId { get; set; }
        public List<int>? ColorIds { get; set; }
        public bool? AvailabilityStatus { get; set; }
        public string? SortBy { get; set; } // "name", "price", "createdAt", "id"
        public string? SortOrder { get; set; } // "asc", "desc"
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
