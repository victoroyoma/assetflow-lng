using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using buildone.Services;
using buildone.Data;
using buildone.Data.Enums;

namespace buildone.Pages.Reports
{
    public class IndexModel : PageModel
    {
        private readonly IAssetService _assetService;
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly IImagingJobService _imagingJobService;

        public IndexModel(
            IAssetService assetService,
            IEmployeeService employeeService,
            IDepartmentService departmentService,
            IImagingJobService imagingJobService)
        {
            _assetService = assetService;
            _employeeService = employeeService;
            _departmentService = departmentService;
            _imagingJobService = imagingJobService;
        }

        // Dashboard KPIs
        public int TotalAssets { get; set; }
        public int ActiveAssets { get; set; }
        public int AssignedAssets { get; set; }
        public int UnassignedAssets { get; set; }
        public int MaintenanceAssets { get; set; }
        public int RetiredAssets { get; set; }

        public int TotalEmployees { get; set; }
        public int EmployeesWithAssets { get; set; }
        public int TotalDepartments { get; set; }

        public int TotalImagingJobs { get; set; }
        public int PendingJobs { get; set; }
        public int InProgressJobs { get; set; }
        public int CompletedJobs { get; set; }
        public int FailedJobs { get; set; }

        // Recent Activity
        public IList<Asset> RecentAssets { get; set; } = new List<Asset>();
        public IList<ImagingJob> RecentJobs { get; set; } = new List<ImagingJob>();

        // Asset Distribution
        public Dictionary<string, int> AssetsByType { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> AssetsByStatus { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> AssetsByDepartment { get; set; } = new Dictionary<string, int>();

        // Performance Metrics
        public double AverageJobCompletionTime { get; set; }
        public int JobsCompletedThisMonth { get; set; }
        public double AssetUtilizationRate { get; set; }

        // Date ranges for filtering
        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Set default date range if not provided
                EndDate ??= DateTime.Now;
                StartDate ??= EndDate.Value.AddMonths(-1);

                // Load all data
                var allAssets = await _assetService.GetAllAssetsAsync();
                var allEmployees = await _employeeService.GetAllEmployeesAsync();
                var allDepartments = await _departmentService.GetAllDepartmentsAsync();
                var allJobs = await _imagingJobService.GetAllImagingJobsAsync();

                // Calculate Asset KPIs
                TotalAssets = allAssets.Count();
                ActiveAssets = allAssets.Count(a => a.Status == AssetStatus.Active);
                AssignedAssets = allAssets.Count(a => a.AssignedEmployeeId.HasValue);
                UnassignedAssets = allAssets.Count(a => !a.AssignedEmployeeId.HasValue);
                MaintenanceAssets = allAssets.Count(a => a.Status == AssetStatus.Maintenance);
                RetiredAssets = allAssets.Count(a => a.Status == AssetStatus.Retired);

                // Calculate Employee KPIs
                TotalEmployees = allEmployees.Count();
                EmployeesWithAssets = allEmployees.Count(e => allAssets.Any(a => a.AssignedEmployeeId == e.Id));
                TotalDepartments = allDepartments.Count();

                // Calculate Imaging Job KPIs
                TotalImagingJobs = allJobs.Count();
                PendingJobs = allJobs.Count(j => j.Status == JobStatus.Pending);
                InProgressJobs = allJobs.Count(j => j.Status == JobStatus.InProgress);
                CompletedJobs = allJobs.Count(j => j.Status == JobStatus.Completed);
                FailedJobs = allJobs.Count(j => j.Status == JobStatus.Failed);

                // Recent Activity (last 10 items)
                RecentAssets = allAssets
                    .OrderByDescending(a => a.UpdatedAt ?? a.CreatedAt)
                    .Take(10)
                    .ToList();

                RecentJobs = allJobs
                    .OrderByDescending(j => j.UpdatedAt)
                    .Take(10)
                    .ToList();

                // Asset Distribution Analysis
                AssetsByType = allAssets
                    .GroupBy(a => a.Type ?? "Unknown")
                    .ToDictionary(g => g.Key, g => g.Count());

                AssetsByStatus = allAssets
                    .GroupBy(a => a.Status.ToString())
                    .ToDictionary(g => g.Key, g => g.Count());

                AssetsByDepartment = allAssets
                    .Where(a => a.Department != null)
                    .GroupBy(a => a.Department!.Name)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Performance Metrics
                var completedJobsWithTimes = allJobs
                    .Where(j => j.Status == JobStatus.Completed && j.StartedAt.HasValue && j.CompletedAt.HasValue)
                    .ToList();

                if (completedJobsWithTimes.Any())
                {
                    AverageJobCompletionTime = completedJobsWithTimes
                        .Average(j => (j.CompletedAt!.Value - j.StartedAt!.Value).TotalHours);
                }

                JobsCompletedThisMonth = allJobs
                    .Count(j => j.Status == JobStatus.Completed && 
                               j.CompletedAt.HasValue && 
                               j.CompletedAt.Value.Month == DateTime.Now.Month &&
                               j.CompletedAt.Value.Year == DateTime.Now.Year);

                AssetUtilizationRate = TotalAssets > 0 ? (double)AssignedAssets / TotalAssets * 100 : 0;
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error loading dashboard data: {ex.Message}");
                
                // Initialize with default values
                TotalAssets = 0;
                TotalEmployees = 0;
                TotalDepartments = 0;
                TotalImagingJobs = 0;
                AssetsByType = new Dictionary<string, int>();
                AssetsByStatus = new Dictionary<string, int>();
                AssetsByDepartment = new Dictionary<string, int>();
            }
        }
    }
}