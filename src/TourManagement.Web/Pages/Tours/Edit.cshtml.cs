using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Web.Pages.Tours;

public class EditModel : PageModel
{
    private readonly ITourService _tourService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<EditModel> _logger;

    public EditModel(
        ITourService tourService,
        IWebHostEnvironment environment,
        ILogger<EditModel> logger)
    {
        _tourService = tourService;
        _environment = environment;
        _logger = logger;
    }

    [BindProperty]
    public int Id { get; set; }

    [BindProperty]
    [Required]
    [StringLength(200)]
    public string TourName { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [StringLength(200)]
    public string Place { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [Range(1, 365)]
    public int Days { get; set; }

    [BindProperty]
    [Required]
    [Range(0, 1000000)]
    public decimal Price { get; set; }

    [BindProperty]
    [StringLength(500)]
    public string Locations { get; set; } = string.Empty;

    [BindProperty]
    [StringLength(2000)]
    public string TourInfo { get; set; } = string.Empty;

    [BindProperty]
    public IFormFile? ImageFile { get; set; }

    [BindProperty]
    public string? ExistingPic { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            var tour = await _tourService.GetTourByIdAsync(id);

            if (tour == null)
            {
                return NotFound();
            }

            Id = tour.Id;
            TourName = tour.TourName;
            Place = tour.Place;
            Days = tour.Days;
            Price = tour.Price;
            Locations = tour.Locations;
            TourInfo = tour.TourInfo;
            ExistingPic = tour.Pic;

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tour for edit, id {TourId}", id);
            return RedirectToPage("Index");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var tour = await _tourService.GetTourByIdAsync(Id);
            if (tour == null)
            {
                return NotFound();
            }

            string? fileName = ExistingPic;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images");
                Directory.CreateDirectory(uploadsFolder);

                fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                if (!string.IsNullOrEmpty(ExistingPic))
                {
                    var oldFilePath = Path.Combine(uploadsFolder, ExistingPic);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
            }

            tour.TourName = TourName;
            tour.Place = Place;
            tour.Days = Days;
            tour.Price = Price;
            tour.Locations = Locations;
            tour.TourInfo = TourInfo;
            tour.Pic = fileName;
            tour.ModifiedBy = "Admin";

            await _tourService.UpdateTourAsync(tour);

            TempData["SuccessMessage"] = "Tour updated successfully!";
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tour");
            ModelState.AddModelError(string.Empty, "An error occurred while updating the tour.");
            return Page();
        }
    }
}
