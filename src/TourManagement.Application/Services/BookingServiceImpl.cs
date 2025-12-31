using Microsoft.Extensions.Logging;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Exceptions;
using TourManagement.Domain.Interfaces.Repositories;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Application.Services;

public class BookingServiceImpl : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ITourRepository _tourRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<BookingServiceImpl> _logger;

    public BookingServiceImpl(
        IBookingRepository bookingRepository,
        ITourRepository tourRepository,
        IUserRepository userRepository,
        ILogger<BookingServiceImpl> logger)
    {
        _bookingRepository = bookingRepository;
        _tourRepository = tourRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public Task<IEnumerable<Booking>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _bookingRepository.GetAllAsync(cancellationToken);

    public Task<Booking?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _bookingRepository.GetByIdAsync(id, cancellationToken);

    public Task<IEnumerable<Booking>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default) =>
        _bookingRepository.GetByUserIdAsync(userId, cancellationToken);

    public Task<IEnumerable<Booking>> GetByTourIdAsync(int tourId, CancellationToken cancellationToken = default) =>
        _bookingRepository.GetByTourIdAsync(tourId, cancellationToken);

    public async Task<Booking> CreateAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        var tourExists = await _tourRepository.ExistsAsync(booking.TourId, cancellationToken);
        if (!tourExists)
        {
            throw new EntityNotFoundException(nameof(Tour), booking.TourId);
        }

        var userExists = await _userRepository.ExistsAsync(booking.UserId, cancellationToken);
        if (!userExists)
        {
            throw new EntityNotFoundException(nameof(User), booking.UserId);
        }

        return await _bookingRepository.AddAsync(booking, cancellationToken);
    }

    public Task UpdateAsync(Booking booking, CancellationToken cancellationToken = default) =>
        _bookingRepository.UpdateAsync(booking, cancellationToken);

    public Task DeleteAsync(int id, CancellationToken cancellationToken = default) =>
        _bookingRepository.DeleteAsync(id, cancellationToken);
}
