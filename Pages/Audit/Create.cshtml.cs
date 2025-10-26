using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace buildone.Pages.Audit;

[Authorize]
public class CreateModel : PageModel
{
    [BindProperty]
    public string AuditSessionId { get; set; } = Guid.NewGuid().ToString();

    [BindProperty]
    [Required(ErrorMessage = "Audited By is required")]
    [StringLength(100)]
    public string AuditedBy { get; set; } = string.Empty;

    [BindProperty]
    [StringLength(1000)]
    public string? Notes { get; set; }

    public void OnGet()
    {
        // Pre-fill with current user's name if available
        AuditedBy = User.Identity?.Name ?? string.Empty;
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Store audit session info in TempData to pass to Scan page
        TempData["AuditSessionId"] = AuditSessionId;
        TempData["AuditedBy"] = AuditedBy;
        TempData["SessionNotes"] = Notes;

        return RedirectToPage("Scan");
    }
}
