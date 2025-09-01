using System;
using System.Collections.Generic;

namespace MonAmourDb_BE.Models;

public partial class Booking
{
    public int BookingId { get; set; }

    public int? UserId { get; set; }

    public int? ConceptId { get; set; }

    public DateOnly? BookingDate { get; set; }

    public TimeOnly? BookingTime { get; set; }

    public string? Status { get; set; }

    public string? PaymentStatus { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public decimal? TotalPrice { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Concept? Concept { get; set; }

    public virtual ICollection<PaymentDetail> PaymentDetails { get; set; } = new List<PaymentDetail>();

    public virtual User? User { get; set; }
}
