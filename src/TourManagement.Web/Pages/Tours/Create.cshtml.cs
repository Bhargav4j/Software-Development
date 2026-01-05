using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Web.Pages.Tours;

public class CreateModel : PageModel
{
    private readonly ITourService _tourService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(ITourService tourService, IWebHostEnvironment environment, ILogger<CreateModel> logger)
    {
        _tourService = tourService;
        _environment = environment;
        _logger = logger;
    }

    [BindProperty]
    [Required(ErrorMessage = "Tour name is required")]
    [StringLength(20, ErrorMessage = "Tour name cannot exceed 20 characters")]
    public string TourName { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Place is required")]
    [StringLength(20, ErrorMessage = "Place cannot exceed 20 characters")]
    public string Place { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Days is required")]
    [Range(1, 99, ErrorMessage = "Days must be between 1 and 99")]
    public int Days { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Price is required")]
    [Range(0, 999999, ErrorMessage = "Price must be between 0 and 999999")]
    public decimal Price { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Locations is required")]
    [StringLength(100, ErrorMessage = "Locations cannot exceed 100 characters")]
    public string Locations { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Tour information is required")]
    [StringLength(200, ErrorMessage = "Tour information cannot exceed 200 characters")]
    public string TourInfo { get; set; } = string.Empty;

    [BindProperty]
    public IFormFile? PictureFile { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            string? pictureFileName = null;

            if (PictureFile != null && PictureFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "Tour_pics");
                Directory.CreateDirectory(uploadsFolder);

                pictureFileName = Guid.NewGuid().ToString() + Path.GetExtension(PictureFile.FileName);
                var filePath = Path.Combine(uploadsFolder, pictureFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await PictureFile.CopyToAsync(fileStream, cancellationToken);
                }
            }

            var tour = new Tour
            {
                Name = TourName,
                Place = Place,
                Days = Days,
                Price = Price,
                Locations = Locations,
                TourInfo = TourInfo,
                PictureFileName = pictureFileName,
                CreatedBy = "admin"
            };

            await _tourService.CreateTourAsync(tour, cancellationToken);

            TempData["SuccessMessage"] = "Tour created successfully!";
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tour");
            ModelState.AddModelError(string.Empty, "An error occurred while creating the tour.");
            return Page();
        }
    }
}
