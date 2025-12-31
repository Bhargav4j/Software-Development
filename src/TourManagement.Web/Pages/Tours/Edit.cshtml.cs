using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TourManagement.Application.DTOs;
using TourManagement.Application.Services;

namespace TourManagement.Web.Pages.Tours;

public class EditModel : PageModel
{
    private readonly TourFacade _tourFacade;
    private readonly IWebHostEnvironment _environment;

    public EditModel(TourFacade tourFacade, IWebHostEnvironment environment)
    {
        _tourFacade = tourFacade;
        _environment = environment;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public int Id { get; set; }

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

        public string? CurrentPictureFileName { get; set; }
        public IFormFile? PictureFile { get; set; }
        public bool IsActive { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var tour = await _tourFacade.GetByIdAsync(id);
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
            CurrentPictureFileName = tour.PictureFileName,
            IsActive = tour.IsActive
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var tourUpdateDto = new TourUpdateDto
        {
            TourName = Input.TourName,
            Place = Input.Place,
            Days = Input.Days,
            Price = Input.Price,
            Locations = Input.Locations,
            TourInfo = Input.TourInfo,
            PictureFileName = Input.CurrentPictureFileName,
            IsActive = Input.IsActive
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

            tourUpdateDto.PictureFileName = uniqueFileName;
        }

        await _tourFacade.UpdateAsync(Input.Id, tourUpdateDto);

        return RedirectToPage("Index");
    }
}
