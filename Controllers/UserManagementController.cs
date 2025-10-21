using buildone.Authorization;
using buildone.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace buildone.Controllers;

[Authorize(Policy = Policies.CanManageUsers)]
public class UserManagementController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ApplicationDbContext _context;

    public UserManagementController(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    // GET: UserManagement
    public async Task<IActionResult> Index()
    {
        // Performance: Use AsNoTracking for read-only query
        var users = await _userManager.Users
            .Include(u => u.Employee)
            .AsNoTracking()
            .OrderBy(u => u.FullName)
            .Select(u => new
            {
                User = u,
                EmployeeName = u.Employee != null ? u.Employee.FullName : null
            })
            .ToListAsync();

        var userViewModels = new List<UserViewModel>();

        foreach (var item in users)
        {
            var roles = await _userManager.GetRolesAsync(item.User);
            userViewModels.Add(new UserViewModel
            {
                Id = item.User.Id,
                FullName = item.User.FullName,
                Email = item.User.Email!,
                UserName = item.User.UserName!,
                IsActive = item.User.IsActive,
                CreatedAt = item.User.CreatedAt,
                LastLoginAt = item.User.LastLoginAt,
                Roles = roles.ToList(),
                EmployeeName = item.EmployeeName
            });
        }

        return View(userViewModels);
    }

    // GET: UserManagement/Create
    public async Task<IActionResult> Create()
    {
        var model = new CreateUserViewModel();
        await PopulateRolesAndEmployees(model);
        return View(model);
    }

    // POST: UserManagement/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Auto-create employee if not linked to existing one and user will be a Technician
            if (!model.EmployeeId.HasValue && model.SelectedRoles != null && model.SelectedRoles.Contains("Technician"))
            {
                var employee = new Employee
                {
                    FullName = model.FullName,
                    Username = model.Email.Split('@')[0], // Use email prefix as username
                    Email = model.Email,
                    Phone = model.Phone
                };

                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();
                
                model.EmployeeId = employee.Id;
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Phone = model.Phone,
                EmployeeId = model.EmployeeId,
                IsActive = model.IsActive,
                EmailConfirmed = true // Auto-confirm for admin-created users
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Assign selected roles
                if (model.SelectedRoles != null && model.SelectedRoles.Any())
                {
                    await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                }

                var employeeMessage = model.EmployeeId.HasValue ? " and linked to employee record" : "";
                TempData["SuccessMessage"] = $"User '{user.FullName}' created successfully{employeeMessage}.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        await PopulateRolesAndEmployees(model);
        return View(model);
    }

    // GET: UserManagement/Edit/5
    public async Task<IActionResult> Edit(string id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var userRoles = await _userManager.GetRolesAsync(user);

        var model = new EditUserViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            Phone = user.Phone,
            EmployeeId = user.EmployeeId,
            IsActive = user.IsActive,
            SelectedRoles = userRoles.ToList()
        };

        await PopulateRolesAndEmployees(model);
        return View(model);
    }

    // POST: UserManagement/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, EditUserViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email; // Keep username in sync with email
            user.Phone = model.Phone;
            user.EmployeeId = model.EmployeeId;
            user.IsActive = model.IsActive;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Update roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                var selectedRoles = model.SelectedRoles ?? new List<string>();

                var rolesToRemove = currentRoles.Except(selectedRoles);
                var rolesToAdd = selectedRoles.Except(currentRoles);

                if (rolesToRemove.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                }

                if (rolesToAdd.Any())
                {
                    await _userManager.AddToRolesAsync(user, rolesToAdd);
                }

                TempData["SuccessMessage"] = $"User '{user.FullName}' updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        await PopulateRolesAndEmployees(model);
        return View(model);
    }

    // GET: UserManagement/ResetPassword/5
    public async Task<IActionResult> ResetPassword(string id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var model = new ResetPasswordViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email!
        };

        return View(model);
    }

    // POST: UserManagement/ResetPassword/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string id, ResetPasswordViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Password reset successfully for '{user.FullName}'.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    // POST: UserManagement/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // Prevent deletion of the last administrator
        var admins = await _userManager.GetUsersInRoleAsync("Administrator");
        var isAdmin = await _userManager.IsInRoleAsync(user, "Administrator");
        
        if (isAdmin && admins.Count <= 1)
        {
            TempData["ErrorMessage"] = "Cannot delete the last administrator account.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = $"User '{user.FullName}' deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Error deleting user: " + string.Join(", ", result.Errors.Select(e => e.Description));
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateRolesAndEmployees(dynamic model)
    {
        // Get all roles
        var roles = await _roleManager.Roles
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync();

        // Get employees not already linked to users
        var employees = await _context.Employees
            .Where(e => !_context.Users.Any(u => u.EmployeeId == e.Id))
            .OrderBy(e => e.FullName)
            .ToListAsync();

        // If editing, include the current employee
        if (model is EditUserViewModel editModel && editModel.EmployeeId.HasValue)
        {
            var currentEmployee = await _context.Employees.FindAsync(editModel.EmployeeId.Value);
            if (currentEmployee != null && !employees.Any(e => e.Id == currentEmployee.Id))
            {
                employees.Insert(0, currentEmployee);
            }
        }

        model.AvailableRoles = roles;
        model.AvailableEmployees = employees;
    }
}

// View Models
public class UserViewModel
{
    public string Id { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string UserName { get; set; } = "";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<string> Roles { get; set; } = new();
    public string? EmployeeName { get; set; }
}

public class CreateUserViewModel
{
    [Required]
    [StringLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = "";

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [StringLength(20)]
    [Display(Name = "Phone")]
    public string? Phone { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = "";

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = "";

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Employee")]
    public int? EmployeeId { get; set; }

    [Display(Name = "Roles")]
    public List<string>? SelectedRoles { get; set; }

    // For dropdowns
    public List<ApplicationRole> AvailableRoles { get; set; } = new();
    public List<Employee> AvailableEmployees { get; set; } = new();
}

public class EditUserViewModel
{
    public string Id { get; set; } = "";

    [Required]
    [StringLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = "";

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [StringLength(20)]
    [Display(Name = "Phone")]
    public string? Phone { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Employee")]
    public int? EmployeeId { get; set; }

    [Display(Name = "Roles")]
    public List<string>? SelectedRoles { get; set; }

    // For dropdowns
    public List<ApplicationRole> AvailableRoles { get; set; } = new();
    public List<Employee> AvailableEmployees { get; set; } = new();
}

public class ResetPasswordViewModel
{
    public string Id { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; } = "";

    [DataType(DataType.Password)]
    [Display(Name = "Confirm New Password")]
    [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = "";
}