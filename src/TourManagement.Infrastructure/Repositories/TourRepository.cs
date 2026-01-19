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
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<Tour>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Tours
                .AsNoTracking()
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.CreatedDate)
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
            _logger.LogError(ex, "Error retrieving tour with id {TourId} from database", id);
            throw;
        }
    }

    public async Task<Tour> AddAsync(Tour tour, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Tours.AddAsync(tour, cancellationToken);
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
            _logger.LogError(ex, "Error updating tour with id {TourId} in database", tour.Id);
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
            _logger.LogError(ex, "Error deleting tour with id {TourId} from database", id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Tours.AnyAsync(t => t.Id == id && t.IsActive, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if tour with id {TourId} exists", id);
            throw;
        }
    }

    public async Task<IEnumerable<Tour>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Tours
                .AsNoTracking()
                .Where(t => t.IsActive);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(t =>
                    t.TourName.ToLower().Contains(searchTerm) ||
                    t.Place.ToLower().Contains(searchTerm) ||
                    t.Locations.ToLower().Contains(searchTerm));
            }

            return await query
                .OrderByDescending(t => t.CreatedDate)
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
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active tours from database");
            throw;
        }
    }
}
