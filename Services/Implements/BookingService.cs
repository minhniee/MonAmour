using Microsoft.EntityFrameworkCore;
using MonAmour.Models;
using MonAmour.Services.Interfaces;
using MonAmour.ViewModels;

namespace MonAmour.Services.Implements
{
    public class BookingService : IBookingService
    {
        private readonly MonAmourDbContext _context;
        private readonly ILogger<BookingService> _logger;

        public BookingService(MonAmourDbContext context, ILogger<BookingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<BookingViewModel>> GetAllBookingsAsync()
        {
            try
            {
                var bookings = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Concept)
                        .ThenInclude(c => c.Location)
                    .Include(b => b.Concept)
                        .ThenInclude(c => c.Category)
                    .Include(b => b.Concept)
                        .ThenInclude(c => c.Color)
                    .Include(b => b.Concept)
                        .ThenInclude(c => c.Ambience)
                    .OrderByDescending(b => b.CreatedAt)
                    .Select(b => new BookingViewModel
                    {
                        BookingId = b.BookingId,
                        UserId = b.UserId,
                        UserName = b.User != null ? b.User.Name : null,
                        UserEmail = b.User != null ? b.User.Email : null,
                        ConceptId = b.ConceptId,
                        ConceptName = b.Concept != null ? b.Concept.Name : null,
                        ConceptDescription = b.Concept != null ? b.Concept.Description : null,
                        ConceptPrice = b.Concept != null ? b.Concept.Price : null,
                        BookingDate = b.BookingDate,
                        BookingTime = b.BookingTime,
                        Status = b.Status,
                        PaymentStatus = b.PaymentStatus,
                        ConfirmedAt = b.ConfirmedAt,
                        CancelledAt = b.CancelledAt,
                        TotalPrice = b.TotalPrice,
                        CreatedAt = b.CreatedAt,
                        // UpdatedAt = b.UpdatedAt, // Temporarily disabled
                        LocationName = b.Concept != null && b.Concept.Location != null ? b.Concept.Location.Name : null,
                        LocationAddress = b.Concept != null && b.Concept.Location != null ? b.Concept.Location.Address : null,
                        LocationDistrict = b.Concept != null && b.Concept.Location != null ? b.Concept.Location.District : null,
                        LocationCity = b.Concept != null && b.Concept.Location != null ? b.Concept.Location.City : null,
                        LocationStatus = b.Concept != null && b.Concept.Location != null ? b.Concept.Location.Status : null,
                        LocationGgmapLink = b.Concept != null && b.Concept.Location != null ? b.Concept.Location.GgmapLink : null,
                        CategoryName = b.Concept != null && b.Concept.Category != null ? b.Concept.Category.Name : null,
                        ColorName = b.Concept != null && b.Concept.Color != null ? b.Concept.Color.Name : null,
                        AmbienceName = b.Concept != null && b.Concept.Ambience != null ? b.Concept.Ambience.Name : null
                    })
                    .ToListAsync();

                return bookings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all bookings");
                throw;
            }
        }

        public async Task<BookingViewModel?> GetBookingByIdAsync(int bookingId)
        {
            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Concept)
                        .ThenInclude(c => c.Location)
                    .Include(b => b.Concept)
                        .ThenInclude(c => c.Category)
                    .Include(b => b.Concept)
                        .ThenInclude(c => c.Color)
                    .Include(b => b.Concept)
                        .ThenInclude(c => c.Ambience)
                    .Where(b => b.BookingId == bookingId)
                    .Select(b => new BookingViewModel
                    {
                        BookingId = b.BookingId,
                        UserId = b.UserId,
                        UserName = b.User != null ? b.User.Name : null,
                        UserEmail = b.User != null ? b.User.Email : null,
                        ConceptId = b.ConceptId,
                        ConceptName = b.Concept != null ? b.Concept.Name : null,
                        ConceptDescription = b.Concept != null ? b.Concept.Description : null,
                        ConceptPrice = b.Concept != null ? b.Concept.Price : null,
                        BookingDate = b.BookingDate,
                        BookingTime = b.BookingTime,
                        Status = b.Status,
                        PaymentStatus = b.PaymentStatus,
                        ConfirmedAt = b.ConfirmedAt,
                        CancelledAt = b.CancelledAt,
                        TotalPrice = b.TotalPrice,
                        CreatedAt = b.CreatedAt,
                        // UpdatedAt = b.UpdatedAt, // Temporarily disabled
                        LocationName = b.Concept != null && b.Concept.Location != null ? b.Concept.Location.Name : null,
                        CategoryName = b.Concept != null && b.Concept.Category != null ? b.Concept.Category.Name : null,
                        ColorName = b.Concept != null && b.Concept.Color != null ? b.Concept.Color.Name : null,
                        AmbienceName = b.Concept != null && b.Concept.Ambience != null ? b.Concept.Ambience.Name : null
                    })
                    .FirstOrDefaultAsync();

