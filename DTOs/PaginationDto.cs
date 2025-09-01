using System.ComponentModel.DataAnnotations;

namespace MonAmourDb_BE.DTOs
{
    public class PaginationDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page phải lớn hơn 0")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "PageSize phải từ 1 đến 100")]
        public int PageSize { get; set; } = 10;
    }

}
