using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Web.Pages.User;

public class RegisterModel : PageModel
{
    private readonly IUserInfoService _userInfoService;
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(IUserInfoService userInfoService, ILogger<RegisterModel> logger)
    {
        _userInfoService = userInfoService;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string Gender { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(50)]
        public string Street { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string State { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Please fill in all required fields correctly";
            return Page();
        }

        try
        {
            var userInfo = new UserInfo
            {
                Email = Input.Email,
                Password = Input.Password,
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                Gender = Input.Gender,
                DateOfBirth = Input.DateOfBirth,
                Street = Input.Street,
                City = Input.City,
                State = Input.State
            };

            await _userInfoService.RegisterUserAsync(userInfo, cancellationToken);

            _logger.LogInformation("User registered successfully: {Email}", Input.Email);
            TempData["SuccessMessage"] = "Registration successful! Please login.";
            return RedirectToPage("/User/Login");
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
            _logger.LogWarning(ex, "Registration failed for email: {Email}", Input.Email);
            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = "An error occurred during registration. Please try again.";
            _logger.LogError(ex, "Error during registration for email: {Email}", Input.Email);
            return Page();
        }
    }
}
