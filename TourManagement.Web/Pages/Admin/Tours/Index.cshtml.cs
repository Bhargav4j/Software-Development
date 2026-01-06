using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Web.Pages.Admin.Tours;

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

    public async Task<IActionResult> OnGetAsync()
    {
        var adminId = HttpContext.Session.GetInt32("AdminId");
        if (adminId == null)
        {
            return RedirectToPage("/Admin/Login");
        }

        try
        {
            Tours = await _tourService.GetAllToursAsync();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tours");
            return RedirectToPage("/Error");
        }
    }
}
