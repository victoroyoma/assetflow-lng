using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using buildone.Data.Enums;

namespace buildone.Data;

public class Asset
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Asset tag is required")]
    [StringLength(50, ErrorMessage = "Asset tag cannot exceed 50 characters")]
    [Display(Name = "Asset Tag")]
    public string AssetTag { get; set; } = string.Empty;

    [Required(ErrorMessage = "PC ID is required")]
    [StringLength(50, ErrorMessage = "PC ID cannot exceed 50 characters")]
    [Display(Name = "PC ID")]
    public string PcId { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Brand cannot exceed 50 characters")]
    [Display(Name = "Brand")]
    public string? Brand { get; set; }

    [StringLength(100, ErrorMessage = "Model cannot exceed 100 characters")]
    [Display(Name = "Model")]
    public string? Model { get; set; }

    [StringLength(100, ErrorMessage = "Serial number cannot exceed 100 characters")]
    [Display(Name = "Serial Number")]
    public string? SerialNumber { get; set; }

    [StringLength(50, ErrorMessage = "Asset type cannot exceed 50 characters")]
    [Display(Name = "Asset Type")]
    public string Type { get; set; } = "Desktop";

    [Display(Name = "Warranty Expiry")]
    public DateTime? WarrantyExpiry { get; set; }

    [Required]
    [Display(Name = "Imaging Type")]
    public ImagingType ImagingType { get; set; } = ImagingType.None;

    [Required]
    [Display(Name = "Deployment Type")]
    public DeploymentType DeploymentType { get; set; } = DeploymentType.InPlace;

    [Required]
    [Display(Name = "Status")]
    public AssetStatus Status { get; set; } = AssetStatus.InStock;

    [Display(Name = "Assigned Employee")]
    public int? AssignedEmployeeId { get; set; }

    [Display(Name = "Department")]
    public int? DepartmentId { get; set; }

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    [Display(Name = "Created Date")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Updated Date")]
    public DateTime? UpdatedAt { get; set; }

    [Display(Name = "Last Updated")]
    public DateTime LastUpdated => UpdatedAt ?? CreatedAt;

    // Navigation properties
    [ForeignKey("AssignedEmployeeId")]
    public Employee? AssignedEmployee { get; set; }

    [ForeignKey("DepartmentId")]
    public Department? Department { get; set; }

    public ICollection<ImagingJob> ImagingJobs { get; set; } = new List<ImagingJob>();
    public ICollection<AssetHistory> History { get; set; } = new List<AssetHistory>();
    public ICollection<AssetAttachment> Attachments { get; set; } = new List<AssetAttachment>();

    // Computed properties for display
    [Display(Name = "Asset Description")]
    public string AssetDescription => !string.IsNullOrEmpty(Brand) && !string.IsNullOrEmpty(Model) 
        ? $"{Brand} {Model}" 
        : AssetTag;

    [Display(Name = "Assignment Info")]
    public string AssignmentInfo => AssignedEmployee != null 
        ? $"{AssignedEmployee.FullName} ({Department?.Name ?? "No Department"})" 
        : "Unassigned";
}