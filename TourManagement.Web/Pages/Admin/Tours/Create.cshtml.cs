using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Web.Pages.Admin.Tours;

public class CreateModel : PageModel
{
    private readonly ITourService _tourService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<CreateModel> _logger;

    [BindProperty]
    [Required(ErrorMessage = "Tour name is required")]
    public string TourName { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Place is required")]
    public string Place { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Days is required")]
    [Range(1, 365, ErrorMessage = "Days must be between 1 and 365")]
    public int Days { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [BindProperty]
    public string? Locations { get; set; }

    [BindProperty]
    public string? TourInfo { get; set; }

    [BindProperty]
    public IFormFile? PictureFile { get; set; }

    public string? ErrorMessage { get; set; }

    public CreateModel(ITourService tourService, IWebHostEnvironment environment, ILogger<CreateModel> logger)
    {
        _tourService = tourService;
        _environment = environment;
        _logger = logger;
    }

    public IActionResult OnGet()
    {
        var adminId = HttpContext.Session.GetInt32("AdminId");
        if (adminId == null)
        {
            return RedirectToPage("/Admin/Login");
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var adminId = HttpContext.Session.GetInt32("AdminId");
        if (adminId == null)
        {
            return RedirectToPage("/Admin/Login");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            string? picturePath = null;

            if (PictureFile != null)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{PictureFile.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await PictureFile.CopyToAsync(fileStream);
                }

                picturePath = uniqueFileName;
            }

            var tour = new Tour
            {
                TourName = TourName,
                Place = Place,
                Days = Days,
                Price = Price,
                Locations = Locations,
                TourInfo = TourInfo,
                PicturePath = picturePath,
                CreatedBy = HttpContext.Session.GetString("AdminEmail") ?? "Admin"
            };

            await _tourService.CreateTourAsync(tour);

            _logger.LogInformation("Tour {TourName} created successfully", TourName);

            TempData["SuccessMessage"] = "Tour created successfully!";
            return RedirectToPage("/Admin/Tours/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tour {TourName}", TourName);
            ErrorMessage = "An error occurred while creating the tour. Please try again.";
            return Page();
        }
    }
}
