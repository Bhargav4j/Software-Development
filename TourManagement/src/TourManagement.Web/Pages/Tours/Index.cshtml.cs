using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Web.Pages.Tours;

public class IndexModel : PageModel
{
    private readonly ITourService _tourService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ITourService tourService, ILogger<IndexModel> logger)
    {
        _tourService = tourService;
        _logger = logger;
    }

    public IEnumerable<Tour> Tours { get; set; } = new List<Tour>();
    public string? Message { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            Tours = await _tourService.GetAllAsync();
            _logger.LogInformation("Retrieved {Count} tours", Tours.Count());
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tours");
            Message = "An error occurred while loading tours. Please try again later.";
            return Page();
        }
    }
}
