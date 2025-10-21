using buildone.Data;
using buildone.Data.Enums;
using buildone.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace buildone.Pages.Assets
{
    public class EditModel : PageModel
    {
        private readonly IAssetService _assetService;
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;

        public EditModel(
            IAssetService assetService,
            IEmployeeService employeeService,
            IDepartmentService departmentService)
        {
            _assetService = assetService;
            _employeeService = employeeService;
            _departmentService = departmentService;
        }

        [BindProperty]
        public AssetEditModel Asset { get; set; } = default!;

        public List<Employee> Employees { get; set; } = new();
        public List<Department> Departments { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var asset = await _assetService.GetAssetByIdAsync(id);
                if (asset == null)
                {
                    TempData["ErrorMessage"] = "Asset not found.";
                    return RedirectToPage("./Index");
                }

                await LoadDropdownData();

                Asset = new AssetEditModel
                {
                    Id = asset.Id,
                    AssetTag = asset.AssetTag,
                    PcId = asset.PcId,
                    Brand = asset.Brand,
                    Model = asset.Model,
                    SerialNumber = asset.SerialNumber,
                    Type = asset.Type,
                    WarrantyExpiry = asset.WarrantyExpiry,
                    ImagingType = asset.ImagingType,
                    DeploymentType = asset.DeploymentType,
                    Status = asset.Status,
                    AssignedEmployeeId = asset.AssignedEmployeeId,
                    DepartmentId = asset.DepartmentId,
                    Notes = asset.Notes,
                    CreatedAt = asset.CreatedAt,
                    UpdatedAt = asset.UpdatedAt,
                    AssignedEmployee = asset.AssignedEmployee,
                    Department = asset.Department
                };

                return Page();
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Error loading asset data. Please try again.";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownData();
                return Page();
            }

            try
            {
                // Check if asset tag is unique (excluding current asset)
                var existingAsset = await _assetService.GetAssetByAssetTagAsync(Asset.AssetTag);
                if (existingAsset != null && existingAsset.Id != Asset.Id)
                {
                    ModelState.AddModelError("Asset.AssetTag", "An asset with this tag already exists.");
                    await LoadDropdownData();
                    return Page();
                }

                // Get the current asset to preserve created date and other audit fields
                var currentAsset = await _assetService.GetAssetByIdAsync(Asset.Id);
                if (currentAsset == null)
                {
                    TempData["ErrorMessage"] = "Asset not found.";
                    return RedirectToPage("./Index");
                }

                // Update asset properties
                currentAsset.AssetTag = Asset.AssetTag;
                currentAsset.PcId = Asset.PcId;
                currentAsset.Brand = Asset.Brand;
                currentAsset.Model = Asset.Model;
                currentAsset.SerialNumber = Asset.SerialNumber;
                currentAsset.Type = Asset.Type;
                currentAsset.WarrantyExpiry = Asset.WarrantyExpiry;
                currentAsset.ImagingType = Asset.ImagingType;
                currentAsset.DeploymentType = Asset.DeploymentType;
                currentAsset.Status = Asset.Status;
                currentAsset.AssignedEmployeeId = Asset.AssignedEmployeeId.HasValue && Asset.AssignedEmployeeId.Value > 0 ? Asset.AssignedEmployeeId : null;
                currentAsset.DepartmentId = Asset.DepartmentId.HasValue && Asset.DepartmentId.Value > 0 ? Asset.DepartmentId : null;
                currentAsset.Notes = Asset.Notes;
                currentAsset.UpdatedAt = DateTime.UtcNow;

                var success = await _assetService.UpdateAssetAsync(currentAsset);
                if (success)
                {
                    TempData["SuccessMessage"] = $"Asset '{currentAsset.AssetTag}' has been updated successfully.";
                    return RedirectToPage("./Details", new { id = Asset.Id });
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to update the asset. Please try again.");
                    await LoadDropdownData();
                    return Page();
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while updating the asset. Please try again.");
                await LoadDropdownData();
                return Page();
            }
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
                ModelState.AddModelError(string.Empty, "Error loading dropdown data.");
            }
        }
    }

    public class AssetEditModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Asset tag is required")]
        [StringLength(50, ErrorMessage = "Asset tag cannot exceed 50 characters")]
        [Display(Name = "Asset Tag")]
        public string AssetTag { get; set; } = string.Empty;

        [Required(ErrorMessage = "PC ID is required")]
        [StringLength(50, ErrorMessage = "PC ID cannot exceed 50 characters")]
        [Display(Name = "PC ID")]
        public string PcId { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Brand cannot exceed 50 characters")]
        [Display(Name = "Brand")]
        public string? Brand { get; set; }

        [StringLength(100, ErrorMessage = "Model cannot exceed 100 characters")]
        [Display(Name = "Model")]
        public string? Model { get; set; }

        [StringLength(100, ErrorMessage = "Serial number cannot exceed 100 characters")]
        [Display(Name = "Serial Number")]
        public string? SerialNumber { get; set; }

        [StringLength(50, ErrorMessage = "Asset type cannot exceed 50 characters")]
        [Display(Name = "Asset Type")]
        public string Type { get; set; } = "Desktop";

        [Display(Name = "Warranty Expiry")]
        public DateTime? WarrantyExpiry { get; set; }

        [Required]
        [Display(Name = "Imaging Type")]
        public ImagingType ImagingType { get; set; } = ImagingType.None;

        [Required]
        [Display(Name = "Deployment Type")]
        public DeploymentType DeploymentType { get; set; } = DeploymentType.InPlace;

        [Required]
        [Display(Name = "Status")]
        public AssetStatus Status { get; set; } = AssetStatus.InStock;

        [Display(Name = "Assigned Employee")]
        public int? AssignedEmployeeId { get; set; }

        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // Audit fields (read-only)
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties for display
        public Employee? AssignedEmployee { get; set; }
        public Department? Department { get; set; }
    }
}