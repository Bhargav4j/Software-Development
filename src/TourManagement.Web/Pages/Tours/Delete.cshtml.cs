using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TourManagement.Application.DTOs;
using TourManagement.Application.Services;

namespace TourManagement.Web.Pages.Tours;

public class DeleteModel : PageModel
{
    private readonly TourFacade _tourFacade;

    public DeleteModel(TourFacade tourFacade)
    {
        _tourFacade = tourFacade;
    }

    [BindProperty]
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

    public async Task<IActionResult> OnPostAsync(int id)
    {
        await _tourFacade.DeleteAsync(id);
        return RedirectToPage("Index");
    }
}
