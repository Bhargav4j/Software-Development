namespace TourManagement.Application.DTOs;

/// <summary>
/// Data transfer object for Booking entity
/// </summary>
public class BookingDto
{
    public int Id { get; set; }
    public int TourId { get; set; }
    public string TourName { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public int NumberOfPeople { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Notes { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO for creating a new booking
/// </summary>
public class BookingCreateDto
{
    public int TourId { get; set; }
    public int UserId { get; set; }
    public DateTime BookingDate { get; set; }
    public int NumberOfPeople { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating an existing booking
/// </summary>
public class BookingUpdateDto
{
    public DateTime BookingDate { get; set; }
    public int NumberOfPeople { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}
