using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using buildone.Services;
using buildone.Data;
using buildone.Data.Enums;

namespace buildone.Pages.Reports
{
    public class AssetsModel : PageModel
    {
        private readonly IAssetService _assetService;
        private readonly IDepartmentService _departmentService;
        private readonly IEmployeeService _employeeService;

        public AssetsModel(IAssetService assetService, IDepartmentService departmentService, IEmployeeService employeeService)
        {
            _assetService = assetService;
            _departmentService = departmentService;
            _employeeService = employeeService;
        }

        public IList<Asset> Assets { get; set; } = new List<Asset>();
        public IList<Department> Departments { get; set; } = new List<Department>();

        // Filter properties
        [BindProperty(SupportsGet = true)]
        public string? ReportType { get; set; } = "overview";

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? TypeFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? DepartmentFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        // Analytics data
        public Dictionary<string, int> AssetsByType { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> AssetsByStatus { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> AssetsByDepartment { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> AssetsByMonth { get; set; } = new Dictionary<string, int>();

        // Asset health metrics
        public List<Asset> AssetsNearingWarrantyExpiry { get; set; } = new List<Asset>();
        public List<Asset> UnassignedAssets { get; set; } = new List<Asset>();
        public List<Asset> HighValueAssets { get; set; } = new List<Asset>();
        public List<Asset> OldestAssets { get; set; } = new List<Asset>();

        // Summary statistics
        public int TotalAssets { get; set; }
        public int ActiveAssets { get; set; }
        public int AssignedAssets { get; set; }
        public double UtilizationRate { get; set; }
        public double AverageAssetAge { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Set default date range
                EndDate ??= DateTime.Now;
                StartDate ??= EndDate.Value.AddYears(-1);

                // Load data
                var allAssets = await _assetService.GetAllAssetsAsync();
                Departments = (await _departmentService.GetAllDepartmentsAsync()).ToList();

                // Apply filters
                var filteredAssets = allAssets.AsQueryable();

                if (!string.IsNullOrEmpty(StatusFilter))
                {
                    if (Enum.TryParse<AssetStatus>(StatusFilter, out var status))
                    {
                        filteredAssets = filteredAssets.Where(a => a.Status == status);
                    }
                }

                if (!string.IsNullOrEmpty(TypeFilter))
                {
                    filteredAssets = filteredAssets.Where(a => a.Type == TypeFilter);
                }

                if (!string.IsNullOrEmpty(DepartmentFilter) && int.TryParse(DepartmentFilter, out var deptId))
                {
                    filteredAssets = filteredAssets.Where(a => a.DepartmentId == deptId);
                }

                // Date range filter
                filteredAssets = filteredAssets.Where(a => 
                    a.CreatedAt >= StartDate && a.CreatedAt <= EndDate);

                Assets = filteredAssets.OrderBy(a => a.AssetTag).ToList();

                // Calculate analytics
                CalculateAnalytics(allAssets);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading asset reports: {ex.Message}");
                Assets = new List<Asset>();
                Departments = new List<Department>();
            }
        }

        private void CalculateAnalytics(IEnumerable<Asset> allAssets)
        {
            var assetList = allAssets.ToList();
            
            // Basic statistics
            TotalAssets = assetList.Count;
            ActiveAssets = assetList.Count(a => a.Status == AssetStatus.Active);
            AssignedAssets = assetList.Count(a => a.AssignedEmployeeId.HasValue);
            UtilizationRate = TotalAssets > 0 ? (double)AssignedAssets / TotalAssets * 100 : 0;

            // Asset age calculation
            var assetsWithAge = assetList.Where(a => a.CreatedAt != default).ToList();
            if (assetsWithAge.Any())
            {
                AverageAssetAge = assetsWithAge.Average(a => (DateTime.Now - a.CreatedAt).TotalDays / 365.25);
            }

            // Distribution analysis
            AssetsByType = assetList
                .GroupBy(a => a.Type ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Count());

            AssetsByStatus = assetList
                .GroupBy(a => a.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            AssetsByDepartment = assetList
                .Where(a => a.Department != null)
                .GroupBy(a => a.Department!.Name)
                .ToDictionary(g => g.Key, g => g.Count());

            // Monthly creation trends
            AssetsByMonth = assetList
                .Where(a => a.CreatedAt >= DateTime.Now.AddMonths(-12))
                .GroupBy(a => a.CreatedAt.ToString("yyyy-MM"))
                .ToDictionary(g => g.Key, g => g.Count());

            // Asset health analysis
            var now = DateTime.Now;
            
            AssetsNearingWarrantyExpiry = assetList
                .Where(a => a.WarrantyExpiry.HasValue && 
                           a.WarrantyExpiry.Value <= now.AddMonths(3) &&
                           a.WarrantyExpiry.Value >= now)
                .OrderBy(a => a.WarrantyExpiry)
                .Take(10)
                .ToList();

            UnassignedAssets = assetList
                .Where(a => !a.AssignedEmployeeId.HasValue && a.Status == AssetStatus.Active)
                .OrderBy(a => a.CreatedAt)
                .Take(10)
                .ToList();

            OldestAssets = assetList
                .Where(a => a.CreatedAt != default)
                .OrderBy(a => a.CreatedAt)
                .Take(10)
                .ToList();
        }
    }
}