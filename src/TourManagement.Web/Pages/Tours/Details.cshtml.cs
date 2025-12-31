using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TourManagement.Application.DTOs;
using TourManagement.Application.Services;

namespace TourManagement.Web.Pages.Tours;

public class DetailsModel : PageModel
{
    private readonly TourFacade _tourFacade;

    public DetailsModel(TourFacade tourFacade)
    {
        _tourFacade = tourFacade;
    }

    public TourDto? Tour { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Tour = await _tourFacade.GetByIdAsync(id);

        if (Tour == null)
        {
            return NotFound();
        }

        return Page();
    }
}
