using buildone.Data;

namespace buildone.Services
{
    public interface IAssetHistoryService
    {
        Task<IEnumerable<AssetHistory>> GetAssetHistoryAsync(int assetId);
        Task<IEnumerable<AssetHistory>> GetAllHistoryAsync();
        Task<AssetHistory?> GetHistoryEntryByIdAsync(int id);
        Task LogActionAsync(int assetId, int? actorId, string action, string? fromValue = null, string? toValue = null, string? notes = null);
        Task LogAssetCreatedAsync(int assetId, int? actorId, string? notes = null);
        Task LogAssetStatusChangeAsync(int assetId, int? actorId, string oldStatus, string newStatus, string? notes = null);
        Task LogAssetAssignmentAsync(int assetId, int? actorId, string? oldEmployee, string? newEmployee, string? notes = null);
        Task LogAssetUpdatedAsync(int assetId, int? actorId, string propertyName, string? oldValue, string? newValue, string? notes = null);
        Task LogAssetDeletedAsync(int assetId, int? actorId, string? notes = null);
        Task<IEnumerable<AssetHistory>> GetHistoryByActorAsync(int actorId);
        Task<IEnumerable<AssetHistory>> GetHistoryByActionAsync(string action);
        Task<IEnumerable<AssetHistory>> GetHistoryByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}