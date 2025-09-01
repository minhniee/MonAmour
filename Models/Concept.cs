using System;
using System.Collections.Generic;

namespace MonAmourDb_BE.Models;

public partial class Concept
{
    public int ConceptId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public int? LocationId { get; set; }

    public int? ColorId { get; set; }

    public int? CategoryId { get; set; }

    public int? AmbienceId { get; set; }

    public int? PreparationTime { get; set; }

    public bool? AvailabilityStatus { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ConceptAmbience? Ambience { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ConceptCategory? Category { get; set; }

    public virtual ConceptColor? Color { get; set; }

    public virtual ICollection<ConceptImg> ConceptImgs { get; set; } = new List<ConceptImg>();

    public virtual Location? Location { get; set; }

    public virtual ICollection<WishList> WishLists { get; set; } = new List<WishList>();
}
