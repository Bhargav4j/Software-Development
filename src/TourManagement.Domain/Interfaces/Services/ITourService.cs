using TourManagement.Domain.Entities;

namespace TourManagement.Domain.Interfaces.Services;

public interface ITourService
{
    Task<IEnumerable<Tour>> GetAllToursAsync(CancellationToken cancellationToken = default);
    Task<Tour?> GetTourByIdAsync(int tourId, CancellationToken cancellationToken = default);
    Task<int> CreateTourAsync(Tour tour, CancellationToken cancellationToken = default);
    Task UpdateTourAsync(Tour tour, CancellationToken cancellationToken = default);
    Task DeleteTourAsync(int tourId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tour>> SearchToursAsync(string searchTerm, CancellationToken cancellationToken = default);
}
