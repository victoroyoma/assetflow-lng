using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using buildone.Services;
using buildone.Data;
using buildone.Data.Enums;

namespace buildone.Pages.Assets
{
    [Authorize]
    public class AssetsListModel : PageModel
    {
        private readonly IAssetService _assetService;
        private readonly IDepartmentService _departmentService;
        private readonly ILogger<AssetsListModel> _logger;

        public AssetsListModel(
            IAssetService assetService,
            IDepartmentService departmentService,
            ILogger<AssetsListModel> logger)
        {
            _assetService = assetService;
            _departmentService = departmentService;
            _logger = logger;
        }

        public IEnumerable<Asset> Assets { get; set; } = new List<Asset>();
        public IEnumerable<SelectListItem> AvailableStatuses { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> AvailableDepartments { get; set; } = new List<SelectListItem>();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public AssetStatus? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? DepartmentFilter { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Load all assets
                var allAssets = await _assetService.GetAllAssetsAsync();

                // Apply filters
                var filteredAssets = allAssets.AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    var search = SearchTerm.ToLower();
                    filteredAssets = filteredAssets.Where(a =>
                        (a.PcId != null && a.PcId.ToLower().Contains(search)) ||
                        (a.SerialNumber != null && a.SerialNumber.ToLower().Contains(search)) ||
                        (a.AssetTag != null && a.AssetTag.ToLower().Contains(search)));
                }

                if (StatusFilter.HasValue)
                {
                    filteredAssets = filteredAssets.Where(a => a.Status == StatusFilter.Value);
                }

                if (DepartmentFilter.HasValue)
                {
                    filteredAssets = filteredAssets.Where(a => a.DepartmentId == DepartmentFilter.Value);
                }

                Assets = filteredAssets.OrderBy(a => a.AssetTag).ToList();

                // Load departments for dropdown
                var departments = await _departmentService.GetAllDepartmentsAsync();
                AvailableDepartments = departments.Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name
                });

                // Load statuses for dropdown
                AvailableStatuses = Enum.GetValues<AssetStatus>()
                    .Select(s => new SelectListItem
                    {
                        Value = s.ToString(),
                        Text = s.ToString()
                    });

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading assets list");
                TempData["ErrorMessage"] = "An error occurred while loading the assets list.";
                return Page();
            }
        }

        public string GetStatusBadgeClass(AssetStatus status)
        {
            return status switch
            {
                AssetStatus.InStock => "badge-secondary",
                AssetStatus.Assigned => "badge-success",
                AssetStatus.Deployed => "badge-primary",
                AssetStatus.InRepair => "badge-warning",
                AssetStatus.Retired => "badge-dark",
                AssetStatus.Lost => "badge-danger",
                _ => "badge-light"
            };
        }
    }
}