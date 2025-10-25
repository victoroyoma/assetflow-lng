using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using buildone.Data;
using buildone.Data.Enums;
using buildone.Models;

namespace buildone.Pages.Inventory;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public PaginatedList<Data.Inventory> Inventories { get; set; } = null!;
    public string? SearchTerm { get; set; }
    public string? CategoryFilter { get; set; }
    public string? StockStatusFilter { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    
    public List<string> Categories { get; set; } = new();
    public InventoryStatistics Statistics { get; set; } = new();

    public async Task OnGetAsync(string? searchTerm, string? categoryFilter, string? stockStatusFilter, int? pageIndex, int? pageSize)
    {
        SearchTerm = searchTerm;
        CategoryFilter = categoryFilter;
        StockStatusFilter = stockStatusFilter;
        PageIndex = pageIndex ?? 1;
        PageSize = pageSize ?? 25;

        // Get categories for filter
        Categories = await _context.Inventories
            .Select(i => i.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        // Calculate statistics
        Statistics = new InventoryStatistics
        {
            TotalItems = await _context.Inventories.CountAsync(),
            OutOfStock = await _context.Inventories.CountAsync(i => i.CurrentQuantity <= 0),
            LowStock = await _context.Inventories.CountAsync(i => i.CurrentQuantity > 0 && i.CurrentQuantity <= i.MinimumQuantity),
            InStock = await _context.Inventories.CountAsync(i => i.CurrentQuantity > i.MinimumQuantity && i.CurrentQuantity < i.MaximumQuantity),
            FullyStocked = await _context.Inventories.CountAsync(i => i.CurrentQuantity >= i.MaximumQuantity),
            TotalValue = await _context.Inventories.Where(i => i.UnitPrice.HasValue).SumAsync(i => i.CurrentQuantity * i.UnitPrice!.Value)
        };

        // Build query
        var query = _context.Inventories.AsQueryable();

        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            query = query.Where(i => i.ItemName.Contains(SearchTerm) ||
                                    i.SKU!.Contains(SearchTerm) ||
                                    i.Description!.Contains(SearchTerm) ||
                                    i.Category.Contains(SearchTerm));
        }

        if (!string.IsNullOrWhiteSpace(CategoryFilter))
        {
            query = query.Where(i => i.Category == CategoryFilter);
        }

        if (!string.IsNullOrWhiteSpace(StockStatusFilter))
        {
            if (Enum.TryParse<StockStatus>(StockStatusFilter, out var status))
            {
                query = query.Where(i => i.StockStatus == status);
            }
        }

        // Apply pagination
        Inventories = await PaginatedList<Data.Inventory>.CreateAsync(
            query.OrderBy(i => i.ItemName),
            PageIndex,
            PageSize
        );
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var inventory = await _context.Inventories.FindAsync(id);

        if (inventory == null)
        {
            return NotFound();
        }

        _context.Inventories.Remove(inventory);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Inventory item deleted successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteSelectedAsync(List<int> selectedIds)
    {
        if (selectedIds == null || !selectedIds.Any())
        {
            TempData["ErrorMessage"] = "No items selected to delete.";
            return RedirectToPage();
        }

        var itemsToDelete = await _context.Inventories
            .Where(i => selectedIds.Contains(i.Id))
            .ToListAsync();

        if (!itemsToDelete.Any())
        {
            return NotFound();
        }

        _context.Inventories.RemoveRange(itemsToDelete);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"{itemsToDelete.Count} inventory items deleted successfully.";
        return RedirectToPage();
    }

    public class InventoryStatistics
    {
        public int TotalItems { get; set; }
        public int OutOfStock { get; set; }
        public int LowStock { get; set; }
        public int InStock { get; set; }
        public int FullyStocked { get; set; }
        public decimal TotalValue { get; set; }
    }
}
