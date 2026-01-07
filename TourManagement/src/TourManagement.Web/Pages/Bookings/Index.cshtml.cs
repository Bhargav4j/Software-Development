using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Web.Pages.Bookings;

public class IndexModel : PageModel
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IBookingService bookingService, ILogger<IndexModel> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    public IEnumerable<Booking> Bookings { get; set; } = new List<Booking>();
    public string? Message { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userEmail = HttpContext.Session.GetString("UserEmail");

        if (string.IsNullOrEmpty(userEmail))
        {
            return RedirectToPage("/Users/Login");
        }

        try
        {
            Bookings = await _bookingService.GetByEmailAsync(userEmail);
            _logger.LogInformation("Retrieved {Count} bookings for user {Email}", Bookings.Count(), userEmail);
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving bookings for user {Email}", userEmail);
            Message = "An error occurred while loading your bookings. Please try again later.";
            return Page();
        }
    }
}
