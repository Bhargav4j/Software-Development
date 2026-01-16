using TourManagement.Domain.Entities;

namespace TourManagement.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Tour entity
/// </summary>
public interface ITourRepository
{
    Task<IEnumerable<Tour>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Tour?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(Tour tour, CancellationToken cancellationToken = default);
    Task UpdateAsync(Tour tour, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tour>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
}
