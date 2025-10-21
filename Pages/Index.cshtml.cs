using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using buildone.Services;
using buildone.Data.Enums;

namespace buildone.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IAssetService _assetService;
    private readonly IImagingJobService _imagingJobService;

    public IndexModel(ILogger<IndexModel> logger, IAssetService assetService, IImagingJobService imagingJobService)
    {
        _logger = logger;
        _assetService = assetService;
        _imagingJobService = imagingJobService;
    }

    public int TotalAssets { get; set; }
    public int ActiveAssets { get; set; }
    public int MaintenanceAssets { get; set; }
    public int UnassignedAssets { get; set; }
    public int PendingJobs { get; set; }
    public int CompletedJobsToday { get; set; }

    public async Task OnGetAsync()
    {
        _logger.LogInformation("Asset Management Dashboard accessed");
        
        var allAssets = await _assetService.GetAllAssetsAsync();
        TotalAssets = allAssets.Count();
        ActiveAssets = allAssets.Count(a => a.Status == AssetStatus.Active);
        MaintenanceAssets = allAssets.Count(a => a.Status == AssetStatus.Maintenance);
        UnassignedAssets = allAssets.Count(a => a.Status == AssetStatus.InStock);
        
        var allJobs = await _imagingJobService.GetAllImagingJobsAsync();
        PendingJobs = allJobs.Count(j => j.Status == JobStatus.Pending || j.Status == JobStatus.Scheduled);
        CompletedJobsToday = allJobs.Count(j => j.Status == JobStatus.Completed && j.CompletedAt?.Date == DateTime.Today);
    }
}