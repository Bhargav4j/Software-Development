using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Web.Pages.Tours;

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
    public string Locations { get; set; } = string.Empty;

    [BindProperty]
    public string TourInfo { get; set; } = string.Empty;

    [BindProperty]
    public IFormFile? PictureFile { get; set; }

    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        var isAdmin = HttpContext.Session.GetString("IsAdmin");
        if (isAdmin != "true")
        {
            return RedirectToPage("/Account/Login");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var isAdmin = HttpContext.Session.GetString("IsAdmin");
        if (isAdmin != "true")
        {
            return RedirectToPage("/Account/Login");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            string? fileName = null;

            if (PictureFile != null)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                fileName = Guid.NewGuid().ToString() + Path.GetExtension(PictureFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await PictureFile.CopyToAsync(fileStream);
                }
            }

            var tour = new Tour
            {
                TourName = TourName,
                Place = Place,
                Days = Days,
                Price = Price,
                Locations = Locations,
                TourInfo = TourInfo,
                PicturePath = fileName,
                CreatedBy = HttpContext.Session.GetString("UserEmail") ?? "System"
            };

            await _tourService.CreateTourAsync(tour);

            _logger.LogInformation("Tour created successfully: {TourName}", TourName);

            TempData["SuccessMessage"] = "Tour created successfully";
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tour: {TourName}", TourName);
            ErrorMessage = "An error occurred while creating the tour. Please try again.";
            return Page();
        }
    }
}
