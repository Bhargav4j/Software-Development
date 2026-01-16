using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Web.Pages.Booking;

public class MyBookingsModel : PageModel
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<MyBookingsModel> _logger;

    public MyBookingsModel(IBookingService bookingService, ILogger<MyBookingsModel> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    public IEnumerable<Domain.Entities.Booking> Bookings { get; set; } = new List<Domain.Entities.Booking>();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var userEmail = HttpContext.Session.GetString("UserEmail");

        if (string.IsNullOrEmpty(userEmail))
        {
            return RedirectToPage("/User/Login");
        }

        try
        {
            Bookings = await _bookingService.GetBookingsByUserEmailAsync(userEmail, cancellationToken);
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving bookings for user: {Email}", userEmail);
            Bookings = new List<Domain.Entities.Booking>();
            return Page();
        }
    }
}
