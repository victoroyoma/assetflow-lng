using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using buildone.Data;
using buildone.Services;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace buildone.Pages.Employees
{
    public class EditModel : PageModel
    {
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;

        public EditModel(IEmployeeService employeeService, IDepartmentService departmentService)
        {
            _employeeService = employeeService;
            _departmentService = departmentService;
        }

        [BindProperty]
        public EmployeeEditModel EmployeeEdit { get; set; } = new();

        public SelectList Departments { get; set; } = new(new List<object>(), "", "");

        public class EmployeeEditModel
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "Full name is required")]
            [StringLength(200, ErrorMessage = "Full name must be less than 200 characters")]
            [Display(Name = "Full Name")]
            public string FullName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Username is required")]
            [StringLength(100, ErrorMessage = "Username must be less than 100 characters")]
            [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "Username can only contain letters, numbers, dots, hyphens, and underscores")]
            public string Username { get; set; } = string.Empty;

            [EmailAddress(ErrorMessage = "Please enter a valid email address")]
            [StringLength(250, ErrorMessage = "Email must be less than 250 characters")]
            public string? Email { get; set; }

            [Phone(ErrorMessage = "Please enter a valid phone number")]
            [StringLength(20, ErrorMessage = "Phone number must be less than 20 characters")]
            public string? Phone { get; set; }

            [Display(Name = "Department")]
            public int? DepartmentId { get; set; }
        }

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

            EmployeeEdit = new EmployeeEditModel
            {
                Id = employee.Id,
                FullName = employee.FullName,
                Username = employee.Username,
                Email = employee.Email,
                Phone = employee.Phone,
                DepartmentId = employee.DepartmentId
            };

            await LoadDepartmentsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDepartmentsAsync();
                return Page();
            }

            try
            {
                // Check if username is already taken by another employee
                var existingEmployee = await _employeeService.GetEmployeeByUsernameAsync(EmployeeEdit.Username);
                if (existingEmployee != null && existingEmployee.Id != EmployeeEdit.Id)
                {
                    ModelState.AddModelError("EmployeeEdit.Username", "This username is already taken.");
                    await LoadDepartmentsAsync();
                    return Page();
                }

                // Check if email is already taken by another employee (if provided)
                if (!string.IsNullOrWhiteSpace(EmployeeEdit.Email))
                {
                    existingEmployee = await _employeeService.GetEmployeeByEmailAsync(EmployeeEdit.Email);
                    if (existingEmployee != null && existingEmployee.Id != EmployeeEdit.Id)
                    {
                        ModelState.AddModelError("EmployeeEdit.Email", "This email address is already in use.");
                        await LoadDepartmentsAsync();
                        return Page();
                    }
                }

                var employee = await _employeeService.GetEmployeeByIdAsync(EmployeeEdit.Id);
                if (employee == null)
                {
                    TempData["ErrorMessage"] = "Employee not found.";
                    return RedirectToPage("./Index");
                }

                // Update employee properties
                employee.FullName = EmployeeEdit.FullName;
                employee.Username = EmployeeEdit.Username;
                employee.Email = string.IsNullOrWhiteSpace(EmployeeEdit.Email) ? null : EmployeeEdit.Email;
                employee.Phone = string.IsNullOrWhiteSpace(EmployeeEdit.Phone) ? null : EmployeeEdit.Phone;
                employee.DepartmentId = EmployeeEdit.DepartmentId;

                await _employeeService.UpdateEmployeeAsync(employee);

                TempData["SuccessMessage"] = $"Employee '{employee.FullName}' has been updated successfully.";
                return RedirectToPage("./Details", new { id = employee.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred while updating the employee: {ex.Message}");
                await LoadDepartmentsAsync();
                return Page();
            }
        }

        private async Task LoadDepartmentsAsync()
        {
            try
            {
                var departments = await _departmentService.GetAllDepartmentsAsync();
                Departments = new SelectList(departments, "Id", "Name", EmployeeEdit.DepartmentId);
            }
            catch (Exception)
            {
                Departments = new SelectList(new List<object>(), "", "");
                ModelState.AddModelError(string.Empty, "Unable to load departments. Please try again.");
            }
        }

        public async Task<JsonResult> OnGetCheckUsernameAsync(string username, int id)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return new JsonResult(new { isAvailable = false, message = "Username is required" });
            }

            try
            {
                var existingEmployee = await _employeeService.GetEmployeeByUsernameAsync(username);
                bool isAvailable = existingEmployee == null || existingEmployee.Id == id;
                
                return new JsonResult(new { 
                    isAvailable = isAvailable, 
                    message = isAvailable ? "Username is available" : "Username is already taken" 
                });
            }
            catch (Exception)
            {
                return new JsonResult(new { isAvailable = false, message = "Unable to check username availability" });
            }
        }

        public async Task<JsonResult> OnGetCheckEmailAsync(string email, int id)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return new JsonResult(new { isAvailable = true, message = "Email is optional" });
            }

            try
            {
                var existingEmployee = await _employeeService.GetEmployeeByEmailAsync(email);
                bool isAvailable = existingEmployee == null || existingEmployee.Id == id;
                
                return new JsonResult(new { 
                    isAvailable = isAvailable, 
                    message = isAvailable ? "Email is available" : "Email is already in use" 
                });
            }
            catch (Exception)
            {
                return new JsonResult(new { isAvailable = false, message = "Unable to check email availability" });
            }
        }
    }
}