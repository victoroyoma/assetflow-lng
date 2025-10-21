using buildone.Data;
using buildone.Data.Enums;

namespace buildone.Services;

public interface IAssetService
{
    Task<IEnumerable<Asset>> GetAllAssetsAsync();
    Task<Asset?> GetAssetByIdAsync(int id);
    Task<Asset?> GetAssetByAssetTagAsync(string assetTag);
    Task<Asset?> GetAssetByPcIdAsync(string pcId);
    Task<Asset> CreateAssetAsync(Asset asset);
    Task<bool> UpdateAssetAsync(Asset asset);
    Task<bool> DeleteAssetAsync(int id);
    Task<bool> AssetExistsAsync(int id);
    Task<bool> AssetTagExistsAsync(string assetTag, int? excludeId = null);
    Task<bool> PcIdExistsAsync(string pcId, int? excludeId = null);
    Task<IEnumerable<Asset>> SearchAssetsAsync(string searchTerm);
    Task<IEnumerable<Asset>> GetAssetsByStatusAsync(AssetStatus status);
    Task<IEnumerable<Asset>> GetAssetsByEmployeeAsync(int employeeId);
    Task<IEnumerable<Asset>> GetAssetsByDepartmentAsync(int departmentId);
    Task<IEnumerable<Asset>> GetUnassignedAssetsAsync();
    Task<bool> AssignAssetToEmployeeAsync(int assetId, int employeeId, int? departmentId = null);
    Task<bool> UnassignAssetAsync(int assetId);
}