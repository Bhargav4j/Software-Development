using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Web.Pages.User;

public class LoginModel : PageModel
{
    private readonly IUserInfoService _userInfoService;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(IUserInfoService userInfoService, ILogger<LoginModel> logger)
    {
        _userInfoService = userInfoService;
        _logger = logger;
    }

    [BindProperty]
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Please fill in all required fields";
            return Page();
        }

        try
        {
            var user = await _userInfoService.ValidateUserAsync(Email, Password, cancellationToken);

            if (user == null)
            {
                ErrorMessage = "Invalid email or password";
                _logger.LogWarning("Failed login attempt for email: {Email}", Email);
                return Page();
            }

            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");

            _logger.LogInformation("User logged in successfully: {Email}", Email);
            return RedirectToPage("/User/Profile");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", Email);
            ErrorMessage = "An error occurred during login. Please try again.";
            return Page();
        }
    }
}
