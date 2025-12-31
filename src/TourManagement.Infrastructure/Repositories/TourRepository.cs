using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Repositories;
using TourManagement.Infrastructure.Data;

namespace TourManagement.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Tour entity
/// </summary>
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
                .OrderBy(t => t.TourName)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all tours from database");
            throw;
        }
    }

    public async Task<Tour?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Tours
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tour {TourId} from database", id);
            throw;
        }
    }

    public async Task<Tour> AddAsync(Tour tour, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Tours.Add(tour);
            await _context.SaveChangesAsync(cancellationToken);
            return tour;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding tour to database");
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
            _logger.LogError(ex, "Error updating tour {TourId} in database", tour.Id);
            throw;
        }
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var tour = await _context.Tours.FindAsync(new object[] { id }, cancellationToken);
            if (tour != null)
            {
                tour.IsActive = false;
                tour.ModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tour {TourId} from database", id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Tours
                .AnyAsync(t => t.Id == id && t.IsActive, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if tour {TourId} exists", id);
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
                .OrderBy(t => t.TourName)
                .ToListAsync(cancellationToken);
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
            return await _context.Tours
                .AsNoTracking()
                .Where(t => t.IsActive)
                .OrderBy(t => t.TourName)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active tours");
            throw;
        }
    }
}
