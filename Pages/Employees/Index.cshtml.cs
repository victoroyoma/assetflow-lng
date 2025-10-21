using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using buildone.Services;
using buildone.Data;
using buildone.Data.Enums;

namespace buildone.Pages.Employees
{
    [Authorize(Roles = "Administrator,Technician")]
    public class IndexModel : PageModel
    {
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;

        public IndexModel(IEmployeeService employeeService, IDepartmentService departmentService)
        {
            _employeeService = employeeService;
            _departmentService = departmentService;
        }

        public IList<Employee> Employees { get; set; } = default!;
        public IList<Department> Departments { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? DepartmentFilter { get; set; }

        // Statistics
        public int TotalEmployees { get; set; }
        public int AssignedEmployees { get; set; }
        public int UnassignedEmployees { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Load all employees and departments
                var allEmployees = await _employeeService.GetAllEmployeesAsync();
                Departments = (await _departmentService.GetAllDepartmentsAsync()).ToList();

                // Apply filters
                var filteredEmployees = allEmployees.AsQueryable();

                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    filteredEmployees = filteredEmployees.Where(e => 
                        e.FullName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (e.Username != null && e.Username.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                        (e.Email != null && e.Email.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)));
                }

                if (!string.IsNullOrEmpty(DepartmentFilter) && int.TryParse(DepartmentFilter, out var deptId))
                {
                    filteredEmployees = filteredEmployees.Where(e => e.DepartmentId == deptId);
                }

                Employees = filteredEmployees.OrderBy(e => e.FullName).ToList();

                // Calculate statistics from all employees (not filtered)
                TotalEmployees = allEmployees.Count();
                AssignedEmployees = allEmployees.Count(e => e.Assets.Any());
                UnassignedEmployees = TotalEmployees - AssignedEmployees;
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Error loading employees. Please try again.";
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(id);
                if (employee == null)
                {
                    TempData["ErrorMessage"] = "Employee not found.";
                    return RedirectToPage();
                }

                // Enhanced validation before deletion
                var validationResult = await ValidateEmployeeDeletionAsync(employee);
                if (!validationResult.CanDelete)
                {
                    TempData["ErrorMessage"] = validationResult.ErrorMessage;
                    return RedirectToPage();
                }

                // Handle cascade operations before deletion
                await HandleEmployeeDeletionCascade(employee);

                // Delete the employee
                var success = await _employeeService.DeleteEmployeeAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = $"Employee '{employee.FullName}' has been deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete employee. Please try again.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting employee: {ex.Message}";
            }

            return RedirectToPage();
        }

        private Task<EmployeeDeletionValidation> ValidateEmployeeDeletionAsync(Employee employee)
        {
            var result = new EmployeeDeletionValidation();

            // Check if employee has assets assigned
            if (employee.Assets.Any())
            {
                var assetCount = employee.Assets.Count();
                result.CanDelete = false;
                result.ErrorMessage = $"Cannot delete employee with {assetCount} assigned asset(s). Please reassign assets first.";
                return Task.FromResult(result);
            }

            // Check for active imaging jobs as technician
            // This would require additional service method to check technician assignments
            // For now, we'll assume this check is handled in the service layer

            result.CanDelete = true;
            return Task.FromResult(result);
        }

        private Task HandleEmployeeDeletionCascade(Employee employee)
        {
            try
            {
                // Unassign any remaining assets (as a safety measure)
                foreach (var asset in employee.Assets)
                {
                    asset.AssignedEmployeeId = null;
                    asset.Status = AssetStatus.InStock;
                    // This would typically be handled by the service layer
                }

                // Handle other cascade operations like updating department statistics
                if (employee.DepartmentId.HasValue)
                {
                    // Update department employee count or other related data
                    // This would be implemented based on business requirements
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the main deletion
                Console.WriteLine($"Error handling employee deletion cascade: {ex.Message}");
            }

            return Task.CompletedTask;
        }
    }

    public class EmployeeDeletionValidation
    {
        public bool CanDelete { get; set; } = true;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}