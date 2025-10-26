using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using buildone.Data.Enums;

namespace buildone.Data;

/// <summary>
/// Represents an asset audit record tracking physical verification of assets
/// </summary>
public class AssetAudit
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Audit Date")]
    public DateTime AuditDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Asset ID if the asset exists, null if it's a new discovery
    /// </summary>
    [Display(Name = "Asset ID")]
    public int? AssetId { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Asset Tag")]
    public string AssetTag { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Display(Name = "Asset Type")]
    public string AssetType { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Status")]
    public AssetStatus Status { get; set; }

    [StringLength(100)]
    [Display(Name = "Location")]
    public string? Location { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Audited By")]
    public string AuditedBy { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    /// <summary>
    /// Indicates if this asset was newly discovered during audit (not in system)
    /// </summary>
    [Display(Name = "Is New Asset")]
    public bool IsNewAsset { get; set; }

    /// <summary>
    /// Previous status before audit (for comparison)
    /// </summary>
    [Display(Name = "Previous Status")]
    public AssetStatus? PreviousStatus { get; set; }

    /// <summary>
    /// Previous location before audit (for comparison)
    /// </summary>
    [StringLength(100)]
    [Display(Name = "Previous Location")]
    public string? PreviousLocation { get; set; }

    [StringLength(100)]
    [Display(Name = "Serial Number")]
    public string? SerialNumber { get; set; }

    [StringLength(100)]
    [Display(Name = "Brand")]
    public string? Brand { get; set; }

    [StringLength(100)]
    [Display(Name = "Model")]
    public string? Model { get; set; }

    /// <summary>
    /// Audit session ID to group multiple scans in one audit session
    /// </summary>
    [StringLength(50)]
    [Display(Name = "Audit Session ID")]
    public string? AuditSessionId { get; set; }

    [Display(Name = "Created At")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Soft delete flag - true if record has been deleted from audit
    /// </summary>
    [Display(Name = "Is Deleted")]
    public bool IsDeleted { get; set; } = false;

    [Display(Name = "Deleted At")]
    public DateTime? DeletedAt { get; set; }

    [StringLength(100)]
    [Display(Name = "Deleted By")]
    public string? DeletedBy { get; set; }

    // Navigation property
    public virtual Asset? Asset { get; set; }
}
