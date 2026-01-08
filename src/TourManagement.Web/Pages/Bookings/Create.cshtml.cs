using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Web.Pages.Bookings;

[Authorize]
public class BookingCreateModel : PageModel
{
    private readonly IBookingService _bookingService;
    private readonly ITourService _tourService;
    private readonly ILogger<BookingCreateModel> _logger;

    public BookingCreateModel(IBookingService bookingService, ITourService tourService, ILogger<BookingCreateModel> logger)
    {
        _bookingService = bookingService;
        _tourService = tourService;
        _logger = logger;
    }

    [BindProperty]
    public int TourId { get; set; }

    [BindProperty]
    [Required]
    [Range(1, 100)]
    public int NumberOfPeople { get; set; } = 1;

    public Tour? Tour { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int? tourId)
    {
        if (!tourId.HasValue)
        {
            return RedirectToPage("/Tours/Index");
        }

        TourId = tourId.Value;
        Tour = await _tourService.GetByIdAsync(TourId);

        if (Tour == null)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Tour = await _tourService.GetByIdAsync(TourId);
        if (Tour == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToPage("/Account/Login");
            }

            var userId = int.Parse(userIdClaim.Value);
            var totalAmount = Tour.Price * NumberOfPeople;

            var booking = new Booking
            {
                UserId = userId,
                TourId = TourId,
                NumberOfPeople = NumberOfPeople,
                TotalAmount = totalAmount,
                Status = "Pending",
                CreatedBy = User.Identity?.Name ?? "System"
            };

            await _bookingService.CreateAsync(booking);
            _logger.LogInformation("Booking created for user {UserId} and tour {TourId}", userId, TourId);

            return RedirectToPage("/Bookings/MyBookings");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking for tour {TourId}", TourId);
            ErrorMessage = "An error occurred while creating the booking. Please try again.";
            return Page();
        }
    }
}
