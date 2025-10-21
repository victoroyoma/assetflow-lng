using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using buildone.Data;
using buildone.Services;

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

        public IList<ImagingJob> Jobs { get; set; } = new List<ImagingJob>();
        public JobStatistics Statistics { get; set; } = new();

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
                var allJobs = (await _imagingJobService.GetAllImagingJobsAsync()).ToList();
                
                // Get the latest 50 jobs
                Jobs = allJobs.OrderByDescending(j => j.CreatedAt).Take(50).ToList();

                // Calculate statistics
                Statistics = new JobStatistics
                {
                    TotalJobs = allJobs.Count,
                    InProgress = allJobs.Count(j => j.Status == Data.Enums.JobStatus.InProgress),
                    Completed = allJobs.Count(j => j.Status == Data.Enums.JobStatus.Completed),
                    Pending = allJobs.Count(j => j.Status == Data.Enums.JobStatus.Pending),
                    Failed = allJobs.Count(j => j.Status == Data.Enums.JobStatus.Failed)
                };
            }
            catch (Exception)
            {
                // Log error
                Jobs = new List<ImagingJob>();
                Statistics = new JobStatistics();
            }
        }
    }
}
