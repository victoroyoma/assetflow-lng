using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using buildone.Data;
using buildone.Data.Enums;
using buildone.Services;
using System.ComponentModel.DataAnnotations;

namespace buildone.Pages.Imaging
{
    public class CreateModel : PageModel
    {
        private readonly IImagingJobService _imagingJobService;
        private readonly IAssetService _assetService;
        private readonly IEmployeeService _employeeService;

        public CreateModel(
            IImagingJobService imagingJobService,
            IAssetService assetService,
            IEmployeeService employeeService)
        {
            _imagingJobService = imagingJobService;
            _assetService = assetService;
            _employeeService = employeeService;
        }

        [BindProperty]
        public ImagingJobCreateModel ImagingJob { get; set; } = default!;

        public List<Asset> AvailableAssets { get; set; } = new();
        public List<Employee> Technicians { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                await LoadDropdownData();
                ImagingJob = new ImagingJobCreateModel
                {
                    ScheduledAt = DateTime.Now.AddHours(1) // Default to 1 hour from now
                };
                return Page();
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Error loading form data. Please try again.";
                return RedirectToPage("./Queue");
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
                // Check if asset exists and is available for imaging
                var asset = await _assetService.GetAssetByIdAsync(ImagingJob.AssetId);
                if (asset == null)
                {
                    ModelState.AddModelError("ImagingJob.AssetId", "Selected asset does not exist.");
                    await LoadDropdownData();
                    return Page();
                }

                // Check if asset already has a pending/in-progress imaging job
                var existingJob = await _imagingJobService.GetActiveJobForAssetAsync(ImagingJob.AssetId);
                if (existingJob != null)
                {
                    ModelState.AddModelError("ImagingJob.AssetId", $"Asset already has an active imaging job (Status: {existingJob.Status}).");
                    await LoadDropdownData();
                    return Page();
                }

                // Validate technician if assigned
                if (ImagingJob.TechnicianId.HasValue)
                {
                    var technician = await _employeeService.GetEmployeeByIdAsync(ImagingJob.TechnicianId.Value);
                    if (technician == null)
                    {
                        ModelState.AddModelError("ImagingJob.TechnicianId", "Selected technician does not exist.");
                        await LoadDropdownData();
                        return Page();
                    }
                }

                // Create the imaging job
                var imagingJob = new ImagingJob
                {
                    AssetId = ImagingJob.AssetId,
                    TechnicianId = ImagingJob.TechnicianId == 0 ? null : ImagingJob.TechnicianId,
                    ImagingType = ImagingJob.ImagingType,
                    ImageVersion = ImagingJob.ImageVersion?.Trim(),
                    Priority = ImagingJob.Priority,
                    Status = JobStatus.Pending,
                    ScheduledAt = ImagingJob.ScheduledAt,
                    DueDate = ImagingJob.DueDate,
                    Notes = ImagingJob.Notes?.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                var createdJob = await _imagingJobService.CreateImagingJobAsync(imagingJob);

                TempData["SuccessMessage"] = $"Imaging job for asset '{asset.AssetTag}' has been scheduled successfully.";
                return RedirectToPage("./Queue");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred while creating the imaging job: {ex.Message}");
                await LoadDropdownData();
                return Page();
            }
        }

        private async Task LoadDropdownData()
        {
            try
            {
                // Load assets that are available for imaging (not currently being imaged)
                var allAssets = await _assetService.GetAllAssetsAsync();
                var assetsWithActiveJobs = await _imagingJobService.GetAssetsWithActiveJobsAsync();
                
                AvailableAssets = allAssets
                    .Where(a => !assetsWithActiveJobs.Contains(a.Id))
                    .OrderBy(a => a.AssetTag)
                    .ToList();

                // Load all employees as potential technicians
                Technicians = (await _employeeService.GetAllEmployeesAsync())
                    .OrderBy(e => e.FullName)
                    .ToList();
            }
            catch (Exception)
            {
                AvailableAssets = new List<Asset>();
                Technicians = new List<Employee>();
                ModelState.AddModelError(string.Empty, "Error loading dropdown data.");
            }
        }
    }

    public class ImagingJobCreateModel
    {
        [Required(ErrorMessage = "Asset is required")]
        [Display(Name = "Asset")]
        public int AssetId { get; set; }

        [Display(Name = "Technician")]
        public int? TechnicianId { get; set; }

        [Required(ErrorMessage = "Imaging type is required")]
        [Display(Name = "Imaging Type")]
        public ImagingType ImagingType { get; set; } = ImagingType.Fresh;

        [StringLength(50, ErrorMessage = "Image version cannot exceed 50 characters")]
        [Display(Name = "Image Version")]
        public string? ImageVersion { get; set; }

        [Required(ErrorMessage = "Priority is required")]
        [Display(Name = "Priority")]
        public JobPriority Priority { get; set; } = JobPriority.Normal;

        [Display(Name = "Scheduled Date & Time")]
        [DataType(DataType.DateTime)]
        public DateTime? ScheduledAt { get; set; }

        [Display(Name = "Due Date")]
        [DataType(DataType.DateTime)]
        public DateTime? DueDate { get; set; }

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }
}