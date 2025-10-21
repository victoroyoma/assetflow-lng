using buildone.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace buildone.DTOs;

/// <summary>
/// DTO for Asset responses
/// </summary>
public class AssetResponseDto
{
    public int Id { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string PcId { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string Type { get; set; } = "Desktop";
    public DateTime? WarrantyExpiry { get; set; }
    public ImagingType ImagingType { get; set; }
    public DeploymentType DeploymentType { get; set; }
    public AssetStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties (basic info only)
    public EmployeeBasicDto? AssignedEmployee { get; set; }
    public DepartmentBasicDto? Department { get; set; }
}

/// <summary>
/// DTO for creating assets
/// </summary>
public class CreateAssetDto
{
    [Required(ErrorMessage = "Asset tag is required")]
    [StringLength(50)]
    public string AssetTag { get; set; } = string.Empty;

    [Required(ErrorMessage = "PC ID is required")]
    [StringLength(50)]
    public string PcId { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Brand { get; set; }

    [StringLength(100)]
    public string? Model { get; set; }

    [StringLength(100)]
    public string? SerialNumber { get; set; }

    [StringLength(50)]
    public string Type { get; set; } = "Desktop";

    public DateTime? WarrantyExpiry { get; set; }
    public ImagingType ImagingType { get; set; } = ImagingType.None;
    public DeploymentType DeploymentType { get; set; } = DeploymentType.InPlace;
    public AssetStatus Status { get; set; } = AssetStatus.InStock;
    public int? AssignedEmployeeId { get; set; }
    public int? DepartmentId { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating assets
/// </summary>
public class UpdateAssetDto : CreateAssetDto
{
    public int Id { get; set; }
}

/// <summary>
/// Basic employee information for nested DTOs
/// </summary>
public class EmployeeBasicDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
}

/// <summary>
/// Basic department information for nested DTOs
/// </summary>
public class DepartmentBasicDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
}
