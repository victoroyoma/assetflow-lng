using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using buildone.Data;

namespace buildone.Pages.Inventory;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DetailsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public Data.Inventory Inventory { get; set; } = null!;
    public List<InventoryTransaction> Transactions { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(m => m.Id == id);

        if (inventory == null)
        {
            return NotFound();
        }

        Inventory = inventory;

        // Get transaction history
        Transactions = await _context.InventoryTransactions
            .Where(t => t.InventoryId == id)
            .OrderByDescending(t => t.TransactionDate)
            .Take(50)
            .ToListAsync();

        return Page();
    }
}
