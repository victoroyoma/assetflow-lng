using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using buildone.Services;
using buildone.Data;
using buildone.Data.Enums;
using buildone.Models;

namespace buildone.Pages.Assets
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IAssetService _assetService;
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly IAssetHistoryService _assetHistoryService;
        private readonly IImagingJobService _imagingJobService;

        public IndexModel(
            IAssetService assetService, 
            IEmployeeService employeeService, 
            IDepartmentService departmentService,
            IAssetHistoryService assetHistoryService,
            IImagingJobService imagingJobService)
        {
            _assetService = assetService;
            _employeeService = employeeService;
            _departmentService = departmentService;
            _assetHistoryService = assetHistoryService;
            _imagingJobService = imagingJobService;
        }

        public PaginatedList<Asset> Assets { get; set; } = null!;
        public IList<Department> Departments { get; set; } = new List<Department>();
        public IList<Employee> Employees { get; set; } = new List<Employee>();

        // Pagination properties
        [BindProperty(SupportsGet = true)]
        public int PageIndex { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 25;

        // Filter properties
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? TypeFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? DepartmentFilter { get; set; }

        // Statistics properties
        public int TotalAssets { get; set; }
        public int ActiveAssets { get; set; }
        public int MaintenanceAssets { get; set; }
        public int UnassignedAssets { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Load data sequentially to avoid DbContext concurrency issues
                var allAssets = await _assetService.GetAllAssetsAsync();
                Departments = (await _departmentService.GetAllDepartmentsAsync()).ToList();
                Employees = (await _employeeService.GetAllEmployeesAsync()).ToList();

                // Apply filters
                var filteredAssets = allAssets.AsQueryable();

                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    filteredAssets = filteredAssets.Where(a => 
                        a.AssetTag.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (a.SerialNumber != null && a.SerialNumber.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                        (a.PcId != null && a.PcId.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                        (a.Brand != null && a.Brand.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                        (a.Model != null && a.Model.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)));
                }

                if (!string.IsNullOrEmpty(StatusFilter))
                {
                    if (Enum.TryParse<AssetStatus>(StatusFilter, out var status))
                    {
                        filteredAssets = filteredAssets.Where(a => a.Status == status);
                    }
                }

                if (!string.IsNullOrEmpty(TypeFilter))
                {
                    filteredAssets = filteredAssets.Where(a => a.Type == TypeFilter);
                }

                if (!string.IsNullOrEmpty(DepartmentFilter) && int.TryParse(DepartmentFilter, out var deptId))
                {
                    filteredAssets = filteredAssets.Where(a => a.DepartmentId == deptId);
                }

                // Apply pagination
                Assets = PaginatedList<Asset>.Create(
                    filteredAssets.OrderBy(a => a.AssetTag),
                    PageIndex,
                    PageSize
                );

                // Calculate statistics from all assets (not filtered)
                TotalAssets = allAssets.Count();
                ActiveAssets = allAssets.Count(a => a.Status == AssetStatus.Active);
                MaintenanceAssets = allAssets.Count(a => a.Status == AssetStatus.Maintenance);
                UnassignedAssets = allAssets.Count(a => a.AssignedEmployeeId == null);
            }
            catch (Exception ex)
            {
                // Log the error (implement logging as needed)
                Console.WriteLine($"Error loading assets: {ex.Message}");
                
                // Initialize empty collections to prevent null reference exceptions
                Assets = PaginatedList<Asset>.Create(new List<Asset>(), 1, PageSize);
                Departments = new List<Department>();
                Employees = new List<Employee>();
                TotalAssets = 0;
                ActiveAssets = 0;
                MaintenanceAssets = 0;
                UnassignedAssets = 0;
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                // Get asset with related data for validation and history
                var asset = await _assetService.GetAssetByIdAsync(id);
                if (asset == null)
                {
                    TempData["ErrorMessage"] = "Asset not found.";
                    return RedirectToPage();
                }

                // Check for dependencies before deletion
                var validationResult = await ValidateAssetDeletionAsync(asset);
                if (!validationResult.CanDelete)
                {
                    TempData["ErrorMessage"] = validationResult.ErrorMessage;
                    return RedirectToPage();
                }

                // Log deletion in history before actual deletion
                await _assetHistoryService.LogAssetDeletedAsync(
                    asset.Id,
                    null, // In real app, this would be current user ID
                    $"Asset {asset.AssetTag} deleted"
                );

                // Handle related data cleanup
                await HandleAssetDeletionCascade(asset);

                // Delete the asset
                var success = await _assetService.DeleteAssetAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = $"Asset '{asset.AssetTag}' has been deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete asset. Please try again.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting asset: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostBulkDeleteAsync([FromBody] List<int> assetIds)
        {
            try
            {
                if (assetIds == null || !assetIds.Any())
                {
                    return BadRequest("No assets selected for deletion.");
                }

                foreach (var id in assetIds)
                {
                    await _assetService.DeleteAssetAsync(id);
                }

                TempData["SuccessMessage"] = $"{assetIds.Count} assets have been deleted successfully.";
                return new JsonResult(new { success = true, message = "Assets deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting assets: {ex.Message}");
            }
        }

        public async Task<IActionResult> OnPostBulkUpdateStatusAsync([FromBody] BulkUpdateModel model)
        {
            try
            {
                if (model.AssetIds == null || !model.AssetIds.Any())
                {
                    return BadRequest("No assets selected for update.");
                }

                if (!Enum.TryParse<AssetStatus>(model.NewStatus, out var status))
                {
                    return BadRequest("Invalid status value.");
                }

                foreach (var id in model.AssetIds)
                {
                    var asset = await _assetService.GetAssetByIdAsync(id);
                    if (asset != null)
                    {
                        asset.Status = status;
                        asset.UpdatedAt = DateTime.UtcNow;
                        await _assetService.UpdateAssetAsync(asset);
                    }
                }

                TempData["SuccessMessage"] = $"{model.AssetIds.Count} assets have been updated successfully.";
                return new JsonResult(new { success = true, message = "Assets updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating assets: {ex.Message}");
            }
        }

        public async Task<IActionResult> OnPostBulkAssignAsync([FromBody] BulkAssignModel model)
        {
            try
            {
                if (model.AssetIds == null || !model.AssetIds.Any())
                {
                    return BadRequest("No assets selected for assignment.");
                }

                // Validate employee exists if specified
                if (model.EmployeeId.HasValue)
                {
                    var employee = await _employeeService.GetEmployeeByIdAsync(model.EmployeeId.Value);
                    if (employee == null)
                    {
                        return BadRequest("Invalid employee selected.");
                    }
                }

                foreach (var id in model.AssetIds)
                {
                    var asset = await _assetService.GetAssetByIdAsync(id);
                    if (asset != null)
                    {
                        asset.AssignedEmployeeId = model.EmployeeId;
                        asset.UpdatedAt = DateTime.UtcNow;
                        await _assetService.UpdateAssetAsync(asset);
                    }
                }

                string message = model.EmployeeId.HasValue 
                    ? $"{model.AssetIds.Count} assets have been assigned successfully."
                    : $"{model.AssetIds.Count} assets have been unassigned successfully.";

                TempData["SuccessMessage"] = message;
                return new JsonResult(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error assigning assets: {ex.Message}");
            }
        }

        public async Task<IActionResult> OnGetGetAssetDetailsAsync(int assetId)
        {
            try
            {
                var asset = await _assetService.GetAssetByIdAsync(assetId);
                if (asset == null)
                {
                    return new JsonResult(new { success = false, message = "Asset not found." });
                }

                var result = new
                {
                    success = true,
                    asset = new
                    {
                        id = asset.Id,
                        assetTag = asset.AssetTag,
                        pcId = asset.PcId,
                        type = asset.Type,
                        status = asset.Status.ToString(),
                        brand = asset.Brand,
                        model = asset.Model,
                        serialNumber = asset.SerialNumber,
                        warrantyExpiry = asset.WarrantyExpiry?.ToString("yyyy-MM-dd"),
                        assignedEmployee = asset.AssignedEmployee?.FullName,
                        department = asset.Department?.Name,
                        imagingType = asset.ImagingType.ToString(),
                        deploymentType = asset.DeploymentType.ToString(),
                        notes = asset.Notes,
                        createdAt = asset.CreatedAt.ToString("yyyy-MM-dd"),
                        updatedAt = asset.UpdatedAt?.ToString("yyyy-MM-dd")
                    }
                };

                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error loading asset details: {ex.Message}" });
            }
        }

        public async Task<IActionResult> OnPostUnassignAssetAsync([FromBody] UnassignAssetModel model)
        {
            try
            {
                var asset = await _assetService.GetAssetByIdAsync(model.AssetId);
                if (asset == null)
                {
                    return new JsonResult(new { success = false, message = "Asset not found." });
                }

                if (!asset.AssignedEmployeeId.HasValue)
                {
                    return new JsonResult(new { success = false, message = "Asset is not currently assigned." });
                }

                // Log the unassignment in history
                await _assetHistoryService.LogAssetAssignmentAsync(
                    asset.Id,
                    null, // In real app, this would be current user ID
                    asset.AssignedEmployee?.FullName,
                    null,
                    "Asset unassigned via quick action"
                );

                // Unassign the asset
                asset.AssignedEmployeeId = null;
                asset.UpdatedAt = DateTime.UtcNow;
                await _assetService.UpdateAssetAsync(asset);

                return new JsonResult(new { success = true, message = "Asset unassigned successfully." });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error unassigning asset: {ex.Message}" });
            }
        }

        public async Task<IActionResult> OnPostAssignAssetAsync([FromBody] AssignAssetModel model)
        {
            try
            {
                var asset = await _assetService.GetAssetByIdAsync(model.AssetId);
                if (asset == null)
                {
                    return new JsonResult(new { success = false, message = "Asset not found." });
                }

                var employee = await _employeeService.GetEmployeeByIdAsync(model.EmployeeId);
                if (employee == null)
                {
                    return new JsonResult(new { success = false, message = "Employee not found." });
                }

                // Log the assignment in history
                await _assetHistoryService.LogAssetAssignmentAsync(
                    asset.Id,
                    null, // In real app, this would be current user ID
                    asset.AssignedEmployee?.FullName,
                    employee.FullName,
                    "Asset assigned via quick action"
                );

                // Assign the asset
                asset.AssignedEmployeeId = model.EmployeeId;
                asset.UpdatedAt = DateTime.UtcNow;
                await _assetService.UpdateAssetAsync(asset);

                return new JsonResult(new { success = true, message = $"Asset assigned to {employee.FullName} successfully." });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error assigning asset: {ex.Message}" });
            }
        }

        public async Task<IActionResult> OnGetGetAssetHistoryAsync(int assetId)
        {
            try
            {
                var history = await _assetHistoryService.GetAssetHistoryAsync(assetId);
                var historyItems = history.Select(h => new
                {
                    action = h.Action,
                    description = !string.IsNullOrEmpty(h.FromValue) && !string.IsNullOrEmpty(h.ToValue)
                        ? $"Changed from '{h.FromValue}' to '{h.ToValue}'"
                        : h.Notes ?? h.Action,
                    timestamp = h.CreatedAt.ToString("MMM dd, yyyy HH:mm"),
                    actor = h.Actor?.FullName ?? "System"
                }).ToList();

                return new JsonResult(new { success = true, history = historyItems });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error loading asset history: {ex.Message}" });
            }
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync([FromBody] UpdateStatusModel model)
        {
            try
            {
                var asset = await _assetService.GetAssetByIdAsync(model.AssetId);
                if (asset == null)
                {
                    return new JsonResult(new { success = false, message = "Asset not found." });
                }

                if (!Enum.TryParse<AssetStatus>(model.NewStatus, out var newStatus))
                {
                    return new JsonResult(new { success = false, message = "Invalid status value." });
                }

                var oldStatus = asset.Status;
                
                // Log the status change in history
                await _assetHistoryService.LogAssetStatusChangeAsync(
                    asset.Id,
                    null, // In real app, this would be current user ID
                    oldStatus.ToString(),
                    newStatus.ToString(),
                    "Status changed via quick action"
                );

                // Update the asset status
                asset.Status = newStatus;
                asset.UpdatedAt = DateTime.UtcNow;
                await _assetService.UpdateAssetAsync(asset);

                return new JsonResult(new { success = true, message = $"Status changed from {oldStatus} to {newStatus} successfully." });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error updating status: {ex.Message}" });
            }
        }

        private async Task<AssetDeletionValidation> ValidateAssetDeletionAsync(Asset asset)
        {
            var result = new AssetDeletionValidation();

            // Check if asset is currently assigned
            if (asset.AssignedEmployeeId.HasValue)
            {
                result.CanDelete = false;
                result.ErrorMessage = "Cannot delete asset that is currently assigned to an employee. Please unassign first.";
                return result;
            }

            // Check for active imaging jobs
            var imagingJobs = await _imagingJobService.GetImagingJobsByAssetAsync(asset.Id);
            var activeJobs = imagingJobs.Where(j => j.Status == JobStatus.Pending || j.Status == JobStatus.InProgress);
            
            if (activeJobs.Any())
            {
                result.CanDelete = false;
                result.ErrorMessage = "Cannot delete asset with active imaging jobs. Please complete or cancel the jobs first.";
                return result;
            }

            // Check asset status
            if (asset.Status == AssetStatus.Active)
            {
                result.CanDelete = false;
                result.ErrorMessage = "Cannot delete active asset. Please change status first.";
                return result;
            }

            result.CanDelete = true;
            return result;
        }

        private async Task HandleAssetDeletionCascade(Asset asset)
        {
            try
            {
                // Clean up completed imaging jobs (keep for historical record but mark as archived)
                var imagingJobs = await _imagingJobService.GetImagingJobsByAssetAsync(asset.Id);
                foreach (var job in imagingJobs.Where(j => j.Status == JobStatus.Completed || j.Status == JobStatus.Failed))
                {
                    // In a real system, you might soft-delete or archive these
                    // For now, we'll leave them as they provide historical value
                }

                // Asset history will be handled by cascade delete in database
                // or you could manually clean it here if needed
            }
            catch (Exception ex)
            {
                // Log error but don't fail the main deletion
                Console.WriteLine($"Error handling asset deletion cascade: {ex.Message}");
            }
        }
    }

    public class AssetDeletionValidation
    {
        public bool CanDelete { get; set; } = true;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class BulkUpdateModel
    {
        public List<int> AssetIds { get; set; } = new();
        public string NewStatus { get; set; } = string.Empty;
    }

    public class BulkAssignModel
    {
        public List<int> AssetIds { get; set; } = new();
        public int? EmployeeId { get; set; }
    }

    public class UnassignAssetModel
    {
        public int AssetId { get; set; }
    }

    public class AssignAssetModel
    {
        public int AssetId { get; set; }
        public int EmployeeId { get; set; }
    }

    public class UpdateStatusModel
    {
        public int AssetId { get; set; }
        public string NewStatus { get; set; } = string.Empty;
    }
}