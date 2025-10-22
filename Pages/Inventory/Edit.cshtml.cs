using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using buildone.Data;

namespace buildone.Pages.Inventory
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Data.Inventory Inventory { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventory = await _context.Inventories.FirstOrDefaultAsync(m => m.Id == id);
            if (inventory == null)
            {
                return NotFound();
            }

            Inventory = inventory;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Load the existing inventory from database
            var existingInventory = await _context.Inventories.FindAsync(Inventory.Id);
            if (existingInventory == null)
            {
                return NotFound();
            }

            // Update only editable fields
            existingInventory.ItemName = Inventory.ItemName;
            existingInventory.SKU = Inventory.SKU;
            existingInventory.Description = Inventory.Description;
            existingInventory.Category = Inventory.Category;
            existingInventory.MinimumQuantity = Inventory.MinimumQuantity;
            existingInventory.MaximumQuantity = Inventory.MaximumQuantity;
            existingInventory.UnitPrice = Inventory.UnitPrice;
            existingInventory.Unit = Inventory.Unit;
            existingInventory.StorageLocation = Inventory.StorageLocation;
            existingInventory.Supplier = Inventory.Supplier;

            // Update audit fields
            existingInventory.LastUpdated = DateTime.UtcNow;
            existingInventory.UpdatedBy = User.Identity?.Name;

            // Recalculate stock status based on new thresholds
            if (existingInventory.CurrentQuantity == 0)
            {
                existingInventory.StockStatus = Data.Enums.StockStatus.OutOfStock;
            }
            else if (existingInventory.CurrentQuantity <= existingInventory.MinimumQuantity)
            {
                existingInventory.StockStatus = Data.Enums.StockStatus.LowStock;
            }
            else if (existingInventory.CurrentQuantity >= existingInventory.MaximumQuantity)
            {
                existingInventory.StockStatus = Data.Enums.StockStatus.FullyStocked;
            }
            else
            {
                existingInventory.StockStatus = Data.Enums.StockStatus.InStock;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InventoryExists(Inventory.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Details", new { id = Inventory.Id });
        }

        private bool InventoryExists(int id)
        {
            return _context.Inventories.Any(e => e.Id == id);
        }
    }
}
