using buildone.Data;
using buildone.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace buildone.Pages.Departments
{
    public class CreateModel : PageModel
    {
        private readonly IDepartmentService _departmentService;

        public CreateModel(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [BindProperty]
        public DepartmentCreateModel Department { get; set; } = default!;

        public IActionResult OnGet()
        {
            Department = new DepartmentCreateModel();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Check if department code already exists
                var codeExists = await _departmentService.DepartmentCodeExistsAsync(Department.Code);
                if (codeExists)
                {
                    ModelState.AddModelError("Department.Code", "A department with this code already exists.");
                    return Page();
                }

                // Check if department name already exists
                var nameExists = await _departmentService.DepartmentNameExistsAsync(Department.Name);
                if (nameExists)
                {
                    ModelState.AddModelError("Department.Name", "A department with this name already exists.");
                    return Page();
                }

                // Create new department
                var department = new Department
                {
                    Code = Department.Code.Trim().ToUpper(),
                    Name = Department.Name.Trim()
                };

                var createdDepartment = await _departmentService.CreateDepartmentAsync(department);
                
                TempData["SuccessMessage"] = $"Department '{createdDepartment.Name}' has been created successfully.";
                
                // If this was called from asset creation, redirect back with the new department ID
                if (Request.Query.ContainsKey("returnUrl"))
                {
                    var returnUrl = Request.Query["returnUrl"].ToString();
                    return Redirect($"{returnUrl}?newDepartmentId={createdDepartment.Id}");
                }
                
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred while creating the department: {ex.Message}");
                return Page();
            }
        }
    }

    public class DepartmentCreateModel
    {
        [Required(ErrorMessage = "Department code is required")]
        [StringLength(10, MinimumLength = 2, ErrorMessage = "Department code must be between 2 and 10 characters")]
        [Display(Name = "Department Code")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department name is required")]
        [StringLength(100, ErrorMessage = "Department name cannot exceed 100 characters")]
        [Display(Name = "Department Name")]
        public string Name { get; set; } = string.Empty;
    }
}