namespace TourManagement.Domain.Entities;

/// <summary>
/// Booking entity representing tour bookings
/// </summary>
public class Booking
{
    public int Id { get; set; }
    public int TourId { get; set; }
    public int UserId { get; set; }
    public DateTime BookingDate { get; set; }
    public int NumberOfPersons { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; }
    public string CreatedBy { get; set; } = "System";
    public string? ModifiedBy { get; set; }

    public Tour Tour { get; set; } = null!;
    public User User { get; set; } = null!;
}
