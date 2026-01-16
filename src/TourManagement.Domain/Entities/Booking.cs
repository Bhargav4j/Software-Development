namespace TourManagement.Domain.Entities;

/// <summary>
/// Booking entity
/// </summary>
public class Booking
{
    public int BookingId { get; set; }
    public int TourId { get; set; }
    public string TourName { get; set; } = string.Empty;
    public string Place { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending";
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string CreatedBy { get; set; } = string.Empty;
    public string? ModifiedBy { get; set; }

    // Navigation properties
    public Tour? Tour { get; set; }
    public UserInfo? User { get; set; }
}
