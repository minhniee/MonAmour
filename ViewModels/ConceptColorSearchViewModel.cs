using System.ComponentModel.DataAnnotations;

namespace MonAmour.ViewModels
{
    public class ConceptColorSearchViewModel
    {
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; } // "name", "code", "id"
        public string? SortOrder { get; set; } // "asc", "desc"
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
