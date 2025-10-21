using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using buildone.Services;
using buildone.Data;
using buildone.Data.Enums;
using buildone.Models.Requests;

namespace buildone.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for all actions
    public class AssetsController : ControllerBase
    {
        private readonly IAssetService _assetService;
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly IImagingJobService _imagingJobService;
        private readonly IAssetHistoryService _assetHistoryService;
        private readonly ILogger<AssetsController> _logger;

        public AssetsController(
            IAssetService assetService,
            IEmployeeService employeeService,
            IDepartmentService departmentService,
            IImagingJobService imagingJobService,
            IAssetHistoryService assetHistoryService,
            ILogger<AssetsController> logger)
        {
            _assetService = assetService;
            _employeeService = employeeService;
            _departmentService = departmentService;
            _imagingJobService = imagingJobService;
            _assetHistoryService = assetHistoryService;
            _logger = logger;
        }

        // GET /api/assets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Asset>>> GetAssets(
            [FromQuery] AssetStatus? status = null,
            [FromQuery] int? departmentId = null,
            [FromQuery] string? search = null)
        {
            try
            {
                var assets = await _assetService.GetAllAssetsAsync();

                // Apply filters
                if (status.HasValue)
                {
                    assets = assets.Where(a => a.Status == status.Value);
                }

                if (departmentId.HasValue)
                {
                    assets = assets.Where(a => a.DepartmentId == departmentId.Value);
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.ToLower();
                    assets = assets.Where(a => 
                        (a.PcId != null && a.PcId.ToLower().Contains(searchLower)) ||
                        (a.SerialNumber != null && a.SerialNumber.ToLower().Contains(searchLower)) ||
                        (a.AssetTag != null && a.AssetTag.ToLower().Contains(searchLower)));
                }

                return Ok(assets.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving assets");
                return StatusCode(500, "An error occurred while retrieving assets");
            }
        }

        // GET /api/assets/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetAsset(int id)
        {
            try
            {
                var asset = await _assetService.GetAssetByIdAsync(id);
                if (asset == null)
                {
                    return NotFound($"Asset with ID {id} not found");
                }

                // Get related data
                var history = await _assetHistoryService.GetAssetHistoryAsync(id);
                var imagingJobs = await _imagingJobService.GetImagingJobsByAssetAsync(id);

                var result = new
                {
                    Asset = asset,
                    History = history,
                    ImagingJobs = imagingJobs
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving asset {AssetId}", id);
                return StatusCode(500, "An error occurred while retrieving the asset");
            }
        }

        // POST /api/assets
        [HttpPost]
        public async Task<ActionResult<Asset>> CreateAsset([FromBody] CreateAssetRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if AssetTag or PcId already exists
                var existingAssets = await _assetService.GetAllAssetsAsync();
                if (existingAssets.Any(a => a.AssetTag == request.AssetTag))
                {
                    return Conflict($"Asset with tag '{request.AssetTag}' already exists");
                }
                if (existingAssets.Any(a => a.PcId == request.PcId))
                {
                    return Conflict($"Asset with PC ID '{request.PcId}' already exists");
                }

                var asset = new Asset
                {
                    AssetTag = request.AssetTag,
                    PcId = request.PcId,
                    Brand = request.Brand,
                    Model = request.Model,
                    SerialNumber = request.SerialNumber,
                    ImagingType = request.ImagingType,
                    DeploymentType = request.DeploymentType,
                    Status = request.Status ?? AssetStatus.InStock,
                    AssignedEmployeeId = request.AssignedEmployeeId,
                    DepartmentId = request.DepartmentId,
                    Notes = request.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                await _assetService.CreateAssetAsync(asset);

                // Log asset creation
                await _assetHistoryService.LogAssetCreatedAsync(asset.Id, null, "Asset created via API");

                return CreatedAtAction(nameof(GetAsset), new { id = asset.Id }, asset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating asset");
                return StatusCode(500, "An error occurred while creating the asset");
            }
        }

        // PUT /api/assets/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsset(int id, [FromBody] UpdateAssetRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingAsset = await _assetService.GetAssetByIdAsync(id);
                if (existingAsset == null)
                {
                    return NotFound($"Asset with ID {id} not found");
                }

                // Track changes for history
                var changes = new List<(string Property, string? OldValue, string? NewValue)>();

                if (existingAsset.Brand != request.Brand)
                    changes.Add(("Brand", existingAsset.Brand, request.Brand));
                if (existingAsset.Model != request.Model)
                    changes.Add(("Model", existingAsset.Model, request.Model));
                if (existingAsset.SerialNumber != request.SerialNumber)
                    changes.Add(("SerialNumber", existingAsset.SerialNumber, request.SerialNumber));
                if (existingAsset.Status != request.Status)
                    changes.Add(("Status", existingAsset.Status.ToString(), request.Status.ToString()));
                if (existingAsset.Notes != request.Notes)
                    changes.Add(("Notes", existingAsset.Notes, request.Notes));

                // Update asset properties
                existingAsset.Brand = request.Brand;
                existingAsset.Model = request.Model;
                existingAsset.SerialNumber = request.SerialNumber;
                existingAsset.ImagingType = request.ImagingType;
                existingAsset.DeploymentType = request.DeploymentType;
                existingAsset.Status = request.Status;
                existingAsset.Notes = request.Notes;

                await _assetService.UpdateAssetAsync(existingAsset);

                // Log changes
                foreach (var change in changes)
                {
                    await _assetHistoryService.LogAssetUpdatedAsync(
                        id, null, change.Property, change.OldValue, change.NewValue);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating asset {AssetId}", id);
                return StatusCode(500, "An error occurred while updating the asset");
            }
        }

        // DELETE /api/assets/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsset(int id)
        {
            try
            {
                var asset = await _assetService.GetAssetByIdAsync(id);
                if (asset == null)
                {
                    return NotFound($"Asset with ID {id} not found");
                }

                // Log deletion before removing
                await _assetHistoryService.LogAssetDeletedAsync(id, null, "Asset deleted via API");

                await _assetService.DeleteAssetAsync(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting asset {AssetId}", id);
                return StatusCode(500, "An error occurred while deleting the asset");
            }
        }

        // POST /api/assets/{id}/assign
        [HttpPost("{id}/assign")]
        public async Task<IActionResult> AssignAsset(int id, [FromBody] AssignAssetRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var asset = await _assetService.GetAssetByIdAsync(id);
                if (asset == null)
                {
                    return NotFound($"Asset with ID {id} not found");
                }

                // Validate employee exists
                Employee? employee = null;
                if (request.EmployeeId.HasValue)
                {
                    employee = await _employeeService.GetEmployeeByIdAsync(request.EmployeeId.Value);
                    if (employee == null)
                    {
                        return BadRequest($"Employee with ID {request.EmployeeId} not found");
                    }
                }

                // Validate department exists
                Department? department = null;
                if (request.DepartmentId.HasValue)
                {
                    department = await _departmentService.GetDepartmentByIdAsync(request.DepartmentId.Value);
                    if (department == null)
                    {
                        return BadRequest($"Department with ID {request.DepartmentId} not found");
                    }
                }

                // Track assignment changes
                var oldEmployee = asset.AssignedEmployee?.FullName;
                var newEmployee = employee?.FullName;

                // Update assignment
                asset.AssignedEmployeeId = request.EmployeeId;
                asset.DepartmentId = request.DepartmentId;
                asset.Status = request.EmployeeId.HasValue ? AssetStatus.Assigned : AssetStatus.InStock;

                await _assetService.UpdateAssetAsync(asset);

                // Log assignment change
                await _assetHistoryService.LogAssetAssignmentAsync(
                    id, null, oldEmployee, newEmployee, request.Notes);

                return Ok(new { Message = "Asset assignment updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning asset {AssetId}", id);
                return StatusCode(500, "An error occurred while assigning the asset");
            }
        }

        // POST /api/assets/{id}/imaging
        [HttpPost("{id}/imaging")]
        public async Task<ActionResult<ImagingJob>> CreateImagingJob(int id, [FromBody] CreateImagingJobRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var asset = await _assetService.GetAssetByIdAsync(id);
                if (asset == null)
                {
                    return NotFound($"Asset with ID {id} not found");
                }

                // Validate technician exists
                Employee? technician = null;
                if (request.TechnicianId.HasValue)
                {
                    technician = await _employeeService.GetEmployeeByIdAsync(request.TechnicianId.Value);
                    if (technician == null)
                    {
                        return BadRequest($"Technician with ID {request.TechnicianId} not found");
                    }
                }

                var imagingJob = new ImagingJob
                {
                    AssetId = id,
                    TechnicianId = request.TechnicianId,
                    ImagingType = request.ImagingType,
                    Status = JobStatus.Scheduled,
                    ScheduledAt = request.ScheduledAt ?? DateTime.UtcNow.AddHours(1),
                    ImageVersion = request.ImageVersion,
                    Notes = request.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                await _imagingJobService.CreateImagingJobAsync(imagingJob);

                // Log imaging job creation
                await _assetHistoryService.LogActionAsync(
                    id, request.TechnicianId, "Imaging Job Created", 
                    null, request.ImagingType.ToString(),
                    $"Imaging job scheduled for {imagingJob.ScheduledAt:yyyy-MM-dd HH:mm}");

                return CreatedAtAction("GetImagingJob", "ImagingJobs", new { id = imagingJob.Id }, imagingJob);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating imaging job for asset {AssetId}", id);
                return StatusCode(500, "An error occurred while creating the imaging job");
            }
        }

        // GET /api/assets/reports/warranty-expiry
        [HttpGet("reports/warranty-expiry")]
        public async Task<ActionResult<IEnumerable<object>>> GetWarrantyExpiryReport([FromQuery] int days = 30)
        {
            try
            {
                var assets = await _assetService.GetAllAssetsAsync();
                var cutoffDate = DateTime.UtcNow.AddDays(days);

                var expiringAssets = assets
                    .Where(a => a.WarrantyExpiry.HasValue && a.WarrantyExpiry.Value <= cutoffDate && a.WarrantyExpiry.Value >= DateTime.UtcNow)
                    .OrderBy(a => a.WarrantyExpiry)
                    .Select(a => new
                    {
                        a.Id,
                        a.AssetTag,
                        a.PcId,
                        a.Brand,
                        a.Model,
                        a.SerialNumber,
                        a.WarrantyExpiry,
                        DaysUntilExpiry = a.WarrantyExpiry.HasValue ? (a.WarrantyExpiry.Value - DateTime.UtcNow).Days : (int?)null,
                        AssignedEmployee = a.AssignedEmployee?.FullName,
                        Department = a.Department?.Name,
                        a.Status
                    })
                    .ToList();

                return Ok(expiringAssets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating warranty expiry report");
                return StatusCode(500, "An error occurred while generating the warranty expiry report");
            }
        }
    }

    // Request DTOs
    public class CreateAssetRequest
    {
        public string AssetTag { get; set; } = string.Empty;
        public string PcId { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public ImagingType ImagingType { get; set; }
        public DeploymentType DeploymentType { get; set; }
        public AssetStatus? Status { get; set; }
        public int? AssignedEmployeeId { get; set; }
        public int? DepartmentId { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateAssetRequest
    {
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public ImagingType ImagingType { get; set; }
        public DeploymentType DeploymentType { get; set; }
        public AssetStatus Status { get; set; }
        public string? Notes { get; set; }
    }

    public class AssignAssetRequest
    {
        public int? EmployeeId { get; set; }
        public int? DepartmentId { get; set; }
        public string? Notes { get; set; }
    }
}