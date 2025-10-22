using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using buildone.Data;
using buildone.Services;
using buildone.Models;

namespace buildone.Pages.Imaging
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IImagingJobService _imagingJobService;

        public IndexModel(IImagingJobService imagingJobService)
        {
            _imagingJobService = imagingJobService;
        }

        public PaginatedList<ImagingJob> Jobs { get; set; } = null!;
        public JobStatistics Statistics { get; set; } = new();

        // Pagination properties
        [BindProperty(SupportsGet = true)]
        public int PageIndex { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 25;

        // Filter properties
        [BindProperty(SupportsGet = true)]
        public string? JobStatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? PriorityFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public class JobStatistics
        {
            public int TotalJobs { get; set; }
            public int InProgress { get; set; }
            public int Completed { get; set; }
            public int Pending { get; set; }
            public int Failed { get; set; }
        }

        public async Task OnGetAsync()
        {
            try
            {
                var allJobs = (await _imagingJobService.GetAllImagingJobsAsync()).AsQueryable();
                
                // Apply filters
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    allJobs = allJobs.Where(j => 
                        (j.Asset != null && j.Asset.AssetTag != null && j.Asset.AssetTag.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                        (j.ImageVersion != null && j.ImageVersion.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                        (j.Technician != null && j.Technician.FullName != null && j.Technician.FullName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)));
                }

                if (!string.IsNullOrEmpty(JobStatusFilter) && Enum.TryParse<Data.Enums.JobStatus>(JobStatusFilter, out var status))
                {
                    allJobs = allJobs.Where(j => j.Status == status);
                }

                if (!string.IsNullOrEmpty(PriorityFilter) && Enum.TryParse<Data.Enums.JobPriority>(PriorityFilter, out var priority))
                {
                    allJobs = allJobs.Where(j => j.Priority == priority);
                }

                // Apply pagination
                Jobs = PaginatedList<ImagingJob>.Create(
                    allJobs.OrderByDescending(j => j.CreatedAt),
                    PageIndex,
                    PageSize
                );

                // Calculate statistics from all jobs (not filtered)
                var allJobsList = (await _imagingJobService.GetAllImagingJobsAsync()).ToList();
                Statistics = new JobStatistics
                {
                    TotalJobs = allJobsList.Count,
                    InProgress = allJobsList.Count(j => j.Status == Data.Enums.JobStatus.InProgress),
                    Completed = allJobsList.Count(j => j.Status == Data.Enums.JobStatus.Completed),
                    Pending = allJobsList.Count(j => j.Status == Data.Enums.JobStatus.Pending),
                    Failed = allJobsList.Count(j => j.Status == Data.Enums.JobStatus.Failed)
                };
            }
            catch (Exception)
            {
                Jobs = PaginatedList<ImagingJob>.Create(new List<ImagingJob>(), 1, PageSize);
                Statistics = new JobStatistics();
            }
        }
    }
}
