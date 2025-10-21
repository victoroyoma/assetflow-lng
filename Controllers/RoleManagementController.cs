using buildone.Authorization;
using buildone.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace buildone.Controllers
{
    [Authorize(Policy = Policies.CanManageRoles)]
    public class RoleManagementController : Controller
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoleManagementController(
            RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        // GET: RoleManagement
        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles
                .AsNoTracking()
                .OrderBy(r => r.Name)
                .ToListAsync();
            
            return View(roles);
        }

        // GET: RoleManagement/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            // Get users in this role
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            
            ViewBag.UsersInRole = usersInRole;
            return View(role);
        }

        // GET: RoleManagement/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RoleManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationRole role)
        {
            if (ModelState.IsValid)
            {
                role.CreatedAt = DateTime.UtcNow;
                role.IsActive = true;
                
                var result = await _roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = $"Role '{role.Name}' created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(role);
        }

        // GET: RoleManagement/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            // Prevent editing of system roles
            if (role.Name == "Administrator" || role.Name == "User" || role.Name == "Technician")
            {
                TempData["ErrorMessage"] = "System roles cannot be edited.";
                return RedirectToAction(nameof(Index));
            }

            return View(role);
        }

        // POST: RoleManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ApplicationRole role)
        {
            if (id != role.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingRole = await _roleManager.FindByIdAsync(id);
                    if (existingRole == null)
                    {
                        return NotFound();
                    }

                    // Prevent editing of system roles
                    if (existingRole.Name == "Administrator" || existingRole.Name == "User" || existingRole.Name == "Technician")
                    {
                        TempData["ErrorMessage"] = "System roles cannot be edited.";
                        return RedirectToAction(nameof(Index));
                    }

                    existingRole.Name = role.Name;
                    existingRole.NormalizedName = role.Name?.ToUpperInvariant();
                    existingRole.Description = role.Description;
                    existingRole.IsActive = role.IsActive;

                    var result = await _roleManager.UpdateAsync(existingRole);
                    if (result.Succeeded)
                    {
                        TempData["SuccessMessage"] = $"Role '{role.Name}' updated successfully.";
                        return RedirectToAction(nameof(Index));
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await RoleExistsAsync(role.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(role);
        }

        // GET: RoleManagement/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            // Prevent deletion of system roles
            if (role.Name == "Administrator" || role.Name == "User" || role.Name == "Technician")
            {
                TempData["ErrorMessage"] = "System roles cannot be deleted.";
                return RedirectToAction(nameof(Index));
            }

            // Get users in this role
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            ViewBag.UsersInRole = usersInRole;

            return View(role);
        }

        // POST: RoleManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            // Prevent deletion of system roles
            if (role.Name == "Administrator" || role.Name == "User" || role.Name == "Technician")
            {
                TempData["ErrorMessage"] = "System roles cannot be deleted.";
                return RedirectToAction(nameof(Index));
            }

            // Check if any users are assigned to this role
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            if (usersInRole.Any())
            {
                TempData["ErrorMessage"] = $"Cannot delete role '{role.Name}' as it is assigned to {usersInRole.Count} user(s).";
                return RedirectToAction(nameof(Index));
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Role '{role.Name}' deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error deleting role.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> RoleExistsAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            return role != null;
        }
    }
}