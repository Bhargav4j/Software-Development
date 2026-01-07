using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Web.Pages.Bookings;

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
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    public Tour? Tour { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            Tour = await _tourService.GetByIdAsync(TourId);

            if (Tour == null)
            {
                _logger.LogWarning("Tour with ID {TourId} not found", TourId);
                return Page();
            }

            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userName = HttpContext.Session.GetString("UserName");

            if (!string.IsNullOrEmpty(userEmail))
            {
                Email = userEmail;
                FirstName = userName ?? string.Empty;
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tour for booking");
            return RedirectToPage("/Error");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Tour = await _tourService.GetByIdAsync(TourId);

        if (Tour == null)
        {
            ErrorMessage = "Tour not found.";
            return Page();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var booking = new Booking
            {
                TourId = TourId,
                TourName = Tour.TourName,
                Place = Tour.Place,
                Email = Email,
                FirstName = FirstName,
                CreatedBy = "System"
            };

            await _bookingService.CreateAsync(booking);

            _logger.LogInformation("Booking created successfully for tour {TourId} by {Email}", TourId, Email);

            TempData["SuccessMessage"] = "Booking created successfully!";
            return RedirectToPage("/Bookings/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking for tour {TourId}", TourId);
            ErrorMessage = "An error occurred while creating your booking. Please try again.";
            return Page();
        }
    }
}
