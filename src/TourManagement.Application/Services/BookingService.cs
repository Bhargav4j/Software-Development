using Microsoft.Extensions.Logging;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Repositories;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Application.Services;

/// <summary>
/// Booking service implementation with validation and error handling
/// </summary>
public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ITourRepository _tourRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<BookingService> _logger;

    public BookingService(
        IBookingRepository bookingRepository,
        ITourRepository tourRepository,
        IUserRepository userRepository,
        ILogger<BookingService> logger)
    {
        _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
        _tourRepository = tourRepository ?? throw new ArgumentNullException(nameof(tourRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<Booking>> GetAllBookingsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching all bookings");
            var bookings = await _bookingRepository.GetAllAsync(cancellationToken);
            _logger.LogInformation("Successfully fetched {Count} bookings", bookings.Count());
            return bookings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all bookings");
            throw;
        }
    }

    public async Task<Booking?> GetBookingByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching booking with ID {BookingId}", id);
            var booking = await _bookingRepository.GetByIdAsync(id, cancellationToken);

            if (booking == null)
            {
                _logger.LogWarning("Booking with ID {BookingId} not found", id);
            }

            return booking;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching booking with ID {BookingId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Booking>> GetUserBookingsAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching bookings for user ID {UserId}", userId);
            var bookings = await _bookingRepository.GetByUserIdAsync(userId, cancellationToken);
            _logger.LogInformation("Found {Count} bookings for user ID {UserId}", bookings.Count(), userId);
            return bookings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching bookings for user ID {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<Booking>> GetTourBookingsAsync(int tourId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching bookings for tour ID {TourId}", tourId);
            var bookings = await _bookingRepository.GetByTourIdAsync(tourId, cancellationToken);
            _logger.LogInformation("Found {Count} bookings for tour ID {TourId}", bookings.Count(), tourId);
            return bookings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching bookings for tour ID {TourId}", tourId);
            throw;
        }
    }

    public async Task<Booking> CreateBookingAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(booking);

            _logger.LogInformation("Creating new booking for tour ID {TourId} and user ID {UserId}",
                booking.TourId, booking.UserId);

            var tourExists = await _tourRepository.ExistsAsync(booking.TourId, cancellationToken);
            if (!tourExists)
            {
                throw new InvalidOperationException($"Tour with ID {booking.TourId} not found");
            }

            var userExists = await _userRepository.ExistsAsync(booking.UserId, cancellationToken);
            if (!userExists)
            {
                throw new InvalidOperationException($"User with ID {booking.UserId} not found");
            }

            booking.BookingDate = DateTime.UtcNow;
            booking.CreatedDate = DateTime.UtcNow;
            booking.IsActive = true;
            booking.CreatedBy = "System";

            var createdBooking = await _bookingRepository.AddAsync(booking, cancellationToken);
            _logger.LogInformation("Successfully created booking with ID {BookingId}", createdBooking.BookingId);

            return createdBooking;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking for tour ID {TourId} and user ID {UserId}",
                booking?.TourId, booking?.UserId);
            throw;
        }
    }

    public async Task UpdateBookingAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(booking);

            _logger.LogInformation("Updating booking with ID {BookingId}", booking.BookingId);

            var existingBooking = await _bookingRepository.GetByIdAsync(booking.BookingId, cancellationToken);
            if (existingBooking == null)
            {
                _logger.LogWarning("Booking with ID {BookingId} not found for update", booking.BookingId);
                throw new InvalidOperationException($"Booking with ID {booking.BookingId} not found");
            }

            booking.ModifiedDate = DateTime.UtcNow;
            booking.ModifiedBy = "System";

            await _bookingRepository.UpdateAsync(booking, cancellationToken);
            _logger.LogInformation("Successfully updated booking with ID {BookingId}", booking.BookingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating booking with ID {BookingId}", booking?.BookingId);
            throw;
        }
    }

    public async Task DeleteBookingAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting booking with ID {BookingId}", id);

            var exists = await _bookingRepository.ExistsAsync(id, cancellationToken);
            if (!exists)
            {
                _logger.LogWarning("Booking with ID {BookingId} not found for deletion", id);
                throw new InvalidOperationException($"Booking with ID {id} not found");
            }

            await _bookingRepository.DeleteAsync(id, cancellationToken);
            _logger.LogInformation("Successfully deleted booking with ID {BookingId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting booking with ID {BookingId}", id);
            throw;
        }
    }
}
