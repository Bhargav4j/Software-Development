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

    public async Task OnGetAsync()
    {
        try
        {
            Tours = await _tourService.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tours");
            Tours = new List<Tour>();
        }
    }
}
