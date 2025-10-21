using buildone.Data;
using Microsoft.EntityFrameworkCore;

namespace buildone.Services
{
    public class AssetHistoryService : IAssetHistoryService
    {
        private readonly ApplicationDbContext _context;

        public AssetHistoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AssetHistory>> GetAssetHistoryAsync(int assetId)
        {
            return await _context.AssetHistory
                .Where(h => h.AssetId == assetId)
                .Include(h => h.Asset)
                .Include(h => h.Actor)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<AssetHistory>> GetAllHistoryAsync()
        {
            return await _context.AssetHistory
                .Include(h => h.Asset)
                .Include(h => h.Actor)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
        }

        public async Task<AssetHistory?> GetHistoryEntryByIdAsync(int id)
        {
            return await _context.AssetHistory
                .Include(h => h.Asset)
                .Include(h => h.Actor)
                .FirstOrDefaultAsync(h => h.Id == id);
        }

        public async Task LogActionAsync(int assetId, int? actorId, string action, string? fromValue = null, string? toValue = null, string? notes = null)
        {
            var historyEntry = new AssetHistory
            {
                AssetId = assetId,
                ActorId = actorId,
                Action = action,
                FromValue = fromValue,
                ToValue = toValue,
                Notes = notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.AssetHistory.Add(historyEntry);
            await _context.SaveChangesAsync();
        }

        public async Task LogAssetCreatedAsync(int assetId, int? actorId, string? notes = null)
        {
            await LogActionAsync(assetId, actorId, "Asset Created", null, null, notes ?? "New asset added to inventory");
        }

        public async Task LogAssetStatusChangeAsync(int assetId, int? actorId, string oldStatus, string newStatus, string? notes = null)
        {
            await LogActionAsync(assetId, actorId, "Status Changed", oldStatus, newStatus, notes ?? $"Asset status changed from {oldStatus} to {newStatus}");
        }

        public async Task LogAssetAssignmentAsync(int assetId, int? actorId, string? oldEmployee, string? newEmployee, string? notes = null)
        {
            var action = oldEmployee == null ? "Asset Assigned" : newEmployee == null ? "Asset Unassigned" : "Asset Reassigned";
            var defaultNotes = oldEmployee == null 
                ? $"Asset assigned to {newEmployee}" 
                : newEmployee == null 
                    ? $"Asset unassigned from {oldEmployee}"
                    : $"Asset reassigned from {oldEmployee} to {newEmployee}";
            
            await LogActionAsync(assetId, actorId, action, oldEmployee, newEmployee, notes ?? defaultNotes);
        }

        public async Task LogAssetUpdatedAsync(int assetId, int? actorId, string propertyName, string? oldValue, string? newValue, string? notes = null)
        {
            await LogActionAsync(assetId, actorId, $"{propertyName} Updated", oldValue, newValue, notes ?? $"{propertyName} changed from '{oldValue}' to '{newValue}'");
        }

        public async Task LogAssetDeletedAsync(int assetId, int? actorId, string? notes = null)
        {
            await LogActionAsync(assetId, actorId, "Asset Deleted", null, null, notes ?? "Asset removed from inventory");
        }

        public async Task<IEnumerable<AssetHistory>> GetHistoryByActorAsync(int actorId)
        {
            return await _context.AssetHistory
                .Where(h => h.ActorId == actorId)
                .Include(h => h.Asset)
                .Include(h => h.Actor)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<AssetHistory>> GetHistoryByActionAsync(string action)
        {
            return await _context.AssetHistory
                .Where(h => h.Action.Contains(action))
                .Include(h => h.Asset)
                .Include(h => h.Actor)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<AssetHistory>> GetHistoryByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.AssetHistory
                .Where(h => h.CreatedAt >= startDate && h.CreatedAt <= endDate)
                .Include(h => h.Asset)
                .Include(h => h.Actor)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
        }
    }
}