using buildone.Data;
using buildone.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace buildone.Pages.Departments
{
    public class IndexModel : PageModel
    {
        private readonly IDepartmentService _departmentService;

        public IndexModel(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        public IEnumerable<Department> Departments { get; set; } = new List<Department>();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                Departments = await _departmentService.GetAllDepartmentsWithEmployeesAsync();
                return Page();
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Error loading departments. Please try again.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var department = await _departmentService.GetDepartmentByIdAsync(id);
                if (department == null)
                {
                    TempData["ErrorMessage"] = "Department not found.";
                    return RedirectToPage();
                }

                var deleted = await _departmentService.DeleteDepartmentAsync(id);
                if (deleted)
                {
                    TempData["SuccessMessage"] = $"Department '{department.Name}' has been deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete department. Please try again.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting department: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}