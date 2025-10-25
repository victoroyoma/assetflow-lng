using buildone.Authorization;
using buildone.Data;
using buildone.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace buildone.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class AttachmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<AttachmentsController> _logger;

    // Allowed file extensions
    private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
    private readonly string[] _allowedDocumentExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".csv" };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    public AttachmentsController(
        ApplicationDbContext context,
        IFileStorageService fileStorageService,
        ILogger<AttachmentsController> logger)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    /// <summary>
    /// Upload attachment for an imaging job
    /// </summary>
    [HttpPost("job/{jobId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadJobAttachment(int jobId, [FromForm] IFormFile file, [FromForm] string? description)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file uploaded" });

        if (file.Length > MaxFileSize)
            return BadRequest(new { error = $"File size exceeds maximum limit of {MaxFileSize / (1024 * 1024)} MB" });

        // Validate file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allAllowedExtensions = _allowedImageExtensions.Concat(_allowedDocumentExtensions).ToArray();
        if (!allAllowedExtensions.Contains(extension))
            return BadRequest(new { error = "File type not allowed" });

        // Check if job exists
        var job = await _context.ImagingJobs.FindAsync(jobId);
        if (job == null)
            return NotFound(new { error = "Job not found" });

        try
        {
            // Save file
            var filePath = await _fileStorageService.SaveFileAsync(file, $"jobs/{jobId}");

            // Create attachment record
            var attachment = new JobAttachment
            {
                JobId = jobId,
                FileName = file.FileName,
                FilePath = filePath,
                ContentType = file.ContentType,
                FileSizeBytes = file.Length,
                Description = description,
                UploadedBy = User.Identity?.Name ?? "Unknown",
                UploadedAt = DateTime.UtcNow
            };

            _context.JobAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("File attached to job {JobId}: {FileName}", jobId, file.FileName);

            return Ok(new
            {
                id = attachment.Id,
                fileName = attachment.FileName,
                fileSize = attachment.FormattedFileSize,
                uploadedAt = attachment.UploadedAt,
                message = "File uploaded successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file for job {JobId}", jobId);
            return StatusCode(500, new { error = "Failed to upload file", details = ex.Message });
        }
    }

    /// <summary>
    /// Upload attachment for an asset
    /// </summary>
    [HttpPost("asset/{assetId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadAssetAttachment(int assetId, [FromForm] IFormFile file, [FromForm] string? description)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file uploaded" });

        if (file.Length > MaxFileSize)
            return BadRequest(new { error = $"File size exceeds maximum limit of {MaxFileSize / (1024 * 1024)} MB" });

        // Validate file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allAllowedExtensions = _allowedImageExtensions.Concat(_allowedDocumentExtensions).ToArray();
        if (!allAllowedExtensions.Contains(extension))
            return BadRequest(new { error = "File type not allowed" });

        // Check if asset exists
        var asset = await _context.Assets.FindAsync(assetId);
        if (asset == null)
            return NotFound(new { error = "Asset not found" });

        try
        {
            // Save file
            var filePath = await _fileStorageService.SaveFileAsync(file, $"assets/{assetId}");

            // Create attachment record
            var attachment = new AssetAttachment
            {
                AssetId = assetId,
                FileName = file.FileName,
                FilePath = filePath,
                ContentType = file.ContentType,
                FileSizeBytes = file.Length,
                Description = description,
                UploadedBy = User.Identity?.Name ?? "Unknown",
                UploadedAt = DateTime.UtcNow
            };

            _context.AssetAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("File attached to asset {AssetId}: {FileName}", assetId, file.FileName);

            return Ok(new
            {
                id = attachment.Id,
                fileName = attachment.FileName,
                fileSize = attachment.FormattedFileSize,
                uploadedAt = attachment.UploadedAt,
                message = "File uploaded successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file for asset {AssetId}", assetId);
            return StatusCode(500, new { error = "Failed to upload file", details = ex.Message });
        }
    }

    /// <summary>
    /// Get job attachments
    /// </summary>
    [HttpGet("job/{jobId}")]
    public async Task<IActionResult> GetJobAttachments(int jobId)
    {
        var attachments = await _context.JobAttachments
            .Where(a => a.JobId == jobId)
            .OrderByDescending(a => a.UploadedAt)
            .Select(a => new
            {
                a.Id,
                a.FileName,
                FileSize = a.FormattedFileSize,
                a.ContentType,
                a.Description,
                a.UploadedBy,
                a.UploadedAt,
                a.IsImage,
                a.IsDocument,
                DownloadUrl = $"/api/attachments/download/job/{a.Id}"
            })
            .ToListAsync();

        return Ok(attachments);
    }

    /// <summary>
    /// Get asset attachments
    /// </summary>
    [HttpGet("asset/{assetId}")]
    public async Task<IActionResult> GetAssetAttachments(int assetId)
    {
        var attachments = await _context.AssetAttachments
            .Where(a => a.AssetId == assetId)
            .OrderByDescending(a => a.UploadedAt)
            .Select(a => new
            {
                a.Id,
                a.FileName,
                FileSize = a.FormattedFileSize,
                a.ContentType,
                a.Description,
                a.UploadedBy,
                a.UploadedAt,
                a.IsImage,
                a.IsDocument,
                DownloadUrl = $"/api/attachments/download/asset/{a.Id}"
            })
            .ToListAsync();

        return Ok(attachments);
    }

    /// <summary>
    /// Download job attachment
    /// </summary>
    [HttpGet("download/job/{attachmentId}")]
    public async Task<IActionResult> DownloadJobAttachment(int attachmentId)
    {
        var attachment = await _context.JobAttachments.FindAsync(attachmentId);
        if (attachment == null)
            return NotFound();

        try
        {
            var stream = await _fileStorageService.GetFileStreamAsync(attachment.FilePath);
            return File(stream, attachment.ContentType, attachment.FileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { error = "File not found on disk" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading attachment {AttachmentId}", attachmentId);
            return StatusCode(500, new { error = "Failed to download file" });
        }
    }

    /// <summary>
    /// Download asset attachment
    /// </summary>
    [HttpGet("download/asset/{attachmentId}")]
    public async Task<IActionResult> DownloadAssetAttachment(int attachmentId)
    {
        var attachment = await _context.AssetAttachments.FindAsync(attachmentId);
        if (attachment == null)
            return NotFound();

        try
        {
            var stream = await _fileStorageService.GetFileStreamAsync(attachment.FilePath);
            return File(stream, attachment.ContentType, attachment.FileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { error = "File not found on disk" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading attachment {AttachmentId}", attachmentId);
            return StatusCode(500, new { error = "Failed to download file" });
        }
    }

    /// <summary>
    /// Delete job attachment
    /// </summary>
    [HttpDelete("job/{attachmentId}")]
    [Authorize(Policy = Policies.CanManageImagingJobs)]
    public async Task<IActionResult> DeleteJobAttachment(int attachmentId)
    {
        var attachment = await _context.JobAttachments.FindAsync(attachmentId);
        if (attachment == null)
            return NotFound();

        try
        {
            // Delete file from storage
            await _fileStorageService.DeleteFileAsync(attachment.FilePath);

            // Delete database record
            _context.JobAttachments.Remove(attachment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted job attachment {AttachmentId}", attachmentId);
            return Ok(new { message = "Attachment deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job attachment {AttachmentId}", attachmentId);
            return StatusCode(500, new { error = "Failed to delete attachment" });
        }
    }

    /// <summary>
    /// Delete asset attachment
    /// </summary>
    [HttpDelete("asset/{attachmentId}")]
    [Authorize(Policy = Policies.CanManageAssets)]
    public async Task<IActionResult> DeleteAssetAttachment(int attachmentId)
    {
        var attachment = await _context.AssetAttachments.FindAsync(attachmentId);
        if (attachment == null)
            return NotFound();

        try
        {
            // Delete file from storage
            await _fileStorageService.DeleteFileAsync(attachment.FilePath);

            // Delete database record
            _context.AssetAttachments.Remove(attachment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted asset attachment {AttachmentId}", attachmentId);
            return Ok(new { message = "Attachment deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting asset attachment {AttachmentId}", attachmentId);
            return StatusCode(500, new { error = "Failed to delete attachment" });
        }
    }
}
