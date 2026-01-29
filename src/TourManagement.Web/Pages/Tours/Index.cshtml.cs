using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Web.Pages.Tours;

public class ToursIndexModel : PageModel
{
    private readonly ITourService _tourService;
    private readonly ILogger<ToursIndexModel> _logger;

    public ToursIndexModel(ITourService tourService, ILogger<ToursIndexModel> logger)
    {
        _tourService = tourService;
        _logger = logger;
    }

    public IEnumerable<Tour> Tours { get; set; } = new List<Tour>();
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            Tours = await _tourService.GetAllToursAsync();

            if (TempData["SuccessMessage"] != null)
            {
                SuccessMessage = TempData["SuccessMessage"]?.ToString();
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tours");
            return RedirectToPage("/Error");
        }
    }
}
