using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Web.Pages.User;

public class ProfileModel : PageModel
{
    private readonly IUserInfoService _userInfoService;
    private readonly ILogger<ProfileModel> _logger;

    public ProfileModel(IUserInfoService userInfoService, ILogger<ProfileModel> logger)
    {
        _userInfoService = userInfoService;
        _logger = logger;
    }

    public UserInfo? UserInfo { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var userEmail = HttpContext.Session.GetString("UserEmail");

        if (string.IsNullOrEmpty(userEmail))
        {
            return RedirectToPage("/User/Login");
        }

        try
        {
            UserInfo = await _userInfoService.GetUserByEmailAsync(userEmail, cancellationToken);

            if (UserInfo == null)
            {
                _logger.LogWarning("User not found: {Email}", userEmail);
                return RedirectToPage("/User/Login");
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile: {Email}", userEmail);
            return RedirectToPage("/User/Login");
        }
    }
}
