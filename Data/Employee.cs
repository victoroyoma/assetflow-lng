using System.ComponentModel.DataAnnotations;

namespace buildone.Data;

public class Employee
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    [Display(Name = "Phone")]
    public string? Phone { get; set; }

    [Display(Name = "Department")]
    public int? DepartmentId { get; set; }

    // Navigation properties
    public Department? Department { get; set; }
    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    public ICollection<ImagingJob> ImagingJobs { get; set; } = new List<ImagingJob>();
    public ICollection<AssetHistory> AssetHistoryEntries { get; set; } = new List<AssetHistory>();
}