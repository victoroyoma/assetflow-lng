using System.ComponentModel.DataAnnotations;

namespace buildone.Data;

public class InventoryTransaction
{
    public int Id { get; set; }

    [Required]
    public int InventoryId { get; set; }
    public Inventory Inventory { get; set; } = null!;

    [Required]
    [StringLength(20)]
    public string TransactionType { get; set; } = string.Empty; // Restock, Withdrawal, Adjustment

    [Required]
    public int Quantity { get; set; }

    public int PreviousQuantity { get; set; }

    public int NewQuantity { get; set; }

    [StringLength(500)]
    public string? Remarks { get; set; }

    [StringLength(100)]
    public string? PerformedBy { get; set; }

    [Required]
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string? Reference { get; set; }
}
