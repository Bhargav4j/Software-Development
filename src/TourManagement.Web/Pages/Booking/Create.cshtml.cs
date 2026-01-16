using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Web.Pages.Booking;

[Authorize]
public class CreateBookingModel : PageModel
{
    private readonly IBookingService _bookingService;
    private readonly ITourService _tourService;
    private readonly ILogger<CreateBookingModel> _logger;

    public CreateBookingModel(
        IBookingService bookingService,
        ITourService tourService,
        ILogger<CreateBookingModel> logger)
    {
        _bookingService = bookingService;
        _tourService = tourService;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public int TourId { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public Domain.Entities.Tour? Tour { get; set; }
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        public int TourId { get; set; }
        public string TourName { get; set; } = string.Empty;
        public string Place { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            Tour = await _tourService.GetByIdAsync(TourId);

            if (Tour == null)
            {
                _logger.LogWarning("Tour not found for booking: {TourId}", TourId);
                return NotFound();
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var firstName = User.FindFirstValue("FirstName");

            Input.TourId = Tour.TourId;
            Input.TourName = Tour.TourName;
            Input.Place = Tour.Place;
            Input.Email = userEmail ?? string.Empty;
            Input.FirstName = firstName ?? string.Empty;

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading booking page for tour: {TourId}", TourId);
            return RedirectToPage("/Error");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Tour = await _tourService.GetByIdAsync(Input.TourId);
            return Page();
        }

        try
        {
            var booking = new Domain.Entities.Booking
            {
                TourId = Input.TourId,
                TourName = Input.TourName,
                Place = Input.Place,
                Email = Input.Email,
                FirstName = Input.FirstName,
                BookingDate = DateTime.UtcNow,
                Status = "Pending",
                CreatedBy = Input.Email,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            await _bookingService.CreateAsync(booking);

            _logger.LogInformation("Booking created successfully for user: {Email}, tour: {TourId}", Input.Email, Input.TourId);

            return RedirectToPage("/Booking/MyBookings");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking for user: {Email}, tour: {TourId}", Input.Email, Input.TourId);
            ErrorMessage = "An error occurred while creating your booking. Please try again.";
            Tour = await _tourService.GetByIdAsync(Input.TourId);
            return Page();
        }
    }
}
