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
        return await _context.Tours
            .Where(t => t.IsActive)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Tour?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Tours
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && t.IsActive, cancellationToken);
    }

    public async Task<Tour> AddAsync(Tour tour, CancellationToken cancellationToken = default)
    {
        await _context.Tours.AddAsync(tour, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return tour;
    }

    public async Task UpdateAsync(Tour tour, CancellationToken cancellationToken = default)
    {
        _context.Tours.Update(tour);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var tour = await _context.Tours.FindAsync(new object[] { id }, cancellationToken);
        if (tour != null)
        {
            tour.IsActive = false;
            tour.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Tours.AnyAsync(t => t.Id == id && t.IsActive, cancellationToken);
    }

    public async Task<IEnumerable<Tour>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _context.Tours
            .Where(t => t.IsActive && (
                t.TourName.Contains(searchTerm) ||
                t.Place.Contains(searchTerm) ||
                (t.Locations != null && t.Locations.Contains(searchTerm))
            ))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
