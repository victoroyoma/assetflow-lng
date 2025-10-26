using buildone.Data;
using buildone.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace buildone.Services;

public class AssetAuditService : IAssetAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AssetAuditService> _logger;

    public AssetAuditService(ApplicationDbContext context, ILogger<AssetAuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AssetAudit> CreateAuditAsync(AssetAudit audit)
    {
        _context.AssetAudits.Add(audit);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Asset audit created: {AssetTag} by {AuditedBy}", audit.AssetTag, audit.AuditedBy);
        return audit;
    }

    public async Task<IEnumerable<AssetAudit>> GetAuditsAsync(DateTime? startDate = null, DateTime? endDate = null, string? auditSessionId = null)
    {
        var query = _context.AssetAudits
            .Include(a => a.Asset)
            .Where(a => !a.IsDeleted)
            .AsNoTracking();

        if (startDate.HasValue)
            query = query.Where(a => a.AuditDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.AuditDate <= endDate.Value);

        if (!string.IsNullOrEmpty(auditSessionId))
            query = query.Where(a => a.AuditSessionId == auditSessionId);

        return await query.OrderByDescending(a => a.AuditDate).ToListAsync();
    }

    public async Task<IEnumerable<AssetAudit>> GetAuditsBySessionAsync(string sessionId)
    {
        return await _context.AssetAudits
            .Include(a => a.Asset)
            .Where(a => a.AuditSessionId == sessionId && !a.IsDeleted)
            .OrderBy(a => a.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<AssetAuditResult> ProcessScannedAssetAsync(string scannedCode, string auditedBy, string? location = null, string? notes = null, string? sessionId = null)
    {
        try
        {
            // Check if this asset has already been scanned in the current session
            bool isDuplicate = false;
            DateTime? previousScanTime = null;
            
            if (!string.IsNullOrEmpty(sessionId))
            {
                var existingAudit = await _context.AssetAudits
                    .Where(a => a.AuditSessionId == sessionId && 
                                !a.IsDeleted &&
                                a.AssetTag == scannedCode)
                    .OrderByDescending(a => a.AuditDate)
                    .FirstOrDefaultAsync();

                if (existingAudit != null)
                {
                    isDuplicate = true;
                    previousScanTime = existingAudit.AuditDate;
                }
            }

            // Try to find existing asset by asset tag, serial number, or PC ID
            var asset = await _context.Assets
                .Include(a => a.AssignedEmployee)
                .Include(a => a.Department)
                .FirstOrDefaultAsync(a => 
                    a.AssetTag == scannedCode || 
                    a.SerialNumber == scannedCode || 
                    a.PcId == scannedCode);

            var result = new AssetAuditResult();

            if (asset == null)
            {
                // Asset not found - this is a new discovery
                var audit = new AssetAudit
                {
                    AssetTag = scannedCode,
                    AssetType = "Unknown", // Will need to be specified by user
                    Status = AssetStatus.Active,
                    Location = location,
                    AuditedBy = auditedBy,
                    Notes = notes ?? "Asset discovered during audit - not in system",
                    IsNewAsset = true,
                    AuditSessionId = sessionId,
                    AuditDate = DateTime.UtcNow
                };

                await CreateAuditAsync(audit);

                // Build message with duplicate warning if applicable
                string message = isDuplicate && previousScanTime.HasValue
                    ? $"⚠️ DUPLICATE: Already scanned at {previousScanTime.Value:HH:mm:ss}. Asset not found in system - marked as new discovery"
                    : "Asset not found in system - marked as new discovery";

                result.Success = true;
                result.Message = message;
                result.Audit = audit;
                result.IsNew = true;
                result.StatusChanged = false;
                result.LocationChanged = false;

                _logger.LogInformation("New asset discovered during audit: {AssetTag}", scannedCode);
            }
            else
            {
                // Asset exists - compare and record changes
                var statusChanged = false;
                var locationChanged = false;
                var previousStatus = asset.Status;
                var previousLocation = asset.Location;

                // Check if location changed
                if (!string.IsNullOrEmpty(location) && location != asset.Location)
                {
                    locationChanged = true;
                    asset.Location = location;
                }

                // Create audit record
                var audit = new AssetAudit
                {
                    AssetId = asset.Id,
                    AssetTag = asset.AssetTag,
                    AssetType = asset.Type ?? "Unknown",
                    Status = asset.Status,
                    Location = location ?? asset.Location,
                    AuditedBy = auditedBy,
                    Notes = notes,
                    IsNewAsset = false,
                    PreviousStatus = previousStatus,
                    PreviousLocation = previousLocation,
                    SerialNumber = asset.SerialNumber,
                    Brand = asset.Brand,
                    Model = asset.Model,
                    AuditSessionId = sessionId,
                    AuditDate = DateTime.UtcNow
                };

                await CreateAuditAsync(audit);

                // Update asset: mark as audited and update last audit date
                asset.IsAudited = true;
                asset.LastAuditDate = DateTime.UtcNow;
                
                // Update asset if location changed
                if (locationChanged)
                {
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Still need to save IsAudited and LastAuditDate changes
                    await _context.SaveChangesAsync();
                }

                // Build message with duplicate warning if applicable
                string message = isDuplicate && previousScanTime.HasValue
                    ? $"⚠️ DUPLICATE: Already scanned at {previousScanTime.Value:HH:mm:ss}. {BuildResultMessage(locationChanged, statusChanged)}"
                    : BuildResultMessage(locationChanged, statusChanged);

                result.Success = true;
                result.Message = message;
                result.Audit = audit;
                result.ExistingAsset = asset;
                result.IsNew = false;
                result.StatusChanged = statusChanged;
                result.LocationChanged = locationChanged;

                _logger.LogInformation("Asset audited: {AssetTag}, Location Changed: {LocationChanged}", 
                    asset.AssetTag, locationChanged);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing scanned asset: {ScannedCode}", scannedCode);
            return new AssetAuditResult
            {
                Success = false,
                Message = $"Error processing asset: {ex.Message}"
            };
        }
    }

    public async Task<Dictionary<string, int>> GetAuditStatisticsAsync(string? sessionId = null)
    {
        var query = _context.AssetAudits.Where(a => !a.IsDeleted).AsQueryable();

        if (!string.IsNullOrEmpty(sessionId))
            query = query.Where(a => a.AuditSessionId == sessionId);

        var stats = new Dictionary<string, int>
        {
            ["Total"] = await query.CountAsync(),
            ["NewAssets"] = await query.CountAsync(a => a.IsNewAsset),
            ["ExistingAssets"] = await query.CountAsync(a => !a.IsNewAsset),
            ["LocationChanges"] = await query.CountAsync(a => a.PreviousLocation != null && a.Location != a.PreviousLocation),
            ["StatusChanges"] = await query.CountAsync(a => a.PreviousStatus != null && a.Status != a.PreviousStatus)
        };

        return stats;
    }

    public async Task<IEnumerable<AssetForAuditDto>> GetAllAssetsForAuditAsync(string? sessionId = null)
    {
        var assets = await _context.Assets
            .AsNoTracking()
            .Select(a => new AssetForAuditDto
            {
                Id = a.Id,
                AssetTag = a.AssetTag,
                Type = a.Type,
                Status = a.Status.ToString(),
                Location = a.Location,
                SerialNumber = a.SerialNumber,
                Brand = a.Brand,
                Model = a.Model,
                IsAudited = a.IsAudited,
                LastAuditDate = a.LastAuditDate,
                AuditedInCurrentSession = false // Will be updated below if sessionId provided
            })
            .OrderBy(a => a.AssetTag)
            .ToListAsync();

        // If session ID provided, check which assets were audited in this session
        if (!string.IsNullOrEmpty(sessionId))
        {
            var auditedAssetIds = await _context.AssetAudits
                .Where(a => a.AuditSessionId == sessionId && a.AssetId != null)
                .Select(a => a.AssetId!.Value)
                .Distinct()
                .ToListAsync();

            foreach (var asset in assets)
            {
                asset.AuditedInCurrentSession = auditedAssetIds.Contains(asset.Id);
            }
        }

        return assets;
    }

    public async Task<AssetAuditResult> UpdateAssetStatusAsync(int assetId, string status, string? location, string auditedBy, string? notes = null, string? sessionId = null)
    {
        try
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null)
            {
                return new AssetAuditResult
                {
                    Success = false,
                    Message = "Asset not found"
                };
            }

            var previousStatus = asset.Status;
            var previousLocation = asset.Location;
            var statusChanged = false;
            var locationChanged = false;

            // Parse and update status if provided
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<AssetStatus>(status, out var newStatus))
            {
                if (asset.Status != newStatus)
                {
                    statusChanged = true;
                    asset.Status = newStatus;
                }
            }

            // Update location if provided and different
            if (!string.IsNullOrEmpty(location) && location != asset.Location)
            {
                locationChanged = true;
                asset.Location = location;
            }

            // Mark asset as audited
            asset.IsAudited = true;
            asset.LastAuditDate = DateTime.UtcNow;
            asset.UpdatedAt = DateTime.UtcNow;

            // Create audit record
            var audit = new AssetAudit
            {
                AssetId = asset.Id,
                AssetTag = asset.AssetTag,
                AssetType = asset.Type ?? "Unknown",
                Status = asset.Status,
                Location = asset.Location,
                AuditedBy = auditedBy,
                Notes = notes,
                IsNewAsset = false,
                PreviousStatus = statusChanged ? previousStatus : null,
                PreviousLocation = locationChanged ? previousLocation : null,
                SerialNumber = asset.SerialNumber,
                Brand = asset.Brand,
                Model = asset.Model,
                AuditSessionId = sessionId ?? Guid.NewGuid().ToString(),
                AuditDate = DateTime.UtcNow
            };

            await CreateAuditAsync(audit);
            await _context.SaveChangesAsync();

            return new AssetAuditResult
            {
                Success = true,
                Message = BuildResultMessage(locationChanged, statusChanged),
                Audit = audit,
                ExistingAsset = asset,
                IsNew = false,
                StatusChanged = statusChanged,
                LocationChanged = locationChanged
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating asset status: {AssetId}", assetId);
            return new AssetAuditResult
            {
                Success = false,
                Message = $"Error updating asset: {ex.Message}"
            };
        }
    }

    private string BuildResultMessage(bool locationChanged, bool statusChanged)
    {
        if (locationChanged && statusChanged)
            return "Asset verified - Location and status updated";
        if (locationChanged)
            return "Asset verified - Location updated";
        if (statusChanged)
            return "Asset verified - Status updated";
        return "Asset verified - No changes detected";
    }

    public async Task<bool> UpdateAuditRecordAsync(int auditId, string status, string? location, string? notes, string updatedBy)
    {
        try
        {
            var audit = await _context.AssetAudits.FindAsync(auditId);
            if (audit == null || audit.IsDeleted)
            {
                _logger.LogWarning("Audit record not found or deleted: {AuditId}", auditId);
                return false;
            }

            // Parse and update status
            if (Enum.TryParse<AssetStatus>(status, out var newStatus))
            {
                audit.Status = newStatus;
            }

            // Update location and notes
            audit.Location = location;
            audit.Notes = notes;

            // If this audit is linked to an asset, update the asset as well
            if (audit.AssetId.HasValue)
            {
                var asset = await _context.Assets.FindAsync(audit.AssetId.Value);
                if (asset != null)
                {
                    asset.Status = audit.Status;
                    asset.Location = location;
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Audit record {AuditId} updated by {UpdatedBy}", auditId, updatedBy);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating audit record: {AuditId}", auditId);
            return false;
        }
    }

    public async Task<bool> DeleteAuditRecordAsync(int auditId, string deletedBy, string? deletionReason = null)
    {
        try
        {
            var audit = await _context.AssetAudits.FindAsync(auditId);
            if (audit == null || audit.IsDeleted)
            {
                _logger.LogWarning("Audit record not found or already deleted: {AuditId}", auditId);
                return false;
            }

            // Create delete history record
            var deleteHistory = new AssetAuditDeleteHistory
            {
                OriginalAuditId = audit.Id,
                AuditDate = audit.AuditDate,
                AssetId = audit.AssetId,
                AssetTag = audit.AssetTag,
                AssetType = audit.AssetType,
                Status = audit.Status,
                Location = audit.Location,
                AuditedBy = audit.AuditedBy,
                Notes = audit.Notes,
                IsNewAsset = audit.IsNewAsset,
                PreviousStatus = audit.PreviousStatus,
                PreviousLocation = audit.PreviousLocation,
                SerialNumber = audit.SerialNumber,
                Brand = audit.Brand,
                Model = audit.Model,
                AuditSessionId = audit.AuditSessionId,
                OriginalCreatedAt = audit.CreatedAt,
                DeletedAt = DateTime.UtcNow,
                DeletedBy = deletedBy,
                DeletionReason = deletionReason
            };

            _context.AssetAuditDeleteHistories.Add(deleteHistory);

            // Soft delete the audit record
            audit.IsDeleted = true;
            audit.DeletedAt = DateTime.UtcNow;
            audit.DeletedBy = deletedBy;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Audit record {AuditId} deleted by {DeletedBy}. Reason: {Reason}", 
                auditId, deletedBy, deletionReason ?? "Not specified");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting audit record: {AuditId}", auditId);
            return false;
        }
    }

    public async Task<IEnumerable<AssetAuditDeleteHistory>> GetDeletedAuditHistoryAsync(string? sessionId = null)
    {
        var query = _context.AssetAuditDeleteHistories.AsNoTracking();

        if (!string.IsNullOrEmpty(sessionId))
        {
            query = query.Where(h => h.AuditSessionId == sessionId);
        }

        return await query
            .OrderByDescending(h => h.DeletedAt)
            .ToListAsync();
    }

    public async Task<bool> ClearAllAuditsAsync(string clearedBy)
    {
        try
        {
            _logger.LogWarning("Clearing all audit records initiated by {ClearedBy}", clearedBy);
            
            // Delete all audit records (including soft deleted ones)
            var allAudits = await _context.AssetAudits.ToListAsync();
            _context.AssetAudits.RemoveRange(allAudits);
            
            // Delete all audit delete history
            var allDeleteHistory = await _context.AssetAuditDeleteHistories.ToListAsync();
            _context.AssetAuditDeleteHistories.RemoveRange(allDeleteHistory);
            
            await _context.SaveChangesAsync();
            
            _logger.LogWarning("All audit records cleared by {ClearedBy}. Total audits deleted: {AuditCount}, Total delete history cleared: {HistoryCount}", 
                clearedBy, allAudits.Count, allDeleteHistory.Count);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing all audit records");
            return false;
        }
    }
}


