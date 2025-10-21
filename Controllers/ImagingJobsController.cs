using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using buildone.Services;
using buildone.Data;
using buildone.Data.Enums;
using buildone.Models.Requests;

namespace buildone.Controllers
{
    [ApiController]
    [Route("api/imaging-jobs")]
    [Authorize] // All authenticated users can view imaging jobs, but modifications may be restricted
    public class ImagingJobsController : ControllerBase
    {
        private readonly IImagingJobService _imagingJobService;
        private readonly IAssetService _assetService;
        private readonly IEmployeeService _employeeService;
        private readonly IAssetHistoryService _assetHistoryService;
        private readonly ILogger<ImagingJobsController> _logger;

        public ImagingJobsController(
            IImagingJobService imagingJobService,
            IAssetService assetService,
            IEmployeeService employeeService,
            IAssetHistoryService assetHistoryService,
            ILogger<ImagingJobsController> logger)
        {
            _imagingJobService = imagingJobService;
            _assetService = assetService;
            _employeeService = employeeService;
            _assetHistoryService = assetHistoryService;
            _logger = logger;
        }

        // GET /api/imaging-jobs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ImagingJob>>> GetImagingJobs([FromQuery] JobStatus? status = null)
        {
            try
            {
                var jobs = await _imagingJobService.GetAllImagingJobsAsync();

                // Apply status filter if provided
                if (status.HasValue)
                {
                    jobs = jobs.Where(j => j.Status == status.Value);
                }

                return Ok(jobs.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving imaging jobs");
                return StatusCode(500, "An error occurred while retrieving imaging jobs");
            }
        }

        // GET /api/imaging-jobs/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ImagingJob>> GetImagingJob(int id)
        {
            try
            {
                var job = await _imagingJobService.GetImagingJobByIdAsync(id);
                if (job == null)
                {
                    return NotFound($"Imaging job with ID {id} not found");
                }

                return Ok(job);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving imaging job {JobId}", id);
                return StatusCode(500, "An error occurred while retrieving the imaging job");
            }
        }

        // POST /api/imaging-jobs
        [HttpPost]
        public async Task<ActionResult<ImagingJob>> CreateImagingJob([FromBody] CreateImagingJobRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate asset exists
                var asset = await _assetService.GetAssetByIdAsync(request.AssetId);
                if (asset == null)
                {
                    return BadRequest($"Asset with ID {request.AssetId} not found");
                }

                // Validate technician exists (if provided)
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
                    AssetId = request.AssetId,
                    TechnicianId = request.TechnicianId,
                    ImagingType = request.ImagingType,
                    Status = JobStatus.Scheduled,
                    ScheduledAt = request.ScheduledAt ?? DateTime.UtcNow.AddHours(1),
                    ImageVersion = request.ImageVersion,
                    Notes = request.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                await _imagingJobService.CreateImagingJobAsync(imagingJob);

                // Log imaging job creation in asset history
                await _assetHistoryService.LogActionAsync(
                    request.AssetId, 
                    request.TechnicianId, 
                    "Imaging Job Created",
                    null, 
                    request.ImagingType.ToString(),
                    $"Imaging job {imagingJob.Id} scheduled for {imagingJob.ScheduledAt:yyyy-MM-dd HH:mm}");

                return CreatedAtAction(nameof(GetImagingJob), new { id = imagingJob.Id }, imagingJob);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating imaging job");
                return StatusCode(500, "An error occurred while creating the imaging job");
            }
        }

        // PATCH /api/imaging-jobs/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateImagingJob(int id, [FromBody] UpdateImagingJobRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingJob = await _imagingJobService.GetImagingJobByIdAsync(id);
                if (existingJob == null)
                {
                    return NotFound($"Imaging job with ID {id} not found");
                }

                // Track changes for history
                var changes = new List<(string Property, string? OldValue, string? NewValue)>();
                var oldStatus = existingJob.Status;

                // Update only provided fields
                if (request.Status.HasValue && existingJob.Status != request.Status.Value)
                {
                    changes.Add(("Status", existingJob.Status.ToString(), request.Status.Value.ToString()));
                    existingJob.Status = request.Status.Value;

                    // Auto-set timestamps based on status changes
                    switch (request.Status.Value)
                    {
                        case JobStatus.InProgress:
                            if (!existingJob.StartedAt.HasValue)
                            {
                                existingJob.StartedAt = DateTime.UtcNow;
                                changes.Add(("StartedAt", null, existingJob.StartedAt.Value.ToString("yyyy-MM-dd HH:mm:ss")));
                            }
                            break;
                        case JobStatus.Completed:
                            if (!existingJob.CompletedAt.HasValue)
                            {
                                existingJob.CompletedAt = DateTime.UtcNow;
                                changes.Add(("CompletedAt", null, existingJob.CompletedAt.Value.ToString("yyyy-MM-dd HH:mm:ss")));
                            }
                            if (!existingJob.StartedAt.HasValue)
                            {
                                existingJob.StartedAt = DateTime.UtcNow;
                                changes.Add(("StartedAt", null, existingJob.StartedAt.Value.ToString("yyyy-MM-dd HH:mm:ss")));
                            }
                            break;
                        case JobStatus.Failed:
                            if (!existingJob.CompletedAt.HasValue)
                            {
                                existingJob.CompletedAt = DateTime.UtcNow;
                                changes.Add(("CompletedAt", null, existingJob.CompletedAt.Value.ToString("yyyy-MM-dd HH:mm:ss")));
                            }
                            break;
                    }
                }

