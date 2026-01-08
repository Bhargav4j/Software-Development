using Microsoft.Extensions.Logging;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Repositories;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Application.Services;

public class TourService : ITourService
{
    private readonly ITourRepository _tourRepository;
    private readonly ILogger<TourService> _logger;

    public TourService(ITourRepository tourRepository, ILogger<TourService> logger)
    {
        _tourRepository = tourRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<Tour>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting all tours");
            return await _tourRepository.GetAllAsync(cancellationToken);
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
            _logger.LogInformation("Getting tour with id {TourId}", id);
            return await _tourRepository.GetByIdAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tour with id {TourId}", id);
            throw;
        }
    }

    public async Task<Tour> CreateAsync(Tour tour, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating tour {TourName}", tour.TourName);
            tour.CreatedDate = DateTime.UtcNow;
            tour.IsActive = true;
            return await _tourRepository.AddAsync(tour, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tour {TourName}", tour.TourName);
            throw;
        }
    }

    public async Task UpdateAsync(int id, Tour tour, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating tour with id {TourId}", id);
            var existingTour = await _tourRepository.GetByIdAsync(id, cancellationToken);
            if (existingTour == null)
            {
                throw new InvalidOperationException($"Tour with id {id} not found");
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tour with id {TourId}", id);
            throw;
        }
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting tour with id {TourId}", id);
            await _tourRepository.DeleteAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tour with id {TourId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Tour>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Searching tours with term {SearchTerm}", searchTerm);
            return await _tourRepository.SearchAsync(searchTerm, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching tours with term {SearchTerm}", searchTerm);
            throw;
        }
    }
}
