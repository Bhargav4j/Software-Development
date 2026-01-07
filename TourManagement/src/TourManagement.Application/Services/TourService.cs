using Microsoft.Extensions.Logging;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Exceptions;
using TourManagement.Domain.Interfaces.Repositories;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Application.Services;

/// <summary>
/// Service implementation for Tour business logic
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

    public async Task<IEnumerable<Tour>> GetAllAsync(CancellationToken cancellationToken = default)
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

    public async Task<Tour?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving tour with ID {TourId}", id);
            var tour = await _tourRepository.GetByIdAsync(id, cancellationToken);

            if (tour == null)
            {
                _logger.LogWarning("Tour with ID {TourId} not found", id);
            }

            return tour;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tour with ID {TourId}", id);
            throw;
        }
    }

    public async Task<Tour> CreateAsync(Tour tour, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating new tour: {TourName}", tour.TourName);

            tour.CreatedDate = DateTime.UtcNow;
            tour.IsActive = true;

            var createdTour = await _tourRepository.AddAsync(tour, cancellationToken);
            _logger.LogInformation("Successfully created tour with ID {TourId}", createdTour.Id);

            return createdTour;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tour: {TourName}", tour.TourName);
            throw;
        }
    }

    public async Task UpdateAsync(int id, Tour tour, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating tour with ID {TourId}", id);

            var existingTour = await _tourRepository.GetByIdAsync(id, cancellationToken);
            if (existingTour == null)
            {
                throw new NotFoundException(nameof(Tour), id);
            }

            existingTour.TourName = tour.TourName;
            existingTour.Place = tour.Place;
            existingTour.Days = tour.Days;
            existingTour.Price = tour.Price;
            existingTour.Locations = tour.Locations;
            existingTour.TourInfo = tour.TourInfo;
            existingTour.PicturePath = tour.PicturePath;
            existingTour.ModifiedDate = DateTime.UtcNow;
            existingTour.ModifiedBy = tour.ModifiedBy;

            await _tourRepository.UpdateAsync(existingTour, cancellationToken);
            _logger.LogInformation("Successfully updated tour with ID {TourId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tour with ID {TourId}", id);
            throw;
        }
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting tour with ID {TourId}", id);

            var exists = await _tourRepository.ExistsAsync(id, cancellationToken);
            if (!exists)
            {
                throw new NotFoundException(nameof(Tour), id);
            }

            await _tourRepository.DeleteAsync(id, cancellationToken);
            _logger.LogInformation("Successfully deleted tour with ID {TourId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tour with ID {TourId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Tour>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Searching tours with term: {SearchTerm}", searchTerm);
            var tours = await _tourRepository.SearchAsync(searchTerm, cancellationToken);
            _logger.LogInformation("Found {Count} tours matching '{SearchTerm}'", tours.Count(), searchTerm);
            return tours;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching tours with term: {SearchTerm}", searchTerm);
            throw;
        }
    }
}
