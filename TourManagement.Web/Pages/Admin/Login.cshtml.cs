using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Web.Pages.Admin;

public class LoginModel : PageModel
{
    private readonly IAdminService _adminService;
    private readonly ILogger<LoginModel> _logger;

    [BindProperty]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public LoginModel(IAdminService adminService, ILogger<LoginModel> logger)
    {
        _adminService = adminService;
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
            var admin = await _adminService.AuthenticateAdminAsync(Email, Password);

            if (admin == null)
            {
                ErrorMessage = "Invalid email or password";
                return Page();
            }

            HttpContext.Session.SetInt32("AdminId", admin.Id);
            HttpContext.Session.SetString("AdminEmail", admin.Email);
            HttpContext.Session.SetString("AdminName", admin.Name);

            _logger.LogInformation("Admin {Email} logged in successfully", Email);

            return RedirectToPage("/Admin/Dashboard");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during admin login for {Email}", Email);
            ErrorMessage = "An error occurred during login. Please try again.";
            return Page();
        }
    }
}
