using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using buildone.Services;
using buildone.Data;

namespace buildone.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrator,Technician")] // Only Admins and Technicians can manage employees
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly ILogger<EmployeesController> _logger;

        public EmployeesController(
            IEmployeeService employeeService,
            IDepartmentService departmentService,
            ILogger<EmployeesController> logger)
        {
            _employeeService = employeeService;
            _departmentService = departmentService;
            _logger = logger;
        }

        // GET /api/employees
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees([FromQuery] int? departmentId = null)
        {
            try
            {
                var employees = await _employeeService.GetAllEmployeesAsync();

                if (departmentId.HasValue)
                {
                    employees = employees.Where(e => e.DepartmentId == departmentId.Value);
                }

                return Ok(employees.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employees");
                return StatusCode(500, "An error occurred while retrieving employees");
            }
        }

        // GET /api/employees/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployee(int id)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(id);
                if (employee == null)
                {
                    return NotFound($"Employee with ID {id} not found");
                }
                return Ok(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee {EmployeeId}", id);
                return StatusCode(500, "An error occurred while retrieving the employee");
            }
        }

        // POST /api/employees
        [HttpPost]
        public async Task<ActionResult<Employee>> CreateEmployee([FromBody] CreateEmployeeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate department exists
                if (request.DepartmentId.HasValue)
                {
                    var department = await _departmentService.GetDepartmentByIdAsync(request.DepartmentId.Value);
                    if (department == null)
                    {
                        return BadRequest($"Department with ID {request.DepartmentId} not found");
                    }
                }

                var employee = new Employee
                {
                    FullName = request.FullName,
                    Username = request.Username,
                    Email = request.Email,
                    Phone = request.Phone,
                    DepartmentId = request.DepartmentId
                };

                await _employeeService.CreateEmployeeAsync(employee);
                return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating employee");
                return StatusCode(500, "An error occurred while creating the employee");
            }
        }

        // PUT /api/employees/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingEmployee = await _employeeService.GetEmployeeByIdAsync(id);
                if (existingEmployee == null)
                {
                    return NotFound($"Employee with ID {id} not found");
                }

                // Validate department exists
                if (request.DepartmentId.HasValue)
                {
                    var department = await _departmentService.GetDepartmentByIdAsync(request.DepartmentId.Value);
                    if (department == null)
                    {
                        return BadRequest($"Department with ID {request.DepartmentId} not found");
                    }
                }

                existingEmployee.FullName = request.FullName;
                existingEmployee.Username = request.Username;
                existingEmployee.Email = request.Email;
                existingEmployee.Phone = request.Phone;
                existingEmployee.DepartmentId = request.DepartmentId;

                await _employeeService.UpdateEmployeeAsync(existingEmployee);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating employee {EmployeeId}", id);
                return StatusCode(500, "An error occurred while updating the employee");
            }
        }

        // DELETE /api/employees/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(id);
                if (employee == null)
                {
                    return NotFound($"Employee with ID {id} not found");
                }

                await _employeeService.DeleteEmployeeAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting employee {EmployeeId}", id);
                return StatusCode(500, "An error occurred while deleting the employee");
            }
        }

        // GET /api/employees/by-department/{departmentId}
        [HttpGet("by-department/{departmentId}")]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployeesByDepartment(int departmentId)
        {
            try
            {
                var department = await _departmentService.GetDepartmentByIdAsync(departmentId);
                if (department == null)
                {
                    return NotFound($"Department with ID {departmentId} not found");
                }

                var employees = await _employeeService.GetEmployeesByDepartmentAsync(departmentId);
                return Ok(employees);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employees for department {DepartmentId}", departmentId);
                return StatusCode(500, "An error occurred while retrieving employees for the department");
            }
        }
    }

    // Request DTOs
    public class CreateEmployeeRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int? DepartmentId { get; set; }
    }

    public class UpdateEmployeeRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int? DepartmentId { get; set; }
    }
}