                if (request.StartedAt.HasValue && existingJob.StartedAt != request.StartedAt.Value)
                {
                    changes.Add(("StartedAt", existingJob.StartedAt?.ToString("yyyy-MM-dd HH:mm:ss"), request.StartedAt.Value.ToString("yyyy-MM-dd HH:mm:ss")));
                    existingJob.StartedAt = request.StartedAt.Value;
                }

                if (request.CompletedAt.HasValue && existingJob.CompletedAt != request.CompletedAt.Value)
                {
                    changes.Add(("CompletedAt", existingJob.CompletedAt?.ToString("yyyy-MM-dd HH:mm:ss"), request.CompletedAt.Value.ToString("yyyy-MM-dd HH:mm:ss")));
                    existingJob.CompletedAt = request.CompletedAt.Value;
                }

                if (!string.IsNullOrWhiteSpace(request.Notes) && existingJob.Notes != request.Notes)
                {
                    changes.Add(("Notes", existingJob.Notes, request.Notes));
                    existingJob.Notes = request.Notes;
                }

                if (!string.IsNullOrWhiteSpace(request.ImageVersion) && existingJob.ImageVersion != request.ImageVersion)
                {
                    changes.Add(("ImageVersion", existingJob.ImageVersion, request.ImageVersion));
                    existingJob.ImageVersion = request.ImageVersion;
                }

                await _imagingJobService.UpdateImagingJobAsync(existingJob);

                // Log changes in asset history
                foreach (var change in changes)
                {
                    await _assetHistoryService.LogActionAsync(
                        existingJob.AssetId,
                        existingJob.TechnicianId,
                        $"Imaging Job {change.Property} Updated",
                        change.OldValue,
                        change.NewValue,
                        $"Job {id}: {change.Property} changed from '{change.OldValue}' to '{change.NewValue}'");
                }

                // Special logging for status changes
                if (oldStatus != existingJob.Status)
                {
                    var statusMessage = existingJob.Status switch
                    {
                        JobStatus.InProgress => "Imaging job started",
                        JobStatus.Completed => "Imaging job completed successfully",
                        JobStatus.Failed => "Imaging job failed",
                        JobStatus.Cancelled => "Imaging job cancelled",
                        _ => $"Imaging job status changed to {existingJob.Status}"
                    };

                    await _assetHistoryService.LogActionAsync(
                        existingJob.AssetId,
                        existingJob.TechnicianId,
                        "Imaging Job Status Change",
                        oldStatus.ToString(),
                        existingJob.Status.ToString(),
                        statusMessage);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating imaging job {JobId}", id);
                return StatusCode(500, "An error occurred while updating the imaging job");
            }
        }

        // DELETE /api/imaging-jobs/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImagingJob(int id)
        {
            try
            {
                var job = await _imagingJobService.GetImagingJobByIdAsync(id);
                if (job == null)
                {
                    return NotFound($"Imaging job with ID {id} not found");
                }

                // Log deletion in asset history before removing
                await _assetHistoryService.LogActionAsync(
                    job.AssetId,
                    job.TechnicianId,
                    "Imaging Job Deleted",
                    job.Status.ToString(),
                    null,
                    $"Imaging job {id} was deleted via API");

                await _imagingJobService.DeleteImagingJobAsync(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting imaging job {JobId}", id);
                return StatusCode(500, "An error occurred while deleting the imaging job");
            }
        }

        // Additional endpoint to get jobs by asset
        [HttpGet("by-asset/{assetId}")]
        public async Task<ActionResult<IEnumerable<ImagingJob>>> GetJobsByAsset(int assetId)
        {
            try
            {
                var asset = await _assetService.GetAssetByIdAsync(assetId);
                if (asset == null)
                {
                    return NotFound($"Asset with ID {assetId} not found");
                }

                var jobs = await _imagingJobService.GetImagingJobsByAssetAsync(assetId);
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving imaging jobs for asset {AssetId}", assetId);
                return StatusCode(500, "An error occurred while retrieving imaging jobs for the asset");
            }
        }

        // Additional endpoint to get jobs by technician
        [HttpGet("by-technician/{technicianId}")]
        public async Task<ActionResult<IEnumerable<ImagingJob>>> GetJobsByTechnician(int technicianId)
        {
            try
            {
                var technician = await _employeeService.GetEmployeeByIdAsync(technicianId);
                if (technician == null)
                {
                    return NotFound($"Technician with ID {technicianId} not found");
                }

                var jobs = await _imagingJobService.GetImagingJobsByTechnicianAsync(technicianId);
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving imaging jobs for technician {TechnicianId}", technicianId);
                return StatusCode(500, "An error occurred while retrieving imaging jobs for the technician");
            }
        }
    }

}