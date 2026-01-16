using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Web.Pages.Admin;

public class CreateTourModel : PageModel
{
    private readonly ITourService _tourService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<CreateTourModel> _logger;

    public CreateTourModel(ITourService tourService, IWebHostEnvironment environment, ILogger<CreateTourModel> logger)
    {
        _tourService = tourService;
        _environment = environment;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [StringLength(20)]
        public string TourName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Place { get; set; } = string.Empty;

        [Required]
        [Range(1, 99)]
        public int Days { get; set; }

        [Required]
        [Range(0.01, 999999.99)]
        public decimal Price { get; set; }

        [Required]
        [StringLength(100)]
        public string Locations { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string TourInfo { get; set; } = string.Empty;

        public IFormFile? PictureFile { get; set; }
    }

    public IActionResult OnGet()
    {
        var isAdmin = HttpContext.Session.GetString("IsAdmin");
        if (string.IsNullOrEmpty(isAdmin))
        {
            return RedirectToPage("/Admin/Login");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var isAdmin = HttpContext.Session.GetString("IsAdmin");
        if (string.IsNullOrEmpty(isAdmin))
        {
            return RedirectToPage("/Admin/Login");
        }

        if (!ModelState.IsValid)
        {
            ErrorMessage = "Please fill in all required fields correctly";
            return Page();
        }

        try
        {
            string? picturePath = null;

            if (Input.PictureFile != null)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "tours");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{Input.PictureFile.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.PictureFile.CopyToAsync(fileStream, cancellationToken);
                }

                picturePath = uniqueFileName;
            }

            var tour = new Domain.Entities.Tour
            {
                TourName = Input.TourName,
                Place = Input.Place,
                Days = Input.Days,
                Price = Input.Price,
                Locations = Input.Locations,
                TourInfo = Input.TourInfo,
                PicturePath = picturePath
            };

            await _tourService.CreateTourAsync(tour, cancellationToken);

            _logger.LogInformation("Tour created successfully: {TourName}", Input.TourName);
            TempData["SuccessMessage"] = "Tour created successfully!";
            return RedirectToPage("/Admin/Tours");
        }
        catch (Exception ex)
        {
            ErrorMessage = "An error occurred while creating the tour. Please try again.";
            _logger.LogError(ex, "Error creating tour: {TourName}", Input.TourName);
            return Page();
        }
    }
}
