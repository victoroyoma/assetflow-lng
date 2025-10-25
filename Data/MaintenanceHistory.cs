using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace buildone.Data;

/// <summary>
/// Tracks complete history of all maintenance activities for audit and record purposes
/// </summary>
public class MaintenanceHistory
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Job")]
    public int JobId { get; set; }

    [Required]
    [Display(Name = "Asset")]
    public int AssetId { get; set; }

    [Display(Name = "Technician")]
    public int? TechnicianId { get; set; }

    [Required]
    [StringLength(200)]
    [Display(Name = "Maintenance Type")]
    public string MaintenanceType { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [StringLength(2000)]
    [Display(Name = "Actions Taken")]
    public string? ActionsTaken { get; set; }

    [StringLength(1000)]
    [Display(Name = "Parts Replaced")]
    public string? PartsReplaced { get; set; }

    [StringLength(1000)]
    [Display(Name = "Parts Cost")]
    public decimal? PartsCost { get; set; }

    [Display(Name = "Labor Hours")]
    public decimal? LaborHours { get; set; }

    [Required]
    [Display(Name = "Status Before")]
    public string StatusBefore { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Status After")]
    public string StatusAfter { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Maintenance Date")]
    public DateTime MaintenanceDate { get; set; }

    [Display(Name = "Completion Date")]
    public DateTime? CompletionDate { get; set; }

    [Display(Name = "Next Scheduled Maintenance")]
    public DateTime? NextMaintenanceDate { get; set; }

    [StringLength(1000)]
    [Display(Name = "Recommendations")]
    public string? Recommendations { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Performed By")]
    public string PerformedBy { get; set; } = string.Empty;

    [Display(Name = "Created Date")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Has Attachments")]
    public bool HasAttachments { get; set; }

    [Display(Name = "Attachment Count")]
    public int AttachmentCount { get; set; }

    // Navigation properties
    [ForeignKey("JobId")]
    public ImagingJob Job { get; set; } = null!;

    [ForeignKey("AssetId")]
    public Asset Asset { get; set; } = null!;

    [ForeignKey("TechnicianId")]
    public Employee? Technician { get; set; }
}
