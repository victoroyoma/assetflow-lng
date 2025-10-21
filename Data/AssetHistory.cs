using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace buildone.Data;

public class AssetHistory
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Asset")]
    public int AssetId { get; set; }

    [Display(Name = "Actor")]
    public int? ActorId { get; set; }

    [Required(ErrorMessage = "Action is required")]
    [StringLength(100, ErrorMessage = "Action cannot exceed 100 characters")]
    [Display(Name = "Action")]
    public string Action { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "From value cannot exceed 500 characters")]
    [Display(Name = "From Value")]
    public string? FromValue { get; set; }

    [StringLength(500, ErrorMessage = "To value cannot exceed 500 characters")]
    [Display(Name = "To Value")]
    public string? ToValue { get; set; }

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    [Display(Name = "Created Date")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("AssetId")]
    public Asset Asset { get; set; } = null!;

    [ForeignKey("ActorId")]
    public Employee? Actor { get; set; }

    // Computed properties for display
    [Display(Name = "Change Description")]
    public string ChangeDescription => !string.IsNullOrEmpty(FromValue) && !string.IsNullOrEmpty(ToValue)
        ? $"{Action}: {FromValue} → {ToValue}"
        : Action;

    [Display(Name = "Actor Name")]
    public string ActorName => Actor?.FullName ?? "System";

    [Display(Name = "Has Change Values")]
    public bool HasChangeValues => !string.IsNullOrEmpty(FromValue) || !string.IsNullOrEmpty(ToValue);

    [Display(Name = "Formatted Created Date")]
    public string FormattedCreatedDate => CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
}