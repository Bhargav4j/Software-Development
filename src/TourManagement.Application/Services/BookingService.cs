using Microsoft.Extensions.Logging;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Repositories;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Application.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _repository;
    private readonly ILogger<BookingService> _logger;

    public BookingService(IBookingRepository repository, ILogger<BookingService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<Booking>> GetAllBookingsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all bookings");
            return await _repository.GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all bookings");
            throw;
        }
    }

    public async Task<Booking?> GetBookingByIdAsync(int bookingId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving booking with ID: {BookingId}", bookingId);
            return await _repository.GetByIdAsync(bookingId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving booking with ID: {BookingId}", bookingId);
            throw;
        }
    }

    public async Task<IEnumerable<Booking>> GetBookingsByUserEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving bookings for user: {Email}", email);
            return await _repository.GetByUserEmailAsync(email, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving bookings for user: {Email}", email);
            throw;
        }
    }

    public async Task<int> CreateBookingAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating new booking for tour: {TourId} by user: {Email}", booking.TourId, booking.Email);

            booking.CreatedDate = DateTime.UtcNow;
            booking.BookingDate = DateTime.UtcNow;
            booking.IsActive = true;

            var bookingId = await _repository.AddAsync(booking, cancellationToken);
            _logger.LogInformation("Booking created successfully with ID: {BookingId}", bookingId);
            return bookingId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking for tour: {TourId}", booking.TourId);
            throw;
        }
    }

    public async Task UpdateBookingAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating booking with ID: {BookingId}", booking.BookingId);

            var existingBooking = await _repository.GetByIdAsync(booking.BookingId, cancellationToken);
            if (existingBooking == null)
            {
                _logger.LogWarning("Booking not found with ID: {BookingId}", booking.BookingId);
                throw new InvalidOperationException($"Booking with ID {booking.BookingId} not found");
            }

            booking.ModifiedDate = DateTime.UtcNow;
            await _repository.UpdateAsync(booking, cancellationToken);
            _logger.LogInformation("Booking updated successfully: {BookingId}", booking.BookingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating booking with ID: {BookingId}", booking.BookingId);
            throw;
        }
    }

    public async Task DeleteBookingAsync(int bookingId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting booking with ID: {BookingId}", bookingId);

            if (!await _repository.ExistsAsync(bookingId, cancellationToken))
            {
                _logger.LogWarning("Booking not found with ID: {BookingId}", bookingId);
                throw new InvalidOperationException($"Booking with ID {bookingId} not found");
            }

            await _repository.DeleteAsync(bookingId, cancellationToken);
            _logger.LogInformation("Booking deleted successfully: {BookingId}", bookingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting booking with ID: {BookingId}", bookingId);
            throw;
        }
    }
}
