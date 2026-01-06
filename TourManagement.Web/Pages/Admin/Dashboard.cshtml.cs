using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TourManagement.Web.Pages.Admin;

public class DashboardModel : PageModel
{
    public string AdminName { get; set; } = string.Empty;

    public IActionResult OnGet()
    {
        var adminId = HttpContext.Session.GetInt32("AdminId");
        if (adminId == null)
        {
            return RedirectToPage("/Admin/Login");
        }

        AdminName = HttpContext.Session.GetString("AdminName") ?? "Admin";
        return Page();
    }

    public IActionResult OnPostLogout()
    {
        HttpContext.Session.Clear();
        return RedirectToPage("/Index");
    }
}
