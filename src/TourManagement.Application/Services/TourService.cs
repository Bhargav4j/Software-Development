using Microsoft.Extensions.Logging;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Repositories;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Application.Services;

public class TourService : ITourService
{
    private readonly ITourRepository _repository;
    private readonly ILogger<TourService> _logger;

    public TourService(ITourRepository repository, ILogger<TourService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<Tour>> GetAllToursAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all tours");
            return await _repository.GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all tours");
            throw;
        }
    }

    public async Task<Tour?> GetTourByIdAsync(int tourId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving tour with ID: {TourId}", tourId);
            return await _repository.GetByIdAsync(tourId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tour with ID: {TourId}", tourId);
            throw;
        }
    }

    public async Task<int> CreateTourAsync(Tour tour, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating new tour: {TourName}", tour.TourName);

            tour.CreatedDate = DateTime.UtcNow;
            tour.IsActive = true;

            var tourId = await _repository.AddAsync(tour, cancellationToken);
            _logger.LogInformation("Tour created successfully with ID: {TourId}", tourId);
            return tourId;
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
            _logger.LogInformation("Updating tour with ID: {TourId}", tour.TourId);

            var existingTour = await _repository.GetByIdAsync(tour.TourId, cancellationToken);
            if (existingTour == null)
            {
                _logger.LogWarning("Tour not found with ID: {TourId}", tour.TourId);
                throw new InvalidOperationException($"Tour with ID {tour.TourId} not found");
            }

            tour.ModifiedDate = DateTime.UtcNow;
            await _repository.UpdateAsync(tour, cancellationToken);
            _logger.LogInformation("Tour updated successfully: {TourId}", tour.TourId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tour with ID: {TourId}", tour.TourId);
            throw;
        }
    }

    public async Task DeleteTourAsync(int tourId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting tour with ID: {TourId}", tourId);

            if (!await _repository.ExistsAsync(tourId, cancellationToken))
            {
                _logger.LogWarning("Tour not found with ID: {TourId}", tourId);
                throw new InvalidOperationException($"Tour with ID {tourId} not found");
            }

            await _repository.DeleteAsync(tourId, cancellationToken);
            _logger.LogInformation("Tour deleted successfully: {TourId}", tourId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tour with ID: {TourId}", tourId);
            throw;
        }
    }

    public async Task<IEnumerable<Tour>> SearchToursAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Searching tours with term: {SearchTerm}", searchTerm);
            return await _repository.SearchAsync(searchTerm, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching tours with term: {SearchTerm}", searchTerm);
            throw;
        }
    }
}
