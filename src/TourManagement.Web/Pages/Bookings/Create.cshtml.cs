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

    public CreateModel(IBookingService bookingService, ITourService tourService, ILogger<CreateModel> logger)
    {
        _bookingService = bookingService;
        _tourService = tourService;
        _logger = logger;
    }

    [BindProperty]
    public BookingInputModel BookingInput { get; set; } = new();

    public Tour? Tour { get; set; }

    public class BookingInputModel
    {
        [Required]
        public int TourId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; } = DateTime.Today;

        [Required]
        [Range(1, 50)]
        public int NumberOfPeople { get; set; } = 1;
    }

    public async Task<IActionResult> OnGetAsync(int tourId)
    {
        try
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                return RedirectToPage("/Users/Login");
            }

            Tour = await _tourService.GetTourByIdAsync(tourId);

            if (Tour == null)
            {
                return NotFound();
            }

            BookingInput.TourId = tourId;
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tour for booking");
            return RedirectToPage("/Tours/Index");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        if (!userId.HasValue)
        {
            return RedirectToPage("/Users/Login");
        }

        if (!ModelState.IsValid)
        {
            Tour = await _tourService.GetTourByIdAsync(BookingInput.TourId);
            return Page();
        }

        try
        {
            var booking = new Booking
            {
                TourId = BookingInput.TourId,
                UserId = userId.Value,
                BookingDate = BookingInput.BookingDate,
                NumberOfPeople = BookingInput.NumberOfPeople,
                CreatedBy = HttpContext.Session.GetString("UserEmail") ?? "User"
            };

            await _bookingService.CreateBookingAsync(booking);

            TempData["SuccessMessage"] = "Booking created successfully!";
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking");
            ModelState.AddModelError(string.Empty, "An error occurred while creating the booking.");
            Tour = await _tourService.GetTourByIdAsync(BookingInput.TourId);
            return Page();
        }
    }
}
