namespace buildone.DTOs;

public class InventoryDto
{
    public int Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public int CurrentQuantity { get; set; }
    public int MinimumQuantity { get; set; }
    public int MaximumQuantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public string Unit { get; set; } = "pcs";
    public string StockStatus { get; set; } = string.Empty;
    public string? StorageLocation { get; set; }
    public string? Supplier { get; set; }
    public DateTime? LastRestocked { get; set; }
    public bool IsLowStock { get; set; }
    public bool IsOutOfStock { get; set; }
    public bool IsOverstocked { get; set; }
}

public class CreateInventoryDto
{
    public string ItemName { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public int CurrentQuantity { get; set; }
    public int MinimumQuantity { get; set; } = 10;
    public int MaximumQuantity { get; set; } = 100;
    public decimal? UnitPrice { get; set; }
    public string Unit { get; set; } = "pcs";
    public string? StorageLocation { get; set; }
    public string? Supplier { get; set; }
}

public class UpdateInventoryDto
{
    public string ItemName { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public int MinimumQuantity { get; set; }
    public int MaximumQuantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public string Unit { get; set; } = "pcs";
    public string? StorageLocation { get; set; }
    public string? Supplier { get; set; }
}

public class RestockInventoryDto
{
    public int Quantity { get; set; }
    public string? Remarks { get; set; }
    public string? Reference { get; set; }
}

public class AdjustInventoryDto
{
    public int NewQuantity { get; set; }
    public string? Remarks { get; set; }
}

public class WithdrawInventoryDto
{
    public int Quantity { get; set; }
    public string Remarks { get; set; } = string.Empty;
}

public class InventoryStatisticsDto
{
    public int TotalItems { get; set; }
    public int OutOfStockItems { get; set; }
    public int LowStockItems { get; set; }
    public int InStockItems { get; set; }
    public int FullyStockedItems { get; set; }
    public decimal TotalValue { get; set; }
}
