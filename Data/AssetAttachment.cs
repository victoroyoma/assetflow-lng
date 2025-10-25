using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace buildone.Data;

public class AssetAttachment
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Asset")]
    public int AssetId { get; set; }

    [Required]
    [StringLength(255)]
    [Display(Name = "File Name")]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    [Display(Name = "File Path")]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Content Type")]
    public string ContentType { get; set; } = string.Empty;

    [Required]
    [Display(Name = "File Size (bytes)")]
    public long FileSizeBytes { get; set; }

    [StringLength(500)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Uploaded By")]
    public string UploadedBy { get; set; } = string.Empty;

    [Display(Name = "Upload Date")]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Is Image")]
    public bool IsImage => ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

    [Display(Name = "Is Document")]
    public bool IsDocument => ContentType.Contains("pdf") || 
                              ContentType.Contains("word") || 
                              ContentType.Contains("excel") || 
                              ContentType.Contains("text");

    // Navigation properties
    [ForeignKey("AssetId")]
    public Asset Asset { get; set; } = null!;

    // Computed properties
    [Display(Name = "File Size")]
    public string FormattedFileSize
    {
        get
        {
            if (FileSizeBytes < 1024)
                return $"{FileSizeBytes} B";
            else if (FileSizeBytes < 1024 * 1024)
                return $"{FileSizeBytes / 1024.0:F2} KB";
            else
                return $"{FileSizeBytes / (1024.0 * 1024.0):F2} MB";
        }
    }
}
