using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TourManagement.Web.Pages.Admin;

public class DashboardModel : PageModel
{
    public IActionResult OnGet()
    {
        var isAdmin = HttpContext.Session.GetString("IsAdmin");

        if (string.IsNullOrEmpty(isAdmin))
        {
            return RedirectToPage("/Admin/Login");
        }

        return Page();
    }
}
