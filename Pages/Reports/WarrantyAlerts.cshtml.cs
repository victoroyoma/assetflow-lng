using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using buildone.Services;
using buildone.Data;
using buildone.Data.Enums;

namespace buildone.Pages.Reports
{
    public class WarrantyAlertsModel : PageModel
    {
        private readonly IAssetService _assetService;
        private readonly IDepartmentService _departmentService;

        public WarrantyAlertsModel(IAssetService assetService, IDepartmentService departmentService)
        {
            _assetService = assetService;
            _departmentService = departmentService;
        }

        // Filter properties
        [BindProperty(SupportsGet = true)]
        public string? DaysFilter { get; set; } = "90"; // Default to 90 days

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? DepartmentFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? TypeFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? UrgencyFilter { get; set; }

        // Data properties
        public List<Asset> CriticalAssets { get; set; } = new List<Asset>(); // 0-30 days
        public List<Asset> WarningAssets { get; set; } = new List<Asset>(); // 31-60 days
        public List<Asset> WatchAssets { get; set; } = new List<Asset>(); // 61-90 days
        public List<Asset> ExpiredAssets { get; set; } = new List<Asset>(); // Already expired
        public List<Asset> NoWarrantyAssets { get; set; } = new List<Asset>(); // No warranty data

        public List<Department> Departments { get; set; } = new List<Department>();

        // Statistics
        public int TotalAlertsCount { get; set; }
        public int CriticalCount { get; set; }
        public int WarningCount { get; set; }
        public int WatchCount { get; set; }
        public int ExpiredCount { get; set; }
        public double AverageWarrantyDaysRemaining { get; set; }
        public string MostAffectedDepartment { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            try
            {
                // Load data
                var allAssets = await _assetService.GetAllAssetsAsync();
                Departments = (await _departmentService.GetAllDepartmentsAsync()).ToList();

                // Parse days filter
                int daysAhead = int.TryParse(DaysFilter, out var days) ? days : 90;

                // Apply filters
                var filteredAssets = allAssets.AsQueryable();

                if (!string.IsNullOrEmpty(StatusFilter) && Enum.TryParse<AssetStatus>(StatusFilter, out var status))
                {
                    filteredAssets = filteredAssets.Where(a => a.Status == status);
                }

                if (!string.IsNullOrEmpty(DepartmentFilter) && int.TryParse(DepartmentFilter, out var deptId))
                {
                    filteredAssets = filteredAssets.Where(a => a.DepartmentId == deptId);
                }

                if (!string.IsNullOrEmpty(TypeFilter))
                {
                    filteredAssets = filteredAssets.Where(a => a.Type == TypeFilter);
                }

                var assetList = filteredAssets.ToList();
                var now = DateTime.Now;
                var futureDate = now.AddDays(daysAhead);

                // Categorize assets by warranty status
                var warrantyAssets = assetList.Where(a => a.WarrantyExpiry.HasValue).ToList();

                ExpiredAssets = warrantyAssets
                    .Where(a => a.WarrantyExpiry!.Value < now)
                    .OrderBy(a => a.WarrantyExpiry)
                    .ToList();

                CriticalAssets = warrantyAssets
                    .Where(a => a.WarrantyExpiry!.Value >= now && a.WarrantyExpiry.Value <= now.AddDays(30))
                    .OrderBy(a => a.WarrantyExpiry)
                    .ToList();

                WarningAssets = warrantyAssets
                    .Where(a => a.WarrantyExpiry!.Value > now.AddDays(30) && a.WarrantyExpiry.Value <= now.AddDays(60))
                    .OrderBy(a => a.WarrantyExpiry)
                    .ToList();

                WatchAssets = warrantyAssets
                    .Where(a => a.WarrantyExpiry!.Value > now.AddDays(60) && a.WarrantyExpiry.Value <= futureDate)
                    .OrderBy(a => a.WarrantyExpiry)
                    .ToList();

                NoWarrantyAssets = assetList
                    .Where(a => !a.WarrantyExpiry.HasValue)
                    .OrderBy(a => a.CreatedAt)
                    .ToList();

                // Apply urgency filter if specified
                if (!string.IsNullOrEmpty(UrgencyFilter))
                {
                    switch (UrgencyFilter.ToLower())
                    {
                        case "expired":
                            CriticalAssets.Clear();
                            WarningAssets.Clear();
                            WatchAssets.Clear();
                            NoWarrantyAssets.Clear();
                            break;
                        case "critical":
                            ExpiredAssets.Clear();
                            WarningAssets.Clear();
                            WatchAssets.Clear();
                            NoWarrantyAssets.Clear();
                            break;
                        case "warning":
                            ExpiredAssets.Clear();
                            CriticalAssets.Clear();
                            WatchAssets.Clear();
                            NoWarrantyAssets.Clear();
                            break;
                        case "watch":
                            ExpiredAssets.Clear();
                            CriticalAssets.Clear();
                            WarningAssets.Clear();
                            NoWarrantyAssets.Clear();
                            break;
                        case "no-warranty":
                            ExpiredAssets.Clear();
                            CriticalAssets.Clear();
                            WarningAssets.Clear();
                            WatchAssets.Clear();
                            break;
                    }
                }

                // Calculate statistics
                ExpiredCount = ExpiredAssets.Count;
                CriticalCount = CriticalAssets.Count;
                WarningCount = WarningAssets.Count;
                WatchCount = WatchAssets.Count;
                TotalAlertsCount = ExpiredCount + CriticalCount + WarningCount + WatchCount;

                if (warrantyAssets.Any())
                {
                    AverageWarrantyDaysRemaining = warrantyAssets
                        .Where(a => a.WarrantyExpiry!.Value >= now)
                        .Average(a => (a.WarrantyExpiry!.Value - now).TotalDays);
                }

                // Find most affected department
                var departmentWarrantyIssues = warrantyAssets
                    .Where(a => a.WarrantyExpiry!.Value <= futureDate)
                    .GroupBy(a => a.Department?.Name ?? "Unassigned")
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();

                MostAffectedDepartment = departmentWarrantyIssues?.Key ?? "None";
            }
            catch (Exception ex)
            {
                // Log error and set defaults
                Console.WriteLine($"Error loading warranty alerts: {ex.Message}");
                
                TotalAlertsCount = 0;
                CriticalCount = 0;
                WarningCount = 0;
                WatchCount = 0;
                ExpiredCount = 0;
                AverageWarrantyDaysRemaining = 0;
                MostAffectedDepartment = "Unknown";
            }
        }
    }
}