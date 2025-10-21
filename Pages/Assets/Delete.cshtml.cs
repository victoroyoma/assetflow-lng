using buildone.Data;
using buildone.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace buildone.Pages.Assets
{
    public class DeleteModel : PageModel
    {
        private readonly IAssetService _assetService;

        public DeleteModel(IAssetService assetService)
        {
            _assetService = assetService;
        }

        [BindProperty]
        public Asset Asset { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var asset = await _assetService.GetAssetByIdAsync(id);
                if (asset == null)
                {
                    TempData["ErrorMessage"] = "Asset not found.";
                    return RedirectToPage("./Index");
                }

                Asset = asset;
                return Page();
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Error loading asset details. Please try again.";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync(string confirmAssetTag, bool confirmDeletion)
        {
            try
            {
                // Reload asset to ensure it still exists
                var asset = await _assetService.GetAssetByIdAsync(Asset.Id);
                if (asset == null)
                {
                    TempData["ErrorMessage"] = "Asset not found.";
                    return RedirectToPage("./Index");
                }

                Asset = asset;

                // Validate confirmation inputs
                if (string.IsNullOrWhiteSpace(confirmAssetTag) || confirmAssetTag.Trim() != Asset.AssetTag)
                {
                    ModelState.AddModelError("confirmAssetTag", "Asset tag confirmation does not match.");
                    return Page();
                }

                if (!confirmDeletion)
                {
                    ModelState.AddModelError("confirmDeletion", "You must confirm that you understand this action is permanent.");
                    return Page();
                }

                // Perform the deletion
                var success = await _assetService.DeleteAssetAsync(Asset.Id);
                if (success)
                {
                    TempData["SuccessMessage"] = $"Asset '{Asset.AssetTag}' has been permanently deleted.";
                    return RedirectToPage("./Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to delete the asset. It may be referenced by other records.");
                    return Page();
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the asset. Please try again.");
                
                // Reload asset data for the page
                try
                {
                    var asset = await _assetService.GetAssetByIdAsync(Asset.Id);
                    if (asset != null)
                    {
                        Asset = asset;
                    }
                }
                catch
                {
                    // If we can't reload the asset, redirect to index
                    TempData["ErrorMessage"] = "An error occurred and the asset data could not be loaded.";
                    return RedirectToPage("./Index");
                }
                
                return Page();
            }
        }
    }
}