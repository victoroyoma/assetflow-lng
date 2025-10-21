using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using buildone.Services;
using buildone.Data;
using System.ComponentModel.DataAnnotations;

namespace buildone.Pages.Employees
{
    public class CreateModel : PageModel
    {
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;

        public CreateModel(IEmployeeService employeeService, IDepartmentService departmentService)
        {
            _employeeService = employeeService;
            _departmentService = departmentService;
        }

        [BindProperty]
        public EmployeeCreateModel Employee { get; set; } = default!;

        public List<Department> Departments { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                await LoadDropdownData();
                Employee = new EmployeeCreateModel();
                return Page();
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Error loading form data. Please try again.";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownData();
                return Page();
            }

            try
            {
                // Basic validation
                var usernameExists = await _employeeService.UsernameExistsAsync(Employee.Username);
                if (usernameExists)
                {
                    ModelState.AddModelError("Employee.Username", "A user with this username already exists.");
                    await LoadDropdownData();
                    return Page();
                }

                // Check email uniqueness if provided
                if (!string.IsNullOrWhiteSpace(Employee.Email))
                {
                    var emailExists = await _employeeService.EmailExistsAsync(Employee.Email);
                    if (emailExists)
                    {
                        ModelState.AddModelError("Employee.Email", "A user with this email already exists.");
                        await LoadDropdownData();
                        return Page();
                    }
                }

                // Create new employee entity
                var employee = new Employee
                {
                    FullName = Employee.FullName.Trim(),
                    Username = Employee.Username.Trim().ToLower(),
                    Email = !string.IsNullOrWhiteSpace(Employee.Email) ? Employee.Email.Trim().ToLower() : null,
                    Phone = !string.IsNullOrWhiteSpace(Employee.Phone) ? Employee.Phone.Trim() : null,
                    DepartmentId = Employee.DepartmentId == 0 ? null : Employee.DepartmentId
                };

                // Create employee
                var createdEmployee = await _employeeService.CreateEmployeeAsync(employee);

                TempData["SuccessMessage"] = $"Employee '{createdEmployee.FullName}' has been created successfully.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred while creating the employee: {ex.Message}");
                await LoadDropdownData();
                return Page();
            }
        }

        private async Task LoadDropdownData()
        {
            try
            {
                Departments = (await _departmentService.GetAllDepartmentsAsync()).ToList();
            }
            catch (Exception)
            {
                Departments = new List<Department>();
                ModelState.AddModelError(string.Empty, "Error loading departments.");
            }
        }
    }

    public class EmployeeCreateModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Display(Name = "Phone")]
        public string? Phone { get; set; }

        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }
    }
}