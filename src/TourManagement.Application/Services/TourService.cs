using Microsoft.Extensions.Logging;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Repositories;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Application.Services;

/// <summary>
/// Service implementation for Tour operations
/// </summary>
public class TourService : ITourService
{
    private readonly ITourRepository _repository;
    private readonly ILogger<TourService> _logger;

    public TourService(ITourRepository repository, ILogger<TourService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<Tour>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting all tours");
            return await _repository.GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all tours");
            throw;
        }
    }

    public async Task<Tour?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting tour by ID: {TourId}", id);
            return await _repository.GetByIdAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tour by ID: {TourId}", id);
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

            await _repository.AddAsync(tour, cancellationToken);
            _logger.LogInformation("Tour created successfully: {TourName}", tour.TourName);

            return tour;
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
            _logger.LogInformation("Updating tour: {TourId}", id);

            var existingTour = await _repository.GetByIdAsync(id, cancellationToken);
            if (existingTour == null)
            {
                throw new InvalidOperationException($"Tour with ID {id} not found");
            }

            tour.TourId = id;
            tour.ModifiedDate = DateTime.UtcNow;
            await _repository.UpdateAsync(tour, cancellationToken);

            _logger.LogInformation("Tour updated successfully: {TourId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tour: {TourId}", id);
            throw;
        }
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting tour: {TourId}", id);
            await _repository.DeleteAsync(id, cancellationToken);
            _logger.LogInformation("Tour deleted successfully: {TourId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tour: {TourId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Tour>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
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
