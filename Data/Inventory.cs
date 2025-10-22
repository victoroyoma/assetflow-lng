using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using buildone.Data.Enums;

namespace buildone.Data;

public class Inventory
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Item name is required")]
    [StringLength(100, ErrorMessage = "Item name cannot exceed 100 characters")]
    [Display(Name = "Item Name")]
    public string ItemName { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "SKU cannot exceed 50 characters")]
    [Display(Name = "SKU")]
    public string? SKU { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Category is required")]
    [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
    [Display(Name = "Category")]
    public string Category { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Current Quantity")]
    public int CurrentQuantity { get; set; }

    [Required]
    [Display(Name = "Minimum Quantity")]
    public int MinimumQuantity { get; set; } = 10;

    [Required]
    [Display(Name = "Maximum Quantity")]
    public int MaximumQuantity { get; set; } = 100;

    [Display(Name = "Unit Price")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? UnitPrice { get; set; }

    [StringLength(20, ErrorMessage = "Unit cannot exceed 20 characters")]
    [Display(Name = "Unit")]
    public string Unit { get; set; } = "pcs";

    [Display(Name = "Stock Status")]
    public StockStatus StockStatus { get; set; } = StockStatus.InStock;

    [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters")]
    [Display(Name = "Storage Location")]
    public string? StorageLocation { get; set; }

    [StringLength(100, ErrorMessage = "Supplier cannot exceed 100 characters")]
    [Display(Name = "Supplier")]
    public string? Supplier { get; set; }

    [Display(Name = "Warranty Start Date")]
    public DateTime? WarrantyStartDate { get; set; }

    [Display(Name = "Warranty End Date")]
    public DateTime? WarrantyEndDate { get; set; }

    [Display(Name = "Warranty Period (Months)")]
    public int? WarrantyPeriodMonths { get; set; }

    [StringLength(200, ErrorMessage = "Warranty provider cannot exceed 200 characters")]
    [Display(Name = "Warranty Provider")]
    public string? WarrantyProvider { get; set; }

    [Display(Name = "Last Restocked")]
    public DateTime? LastRestocked { get; set; }

    [Display(Name = "Created Date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Display(Name = "Last Updated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    [StringLength(100, ErrorMessage = "Updated by cannot exceed 100 characters")]
    public string? UpdatedBy { get; set; }

    // Computed properties
    public bool IsLowStock => CurrentQuantity <= MinimumQuantity;
    public bool IsOutOfStock => CurrentQuantity <= 0;
    public bool IsOverstocked => CurrentQuantity >= MaximumQuantity;
    
    [NotMapped]
    public bool HasWarranty => WarrantyEndDate.HasValue && WarrantyEndDate.Value > DateTime.UtcNow;
    
    [NotMapped]
    public bool IsWarrantyExpiringSoon => WarrantyEndDate.HasValue && 
                                          WarrantyEndDate.Value > DateTime.UtcNow && 
                                          WarrantyEndDate.Value <= DateTime.UtcNow.AddMonths(3);
    
    [NotMapped]
    public bool IsWarrantyExpired => WarrantyEndDate.HasValue && WarrantyEndDate.Value <= DateTime.UtcNow;
    
    [NotMapped]
    public int? DaysUntilWarrantyExpiry => WarrantyEndDate.HasValue 
        ? (int?)(WarrantyEndDate.Value - DateTime.UtcNow).TotalDays 
        : null;
}
