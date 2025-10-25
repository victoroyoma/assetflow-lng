using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace buildone.Pages.Inventory;

[Authorize]
public class ScanModel : PageModel
{
    public void OnGet()
    {
    }
}
