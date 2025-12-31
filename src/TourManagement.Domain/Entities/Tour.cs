namespace TourManagement.Domain.Entities;

/// <summary>
/// Represents a tour package entity
/// </summary>
public class Tour
{
    public int TourId { get; set; }
    public string TourName { get; set; } = string.Empty;
    public string Place { get; set; } = string.Empty;
    public int Days { get; set; }
    public string Locations { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string TourInfo { get; set; } = string.Empty;
    public string? PicturePath { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? ModifiedBy { get; set; }

    // Navigation properties
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
