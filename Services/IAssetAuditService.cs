using buildone.Data;

namespace buildone.Services;

public interface IAssetAuditService
{
    Task<AssetAudit> CreateAuditAsync(AssetAudit audit);
    Task<IEnumerable<AssetAudit>> GetAuditsAsync(DateTime? startDate = null, DateTime? endDate = null, string? auditSessionId = null);
    Task<IEnumerable<AssetAudit>> GetAuditsBySessionAsync(string sessionId);
    Task<AssetAuditResult> ProcessScannedAssetAsync(string scannedCode, string auditedBy, string? location = null, string? notes = null, string? sessionId = null);
    Task<Dictionary<string, int>> GetAuditStatisticsAsync(string? sessionId = null);
    Task<IEnumerable<AssetForAuditDto>> GetAllAssetsForAuditAsync(string? sessionId = null);
    Task<AssetAuditResult> UpdateAssetStatusAsync(int assetId, string status, string? location, string auditedBy, string? notes = null, string? sessionId = null);
    Task<bool> UpdateAuditRecordAsync(int auditId, string status, string? location, string? notes, string updatedBy);
    Task<bool> DeleteAuditRecordAsync(int auditId, string deletedBy, string? deletionReason = null);
    Task<IEnumerable<AssetAuditDeleteHistory>> GetDeletedAuditHistoryAsync(string? sessionId = null);
    Task<bool> ClearAllAuditsAsync(string clearedBy);
}

public class AssetForAuditDto
{
    public int Id { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? SerialNumber { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public bool IsAudited { get; set; }
    public DateTime? LastAuditDate { get; set; }
    public bool AuditedInCurrentSession { get; set; }
}

public class AssetAuditResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public AssetAudit? Audit { get; set; }
    public Asset? ExistingAsset { get; set; }
    public bool IsNew { get; set; }
    public bool StatusChanged { get; set; }
    public bool LocationChanged { get; set; }
}
