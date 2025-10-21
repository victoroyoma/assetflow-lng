using buildone.Data;
using buildone.Data.Enums;
using buildone.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace buildone.Pages.Assets
{
    public class DetailsModel : PageModel
    {
        private readonly IAssetService _assetService;
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly IAssetHistoryService _assetHistoryService;
        private readonly IImagingJobService _imagingJobService;

        public DetailsModel(
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

        [BindProperty]
        public Asset Asset { get; set; } = default!;

        public List<Employee> Employees { get; set; } = new();
        public List<Department> Departments { get; set; } = new();
        public List<AssetHistory> AssetHistory { get; set; } = new();
        public List<ImagingJob> ImagingJobs { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                // Enhanced asset retrieval with related data
                var asset = await _assetService.GetAssetByIdAsync(id);
                if (asset == null)
                {
                    TempData["ErrorMessage"] = "Asset not found.";
                    return RedirectToPage("./Index");
                }

                Asset = asset;

                // Load related data for comprehensive view
                await LoadDropdownData();
                await LoadAssetRelatedData();

                return Page();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading asset details: {ex.Message}";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAssignAsync(int employeeId, int? departmentId = null)
        {
            try
            {
                // Validate employee exists
                var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
                if (employee == null)
                {
                    TempData["ErrorMessage"] = "Selected employee not found.";
                    return RedirectToPage(new { id = Asset.Id });
                }

                // Validate department if provided
                if (departmentId.HasValue && departmentId > 0)
                {
                    var department = await _departmentService.GetDepartmentByIdAsync(departmentId.Value);
                    if (department == null)
                    {
                        TempData["ErrorMessage"] = "Selected department not found.";
                        return RedirectToPage(new { id = Asset.Id });
                    }
                }

                // Get current assignment for history tracking
                var currentAsset = await _assetService.GetAssetByIdAsync(Asset.Id);
                var oldEmployee = currentAsset?.AssignedEmployee?.FullName ?? "Unassigned";

                // Perform assignment
                var success = await _assetService.AssignAssetToEmployeeAsync(Asset.Id, employeeId, departmentId);
                if (success)
                {
                    // Log the assignment in history
                    await _assetHistoryService.LogAssetAssignmentAsync(
                        Asset.Id, 
                        null, // In real app, this would be current user ID
                        oldEmployee, 
                        employee.FullName,
                        "Asset assignment updated"
                    );

                    // Update asset status to Active if it was InStock
                    if (currentAsset?.Status == AssetStatus.InStock)
                    {
                        currentAsset.Status = AssetStatus.Active;
                        await _assetService.UpdateAssetAsync(currentAsset);
                        
                        await _assetHistoryService.LogAssetStatusChangeAsync(
                            Asset.Id,
                            null,
                            "InStock",
                            "Active",
                            "Status updated during assignment"
                        );
                    }

                    TempData["SuccessMessage"] = $"Asset has been assigned to {employee.FullName} successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to assign asset. Please try again.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while assigning the asset: {ex.Message}";
            }

            return RedirectToPage(new { id = Asset.Id });
        }

        public async Task<IActionResult> OnPostUnassignAsync()
        {
            try
            {
                // Get current assignment for history tracking
                var currentAsset = await _assetService.GetAssetByIdAsync(Asset.Id);
                var oldEmployee = currentAsset?.AssignedEmployee?.FullName ?? "Unassigned";

                var success = await _assetService.UnassignAssetAsync(Asset.Id);
                if (success)
                {
                    // Log the unassignment in history
                    await _assetHistoryService.LogAssetAssignmentAsync(
                        Asset.Id,
                        null, // In real app, this would be current user ID
                        oldEmployee,
                        "Unassigned",
                        "Asset unassigned"
                    );

                    // Update asset status to InStock
                    if (currentAsset != null)
                    {
                        var oldStatus = currentAsset.Status.ToString();
                        currentAsset.Status = AssetStatus.InStock;
                        currentAsset.AssignedEmployeeId = null;
                        currentAsset.DepartmentId = null;
                        currentAsset.UpdatedAt = DateTime.UtcNow;
                        
                        await _assetService.UpdateAssetAsync(currentAsset);
                        
                        await _assetHistoryService.LogAssetStatusChangeAsync(
                            Asset.Id,
                            null,
                            oldStatus,
                            "InStock",
                            "Status updated during unassignment"
                        );
                    }

                    TempData["SuccessMessage"] = "Asset has been unassigned successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to unassign asset. Please try again.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while unassigning the asset: {ex.Message}";
            }

            return RedirectToPage(new { id = Asset.Id });
        }

        public async Task<IActionResult> OnPostSetMaintenanceAsync()
        {
            try
            {
                var asset = await _assetService.GetAssetByIdAsync(Asset.Id);
                if (asset == null)
                {
                    TempData["ErrorMessage"] = "Asset not found.";
                    return RedirectToPage("./Index");
                }

                asset.Status = AssetStatus.Maintenance;
                asset.UpdatedAt = DateTime.UtcNow;

                var success = await _assetService.UpdateAssetAsync(asset);
                if (success)
                {
                    TempData["SuccessMessage"] = "Asset has been marked for maintenance.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update asset status. Please try again.";
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the asset status. Please try again.";
            }

            return RedirectToPage(new { id = Asset.Id });
        }

        public async Task<IActionResult> OnPostSetActiveAsync()
        {
            try
            {
                var asset = await _assetService.GetAssetByIdAsync(Asset.Id);
                if (asset == null)
                {
                    TempData["ErrorMessage"] = "Asset not found.";
                    return RedirectToPage("./Index");
                }

                asset.Status = AssetStatus.Active;
                asset.UpdatedAt = DateTime.UtcNow;

                var success = await _assetService.UpdateAssetAsync(asset);
                if (success)
                {
                    TempData["SuccessMessage"] = "Asset has been marked as active.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update asset status. Please try again.";
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the asset status. Please try again.";
            }

            return RedirectToPage(new { id = Asset.Id });
        }

        private async Task LoadDropdownData()
        {
            try
            {
                // Load data sequentially to avoid concurrency issues
                Employees = (await _employeeService.GetAllEmployeesAsync()).ToList();
                Departments = (await _departmentService.GetAllDepartmentsAsync()).ToList();
            }
            catch (Exception)
            {
                Employees = new List<Employee>();
                Departments = new List<Department>();
            }
        }

        private async Task LoadAssetRelatedData()
        {
            try
            {
                // Load asset history for comprehensive tracking
                AssetHistory = (await _assetHistoryService.GetAssetHistoryAsync(Asset.Id)).OrderByDescending(h => h.CreatedAt).ToList();
                
                // Load imaging jobs related to this asset
                ImagingJobs = (await _imagingJobService.GetImagingJobsByAssetAsync(Asset.Id)).OrderByDescending(j => j.CreatedAt).ToList();
            }
            catch (Exception ex)
            {
                // Log error and initialize empty lists
                AssetHistory = new List<AssetHistory>();
                ImagingJobs = new List<ImagingJob>();
                // In production, use proper logging
                Console.WriteLine($"Failed to load related data: {ex.Message}");
            }
        }
    }
}