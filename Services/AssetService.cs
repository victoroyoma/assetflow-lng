using buildone.Data;
using buildone.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace buildone.Services;

public class AssetService : IAssetService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AssetService> _logger;

    public AssetService(ApplicationDbContext context, ILogger<AssetService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Asset>> GetAllAssetsAsync()
    {
        try
        {
            return await _context.Assets
                .Include(a => a.AssignedEmployee)
                .Include(a => a.Department)
                .AsNoTracking() // Performance: No tracking for read-only queries
                .OrderBy(a => a.AssetTag)
                .ToListAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error retrieving all assets");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving all assets");
            throw;
        }
    }

    public async Task<Asset?> GetAssetByIdAsync(int id)
    {
        try
        {
            return await _context.Assets
                .Include(a => a.AssignedEmployee)
                .Include(a => a.Department)
                .AsNoTracking() // Performance: No tracking for read-only queries
                .FirstOrDefaultAsync(a => a.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving asset with ID {AssetId}", id);
            throw;
        }
    }

    public async Task<Asset?> GetAssetByAssetTagAsync(string assetTag)
    {
        try
        {
            return await _context.Assets
                .Include(a => a.AssignedEmployee)
                .Include(a => a.Department)
                .AsNoTracking() // Performance: No tracking for read-only queries
                .FirstOrDefaultAsync(a => a.AssetTag == assetTag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving asset with tag {AssetTag}", assetTag);
            throw;
        }
    }

    public async Task<Asset?> GetAssetByPcIdAsync(string pcId)
    {
        try
        {
            return await _context.Assets
                .Include(a => a.AssignedEmployee)
                .Include(a => a.Department)
                .FirstOrDefaultAsync(a => a.PcId == pcId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving asset with PC ID {PcId}", pcId);
            throw;
        }
    }

    public async Task<Asset> CreateAssetAsync(Asset asset)
    {
        try
        {
            asset.CreatedAt = DateTime.UtcNow;
            _context.Assets.Add(asset);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation(
                "Asset created - ID: {AssetId}, Tag: {AssetTag}, Type: {Type}", 
                asset.Id, 
                asset.AssetTag, 
                asset.Type);
            
            return asset;
        }
        catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx)
        {
            if (sqlEx.Number == 2627 || sqlEx.Number == 2601) // Unique constraint violation
            {
                _logger.LogWarning(
                    "Duplicate asset detected - Tag: {AssetTag}, PcId: {PcId}", 
                    asset.AssetTag, 
                    asset.PcId);
                throw new InvalidOperationException("An asset with this tag or PC ID already exists", ex);
            }
            _logger.LogError(ex, "Database error creating asset {AssetTag}", asset.AssetTag);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating asset {AssetTag}", asset.AssetTag);
            throw;
        }
    }

    public async Task<bool> UpdateAssetAsync(Asset asset)
    {
        try
        {
            _context.Entry(asset).State = EntityState.Modified;
            var result = await _context.SaveChangesAsync();
            _logger.LogInformation("Asset updated with ID {AssetId}", asset.Id);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating asset with ID {AssetId}", asset.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAssetAsync(int id)
    {
        try
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset != null)
            {
                _context.Assets.Remove(asset);
                var result = await _context.SaveChangesAsync();
                _logger.LogInformation("Asset deleted with ID {AssetId}", id);
                return result > 0;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting asset with ID {AssetId}", id);
            throw;
        }
    }

    public async Task<bool> AssetExistsAsync(int id)
    {
        try
        {
            return await _context.Assets.AnyAsync(a => a.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if asset exists with ID {AssetId}", id);
            throw;
        }
    }

    public async Task<bool> AssetTagExistsAsync(string assetTag, int? excludeId = null)
    {
        try
        {
            var query = _context.Assets.Where(a => a.AssetTag == assetTag);
            if (excludeId.HasValue)
            {
                query = query.Where(a => a.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if asset tag exists {AssetTag}", assetTag);
            throw;
        }
    }

    public async Task<bool> PcIdExistsAsync(string pcId, int? excludeId = null)
    {
        try
        {
            var query = _context.Assets.Where(a => a.PcId == pcId);
            if (excludeId.HasValue)
            {
                query = query.Where(a => a.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if PC ID exists {PcId}", pcId);
            throw;
        }
    }

    public async Task<IEnumerable<Asset>> SearchAssetsAsync(string searchTerm)
    {
        try
        {
            return await _context.Assets
                .Include(a => a.AssignedEmployee)
                .Include(a => a.Department)
                .Where(a => a.AssetTag.Contains(searchTerm) ||
                           a.PcId.Contains(searchTerm) ||
                           (a.Brand != null && a.Brand.Contains(searchTerm)) ||
                           (a.Model != null && a.Model.Contains(searchTerm)) ||
                           (a.SerialNumber != null && a.SerialNumber.Contains(searchTerm)) ||
                           (a.AssignedEmployee != null && a.AssignedEmployee.FullName.Contains(searchTerm)))
                .OrderBy(a => a.AssetTag)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching assets with term {SearchTerm}", searchTerm);
            throw;
        }
    }

    public async Task<IEnumerable<Asset>> GetAssetsByStatusAsync(AssetStatus status)
    {
        try
        {
            return await _context.Assets
                .Include(a => a.AssignedEmployee)
                .Include(a => a.Department)
                .Where(a => a.Status == status)
                .OrderBy(a => a.AssetTag)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assets by status {Status}", status);
            throw;
        }
    }

    public async Task<IEnumerable<Asset>> GetAssetsByEmployeeAsync(int employeeId)
    {
        try
        {
            return await _context.Assets
                .Include(a => a.AssignedEmployee)
                .Include(a => a.Department)
                .Where(a => a.AssignedEmployeeId == employeeId)
                .OrderBy(a => a.AssetTag)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assets by employee {EmployeeId}", employeeId);
            throw;
        }
    }

    public async Task<IEnumerable<Asset>> GetAssetsByDepartmentAsync(int departmentId)
    {
        try
        {
            return await _context.Assets
                .Include(a => a.AssignedEmployee)
                .Include(a => a.Department)
                .Where(a => a.DepartmentId == departmentId)
                .OrderBy(a => a.AssetTag)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assets by department {DepartmentId}", departmentId);
            throw;
        }
    }

    public async Task<IEnumerable<Asset>> GetUnassignedAssetsAsync()
    {
        try
        {
            return await _context.Assets
                .Include(a => a.Department)
                .Where(a => a.AssignedEmployeeId == null)
                .OrderBy(a => a.AssetTag)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unassigned assets");
            throw;
        }
    }

    public async Task<bool> AssignAssetToEmployeeAsync(int assetId, int employeeId, int? departmentId = null)
    {
        try
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null) return false;

            asset.AssignedEmployeeId = employeeId;
            if (departmentId.HasValue)
            {
                asset.DepartmentId = departmentId.Value;
            }
            asset.Status = AssetStatus.Assigned;

            var result = await _context.SaveChangesAsync();
            _logger.LogInformation("Asset {AssetId} assigned to employee {EmployeeId}", assetId, employeeId);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning asset {AssetId} to employee {EmployeeId}", assetId, employeeId);
            throw;
        }
    }

    public async Task<bool> UnassignAssetAsync(int assetId)
    {
        try
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null) return false;

            asset.AssignedEmployeeId = null;
            asset.Status = AssetStatus.InStock;

            var result = await _context.SaveChangesAsync();
            _logger.LogInformation("Asset {AssetId} unassigned", assetId);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unassigning asset {AssetId}", assetId);
            throw;
        }
    }
}