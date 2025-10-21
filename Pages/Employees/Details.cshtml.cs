using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using buildone.Services;
using buildone.Data;

namespace buildone.Pages.Employees
{
    public class DetailsModel : PageModel
    {
        private readonly IEmployeeService _employeeService;

        public DetailsModel(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public Employee Employee { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(id.Value);
                if (employee == null)
                {
                    TempData["ErrorMessage"] = "Employee not found.";
                    return RedirectToPage("./Index");
                }

                Employee = employee;
                return Page();
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Error loading employee data. Please try again.";
                return RedirectToPage("./Index");
            }
        }
    }
}