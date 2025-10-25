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
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;

        public CreateModel(
            IImagingJobService imagingJobService,
            IAssetService assetService,
            IEmployeeService employeeService,
            IWebHostEnvironment environment,
            ApplicationDbContext context)
        {
            _imagingJobService = imagingJobService;
            _assetService = assetService;
            _employeeService = employeeService;
            _environment = environment;
            _context = context;
        }

        [BindProperty]
        public ImagingJobCreateModel ImagingJob { get; set; } = default!;

        [BindProperty]
        public List<IFormFile>? MaintenanceFiles { get; set; }

        public List<Asset> AvailableAssets { get; set; } = new();
        public List<Employee> Technicians { get; set; } = new();
        
        [BindProperty(SupportsGet = true)]
        public string? JobType { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                await LoadDropdownData();
                ImagingJob = new ImagingJobCreateModel
                {
                    ScheduledAt = DateTime.Now.AddHours(1), // Default to 1 hour from now
                    JobType = JobType == "Maintenance" ? Data.Enums.JobType.Maintenance : Data.Enums.JobType.Imaging
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
                    JobType = ImagingJob.JobType,
                    ImagingType = ImagingJob.JobType == Data.Enums.JobType.Imaging ? ImagingJob.ImagingType : null,
                    ImageVersion = ImagingJob.ImageVersion?.Trim(),
                    Priority = ImagingJob.Priority,
                    Status = JobStatus.Pending,
                    ScheduledAt = ImagingJob.ScheduledAt,
                    DueDate = ImagingJob.DueDate,
                    Notes = ImagingJob.Notes?.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                var createdJob = await _imagingJobService.CreateImagingJobAsync(imagingJob);

                // Handle file uploads for maintenance jobs
                if (ImagingJob.JobType == Data.Enums.JobType.Maintenance && MaintenanceFiles != null && MaintenanceFiles.Any())
                {
                    await SaveMaintenanceFilesAsync(createdJob.Id, MaintenanceFiles);
                }

                var jobTypeText = ImagingJob.JobType == Data.Enums.JobType.Maintenance ? "Maintenance" : "Imaging";
                TempData["SuccessMessage"] = $"{jobTypeText} job for asset '{asset.AssetTag}' has been scheduled successfully.";
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

        private async Task SaveMaintenanceFilesAsync(int jobId, List<IFormFile> files)
        {
            try
            {
                var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "jobs", jobId.ToString());
                Directory.CreateDirectory(uploadPath);

                var userId = User.Identity?.Name ?? "System";

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        // Generate unique filename
                        var fileExtension = Path.GetExtension(file.FileName);
                        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                        var filePath = Path.Combine(uploadPath, uniqueFileName);

                        // Save file to disk
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Create attachment record
                        var attachment = new JobAttachment
                        {
                            JobId = jobId,
                            FileName = file.FileName,
                            FilePath = Path.Combine("uploads", "jobs", jobId.ToString(), uniqueFileName),
                            ContentType = file.ContentType,
                            FileSizeBytes = file.Length,
                            Description = $"Uploaded during job creation",
                            UploadedBy = userId,
                            UploadedAt = DateTime.UtcNow
                        };

                        _context.JobAttachments.Add(attachment);
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log error but don't fail the job creation
                Console.WriteLine($"Error saving maintenance files: {ex.Message}");
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

        [Required(ErrorMessage = "Job type is required")]
        [Display(Name = "Job Type")]
        public JobType JobType { get; set; } = JobType.Imaging;

        [Display(Name = "Imaging Type")]
        public ImagingType? ImagingType { get; set; } = Data.Enums.ImagingType.Fresh;

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