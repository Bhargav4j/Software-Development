using Microsoft.Extensions.Logging;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Exceptions;
using TourManagement.Domain.Interfaces.Repositories;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Application.Services;

public class TourServiceImpl : ITourService
{
    private readonly ITourRepository _tourRepository;
    private readonly ILogger<TourServiceImpl> _logger;

    public TourServiceImpl(ITourRepository tourRepository, ILogger<TourServiceImpl> logger)
    {
        _tourRepository = tourRepository;
        _logger = logger;
    }

    public Task<IEnumerable<Tour>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _tourRepository.GetAllAsync(cancellationToken);

    public Task<Tour?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _tourRepository.GetByIdAsync(id, cancellationToken);

    public Task<Tour> CreateAsync(Tour tour, CancellationToken cancellationToken = default) =>
        _tourRepository.AddAsync(tour, cancellationToken);

    public Task UpdateAsync(Tour tour, CancellationToken cancellationToken = default) =>
        _tourRepository.UpdateAsync(tour, cancellationToken);

    public Task DeleteAsync(int id, CancellationToken cancellationToken = default) =>
        _tourRepository.DeleteAsync(id, cancellationToken);

    public Task<IEnumerable<Tour>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default) =>
        _tourRepository.SearchAsync(searchTerm, cancellationToken);

    public Task<IEnumerable<Tour>> GetActiveToursAsync(CancellationToken cancellationToken = default) =>
        _tourRepository.GetActiveToursAsync(cancellationToken);
}
