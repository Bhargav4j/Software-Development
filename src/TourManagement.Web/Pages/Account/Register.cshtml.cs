using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Web.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly IUserService _userService;
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(IUserService userService, ILogger<RegisterModel> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [BindProperty]
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    public string LastName { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [DataType(DataType.Password)]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    public string Gender { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [BindProperty]
    [Required]
    public string Street { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    public string City { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    public string State { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var existingUser = await _userService.GetByEmailAsync(Email);
            if (existingUser != null)
            {
                ErrorMessage = "A user with this email already exists.";
                return Page();
            }

            var user = new User
            {
                FirstName = FirstName,
                LastName = LastName,
                Email = Email,
                Password = Password,
                Gender = Gender,
                DateOfBirth = DateOfBirth,
                Street = Street,
                City = City,
                State = State,
                CreatedBy = "System"
            };

            await _userService.CreateAsync(user);
            _logger.LogInformation("User {Email} registered successfully", Email);

            return RedirectToPage("/Account/Login");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user {Email}", Email);
            ErrorMessage = "An error occurred during registration. Please try again.";
            return Page();
        }
    }
}
