using AutoMapper;
using TourManagement.Application.DTOs;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Application.Services;

public class BookingFacade
{
    private readonly IBookingService _bookingService;
    private readonly IMapper _mapper;

    public BookingFacade(IBookingService bookingService, IMapper mapper)
    {
        _bookingService = bookingService;
        _mapper = mapper;
    }

    public async Task<IEnumerable<BookingDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var bookings = await _bookingService.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<BookingDto>>(bookings);
    }

    public async Task<BookingDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var booking = await _bookingService.GetByIdAsync(id, cancellationToken);
        return _mapper.Map<BookingDto?>(booking);
    }

    public async Task<IEnumerable<BookingDto>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var bookings = await _bookingService.GetByUserIdAsync(userId, cancellationToken);
        return _mapper.Map<IEnumerable<BookingDto>>(bookings);
    }

    public async Task<IEnumerable<BookingDto>> GetByTourIdAsync(int tourId, CancellationToken cancellationToken = default)
    {
        var bookings = await _bookingService.GetByTourIdAsync(tourId, cancellationToken);
        return _mapper.Map<IEnumerable<BookingDto>>(bookings);
    }

    public async Task<BookingDto> CreateAsync(BookingCreateDto createDto, CancellationToken cancellationToken = default)
    {
        var booking = _mapper.Map<Booking>(createDto);
        var createdBooking = await _bookingService.CreateAsync(booking, cancellationToken);
        return _mapper.Map<BookingDto>(createdBooking);
    }

    public async Task UpdateAsync(int id, BookingUpdateDto updateDto, CancellationToken cancellationToken = default)
    {
        var booking = _mapper.Map<Booking>(updateDto);
        booking.Id = id;
        await _bookingService.UpdateAsync(booking, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await _bookingService.DeleteAsync(id, cancellationToken);
    }
}
