using buildone.Data;
using buildone.Data.Enums;
using buildone.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace buildone.Pages.Assets
{
    public class CreateModel : PageModel
    {
        private readonly IAssetService _assetService;
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;

        public CreateModel(
            IAssetService assetService,
            IEmployeeService employeeService,
            IDepartmentService departmentService)
        {
            _assetService = assetService;
            _employeeService = employeeService;
            _departmentService = departmentService;
        }

        [BindProperty]
        public AssetCreateModel Asset { get; set; } = default!;

        public List<Employee> Employees { get; set; } = new();
        public List<Department> Departments { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? newDepartmentId = null)
        {
            try
            {
                await LoadDropdownData();
                Asset = new AssetCreateModel();
                
                // If a new department was just created, pre-select it
                if (newDepartmentId.HasValue)
                {
                    Asset.DepartmentId = newDepartmentId.Value;
                    TempData["SuccessMessage"] = "Department created successfully. It has been pre-selected below.";
                }
                
                return Page();
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Error loading form data. Please try again.";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Add debugging
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState errors:");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"  {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                await LoadDropdownData();
                return Page();
            }

            try
            {
                Console.WriteLine($"Asset data received - Tag: {Asset.AssetTag}, PcId: {Asset.PcId}, Type: {Asset.Type}, Status: {Asset.Status}");
                
                // Basic validation - check if asset tag is unique
                var tagExists = await _assetService.AssetTagExistsAsync(Asset.AssetTag);
                if (tagExists)
                {
                    ModelState.AddModelError("Asset.AssetTag", "An asset with this tag already exists.");
                    await LoadDropdownData();
                    return Page();
                }

                // Check if PC ID is unique
                var pcIdExists = await _assetService.PcIdExistsAsync(Asset.PcId);
                if (pcIdExists)
                {
                    ModelState.AddModelError("Asset.PcId", "An asset with this PC ID already exists.");
                    await LoadDropdownData();
                    return Page();
                }

                // Create new asset entity with proper data handling
                var asset = new Asset
                {
                    AssetTag = Asset.AssetTag.Trim(),
                    PcId = Asset.PcId.Trim(),
                    SerialNumber = Asset.SerialNumber?.Trim(),
                    Type = Asset.Type,
                    Brand = Asset.Brand?.Trim(),
                    Model = Asset.Model?.Trim(),
                    WarrantyExpiry = Asset.WarrantyExpiry,
                    ImagingType = Asset.ImagingType,
                    DeploymentType = Asset.DeploymentType,
                    Status = Asset.Status,
                    AssignedEmployeeId = Asset.AssignedEmployeeId.HasValue && Asset.AssignedEmployeeId.Value > 0 ? Asset.AssignedEmployeeId : null,
                    DepartmentId = Asset.DepartmentId.HasValue && Asset.DepartmentId.Value > 0 ? Asset.DepartmentId : null,
                    Notes = Asset.Notes?.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Create asset
                Console.WriteLine("About to create asset...");
                var createdAsset = await _assetService.CreateAssetAsync(asset);
                Console.WriteLine($"Asset created successfully with ID: {createdAsset.Id}");
                
                TempData["SuccessMessage"] = $"Asset '{createdAsset.AssetTag}' has been created successfully.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating asset: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                ModelState.AddModelError(string.Empty, $"An error occurred while creating the asset: {ex.Message}");
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

    public class AssetCreateModel
    {
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

        [Required(ErrorMessage = "Asset type is required")]
        [StringLength(50, ErrorMessage = "Asset type cannot exceed 50 characters")]
        [Display(Name = "Asset Type")]
        public string Type { get; set; } = "Desktop";

        [Display(Name = "Warranty Expiry")]
        [DataType(DataType.Date)]
        public DateTime? WarrantyExpiry { get; set; }

        [Required]
        [Display(Name = "Imaging Type")]
        public ImagingType ImagingType { get; set; } = ImagingType.None;

        [Required]
        [Display(Name = "Deployment Type")]
        public DeploymentType DeploymentType { get; set; } = DeploymentType.InPlace;

        [Required(ErrorMessage = "Status is required")]
        public AssetStatus Status { get; set; } = AssetStatus.InStock;

        [Display(Name = "Assigned Employee")]
        public int? AssignedEmployeeId { get; set; }

        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }
    }
}