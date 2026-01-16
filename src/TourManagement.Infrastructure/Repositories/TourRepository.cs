using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Repositories;
using TourManagement.Infrastructure.Data;

namespace TourManagement.Infrastructure.Repositories;

public class TourRepository : ITourRepository
{
    private readonly TourManagementDbContext _context;
    private readonly ILogger<TourRepository> _logger;

    public TourRepository(TourManagementDbContext context, ILogger<TourRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Tour>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Tours
                .AsNoTracking()
                .Where(t => t.IsActive)
                .Include(t => t.Bookings)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all tours from database");
            throw;
        }
    }

    public async Task<Tour?> GetByIdAsync(int tourId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Tours
                .AsNoTracking()
                .Include(t => t.Bookings)
                .FirstOrDefaultAsync(t => t.TourId == tourId && t.IsActive, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tour with ID: {TourId}", tourId);
            throw;
        }
    }

    public async Task<int> AddAsync(Tour tour, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Tours.AddAsync(tour, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return tour.TourId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding tour to database: {TourName}", tour.TourName);
            throw;
        }
    }

    public async Task UpdateAsync(Tour tour, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Tours.Update(tour);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tour in database: {TourId}", tour.TourId);
            throw;
        }
    }

    public async Task DeleteAsync(int tourId, CancellationToken cancellationToken = default)
    {
        try
        {
            var tour = await _context.Tours.FirstOrDefaultAsync(t => t.TourId == tourId, cancellationToken);
            if (tour != null)
            {
                tour.IsActive = false;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tour from database: {TourId}", tourId);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(int tourId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Tours.AnyAsync(t => t.TourId == tourId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if tour exists: {TourId}", tourId);
            throw;
        }
    }

    public async Task<IEnumerable<Tour>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Tours
                .AsNoTracking()
                .Where(t => t.IsActive &&
                    (t.TourName.Contains(searchTerm) ||
                     t.Place.Contains(searchTerm) ||
                     t.Locations.Contains(searchTerm)))
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching tours with term: {SearchTerm}", searchTerm);
            throw;
        }
    }
}
