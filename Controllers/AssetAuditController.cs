using buildone.Authorization;
using buildone.Data;
using buildone.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace buildone.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class AssetAuditController : ControllerBase
{
    private readonly IAssetAuditService _auditService;
    private readonly ILogger<AssetAuditController> _logger;

    public AssetAuditController(IAssetAuditService auditService, ILogger<AssetAuditController> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Process a scanned asset during audit
    /// </summary>
    [HttpPost("process-scan")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessScannedAsset([FromBody] ProcessScanRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ScannedCode))
            return BadRequest(new { error = "Scanned code is required" });

        if (string.IsNullOrWhiteSpace(request.AuditedBy))
            return BadRequest(new { error = "Audited by is required" });

        try
        {
            var result = await _auditService.ProcessScannedAssetAsync(
                request.ScannedCode,
                request.AuditedBy,
                request.Location,
                request.Notes,
                request.SessionId
            );

            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message,
                isNew = result.IsNew,
                statusChanged = result.StatusChanged,
                locationChanged = result.LocationChanged,
                audit = new
                {
                    result.Audit?.Id,
                    result.Audit?.AssetTag,
                    result.Audit?.AssetType,
                    result.Audit?.Status,
                    result.Audit?.Location,
                    result.Audit?.IsNewAsset,
                    result.Audit?.PreviousLocation
                },
                asset = result.ExistingAsset != null ? new
                {
                    result.ExistingAsset.Id,
                    result.ExistingAsset.AssetTag,
                    result.ExistingAsset.Brand,
                    result.ExistingAsset.Model,
                    result.ExistingAsset.SerialNumber,
                    result.ExistingAsset.Type,
                    result.ExistingAsset.Status,
                    result.ExistingAsset.Location
                } : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing scanned asset: {ScannedCode}", request.ScannedCode);
            return StatusCode(500, new { error = "An error occurred while processing the scanned asset" });
        }
    }

    /// <summary>
    /// Get audits by session ID
    /// </summary>
    [HttpGet("session/{sessionId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditsBySession(string sessionId)
    {
        try
        {
            var audits = await _auditService.GetAuditsBySessionAsync(sessionId);
            
            return Ok(new
            {
                success = true,
                data = audits.Select(a => new
                {
                    a.Id,
                    a.AssetTag,
                    a.AssetType,
                    a.Status,
                    a.Location,
                    a.AuditedBy,
                    a.IsNewAsset,
                    a.PreviousStatus,
                    a.PreviousLocation,
                    a.Brand,
                    a.Model,
                    a.SerialNumber,
                    a.Notes,
                    a.AuditDate,
                    Asset = a.Asset != null ? new
                    {
                        a.Asset.Id,
                        a.Asset.AssetTag,
                        a.Asset.Brand,
                        a.Asset.Model,
                        a.Asset.Type
                    } : null
                }),
                count = audits.Count()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audits for session: {SessionId}", sessionId);
            return StatusCode(500, new { error = "An error occurred while retrieving audits" });
        }
    }

    /// <summary>
    /// Get audit statistics
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditStatistics([FromQuery] string? sessionId = null)
    {
        try
        {
            var stats = await _auditService.GetAuditStatisticsAsync(sessionId);
            
            return Ok(new
            {
                success = true,
                data = stats
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit statistics");
            return StatusCode(500, new { error = "An error occurred while retrieving statistics" });
        }
    }

    /// <summary>
    /// Get audits by date range
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAudits(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? sessionId = null)
    {
        try
        {
            var audits = await _auditService.GetAuditsAsync(startDate, endDate, sessionId);
            
            return Ok(new
            {
                success = true,
                data = audits.Select(a => new
                {
                    a.Id,
                    a.AssetTag,
                    a.AssetType,
                    a.Status,
                    a.Location,
                    a.AuditedBy,
                    a.IsNewAsset,
                    a.AuditSessionId,
                    a.AuditDate
                }),
                count = audits.Count()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audits");
            return StatusCode(500, new { error = "An error occurred while retrieving audits" });
        }
    }

    /// <summary>
    /// Get all assets for audit with their current audit status
    /// </summary>
    [HttpGet("assets")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAssetsForAudit([FromQuery] string? sessionId = null)
    {
        try
        {
            var assets = await _auditService.GetAllAssetsForAuditAsync(sessionId);
            
            return Ok(new
            {
                success = true,
                data = assets,
                count = assets.Count(),
                summary = new
                {
                    total = assets.Count(),
                    audited = assets.Count(a => a.AuditedInCurrentSession),
                    notAudited = assets.Count(a => !a.AuditedInCurrentSession),
                    everAudited = assets.Count(a => a.IsAudited)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assets for audit");
            return StatusCode(500, new { error = "An error occurred while retrieving assets" });
        }
    }

    /// <summary>
    /// Update asset status and location during audit
    /// </summary>
    [HttpPut("update-status/{assetId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAssetStatus(int assetId, [FromBody] UpdateAssetStatusRequest request)
    {
        try
        {
            var result = await _auditService.UpdateAssetStatusAsync(
                assetId,
                request.Status,
                request.Location,
                request.AuditedBy,
                request.Notes,
                request.SessionId
            );

            if (!result.Success)
            {
                if (result.Message.Contains("not found"))
                    return NotFound(new { error = result.Message });
                
                return BadRequest(new { error = result.Message });
            }

            return Ok(new
            {
                success = true,
                message = result.Message,
                assetId = result.ExistingAsset?.Id,
                assetTag = result.ExistingAsset?.AssetTag,
                statusChanged = result.StatusChanged,
                locationChanged = result.LocationChanged,
                audit = new
                {
                    result.Audit?.Id,
                    result.Audit?.AuditDate,
                    result.Audit?.AuditSessionId
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating asset status: {AssetId}", assetId);
            return StatusCode(500, new { error = "An error occurred while updating asset" });
        }
    }

    /// <summary>
    /// Update an audit record
    /// </summary>
    [HttpPut("update-audit/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAuditRecord(int id, [FromBody] UpdateAuditRequest request)
    {
        try
        {
            var success = await _auditService.UpdateAuditRecordAsync(
                id,
                request.Status,
                request.Location,
                request.Notes,
                User.Identity?.Name ?? "System"
            );

            if (!success)
                return NotFound(new { error = "Audit record not found or deleted" });

            return Ok(new { success = true, message = "Audit record updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating audit record: {AuditId}", id);
            return StatusCode(500, new { error = "An error occurred while updating audit record" });
        }
    }

    /// <summary>
    /// Delete an audit record (soft delete with history)
    /// </summary>
    [HttpDelete("delete-audit/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAuditRecord(int id, [FromBody] DeleteAuditRequest? request)
    {
        try
        {
            var success = await _auditService.DeleteAuditRecordAsync(
                id,
                User.Identity?.Name ?? "System",
                request?.DeletionReason
            );

            if (!success)
                return NotFound(new { error = "Audit record not found or already deleted" });

            return Ok(new { success = true, message = "Audit record deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting audit record: {AuditId}", id);
            return StatusCode(500, new { error = "An error occurred while deleting audit record" });
        }
    }

    /// <summary>
    /// Get deleted audit history
    /// </summary>
    [HttpGet("deleted-history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeletedHistory([FromQuery] string? sessionId)
    {
        try
        {
            var history = await _auditService.GetDeletedAuditHistoryAsync(sessionId);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving deleted audit history");
            return StatusCode(500, new { error = "An error occurred while retrieving deleted history" });
        }
    }

    /// <summary>
    /// Clear all audit records (hard delete all audits and audit delete history)
    /// </summary>
    [HttpDelete("clear-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearAllAudits()
    {
        try
        {
            var success = await _auditService.ClearAllAuditsAsync(User.Identity?.Name ?? "System");
            
            if (!success)
                return StatusCode(500, new { error = "Failed to clear audit records" });

            return Ok(new { success = true, message = "All audit records cleared successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing all audit records");
            return StatusCode(500, new { error = "An error occurred while clearing audit records" });
        }
    }
}

public class ProcessScanRequest
{
    public string ScannedCode { get; set; } = string.Empty;
    public string AuditedBy { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public string? SessionId { get; set; }
}

public class UpdateAssetStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string AuditedBy { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? SessionId { get; set; }
}

public class UpdateAuditRequest
{
    public string Status { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Notes { get; set; }
}

public class DeleteAuditRequest
{
    public string? DeletionReason { get; set; }
}


