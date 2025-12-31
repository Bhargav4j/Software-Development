using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TourManagement.Application.DTOs;
using TourManagement.Application.Services;

namespace TourManagement.Web.Pages.Tours;

public class CreateModel : PageModel
{
    private readonly TourFacade _tourFacade;
    private readonly IWebHostEnvironment _environment;

    public CreateModel(TourFacade tourFacade, IWebHostEnvironment environment)
    {
        _tourFacade = tourFacade;
        _environment = environment;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [StringLength(200)]
        public string TourName { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Place { get; set; } = string.Empty;

        [Required]
        [Range(1, 365)]
        public int Days { get; set; }

        [Required]
        [Range(0, 999999)]
        public decimal Price { get; set; }

        [Required]
        [StringLength(500)]
        public string Locations { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string TourInfo { get; set; } = string.Empty;

        public IFormFile? PictureFile { get; set; }
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var tourCreateDto = new TourCreateDto
        {
            TourName = Input.TourName,
            Place = Input.Place,
            Days = Input.Days,
            Price = Input.Price,
            Locations = Input.Locations,
            TourInfo = Input.TourInfo
        };

        if (Input.PictureFile != null && Input.PictureFile.Length > 0)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "tours");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(Input.PictureFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await Input.PictureFile.CopyToAsync(fileStream);
            }

            tourCreateDto.PictureFileName = uniqueFileName;
        }

        await _tourFacade.CreateAsync(tourCreateDto);

        return RedirectToPage("Index");
    }
}
