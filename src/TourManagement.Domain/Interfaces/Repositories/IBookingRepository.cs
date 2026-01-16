using TourManagement.Domain.Entities;

namespace TourManagement.Domain.Interfaces.Repositories;

public interface IBookingRepository
{
    Task<IEnumerable<Booking>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Booking?> GetByIdAsync(int bookingId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Booking>> GetByUserEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<int> AddAsync(Booking booking, CancellationToken cancellationToken = default);
    Task UpdateAsync(Booking booking, CancellationToken cancellationToken = default);
    Task DeleteAsync(int bookingId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int bookingId, CancellationToken cancellationToken = default);
}
