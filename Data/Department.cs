using System.ComponentModel.DataAnnotations;

namespace buildone.Data;

public class Department
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Department name is required")]
    [StringLength(100, ErrorMessage = "Department name cannot exceed 100 characters")]
    [Display(Name = "Department Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(10, ErrorMessage = "Department code cannot exceed 10 characters")]
    [Display(Name = "Department Code")]
    public string? Code { get; set; }

    // Navigation properties
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
}