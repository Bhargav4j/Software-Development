using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Web.Pages.Tours;

public class DeleteModel : PageModel
{
    private readonly ITourService _tourService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<DeleteModel> _logger;

    public DeleteModel(ITourService tourService, IWebHostEnvironment environment, ILogger<DeleteModel> logger)
    {
        _tourService = tourService;
        _environment = environment;
        _logger = logger;
    }

    [BindProperty]
    public Tour? Tour { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        try
        {
            Tour = await _tourService.GetTourByIdAsync(id.Value);

            if (Tour == null)
            {
                return NotFound();
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tour for deletion: {TourId}", id);
            return RedirectToPage("Index");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Tour?.Id == null)
        {
            return NotFound();
        }

        try
        {
            var tour = await _tourService.GetTourByIdAsync(Tour.Id);

            if (tour != null && !string.IsNullOrEmpty(tour.PicturePath))
            {
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", tour.PicturePath);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            await _tourService.DeleteTourAsync(Tour.Id);

            _logger.LogInformation("Tour deleted: {TourId}", Tour.Id);

            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tour");
            ModelState.AddModelError(string.Empty, "An error occurred while deleting the tour.");
            return Page();
        }
    }
}
