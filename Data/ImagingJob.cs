using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using buildone.Data.Enums;

namespace buildone.Data;

/// <summary>
/// Represents a job that can be either an Imaging job or a Maintenance job
/// </summary>
[Table("Jobs")]
public class ImagingJob
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Asset")]
    public int AssetId { get; set; }

    [Display(Name = "Technician")]
    public int? TechnicianId { get; set; }

    [Required]
    [Display(Name = "Job Type")]
    public JobType JobType { get; set; } = JobType.Imaging;

    [Display(Name = "Imaging Type")]
    public ImagingType? ImagingType { get; set; }

    [StringLength(50, ErrorMessage = "Image version cannot exceed 50 characters")]
    [Display(Name = "Image Version")]
    public string? ImageVersion { get; set; }

    [Required]
    [Display(Name = "Status")]
    public JobStatus Status { get; set; } = JobStatus.Pending;

    [Required]
    [Display(Name = "Priority")]
    public JobPriority Priority { get; set; } = JobPriority.Normal;

    [Display(Name = "Scheduled At")]
    public DateTime? ScheduledAt { get; set; }

    [Display(Name = "Due Date")]
    public DateTime? DueDate { get; set; }

    [Display(Name = "Estimated Duration (minutes)")]
    public int? EstimatedDurationMinutes { get; set; }

    [Display(Name = "Started At")]
    public DateTime? StartedAt { get; set; }

    [Display(Name = "Completed At")]
    public DateTime? CompletedAt { get; set; }

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    [Display(Name = "Created Date")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Updated Date")]
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey("AssetId")]
    public Asset Asset { get; set; } = null!;

    [ForeignKey("TechnicianId")]
    public Employee? Technician { get; set; }

    // Collection navigation properties
    public ICollection<JobComment> Comments { get; set; } = new List<JobComment>();
    public ICollection<JobAttachment> Attachments { get; set; } = new List<JobAttachment>();

    // Computed properties for display
    [Display(Name = "Duration")]
    public TimeSpan? Duration => StartedAt.HasValue && CompletedAt.HasValue 
        ? CompletedAt.Value - StartedAt.Value 
        : null;

    [Display(Name = "Job Description")]
    public string JobDescription => JobType == JobType.Maintenance 
        ? $"Maintenance - {Asset?.AssetTag ?? "Unknown Asset"}"
        : $"{ImagingType?.ToString() ?? "Imaging"} - {Asset?.AssetTag ?? "Unknown Asset"}";

    [Display(Name = "Is Overdue")]
    public bool IsOverdue => ScheduledAt.HasValue && 
                            ScheduledAt.Value < DateTime.Now && 
                            Status == JobStatus.Pending;

    [Display(Name = "Is In Progress")]
    public bool IsInProgress => Status == JobStatus.InProgress;

    [Display(Name = "Is Completed")]
    public bool IsCompleted => Status == JobStatus.Completed;

    [Display(Name = "Has Failed")]
    public bool HasFailed => Status == JobStatus.Failed;
}