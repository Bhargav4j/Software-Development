using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Web.Pages.Tours;

public class TourDetailsModel : PageModel
{
    private readonly ITourService _tourService;
    private readonly ILogger<TourDetailsModel> _logger;

    public TourDetailsModel(ITourService tourService, ILogger<TourDetailsModel> logger)
    {
        _tourService = tourService;
        _logger = logger;
    }

    public Tour? Tour { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            Tour = await _tourService.GetByIdAsync(id);
            if (Tour == null)
            {
                return NotFound();
            }
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tour with id {TourId}", id);
            return NotFound();
        }
    }
}
