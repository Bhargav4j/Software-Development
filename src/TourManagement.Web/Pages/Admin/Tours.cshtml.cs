using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Web.Pages.Admin;

public class ToursModel : PageModel
{
    private readonly ITourService _tourService;
    private readonly ILogger<ToursModel> _logger;

    public ToursModel(ITourService tourService, ILogger<ToursModel> logger)
    {
        _tourService = tourService;
        _logger = logger;
    }

    public IEnumerable<Domain.Entities.Tour> Tours { get; set; } = new List<Domain.Entities.Tour>();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var isAdmin = HttpContext.Session.GetString("IsAdmin");
        if (string.IsNullOrEmpty(isAdmin))
        {
            return RedirectToPage("/Admin/Login");
        }

        try
        {
            Tours = await _tourService.GetAllToursAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tours for admin");
            Tours = new List<Domain.Entities.Tour>();
        }

        return Page();
    }
}
