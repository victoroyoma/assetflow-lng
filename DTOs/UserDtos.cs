using System.ComponentModel.DataAnnotations;

namespace buildone.DTOs;

/// <summary>
/// DTO for user responses
/// </summary>
public class UserResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<string> Roles { get; set; } = new();
    public EmployeeBasicDto? Employee { get; set; }
}

/// <summary>
/// DTO for creating users
/// </summary>
public class CreateUserDto
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    public int? EmployeeId { get; set; }
    public List<string> Roles { get; set; } = new();
}

/// <summary>
/// DTO for updating users
/// </summary>
public class UpdateUserDto
{
    public string Id { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }

    public bool IsActive { get; set; }
    public int? EmployeeId { get; set; }
    public List<string> Roles { get; set; } = new();
}
