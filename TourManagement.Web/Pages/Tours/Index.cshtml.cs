using Microsoft.AspNetCore.Mvc.RazorPages;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Web.Pages.Tours;

public class IndexModel : PageModel
{
    private readonly ITourService _tourService;
    private readonly ILogger<IndexModel> _logger;

    public IEnumerable<Tour> Tours { get; set; } = new List<Tour>();

    public IndexModel(ITourService tourService, ILogger<IndexModel> logger)
    {
        _tourService = tourService;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        try
        {
            Tours = await _tourService.GetAllToursAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tours");
        }
    }
}
