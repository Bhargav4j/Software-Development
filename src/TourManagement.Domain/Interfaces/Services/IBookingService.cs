using TourManagement.Domain.Entities;

namespace TourManagement.Domain.Interfaces.Services;

/// <summary>
/// Service interface for Booking operations
/// </summary>
public interface IBookingService
{
    Task<IEnumerable<Booking>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Booking?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Booking>> GetByUserEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Booking> CreateAsync(Booking booking, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, Booking booking, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Booking>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
}
