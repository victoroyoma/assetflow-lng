using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using buildone.Services;
using buildone.Data;

namespace buildone.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrator,Technician")] // Only Admins and Technicians can manage departments
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;
        private readonly ILogger<DepartmentsController> _logger;

        public DepartmentsController(IDepartmentService departmentService, ILogger<DepartmentsController> logger)
        {
            _departmentService = departmentService;
            _logger = logger;
        }

        // GET /api/departments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Department>>> GetDepartments()
        {
            try
            {
                var departments = await _departmentService.GetAllDepartmentsAsync();
                return Ok(departments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving departments");
                return StatusCode(500, "An error occurred while retrieving departments");
            }
        }

        // GET /api/departments/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Department>> GetDepartment(int id)
        {
            try
            {
                var department = await _departmentService.GetDepartmentByIdAsync(id);
                if (department == null)
                {
                    return NotFound($"Department with ID {id} not found");
                }
                return Ok(department);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving department {DepartmentId}", id);
                return StatusCode(500, "An error occurred while retrieving the department");
            }
        }

        // POST /api/departments
        [HttpPost]
        public async Task<ActionResult<Department>> CreateDepartment([FromBody] CreateDepartmentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var department = new Department
                {
                    Name = request.Name,
                    Code = request.Code
                };

                await _departmentService.CreateDepartmentAsync(department);
                return CreatedAtAction(nameof(GetDepartment), new { id = department.Id }, department);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating department");
                return StatusCode(500, "An error occurred while creating the department");
            }
        }

        // PUT /api/departments/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepartment(int id, [FromBody] UpdateDepartmentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingDepartment = await _departmentService.GetDepartmentByIdAsync(id);
                if (existingDepartment == null)
                {
                    return NotFound($"Department with ID {id} not found");
                }

                existingDepartment.Name = request.Name;
                existingDepartment.Code = request.Code;

                await _departmentService.UpdateDepartmentAsync(existingDepartment);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating department {DepartmentId}", id);
                return StatusCode(500, "An error occurred while updating the department");
            }
        }

        // DELETE /api/departments/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            try
            {
                var department = await _departmentService.GetDepartmentByIdAsync(id);
                if (department == null)
                {
                    return NotFound($"Department with ID {id} not found");
                }

                await _departmentService.DeleteDepartmentAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting department {DepartmentId}", id);
                return StatusCode(500, "An error occurred while deleting the department");
            }
        }
    }

    // Request DTOs
    public class CreateDepartmentRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
    }

    public class UpdateDepartmentRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
    }
}