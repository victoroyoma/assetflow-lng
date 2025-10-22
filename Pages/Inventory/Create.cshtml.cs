using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using buildone.Data;
using buildone.Data.Enums;

namespace buildone.Pages.Inventory;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Data.Inventory Inventory { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        Inventory.CreatedDate = DateTime.UtcNow;
        Inventory.LastUpdated = DateTime.UtcNow;
        Inventory.UpdatedBy = User.Identity?.Name;

        // Determine initial stock status
        Inventory.StockStatus = DetermineStockStatus(Inventory);

        _context.Inventories.Add(Inventory);
        await _context.SaveChangesAsync();

        // Create initial transaction record
        var transaction = new InventoryTransaction
        {
            InventoryId = Inventory.Id,
            TransactionType = "Initial Stock",
            Quantity = Inventory.CurrentQuantity,
            PreviousQuantity = 0,
            NewQuantity = Inventory.CurrentQuantity,
            Remarks = "Initial inventory creation",
            PerformedBy = User.Identity?.Name,
            TransactionDate = DateTime.UtcNow
        };

        _context.InventoryTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Inventory item '{Inventory.ItemName}' created successfully!";
        return RedirectToPage("./Index");
    }

    private static StockStatus DetermineStockStatus(Data.Inventory inventory)
    {
        if (inventory.CurrentQuantity <= 0)
            return StockStatus.OutOfStock;
        
        if (inventory.CurrentQuantity <= inventory.MinimumQuantity)
            return StockStatus.LowStock;
        
        if (inventory.CurrentQuantity >= inventory.MaximumQuantity)
            return StockStatus.Overstocked;
        
        if (inventory.CurrentQuantity >= (inventory.MaximumQuantity * 0.8))
            return StockStatus.FullyStocked;
        
        return StockStatus.InStock;
    }
}
