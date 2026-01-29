using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TourManagement.Web.Pages.Account;

public class LogoutModel : PageModel
{
    private readonly ILogger<LogoutModel> _logger;

    public LogoutModel(ILogger<LogoutModel> logger)
    {
        _logger = logger;
    }

    public IActionResult OnGet()
    {
        HttpContext.Session.Clear();
        _logger.LogInformation("User logged out");
        return RedirectToPage("/Index");
    }
}
