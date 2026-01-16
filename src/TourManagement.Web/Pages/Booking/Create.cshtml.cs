using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Web.Pages.Booking;

public class CreateModel : PageModel
{
    private readonly IBookingService _bookingService;
    private readonly ITourService _tourService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(IBookingService bookingService, ITourService tourService, ILogger<CreateModel> logger)
    {
        _bookingService = bookingService;
        _tourService = tourService;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public int TourId { get; set; }

    [BindProperty]
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    public Domain.Entities.Tour? Tour { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            Tour = await _tourService.GetTourByIdAsync(TourId, cancellationToken);

            if (Tour == null)
            {
                _logger.LogWarning("Tour not found with ID: {TourId}", TourId);
                return RedirectToPage("/Tour/Index");
            }

            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (!string.IsNullOrEmpty(userEmail))
            {
                Email = userEmail;
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading booking page for tour ID: {TourId}", TourId);
            return RedirectToPage("/Tour/Index");
        }
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            Tour = await _tourService.GetTourByIdAsync(TourId, cancellationToken);
            ErrorMessage = "Please fill in all required fields correctly";
            return Page();
        }

        try
        {
            Tour = await _tourService.GetTourByIdAsync(TourId, cancellationToken);

            if (Tour == null)
            {
                ErrorMessage = "Tour not found";
                return Page();
            }

            var booking = new Domain.Entities.Booking
            {
                TourId = TourId,
                TourName = Tour.TourName,
                Place = Tour.Place,
                Email = Email,
                FirstName = FirstName
            };

            await _bookingService.CreateBookingAsync(booking, cancellationToken);

            _logger.LogInformation("Booking created for tour ID: {TourId} by {Email}", TourId, Email);
            TempData["SuccessMessage"] = "Booking confirmed successfully!";
            return RedirectToPage("/Booking/MyBookings");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking for tour ID: {TourId}", TourId);
            Tour = await _tourService.GetTourByIdAsync(TourId, cancellationToken);
            ErrorMessage = "An error occurred while creating the booking. Please try again.";
            return Page();
        }
    }
}
