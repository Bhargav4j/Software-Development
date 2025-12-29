using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Web.Pages.Bookings;

public class CreateModel : PageModel
{
    private readonly IBookingService _bookingService;
    private readonly IUserService _userService;
    private readonly ITourService _tourService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(
        IBookingService bookingService,
        IUserService userService,
        ITourService tourService,
        ILogger<CreateModel> logger)
    {
        _bookingService = bookingService;
        _userService = userService;
        _tourService = tourService;
        _logger = logger;
    }

    [BindProperty]
    [Required]
    [Display(Name = "User")]
    public int UserId { get; set; }

    [BindProperty]
    [Required]
    [Display(Name = "Tour")]
    public int TourId { get; set; }

    [BindProperty]
    [Required]
    [Display(Name = "Booking Date")]
    public DateTime BookingDate { get; set; } = DateTime.Today;

    [BindProperty]
    [Required]
    [Range(1, 100)]
    [Display(Name = "Number of People")]
    public int NumberOfPeople { get; set; } = 1;

    [BindProperty]
    [Required]
    [Range(0, 1000000)]
    [Display(Name = "Total Amount")]
    public decimal TotalAmount { get; set; }

    public SelectList UserSelectList { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());
    public SelectList TourSelectList { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());

    public async Task OnGetAsync()
    {
        await LoadSelectListsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync();
            return Page();
        }

        try
        {
            var booking = new Booking
            {
                UserId = UserId,
                TourId = TourId,
                BookingDate = BookingDate,
                NumberOfPeople = NumberOfPeople,
                TotalAmount = TotalAmount,
                Status = "Confirmed",
                CreatedBy = "Admin"
            };

            await _bookingService.CreateBookingAsync(booking);

            TempData["SuccessMessage"] = "Booking created successfully!";
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking");
            ModelState.AddModelError(string.Empty, "An error occurred while creating the booking.");
            await LoadSelectListsAsync();
            return Page();
        }
    }

    private async Task LoadSelectListsAsync()
    {
        var users = await _userService.GetAllUsersAsync();
        var tours = await _tourService.GetAllToursAsync();

        UserSelectList = new SelectList(users, "Id", "Email");
        TourSelectList = new SelectList(tours, "Id", "TourName");
    }
}
