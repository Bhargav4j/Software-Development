using TourManagement.Domain.Entities;

namespace TourManagement.Domain.Interfaces.Services;

public interface IBookingService
{
    Task<IEnumerable<Booking>> GetAllBookingsAsync(CancellationToken cancellationToken = default);
    Task<Booking?> GetBookingByIdAsync(int bookingId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Booking>> GetBookingsByUserEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<int> CreateBookingAsync(Booking booking, CancellationToken cancellationToken = default);
    Task UpdateBookingAsync(Booking booking, CancellationToken cancellationToken = default);
    Task DeleteBookingAsync(int bookingId, CancellationToken cancellationToken = default);
}
