using Microsoft.AspNetCore.Mvc.RazorPages;
using TourManagement.Application.DTOs;
using TourManagement.Application.Services;

namespace TourManagement.Web.Pages.Tours;

public class IndexModel : PageModel
{
    private readonly TourFacade _tourFacade;

    public IndexModel(TourFacade tourFacade)
    {
        _tourFacade = tourFacade;
    }

    public IEnumerable<TourDto> Tours { get; set; } = new List<TourDto>();

    public async Task OnGetAsync()
    {
        Tours = await _tourFacade.GetActiveToursAsync();
    }
}
