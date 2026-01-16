using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Web.Pages.Tour;

public class TourDetailsModel : PageModel
{
    private readonly ITourService _tourService;
    private readonly ILogger<TourDetailsModel> _logger;

    public TourDetailsModel(ITourService tourService, ILogger<TourDetailsModel> logger)
    {
        _tourService = tourService;
        _logger = logger;
    }

    public Domain.Entities.Tour? Tour { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            Tour = await _tourService.GetByIdAsync(id);

            if (Tour == null)
            {
                _logger.LogWarning("Tour not found: {TourId}", id);
                return NotFound();
            }

            _logger.LogInformation("Retrieved tour details for: {TourId}", id);
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tour details: {TourId}", id);
            return RedirectToPage("/Error");
        }
    }
}
