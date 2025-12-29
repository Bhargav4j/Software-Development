using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Web.Pages.Bookings;

public class DeleteModel : PageModel
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<DeleteModel> _logger;

    public DeleteModel(IBookingService bookingService, ILogger<DeleteModel> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    [BindProperty]
    public Booking? Booking { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            Booking = await _bookingService.GetBookingByIdAsync(id);

            if (Booking == null)
            {
                return NotFound();
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading booking for delete, id {BookingId}", id);
            return RedirectToPage("Index");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Booking == null || Booking.Id == 0)
        {
            return NotFound();
        }

        try
        {
            await _bookingService.DeleteBookingAsync(Booking.Id);

            TempData["SuccessMessage"] = "Booking deleted successfully!";
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting booking with id {BookingId}", Booking.Id);
            ModelState.AddModelError(string.Empty, "An error occurred while deleting the booking.");
            return Page();
        }
    }
}
