using MonAmour.ViewModels;

namespace MonAmour.Services.Interfaces
{
    public interface IBookingService
    {
        // Booking CRUD
        Task<List<BookingViewModel>> GetAllBookingsAsync();
        Task<BookingViewModel?> GetBookingByIdAsync(int bookingId);
        Task<BookingDetailViewModel?> GetBookingDetailAsync(int bookingId);
        Task<bool> UpdateBookingAsync(BookingEditViewModel model);
        Task<bool> DeleteBookingAsync(int bookingId);
        Task<bool> ToggleBookingStatusAsync(int bookingId);

        // Booking Status Management
        Task<bool> ConfirmBookingAsync(int bookingId);
        Task<bool> CancelBookingAsync(int bookingId);
        Task<bool> UpdateBookingStatusAsync(int bookingId, string status);
        Task<bool> UpdateBookingPaymentStatusAsync(int bookingId, string paymentStatus);

        // Booking Statistics
        Task<BookingStatsViewModel> GetBookingStatsAsync();
        Task<List<BookingViewModel>> GetBookingsByStatusAsync(string status);
        Task<List<BookingViewModel>> GetBookingsByUserAsync(int userId);
        Task<List<BookingViewModel>> GetBookingsByConceptAsync(int conceptId);

        // Dropdown Data
        Task<List<UserDropdownViewModel>> GetUsersForDropdownAsync();
        Task<List<ConceptDropdownViewModel>> GetConceptsForDropdownAsync();
        Task<List<PaymentMethodDropdownViewModel>> GetPaymentMethodsForDropdownAsync();
    }
}
