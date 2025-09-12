using System.ComponentModel.DataAnnotations;

namespace MonAmour.ViewModels
{
    public class LocationSearchViewModel
    {
        public string? SearchTerm { get; set; }
        public string? Status { get; set; } // "Active", "Inactive"
        public string? City { get; set; }
        public int? PartnerId { get; set; }
        public string? SortBy { get; set; } // "name", "city", "conceptCount", "id"
        public string? SortOrder { get; set; } // "asc", "desc"
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
