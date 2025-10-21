using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace buildone.Data;

public class ApplicationRole : IdentityRole
{
    [StringLength(200)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;

    public ApplicationRole() : base()
    {
    }

    public ApplicationRole(string roleName) : base(roleName)
    {
    }

    public ApplicationRole(string roleName, string description) : base(roleName)
    {
        Description = description;
    }
}