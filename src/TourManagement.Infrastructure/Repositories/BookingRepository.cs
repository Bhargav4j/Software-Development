using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Repositories;
using TourManagement.Infrastructure.Data;

namespace TourManagement.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Booking entity
/// </summary>
public class BookingRepository : IBookingRepository
{
    private readonly TourManagementDbContext _context;
    private readonly ILogger<BookingRepository> _logger;

    public BookingRepository(TourManagementDbContext context, ILogger<BookingRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<Booking>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Bookings
                .Include(b => b.Tour)
                .Include(b => b.User)
                .Where(b => b.IsActive)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all bookings");
            throw;
        }
    }

    public async Task<Booking?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Bookings
                .Include(b => b.Tour)
                .Include(b => b.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BookingId == id && b.IsActive, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving booking by ID: {BookingId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Booking>> GetByUserEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Bookings
                .Include(b => b.Tour)
                .Include(b => b.User)
                .Where(b => b.Email == email && b.IsActive)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving bookings for user: {Email}", email);
            throw;
        }
    }

    public async Task AddAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Bookings.AddAsync(booking, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding booking for user: {Email}", booking.Email);
            throw;
        }
    }

    public async Task UpdateAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating booking: {BookingId}", booking.BookingId);
            throw;
        }
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var booking = await _context.Bookings.FindAsync(new object[] { id }, cancellationToken);
            if (booking != null)
            {
                booking.IsActive = false;
                booking.ModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting booking: {BookingId}", id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Bookings
                .AnyAsync(b => b.BookingId == id && b.IsActive, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if booking exists: {BookingId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Booking>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Bookings
                .Include(b => b.Tour)
                .Include(b => b.User)
                .Where(b => b.IsActive && (
                    b.Email.Contains(searchTerm) ||
                    b.FirstName.Contains(searchTerm) ||
                    b.TourName.Contains(searchTerm)))
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching bookings with term: {SearchTerm}", searchTerm);
            throw;
        }
    }
}
