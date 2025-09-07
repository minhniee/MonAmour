using System.ComponentModel.DataAnnotations;

namespace MonAmour.ViewModels
{
    public class CreateBookingRequest
    {
        [Required]
        public int ConceptId { get; set; }
        
        [Required]
        public DateTime BookingDate { get; set; }
        
        [Required]
        public string BookingTime { get; set; } = string.Empty;
    }


}
