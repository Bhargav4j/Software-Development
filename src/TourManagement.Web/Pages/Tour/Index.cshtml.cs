using Microsoft.AspNetCore.Mvc.RazorPages;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Web.Pages.Tour;

public class IndexModel : PageModel
{
    private readonly ITourService _tourService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ITourService tourService, ILogger<IndexModel> logger)
    {
        _tourService = tourService;
        _logger = logger;
    }

    public IEnumerable<Domain.Entities.Tour> Tours { get; set; } = new List<Domain.Entities.Tour>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            Tours = await _tourService.GetAllToursAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tours");
            Tours = new List<Domain.Entities.Tour>();
        }
    }
}
