using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using buildone.Data;
using buildone.Services;

namespace buildone.Pages.Employees
{
    public class DeleteModel : PageModel
    {
        private readonly IEmployeeService _employeeService;

        public DeleteModel(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [BindProperty]
        public Employee Employee { get; set; } = new();

        public bool HasAssets { get; set; }
        public int AssetCount { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Employee ID is required.";
                return RedirectToPage("./Index");
            }

            var employee = await _employeeService.GetEmployeeByIdAsync(id.Value);
            if (employee == null)
            {
                TempData["ErrorMessage"] = "Employee not found.";
                return RedirectToPage("./Index");
            }

            Employee = employee;
            HasAssets = employee.Assets.Any();
            AssetCount = employee.Assets.Count;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Employee ID is required.";
                return RedirectToPage("./Index");
            }

            var employee = await _employeeService.GetEmployeeByIdAsync(id.Value);
            if (employee == null)
            {
                TempData["ErrorMessage"] = "Employee not found.";
                return RedirectToPage("./Index");
            }

            try
            {
                // Check if employee has assigned assets
                if (employee.Assets.Any())
                {
                    TempData["ErrorMessage"] = $"Cannot delete employee '{employee.FullName}' because they have {employee.Assets.Count} assigned asset(s). Please reassign or remove the assets first.";
                    return RedirectToPage("./Details", new { id = employee.Id });
                }

                await _employeeService.DeleteEmployeeAsync(id.Value);
                TempData["SuccessMessage"] = $"Employee '{employee.FullName}' has been deleted successfully.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the employee: {ex.Message}";
                return RedirectToPage("./Details", new { id = employee.Id });
            }
        }
    }
}