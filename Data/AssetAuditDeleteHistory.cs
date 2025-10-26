using System.ComponentModel.DataAnnotations;
using buildone.Data.Enums;

namespace buildone.Data;

/// <summary>
/// Tracks history of deleted asset audit records for audit trail purposes
/// </summary>
public class AssetAuditDeleteHistory
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Original Audit ID")]
    public int OriginalAuditId { get; set; }

    [Required]
    [Display(Name = "Audit Date")]
    public DateTime AuditDate { get; set; }

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

    [Display(Name = "Is New Asset")]
    public bool IsNewAsset { get; set; }

    [Display(Name = "Previous Status")]
    public AssetStatus? PreviousStatus { get; set; }

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

    [StringLength(50)]
    [Display(Name = "Audit Session ID")]
    public string? AuditSessionId { get; set; }

    [Display(Name = "Original Created At")]
    public DateTime OriginalCreatedAt { get; set; }

    [Required]
    [Display(Name = "Deleted At")]
    public DateTime DeletedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(100)]
    [Display(Name = "Deleted By")]
    public string DeletedBy { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Deletion Reason")]
    public string? DeletionReason { get; set; }
}
