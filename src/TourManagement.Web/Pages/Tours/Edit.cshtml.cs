using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
    public InputModel Input { get; set; } = new InputModel();

    public class InputModel
    {
        public int Id { get; set; }

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
        [Range(0.01, 1000000)]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required]
        [StringLength(500)]
        public string Locations { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Tour Information")]
        public string? TourInfo { get; set; }

        [Display(Name = "Picture")]
        public IFormFile? PictureFile { get; set; }

        public string? ExistingPictureFileName { get; set; }
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

            Input = new InputModel
            {
                Id = tour.Id,
                TourName = tour.TourName,
                Place = tour.Place,
                Days = tour.Days,
                Price = tour.Price,
                Locations = tour.Locations,
                TourInfo = tour.TourInfo,
                ExistingPictureFileName = tour.PictureFileName
            };

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tour for edit: {TourId}", id);
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
            var tour = await _tourService.GetTourByIdAsync(Input.Id);

            if (tour == null)
            {
                return NotFound();
            }

            string? pictureFileName = Input.ExistingPictureFileName;

            if (Input.PictureFile != null)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "tours");
                Directory.CreateDirectory(uploadsFolder);

                pictureFileName = $"{Guid.NewGuid()}_{Input.PictureFile.FileName}";
                var filePath = Path.Combine(uploadsFolder, pictureFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.PictureFile.CopyToAsync(stream);
                }

                if (!string.IsNullOrEmpty(Input.ExistingPictureFileName))
                {
                    var oldFilePath = Path.Combine(uploadsFolder, Input.ExistingPictureFileName);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
            }

            tour.TourName = Input.TourName;
            tour.Place = Input.Place;
            tour.Days = Input.Days;
            tour.Price = Input.Price;
            tour.Locations = Input.Locations;
            tour.TourInfo = Input.TourInfo ?? string.Empty;
            tour.PictureFileName = pictureFileName;

            await _tourService.UpdateTourAsync(Input.Id, tour);

            TempData["SuccessMessage"] = "Tour updated successfully!";
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tour: {TourId}", Input.Id);
            ModelState.AddModelError(string.Empty, "An error occurred while updating the tour.");
            return Page();
        }
    }
}
