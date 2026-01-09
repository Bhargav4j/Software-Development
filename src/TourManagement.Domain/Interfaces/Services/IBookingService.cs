using TourManagement.Domain.Entities;

namespace TourManagement.Domain.Interfaces.Services;

/// <summary>
/// Service interface for Booking operations
/// </summary>
public interface IBookingService
{
    Task<IEnumerable<Booking>> GetAllBookingsAsync(CancellationToken cancellationToken = default);
    Task<Booking?> GetBookingByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Booking>> GetUserBookingsAsync(int userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Booking>> GetTourBookingsAsync(int tourId, CancellationToken cancellationToken = default);
    Task<Booking> CreateBookingAsync(Booking booking, CancellationToken cancellationToken = default);
    Task UpdateBookingAsync(Booking booking, CancellationToken cancellationToken = default);
    Task DeleteBookingAsync(int id, CancellationToken cancellationToken = default);
}
