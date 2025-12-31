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
    public TourEditModel Input { get; set; } = new();

    public class TourEditModel
    {
        public int TourId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Tour Name")]
        public string TourName { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Place { get; set; } = string.Empty;

        [Required]
        [Range(1, 365)]
        public int Days { get; set; }

        [Required]
        [StringLength(500)]
        public string Locations { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 1000000)]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required]
        [StringLength(250)]
        [Display(Name = "Tour Information")]
        public string TourInfo { get; set; } = string.Empty;

        [Display(Name = "New Tour Image")]
        public IFormFile? ImageFile { get; set; }

        public string? ExistingPicturePath { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            var tour = await _tourService.GetTourByIdAsync(id);

            if (tour == null)
            {
                return NotFound();
            }

            Input = new TourEditModel
            {
                TourId = tour.TourId,
                TourName = tour.TourName,
                Place = tour.Place,
                Days = tour.Days,
                Locations = tour.Locations,
                Price = tour.Price,
                TourInfo = tour.TourInfo,
                ExistingPicturePath = tour.PicturePath
            };

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tour for edit: {TourId}", id);
            return RedirectToPage("./Index");
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
            var tour = await _tourService.GetTourByIdAsync(Input.TourId);

            if (tour == null)
            {
                return NotFound();
            }

            tour.TourName = Input.TourName;
            tour.Place = Input.Place;
            tour.Days = Input.Days;
            tour.Locations = Input.Locations;
            tour.Price = Input.Price;
            tour.TourInfo = Input.TourInfo;

            if (Input.ImageFile != null)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{Input.ImageFile.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.ImageFile.CopyToAsync(fileStream);
                }

                if (!string.IsNullOrEmpty(tour.PicturePath))
                {
                    var oldFilePath = Path.Combine(_environment.WebRootPath, "uploads", tour.PicturePath);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                tour.PicturePath = uniqueFileName;
            }

            await _tourService.UpdateTourAsync(tour);
            _logger.LogInformation("Tour updated successfully: {TourId}", tour.TourId);

            TempData["SuccessMessage"] = "Tour updated successfully!";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tour: {TourId}", Input.TourId);
            ModelState.AddModelError(string.Empty, "An error occurred while updating the tour.");
            return Page();
        }
    }
}
