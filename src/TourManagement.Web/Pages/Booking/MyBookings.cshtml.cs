using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Web.Pages.Booking;

[Authorize]
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

    public async Task OnGetAsync()
    {
        try
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            if (!string.IsNullOrEmpty(userEmail))
            {
                Bookings = await _bookingService.GetByUserEmailAsync(userEmail);
                _logger.LogInformation("Retrieved {Count} bookings for user: {Email}", Bookings.Count(), userEmail);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving bookings for user");
            Bookings = new List<Domain.Entities.Booking>();
        }
    }
}
