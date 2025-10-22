using buildone.Data;
using buildone.Services;
using buildone.Models;
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

        public PaginatedList<Department> Departments { get; set; } = null!;

        // Pagination properties
        [BindProperty(SupportsGet = true)]
        public int PageIndex { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var allDepartments = (await _departmentService.GetAllDepartmentsWithEmployeesAsync()).AsQueryable();
                
                Departments = PaginatedList<Department>.Create(
                    allDepartments.OrderBy(d => d.Name),
                    PageIndex,
                    PageSize
                );
                
                return Page();
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Error loading departments. Please try again.";
                Departments = PaginatedList<Department>.Create(new List<Department>(), 1, PageSize);
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