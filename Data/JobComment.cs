using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace buildone.Data;

public class JobComment
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Job")]
    public int ImagingJobId { get; set; }

    [Required]
    [Display(Name = "Employee")]
    public int EmployeeId { get; set; }

    [Required]
    [StringLength(2000, ErrorMessage = "Comment cannot exceed 2000 characters")]
    [Display(Name = "Comment")]
    public string Comment { get; set; } = string.Empty;

    [Display(Name = "Created Date")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Is System Comment")]
    public bool IsSystemGenerated { get; set; } = false;

    // Navigation properties
    [ForeignKey("ImagingJobId")]
    public ImagingJob ImagingJob { get; set; } = null!;

    [ForeignKey("EmployeeId")]
    public Employee Employee { get; set; } = null!;
}
