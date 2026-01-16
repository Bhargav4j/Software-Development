using TourManagement.Domain.Entities;

namespace TourManagement.Domain.Interfaces.Repositories;

public interface ITourRepository
{
    Task<IEnumerable<Tour>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Tour?> GetByIdAsync(int tourId, CancellationToken cancellationToken = default);
    Task<int> AddAsync(Tour tour, CancellationToken cancellationToken = default);
    Task UpdateAsync(Tour tour, CancellationToken cancellationToken = default);
    Task DeleteAsync(int tourId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int tourId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tour>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
}
