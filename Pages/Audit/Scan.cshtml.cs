using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace buildone.Pages.Audit;

[Authorize]
public class ScanModel : PageModel
{
    public string AuditSessionId { get; set; } = string.Empty;
    public string AuditedBy { get; set; } = string.Empty;
    public string? SessionNotes { get; set; }

    public IActionResult OnGet()
    {
        // Get audit session info from TempData
        if (TempData["AuditSessionId"] == null || TempData["AuditedBy"] == null)
        {
            TempData["ErrorMessage"] = "Please start an audit session first.";
            return RedirectToPage("Create");
        }

        AuditSessionId = TempData["AuditSessionId"]?.ToString() ?? string.Empty;
        AuditedBy = TempData["AuditedBy"]?.ToString() ?? string.Empty;
        SessionNotes = TempData["SessionNotes"]?.ToString();

        // Keep TempData for subsequent requests
        TempData.Keep();

        return Page();
    }
}
