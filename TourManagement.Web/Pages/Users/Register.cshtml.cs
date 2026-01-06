using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Web.Pages.Users;

public class RegisterModel : PageModel
{
    private readonly IUserService _userService;
    private readonly ILogger<RegisterModel> _logger;

    [BindProperty]
    [Required(ErrorMessage = "First name is required")]
    public string FirstName { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Last name is required")]
    public string LastName { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public string? Gender { get; set; }

    [BindProperty]
    public DateTime? DateOfBirth { get; set; }

    [BindProperty]
    public string? Street { get; set; }

    [BindProperty]
    public string? City { get; set; }

    [BindProperty]
    public string? State { get; set; }

    public string? ErrorMessage { get; set; }

    public RegisterModel(IUserService userService, ILogger<RegisterModel> logger)
    {
        _userService = userService;
        _logger = logger;
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

        try
        {
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
                State = State
            };

            await _userService.CreateUserAsync(user);

            _logger.LogInformation("User registered successfully: {Email}", Email);

            TempData["SuccessMessage"] = "Registration successful! Please login.";
            return RedirectToPage("/Users/Login");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration for {Email}", Email);
            ErrorMessage = ex.Message.Contains("already exists")
                ? "A user with this email already exists."
                : "An error occurred during registration. Please try again.";
            return Page();
        }
    }
}
