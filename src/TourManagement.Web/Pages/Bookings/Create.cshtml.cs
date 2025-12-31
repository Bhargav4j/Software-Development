using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Web.Pages.Bookings;

public class CreateModel : PageModel
{
    private readonly IBookingService _bookingService;
    private readonly ITourService _tourService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(
        IBookingService bookingService,
        ITourService tourService,
        ILogger<CreateModel> logger)
    {
        _bookingService = bookingService;
        _tourService = tourService;
        _logger = logger;
    }

    [BindProperty]
    public BookingInputModel Input { get; set; } = new();

    public Tour? Tour { get; set; }

    public class BookingInputModel
    {
        public int TourId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Place { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync(int? tourId)
    {
        if (!tourId.HasValue)
        {
            return RedirectToPage("/Tours/Index");
        }

        Tour = await _tourService.GetTourByIdAsync(tourId.Value);

        if (Tour == null)
        {
            return NotFound();
        }

        Input.TourId = Tour.TourId;
        Input.Place = Tour.Place;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Tour = await _tourService.GetTourByIdAsync(Input.TourId);
            return Page();
        }

        try
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 1;
            var tour = await _tourService.GetTourByIdAsync(Input.TourId);

            if (tour == null)
            {
                return NotFound();
            }

            var booking = new Booking
            {
                TourId = Input.TourId,
                UserId = userId,
                TourName = tour.TourName,
                Place = Input.Place,
                Email = Input.Email,
                FirstName = Input.FirstName
            };

            await _bookingService.CreateBookingAsync(booking);
            _logger.LogInformation("Booking created successfully for tour: {TourId}", Input.TourId);

            TempData["SuccessMessage"] = "Booking created successfully!";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking");
            ModelState.AddModelError(string.Empty, "An error occurred while creating the booking.");
            Tour = await _tourService.GetTourByIdAsync(Input.TourId);
            return Page();
        }
    }
}
