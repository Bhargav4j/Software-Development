using Microsoft.Extensions.Logging;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Repositories;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Application.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ILogger<BookingService> _logger;

    public BookingService(IBookingRepository bookingRepository, ILogger<BookingService> logger)
    {
        _bookingRepository = bookingRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<Booking>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting all bookings");
            return await _bookingRepository.GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all bookings");
            throw;
        }
    }

    public async Task<Booking?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting booking with id {BookingId}", id);
            return await _bookingRepository.GetByIdAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting booking with id {BookingId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Booking>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting bookings for user {UserId}", userId);
            return await _bookingRepository.GetByUserIdAsync(userId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bookings for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<Booking>> GetByTourIdAsync(int tourId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting bookings for tour {TourId}", tourId);
            return await _bookingRepository.GetByTourIdAsync(tourId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bookings for tour {TourId}", tourId);
            throw;
        }
    }

    public async Task<Booking> CreateAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating booking for user {UserId} and tour {TourId}", booking.UserId, booking.TourId);
            booking.CreatedDate = DateTime.UtcNow;
            booking.IsActive = true;
            booking.BookingDate = DateTime.UtcNow;
            return await _bookingRepository.AddAsync(booking, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking for user {UserId} and tour {TourId}", booking.UserId, booking.TourId);
            throw;
        }
    }

    public async Task UpdateAsync(int id, Booking booking, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating booking with id {BookingId}", id);
            var existingBooking = await _bookingRepository.GetByIdAsync(id, cancellationToken);
            if (existingBooking == null)
            {
                throw new InvalidOperationException($"Booking with id {id} not found");
            }

            existingBooking.NumberOfPeople = booking.NumberOfPeople;
            existingBooking.TotalAmount = booking.TotalAmount;
            existingBooking.Status = booking.Status;
            existingBooking.ModifiedDate = DateTime.UtcNow;
            existingBooking.ModifiedBy = booking.ModifiedBy;

            await _bookingRepository.UpdateAsync(existingBooking, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating booking with id {BookingId}", id);
            throw;
        }
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting booking with id {BookingId}", id);
            await _bookingRepository.DeleteAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting booking with id {BookingId}", id);
            throw;
        }
    }
}
