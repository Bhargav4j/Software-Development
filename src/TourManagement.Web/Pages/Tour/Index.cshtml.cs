using Microsoft.AspNetCore.Mvc.RazorPages;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Web.Pages.Tour;

public class TourIndexModel : PageModel
{
    private readonly ITourService _tourService;
    private readonly ILogger<TourIndexModel> _logger;

    public TourIndexModel(ITourService tourService, ILogger<TourIndexModel> logger)
    {
        _tourService = tourService;
        _logger = logger;
    }

    public IEnumerable<Domain.Entities.Tour> Tours { get; set; } = new List<Domain.Entities.Tour>();

    public async Task OnGetAsync()
    {
        try
        {
            Tours = await _tourService.GetAllAsync();
            _logger.LogInformation("Retrieved {Count} tours", Tours.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tours");
            Tours = new List<Domain.Entities.Tour>();
        }
    }
}
