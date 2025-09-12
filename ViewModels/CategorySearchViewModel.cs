using System.ComponentModel.DataAnnotations;

namespace MonAmour.ViewModels
{
    public class CategorySearchViewModel
    {
        [Display(Name = "Từ khóa tìm kiếm")]
        public string? SearchTerm { get; set; }

        [Display(Name = "Trạng thái")]
        public string? Status { get; set; }

        [Display(Name = "Sắp xếp theo")]
        public string? SortBy { get; set; } = "name";

        [Display(Name = "Thứ tự")]
        public string? SortOrder { get; set; } = "asc";

        [Display(Name = "Trang")]
        public int Page { get; set; } = 1;

        [Display(Name = "Số lượng mỗi trang")]
        public int PageSize { get; set; } = 25;
    }
}
