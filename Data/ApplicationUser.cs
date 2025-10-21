using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace buildone.Data;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Phone { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation property to Employee (optional - if you want to link users to employees)
    public int? EmployeeId { get; set; }
    public virtual Employee? Employee { get; set; }
}