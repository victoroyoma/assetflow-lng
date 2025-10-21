using buildone.Data;
using buildone.Data.Enums;
using buildone.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace buildone.Pages.Imaging
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IImagingJobService _imagingJobService;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(IImagingJobService imagingJobService, ILogger<DetailsModel> logger)
        {
            _imagingJobService = imagingJobService;
            _logger = logger;
        }

        public ImagingJob Job { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var job = await _imagingJobService.GetImagingJobByIdAsync(id);

            if (job == null)
            {
                TempData["ErrorMessage"] = "Imaging job not found.";
                return RedirectToPage("./Queue");
            }

            Job = job;
            return Page();
        }

        public string GetStatusColor(JobStatus status)
        {
            return status switch
            {
                JobStatus.Pending => "warning",
                JobStatus.Scheduled => "info",
                JobStatus.InProgress => "primary",
                JobStatus.Completed => "success",
                JobStatus.Failed => "danger",
                JobStatus.Cancelled => "secondary",
                _ => "secondary"
            };
        }
    }
}
