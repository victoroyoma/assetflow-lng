using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using buildone.Data;
using buildone.Data.Enums;
using buildone.Services;

namespace buildone.Pages.Imaging
{
    public class QueueModel : PageModel
    {
        private readonly IImagingJobService _imagingJobService;
        private readonly IEmployeeService _employeeService;
        private readonly IAssetService _assetService;
        private readonly IAssetHistoryService _assetHistoryService;
        private readonly IExportService _exportService;

        public QueueModel(
            IImagingJobService imagingJobService, 
            IEmployeeService employeeService,
            IAssetService assetService,
            IAssetHistoryService assetHistoryService,
            IExportService exportService)
        {
            _imagingJobService = imagingJobService;
            _employeeService = employeeService;
            _assetService = assetService;
            _assetHistoryService = assetHistoryService;
            _exportService = exportService;
        }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? TechnicianFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? PriorityFilter { get; set; }

        public IEnumerable<ImagingJob> Jobs { get; set; } = new List<ImagingJob>();
        public SelectList Technicians { get; set; } = new(new List<object>(), "", "");
        public QueueStatistics Statistics { get; set; } = new();

        public class QueueStatistics
        {
            public int TotalJobs { get; set; }
            public int PendingJobs { get; set; }
            public int InProgressJobs { get; set; }
            public int CompletedToday { get; set; }
            public int OverdueJobs { get; set; }
            public int FailedJobs { get; set; }
            public double AverageCompletionTime { get; set; }
        }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        public async Task<IActionResult> OnPostStartJobAsync(int jobId, int technicianId)
        {
            try
            {
                // Validate job and technician exist
                var job = await _imagingJobService.GetImagingJobByIdAsync(jobId);
                if (job == null)
                {
                    TempData["ErrorMessage"] = "Imaging job not found.";
                    return RedirectToPage();
                }

                var technician = await _employeeService.GetEmployeeByIdAsync(technicianId);
                if (technician == null)
                {
                    TempData["ErrorMessage"] = "Selected technician not found.";
                    return RedirectToPage();
                }

                // Validate job can be started
                if (job.Status != JobStatus.Pending)
                {
                    TempData["ErrorMessage"] = "Only pending jobs can be started.";
                    return RedirectToPage();
                }

                // Start the job with proper database transaction
                var success = await _imagingJobService.StartImagingJobAsync(jobId, technicianId);
                if (success)
                {
                    // Update related asset status if needed
                    if (job.Asset != null && job.Asset.Status != AssetStatus.Maintenance)
                    {
                        job.Asset.Status = AssetStatus.Maintenance;
                        await _assetService.UpdateAssetAsync(job.Asset);

                        // Log status change
                        await _assetHistoryService.LogAssetStatusChangeAsync(
                            job.AssetId,
                            technicianId,
                            "Active", // or whatever the previous status was
                            "Maintenance",
                            $"Status changed when imaging job {jobId} started"
                        );
                    }

                    TempData["SuccessMessage"] = $"Job started successfully by {technician.FullName}.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to start the job.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error starting job: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCompleteJobAsync(int jobId, string? notes)
        {
            try
            {
                // Validate job exists and can be completed
                var job = await _imagingJobService.GetImagingJobByIdAsync(jobId);
                if (job == null)
                {
                    TempData["ErrorMessage"] = "Imaging job not found.";
                    return RedirectToPage();
                }

                if (job.Status != JobStatus.InProgress)
                {
                    TempData["ErrorMessage"] = "Only in-progress jobs can be completed.";
                    return RedirectToPage();
                }

                // Complete the job with proper database transaction
                var success = await _imagingJobService.CompleteImagingJobAsync(jobId, notes);
                if (success)
                {
                    // Update related asset status back to Active or assigned status
                    if (job.Asset != null)
                    {
                        var newStatus = job.Asset.AssignedEmployeeId.HasValue ? AssetStatus.Active : AssetStatus.InStock;
                        var oldStatus = job.Asset.Status.ToString();
                        
                        job.Asset.Status = newStatus;
                        await _assetService.UpdateAssetAsync(job.Asset);

                        // Log status change
                        await _assetHistoryService.LogAssetStatusChangeAsync(
                            job.AssetId,
                            job.TechnicianId,
                            oldStatus,
                            newStatus.ToString(),
                            $"Status updated when imaging job {jobId} completed"
                        );

                        // Log imaging completion
                        await _assetHistoryService.LogActionAsync(
                            job.AssetId,
                            job.TechnicianId,
                            "Imaging Completed",
                            "In Progress",
                            "Completed",
                            notes ?? "Imaging job completed successfully"
                        );
                    }

                    TempData["SuccessMessage"] = "Job completed successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to complete the job.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error completing job: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostFailJobAsync(int jobId, string? notes)
        {
            try
            {
                var success = await _imagingJobService.FailImagingJobAsync(jobId, notes);
                if (success)
                {
                    TempData["SuccessMessage"] = "Job marked as failed.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update job status.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating job: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAssignTechnicianAsync(int jobId, int technicianId)
        {
            try
            {
                var success = await _imagingJobService.AssignTechnicianAsync(jobId, technicianId);
                if (success)
                {
                    TempData["SuccessMessage"] = "Technician assigned successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to assign technician.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error assigning technician: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetExportCsvAsync()
        {
            try
            {
                var jobs = await _imagingJobService.GetAllImagingJobsAsync();
                var csvData = await _exportService.ExportJobsToCsvAsync(jobs);
                var fileName = $"job_queue_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                
                return File(csvData, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error exporting data: {ex.Message}";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnGetGenerateReportAsync()
        {
            try
            {
                var jobs = await _imagingJobService.GetAllImagingJobsAsync();
                var report = _exportService.GenerateJobQueueReport(jobs);
                var reportData = System.Text.Encoding.UTF8.GetBytes(report);
                var fileName = $"job_queue_report_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                
                return File(reportData, "text/plain", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error generating report: {ex.Message}";
                return RedirectToPage();
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // Load jobs based on filters
                IEnumerable<ImagingJob> allJobs;

                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    allJobs = await _imagingJobService.SearchImagingJobsAsync(SearchTerm);
                }
                else if (!string.IsNullOrEmpty(StatusFilter) && Enum.TryParse<JobStatus>(StatusFilter, out var status))
                {
                    allJobs = await _imagingJobService.GetImagingJobsByStatusAsync(status);
                }
                else
                {
                    allJobs = await _imagingJobService.GetAllImagingJobsAsync();
                }

                // Apply technician filter if specified
                if (TechnicianFilter.HasValue)
                {
                    allJobs = allJobs.Where(j => j.TechnicianId == TechnicianFilter.Value);
                }

                // Apply priority filter if specified
                if (!string.IsNullOrEmpty(PriorityFilter) && Enum.TryParse<JobPriority>(PriorityFilter, out var priority))
                {
                    allJobs = allJobs.Where(j => j.Priority == priority);
                }

                Jobs = allJobs.OrderByDescending(j => (int)j.Priority).ThenBy(j => GetJobPriority(j)).ThenBy(j => j.ScheduledAt ?? j.CreatedAt);

                // Load technicians for filter dropdown
                var technicians = await _employeeService.GetAllEmployeesAsync();
                Technicians = new SelectList(technicians, "Id", "FullName", TechnicianFilter);

                // Calculate statistics
                await LoadStatisticsAsync();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading job queue: {ex.Message}";
                Jobs = new List<ImagingJob>();
            }
        }

        private async Task LoadStatisticsAsync()
        {
            try
            {
                var allJobs = await _imagingJobService.GetAllImagingJobsAsync();
                var today = DateTime.Today;

                Statistics = new QueueStatistics
                {
                    TotalJobs = allJobs.Count(),
                    PendingJobs = allJobs.Count(j => j.Status == JobStatus.Pending),
                    InProgressJobs = allJobs.Count(j => j.Status == JobStatus.InProgress),
                    CompletedToday = allJobs.Count(j => j.Status == JobStatus.Completed && 
                                                      j.CompletedAt?.Date == today),
                    OverdueJobs = allJobs.Count(j => j.IsOverdue),
                    FailedJobs = allJobs.Count(j => j.Status == JobStatus.Failed),
                    AverageCompletionTime = allJobs
                        .Where(j => j.Duration.HasValue)
                        .Select(j => j.Duration!.Value.TotalHours)
                        .DefaultIfEmpty(0)
                        .Average()
                };
            }
            catch (Exception)
            {
                Statistics = new QueueStatistics();
            }
        }

        private static int GetJobPriority(ImagingJob job)
        {
            // Priority order: Overdue (1), In Progress (2), Pending (3), Others (4)
            return job.IsOverdue ? 1 :
                   job.Status == JobStatus.InProgress ? 2 :
                   job.Status == JobStatus.Pending ? 3 : 4;
        }

        public string GetStatusColor(JobStatus status)
        {
            return status switch
            {
                JobStatus.Pending => "warning",
                JobStatus.InProgress => "primary",
                JobStatus.Completed => "success",
                JobStatus.Failed => "danger",
                JobStatus.Cancelled => "secondary",
                JobStatus.Scheduled => "info",
                _ => "secondary"
            };
        }

        public string GetPriorityClass(ImagingJob job)
        {
            if (job.IsOverdue) return "table-danger";
            if (job.Status == JobStatus.InProgress) return "table-primary";
            return "";
        }

        public string GetPriorityColor(JobPriority priority)
        {
            return priority switch
            {
                JobPriority.Urgent => "danger",
                JobPriority.High => "warning",
                JobPriority.Normal => "info",
                JobPriority.Low => "secondary",
                _ => "secondary"
            };
        }

        public string GetPriorityIcon(JobPriority priority)
        {
            return priority switch
            {
                JobPriority.Urgent => "fa-exclamation-circle",
                JobPriority.High => "fa-arrow-up",
                JobPriority.Normal => "fa-minus",
                JobPriority.Low => "fa-arrow-down",
                _ => "fa-minus"
            };
        }
    }
}