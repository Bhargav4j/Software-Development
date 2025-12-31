namespace TourManagement.Domain.Entities;

/// <summary>
/// Represents a tour booking in the system
/// </summary>
public class Booking
{
    public int Id { get; set; }
    public int TourId { get; set; }
    public int UserId { get; set; }
    public DateTime BookingDate { get; set; }
    public int NumberOfPeople { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Notes { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? ModifiedBy { get; set; }

    // Navigation properties
    public Tour Tour { get; set; } = null!;
    public User User { get; set; } = null!;
}
