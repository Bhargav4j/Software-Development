using Microsoft.Extensions.Logging;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Exceptions;
using TourManagement.Domain.Interfaces.Repositories;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Application.Services;

/// <summary>
/// Service implementation for Tour business operations
/// </summary>
public class TourService : ITourService
{
    private readonly ITourRepository _tourRepository;
    private readonly ILogger<TourService> _logger;

    public TourService(ITourRepository tourRepository, ILogger<TourService> logger)
    {
        _tourRepository = tourRepository ?? throw new ArgumentNullException(nameof(tourRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<Tour>> GetAllToursAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all tours");
            var tours = await _tourRepository.GetAllAsync(cancellationToken);
            _logger.LogInformation("Retrieved {Count} tours", tours.Count());
            return tours;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all tours");
            throw;
        }
    }

    public async Task<Tour?> GetTourByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving tour with id {TourId}", id);
            var tour = await _tourRepository.GetByIdAsync(id, cancellationToken);

            if (tour == null)
            {
                _logger.LogWarning("Tour with id {TourId} not found", id);
            }

            return tour;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tour with id {TourId}", id);
            throw;
        }
    }

    public async Task<Tour> CreateTourAsync(Tour tour, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating new tour: {TourName}", tour.TourName);

            tour.CreatedDate = DateTime.UtcNow;
            tour.IsActive = true;

            var createdTour = await _tourRepository.AddAsync(tour, cancellationToken);
            _logger.LogInformation("Tour created successfully with id {TourId}", createdTour.Id);

            return createdTour;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tour: {TourName}", tour.TourName);
            throw;
        }
    }

    public async Task UpdateTourAsync(Tour tour, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating tour with id {TourId}", tour.Id);

            var existingTour = await _tourRepository.GetByIdAsync(tour.Id, cancellationToken);
            if (existingTour == null)
            {
                throw new NotFoundException(nameof(Tour), tour.Id);
            }

            tour.ModifiedDate = DateTime.UtcNow;
            await _tourRepository.UpdateAsync(tour, cancellationToken);

            _logger.LogInformation("Tour with id {TourId} updated successfully", tour.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tour with id {TourId}", tour.Id);
            throw;
        }
    }

    public async Task DeleteTourAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting tour with id {TourId}", id);

            var existingTour = await _tourRepository.GetByIdAsync(id, cancellationToken);
            if (existingTour == null)
            {
                throw new NotFoundException(nameof(Tour), id);
            }

            await _tourRepository.DeleteAsync(id, cancellationToken);
            _logger.LogInformation("Tour with id {TourId} deleted successfully", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tour with id {TourId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Tour>> SearchToursAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Searching tours with term: {SearchTerm}", searchTerm);
            var tours = await _tourRepository.SearchAsync(searchTerm, cancellationToken);
            _logger.LogInformation("Found {Count} tours matching search term", tours.Count());
            return tours;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching tours with term: {SearchTerm}", searchTerm);
            throw;
        }
    }

    public async Task<IEnumerable<Tour>> GetActiveToursAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving active tours");
            var tours = await _tourRepository.GetActiveToursAsync(cancellationToken);
            _logger.LogInformation("Retrieved {Count} active tours", tours.Count());
            return tours;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active tours");
            throw;
        }
    }
}
