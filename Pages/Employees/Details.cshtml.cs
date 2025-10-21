using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using buildone.Services;
using buildone.Data;

namespace buildone.Pages.Employees
{
    public class DetailsModel : PageModel
    {
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(IEmployeeService employeeService, ILogger<DetailsModel> logger)
        {
            _employeeService = employeeService;
            _logger = logger;
        }

        public Employee? Employee { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Employee details accessed without ID");
                TempData["ErrorMessage"] = "Employee ID is required.";
                return RedirectToPage("./Index");
            }

            try
            {
                _logger.LogInformation("Loading employee details for ID: {EmployeeId}", id.Value);
                var employee = await _employeeService.GetEmployeeByIdAsync(id.Value);
                
                if (employee == null)
                {
                    _logger.LogWarning("Employee not found with ID: {EmployeeId}", id.Value);
                    TempData["ErrorMessage"] = $"Employee with ID {id.Value} not found.";
                    return RedirectToPage("./Index");
                }

                Employee = employee;
                _logger.LogInformation("Successfully loaded employee: {EmployeeName} (ID: {EmployeeId})", 
                    employee.FullName, employee.Id);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading employee data for ID: {EmployeeId}", id.Value);
                TempData["ErrorMessage"] = "Error loading employee data. Please try again.";
                return RedirectToPage("./Index");
            }
        }
    }
}