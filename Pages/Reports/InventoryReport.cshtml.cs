using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using buildone.Data;
using buildone.Data.Enums;

namespace buildone.Pages.Reports
{
    public class InventoryReportModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public InventoryReportModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Inventory Statistics
        public int TotalItems { get; set; }
        public int OutOfStockItems { get; set; }
        public int LowStockItems { get; set; }
        public int InStockItems { get; set; }
        public int FullyStockedItems { get; set; }
        public int OverstockedItems { get; set; }

        // Warranty Statistics
        public int ItemsWithWarranty { get; set; }
        public int ActiveWarranties { get; set; }
        public int WarrantyExpiringSoon { get; set; }
        public int ExpiredWarranties { get; set; }

        // Inventory by Category
        public Dictionary<string, int> ItemsByCategory { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ItemsByStockStatus { get; set; } = new Dictionary<string, int>();

        // Lists for detailed views
        public List<Data.Inventory> OutOfStockList { get; set; } = new List<Data.Inventory>();
        public List<Data.Inventory> LowStockList { get; set; } = new List<Data.Inventory>();
        public List<Data.Inventory> WarrantyExpiringSoonList { get; set; } = new List<Data.Inventory>();
        public List<Data.Inventory> ExpiredWarrantyList { get; set; } = new List<Data.Inventory>();

        public async Task OnGetAsync()
        {
            try
            {
                var allInventory = await _context.Inventories.ToListAsync();

                // Calculate Stock Statistics
                TotalItems = allInventory.Count;
                OutOfStockItems = allInventory.Count(i => i.IsOutOfStock);
                LowStockItems = allInventory.Count(i => i.IsLowStock && !i.IsOutOfStock);
                InStockItems = allInventory.Count(i => i.StockStatus == StockStatus.InStock);
                FullyStockedItems = allInventory.Count(i => i.StockStatus == StockStatus.FullyStocked);
                OverstockedItems = allInventory.Count(i => i.IsOverstocked);

                // Calculate Warranty Statistics
                ItemsWithWarranty = allInventory.Count(i => i.WarrantyEndDate.HasValue);
                ActiveWarranties = allInventory.Count(i => i.HasWarranty);
                WarrantyExpiringSoon = allInventory.Count(i => i.IsWarrantyExpiringSoon);
                ExpiredWarranties = allInventory.Count(i => i.IsWarrantyExpired);

                // Group by Category
                ItemsByCategory = allInventory
                    .GroupBy(i => i.Category)
                    .ToDictionary(g => g.Key, g => g.Count())
                    .OrderByDescending(x => x.Value)
                    .ToDictionary(x => x.Key, x => x.Value);

                // Group by Stock Status
                ItemsByStockStatus = allInventory
                    .GroupBy(i => i.StockStatus.ToString())
                    .ToDictionary(g => g.Key, g => g.Count());

                // Get detailed lists
                OutOfStockList = allInventory
                    .Where(i => i.IsOutOfStock)
                    .OrderBy(i => i.ItemName)
                    .ToList();

                LowStockList = allInventory
                    .Where(i => i.IsLowStock && !i.IsOutOfStock)
                    .OrderBy(i => i.CurrentQuantity)
                    .ToList();

                WarrantyExpiringSoonList = allInventory
                    .Where(i => i.IsWarrantyExpiringSoon)
                    .OrderBy(i => i.WarrantyEndDate)
                    .ToList();

                ExpiredWarrantyList = allInventory
                    .Where(i => i.IsWarrantyExpired)
                    .OrderByDescending(i => i.WarrantyEndDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error loading inventory report: {ex.Message}");
            }
        }
    }
}
