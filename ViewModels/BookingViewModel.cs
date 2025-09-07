using System.ComponentModel.DataAnnotations;

namespace MonAmour.ViewModels
{
    public class BookingViewModel
    {
        public int BookingId { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public int? ConceptId { get; set; }
        public string? ConceptName { get; set; }
        public string? ConceptDescription { get; set; }
        public decimal? ConceptPrice { get; set; }
        public DateOnly? BookingDate { get; set; }
        public TimeOnly? BookingTime { get; set; }
        public string? Status { get; set; }
        public string? PaymentStatus { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public decimal? TotalPrice { get; set; }
        public DateTime? CreatedAt { get; set; }
        // public DateTime? UpdatedAt { get; set; } // Temporarily disabled
        
        // Related entities
        public string? LocationName { get; set; }
        public string? LocationAddress { get; set; }
        public string? LocationDistrict { get; set; }
        public string? LocationCity { get; set; }
        public string? LocationStatus { get; set; }
        public string? LocationGgmapLink { get; set; }
        public string? CategoryName { get; set; }
        public string? ColorName { get; set; }
        public string? AmbienceName { get; set; }
    }


    public class BookingEditViewModel
    {
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Người dùng là bắt buộc")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Concept là bắt buộc")]
        public int ConceptId { get; set; }

        [Required(ErrorMessage = "Ngày đặt là bắt buộc")]
        public DateOnly BookingDate { get; set; }

        [Required(ErrorMessage = "Giờ đặt là bắt buộc")]
        public TimeOnly BookingTime { get; set; }

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        public string Status { get; set; } = string.Empty;

        [Required(ErrorMessage = "Trạng thái thanh toán là bắt buộc")]
        public string PaymentStatus { get; set; } = string.Empty;

        public decimal? TotalPrice { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
    }

    public class BookingDetailViewModel
    {
        public int BookingId { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? UserPhone { get; set; }
        public int? ConceptId { get; set; }
        public string? ConceptName { get; set; }
        public string? ConceptDescription { get; set; }
        public decimal? ConceptPrice { get; set; }
        public int? PreparationTime { get; set; }
        public DateOnly? BookingDate { get; set; }
        public TimeOnly? BookingTime { get; set; }
        public string? Status { get; set; }
        public string? PaymentStatus { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public decimal? TotalPrice { get; set; }
        public DateTime? CreatedAt { get; set; }
        // public DateTime? UpdatedAt { get; set; } // Temporarily disabled
        
        // Related entities
        public string? LocationName { get; set; }
        public string? LocationAddress { get; set; }
        public string? LocationDistrict { get; set; }
        public string? LocationCity { get; set; }
        public string? LocationStatus { get; set; }
        public string? LocationGgmapLink { get; set; }
        public string? CategoryName { get; set; }
        public string? ColorName { get; set; }
        public string? AmbienceName { get; set; }
        
        // Payment details
        public List<PaymentDetailViewModel> PaymentDetails { get; set; } = new List<PaymentDetailViewModel>();
    }

    public class PaymentDetailViewModel
    {
        public int PaymentDetailId { get; set; }
        public int? PaymentId { get; set; }
        public int? OrderId { get; set; }
        public int? BookingId { get; set; }
        public decimal? Amount { get; set; }
        public string? PaymentMethodName { get; set; }
        public string? PaymentStatus { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }

    public class BookingStatsViewModel
    {
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int CompletedBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal PendingRevenue { get; set; }
        public decimal ConfirmedRevenue { get; set; }
    }

    public class UserDropdownViewModel
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class ConceptDropdownViewModel
    {
        public int ConceptId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public string? LocationName { get; set; }
    }

    public class PaymentMethodDropdownViewModel
    {
        public int PaymentMethodId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