                return booking;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking by ID: {BookingId}", bookingId);
                throw;
            }
        }

        public async Task<BookingDetailViewModel?> GetBookingDetailAsync(int bookingId)
        {
            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Concept)
                        .ThenInclude(c => c.Location)
                    .Include(b => b.Concept)
                        .ThenInclude(c => c.Category)
                    .Include(b => b.Concept)
                        .ThenInclude(c => c.Color)
                    .Include(b => b.Concept)
                        .ThenInclude(c => c.Ambience)
                    .Include(b => b.PaymentDetails)
                        .ThenInclude(pd => pd.Payment)
                            .ThenInclude(p => p.PaymentMethod)
                    .Where(b => b.BookingId == bookingId)
                    .FirstOrDefaultAsync();

                if (booking == null) return null;

                return new BookingDetailViewModel
                {
                    BookingId = booking.BookingId,
                    UserId = booking.UserId,
                    UserName = booking.User?.Name,
                    UserEmail = booking.User?.Email,
                    UserPhone = booking.User?.Phone,
                    ConceptId = booking.ConceptId,
                    ConceptName = booking.Concept?.Name,
                    ConceptDescription = booking.Concept?.Description,
                    ConceptPrice = booking.Concept?.Price,
                    PreparationTime = booking.Concept?.PreparationTime,
                    BookingDate = booking.BookingDate,
                    BookingTime = booking.BookingTime,
                    Status = booking.Status,
                    PaymentStatus = booking.PaymentStatus,
                    ConfirmedAt = booking.ConfirmedAt,
                    CancelledAt = booking.CancelledAt,
                    TotalPrice = booking.TotalPrice,
                    CreatedAt = booking.CreatedAt,
                    // UpdatedAt = booking.UpdatedAt, // Temporarily disabled
                    LocationName = booking.Concept?.Location?.Name,
                    LocationAddress = booking.Concept?.Location?.Address,
                    LocationDistrict = booking.Concept?.Location?.District,
                    LocationCity = booking.Concept?.Location?.City,
                    LocationStatus = booking.Concept?.Location?.Status,
                    LocationGgmapLink = booking.Concept?.Location?.GgmapLink,
                    CategoryName = booking.Concept?.Category?.Name,
                    ColorName = booking.Concept?.Color?.Name,
                    AmbienceName = booking.Concept?.Ambience?.Name,
                    PaymentDetails = booking.PaymentDetails.Select(pd => new PaymentDetailViewModel
                    {
                        PaymentDetailId = pd.PaymentDetailId,
                        PaymentId = pd.PaymentId,
                        OrderId = pd.OrderId,
                        BookingId = pd.BookingId,
                        Amount = pd.Amount,
                        PaymentMethodName = pd.Payment?.PaymentMethod?.Name,
                        PaymentStatus = pd.Payment?.Status,
                        CreatedAt = pd.Payment?.CreatedAt
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking detail by ID: {BookingId}", bookingId);
                throw;
            }
        }


        public async Task<bool> UpdateBookingAsync(BookingEditViewModel model)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(model.BookingId);
                if (booking == null) return false;

                booking.UserId = model.UserId;
                booking.ConceptId = model.ConceptId;
                booking.BookingDate = model.BookingDate;
                booking.BookingTime = model.BookingTime;
                booking.Status = model.Status;
                booking.PaymentStatus = model.PaymentStatus;
                booking.TotalPrice = model.TotalPrice;
                // booking.UpdatedAt = DateTime.Now; // Temporarily disabled

                // Update confirmation/cancellation dates based on status
                if (model.Status == "confirmed" && booking.ConfirmedAt == null)
                {
                    booking.ConfirmedAt = DateTime.Now;
                }
                else if (model.Status == "cancelled" && booking.CancelledAt == null)
                {
                    booking.CancelledAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated booking with ID: {BookingId}", booking.BookingId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking");
                return false;
            }
        }

        public async Task<bool> DeleteBookingAsync(int bookingId)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(bookingId);
                if (booking == null) return false;

                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted booking with ID: {BookingId}", bookingId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting booking");
                return false;
            }
        }

        public async Task<bool> ToggleBookingStatusAsync(int bookingId)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(bookingId);
                if (booking == null) return false;

                booking.Status = booking.Status == "active" ? "inactive" : "active";
                // booking.UpdatedAt = DateTime.Now; // Temporarily disabled

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling booking status");
                return false;
            }
        }

        public async Task<bool> ConfirmBookingAsync(int bookingId)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(bookingId);
                if (booking == null) return false;

                booking.Status = "confirmed";
                booking.ConfirmedAt = DateTime.Now;
                // booking.UpdatedAt = DateTime.Now; // Temporarily disabled

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming booking");
                return false;
            }
        }

        public async Task<bool> CancelBookingAsync(int bookingId)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(bookingId);
                if (booking == null) return false;

                booking.Status = "cancelled";
                booking.CancelledAt = DateTime.Now;
                // booking.UpdatedAt = DateTime.Now; // Temporarily disabled

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking");
                return false;
            }
        }

        public async Task<bool> UpdateBookingStatusAsync(int bookingId, string status)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(bookingId);
                if (booking == null) return false;

                booking.Status = status;
                // booking.UpdatedAt = DateTime.Now; // Temporarily disabled

                if (status == "confirmed" && booking.ConfirmedAt == null)
                {
                    booking.ConfirmedAt = DateTime.Now;
                }
                else if (status == "cancelled" && booking.CancelledAt == null)
                {
                    booking.CancelledAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking status");
                return false;
            }
        }

        public async Task<BookingStatsViewModel> GetBookingStatsAsync()
        {
            try
            {
                var stats = await _context.Bookings
                    .GroupBy(b => b.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count(), Revenue = g.Sum(b => b.TotalPrice ?? 0) })
                    .ToListAsync();

                var totalBookings = await _context.Bookings.CountAsync();
                var totalRevenue = await _context.Bookings.SumAsync(b => b.TotalPrice ?? 0);

                return new BookingStatsViewModel
                {
                    TotalBookings = totalBookings,
                    PendingBookings = stats.FirstOrDefault(s => s.Status == "pending")?.Count ?? 0,
                    ConfirmedBookings = stats.FirstOrDefault(s => s.Status == "confirmed")?.Count ?? 0,
                    CancelledBookings = stats.FirstOrDefault(s => s.Status == "cancelled")?.Count ?? 0,
                    CompletedBookings = stats.FirstOrDefault(s => s.Status == "completed")?.Count ?? 0,
                    TotalRevenue = totalRevenue,
                    PendingRevenue = stats.FirstOrDefault(s => s.Status == "pending")?.Revenue ?? 0,
                    ConfirmedRevenue = stats.FirstOrDefault(s => s.Status == "confirmed")?.Revenue ?? 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking stats");
                throw;
            }
        }

        public async Task<List<BookingViewModel>> GetBookingsByStatusAsync(string status)
        {
            try
            {
                return await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Concept)
                    .Where(b => b.Status == status)
                    .OrderByDescending(b => b.CreatedAt)
                    .Select(b => new BookingViewModel
                    {
                        BookingId = b.BookingId,
                        UserId = b.UserId,
                        UserName = b.User != null ? b.User.Name : null,
                        UserEmail = b.User != null ? b.User.Email : null,
                        ConceptId = b.ConceptId,
                        ConceptName = b.Concept != null ? b.Concept.Name : null,
                        BookingDate = b.BookingDate,
                        BookingTime = b.BookingTime,
                        Status = b.Status,
                        PaymentStatus = b.PaymentStatus,
                        TotalPrice = b.TotalPrice,
                        CreatedAt = b.CreatedAt
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings by status: {Status}", status);
                throw;
            }
        }

        public async Task<List<BookingViewModel>> GetBookingsByUserAsync(int userId)
        {
            try
            {
                return await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Concept)
                    .Where(b => b.UserId == userId)
                    .OrderByDescending(b => b.CreatedAt)
                    .Select(b => new BookingViewModel
                    {
                        BookingId = b.BookingId,
                        UserId = b.UserId,
                        UserName = b.User != null ? b.User.Name : null,
                        UserEmail = b.User != null ? b.User.Email : null,
                        ConceptId = b.ConceptId,
                        ConceptName = b.Concept != null ? b.Concept.Name : null,
                        BookingDate = b.BookingDate,
                        BookingTime = b.BookingTime,
                        Status = b.Status,
                        PaymentStatus = b.PaymentStatus,
                        TotalPrice = b.TotalPrice,
                        CreatedAt = b.CreatedAt
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings by user: {UserId}", userId);
                throw;
            }
        }

        public async Task<List<BookingViewModel>> GetBookingsByConceptAsync(int conceptId)
        {
            try
            {
                return await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Concept)
                    .Where(b => b.ConceptId == conceptId)
                    .OrderByDescending(b => b.CreatedAt)
                    .Select(b => new BookingViewModel
                    {
                        BookingId = b.BookingId,
                        UserId = b.UserId,
                        UserName = b.User != null ? b.User.Name : null,
                        UserEmail = b.User != null ? b.User.Email : null,
                        ConceptId = b.ConceptId,
                        ConceptName = b.Concept != null ? b.Concept.Name : null,
                        BookingDate = b.BookingDate,
                        BookingTime = b.BookingTime,
                        Status = b.Status,
                        PaymentStatus = b.PaymentStatus,
                        TotalPrice = b.TotalPrice,
                        CreatedAt = b.CreatedAt
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings by concept: {ConceptId}", conceptId);
                throw;
            }
        }

        public async Task<List<UserDropdownViewModel>> GetUsersForDropdownAsync()
        {
            try
            {
                return await _context.Users
                    .Where(u => u.Status == "active")
                    .Select(u => new UserDropdownViewModel
                    {
                        UserId = u.UserId,
                        Name = u.Name ?? "Unknown",
                        Email = u.Email
                    })
                    .OrderBy(u => u.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users for dropdown");
                throw;
            }
        }

        public async Task<List<ConceptDropdownViewModel>> GetConceptsForDropdownAsync()
        {
            try
            {
                return await _context.Concepts
                    .Include(c => c.Location)
                    .Where(c => c.AvailabilityStatus == true)
                    .Select(c => new ConceptDropdownViewModel
                    {
                        ConceptId = c.ConceptId,
                        Name = c.Name ?? "Unknown",
                        Price = c.Price,
                        LocationName = c.Location != null ? c.Location.Name : null
                    })
                    .OrderBy(c => c.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting concepts for dropdown");
                throw;
            }
        }

        public async Task<List<PaymentMethodDropdownViewModel>> GetPaymentMethodsForDropdownAsync()
        {
            try
            {
                return await _context.PaymentMethods
                    .Select(pm => new PaymentMethodDropdownViewModel
                    {
                        PaymentMethodId = pm.PaymentMethodId,
                        Name = pm.Name ?? "Unknown"
                    })
                    .OrderBy(pm => pm.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment methods for dropdown");
                throw;
            }
        }

        public async Task<bool> UpdateBookingPaymentStatusAsync(int bookingId, string paymentStatus)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(bookingId);
                if (booking == null) return false;

                booking.PaymentStatus = paymentStatus;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking payment status {BookingId} to {PaymentStatus}", bookingId, paymentStatus);
                throw;
            }
        }
    }
}
