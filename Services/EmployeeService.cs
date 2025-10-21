using buildone.Data;
using Microsoft.EntityFrameworkCore;

namespace buildone.Services;

public class EmployeeService : IEmployeeService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(ApplicationDbContext context, ILogger<EmployeeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
    {
        try
        {
            return await _context.Employees
                .Include(e => e.Department)
                .OrderBy(e => e.FullName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all employees");
            throw;
        }
    }

    public async Task<Employee?> GetEmployeeByIdAsync(int id)
    {
        try
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Assets)
                .FirstOrDefaultAsync(e => e.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee with ID {EmployeeId}", id);
            throw;
        }
    }

    public async Task<Employee?> GetEmployeeByUsernameAsync(string username)
    {
        try
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Assets)
                .FirstOrDefaultAsync(e => e.Username == username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee with username {Username}", username);
            throw;
        }
    }

    public async Task<Employee?> GetEmployeeByEmailAsync(string email)
    {
        try
        {
            if (string.IsNullOrEmpty(email)) return null;
            
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Assets)
                .FirstOrDefaultAsync(e => e.Email == email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee with email {Email}", email);
            throw;
        }
    }

    public async Task<Employee> CreateEmployeeAsync(Employee employee)
    {
        try
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Employee created with ID {EmployeeId}", employee.Id);
            return employee;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee");
            throw;
        }
    }

    public async Task<bool> UpdateEmployeeAsync(Employee employee)
    {
        try
        {
            _context.Entry(employee).State = EntityState.Modified;
            var result = await _context.SaveChangesAsync();
            _logger.LogInformation("Employee updated with ID {EmployeeId}", employee.Id);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee with ID {EmployeeId}", employee.Id);
            throw;
        }
    }

    public async Task<bool> DeleteEmployeeAsync(int id)
    {
        try
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                var result = await _context.SaveChangesAsync();
                _logger.LogInformation("Employee deleted with ID {EmployeeId}", id);
                return result > 0;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee with ID {EmployeeId}", id);
            throw;
        }
    }

    public async Task<bool> EmployeeExistsAsync(int id)
    {
        try
        {
            return await _context.Employees.AnyAsync(e => e.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if employee exists with ID {EmployeeId}", id);
            throw;
        }
    }

    public async Task<bool> UsernameExistsAsync(string username, int? excludeId = null)
    {
        try
        {
            var query = _context.Employees.Where(e => e.Username == username);
            if (excludeId.HasValue)
            {
                query = query.Where(e => e.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if username exists {Username}", username);
            throw;
        }
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
    {
        try
        {
            if (string.IsNullOrEmpty(email)) return false;
            
            var query = _context.Employees.Where(e => e.Email == email);
            if (excludeId.HasValue)
            {
                query = query.Where(e => e.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if email exists {Email}", email);
            throw;
        }
    }

    public async Task<IEnumerable<Employee>> SearchEmployeesAsync(string searchTerm)
    {
        try
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Where(e => e.FullName.Contains(searchTerm) ||
                           e.Username.Contains(searchTerm) ||
                           (e.Email != null && e.Email.Contains(searchTerm)))
                .OrderBy(e => e.FullName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching employees with term {SearchTerm}", searchTerm);
            throw;
        }
    }

    public async Task<IEnumerable<Employee>> GetEmployeesByDepartmentAsync(int departmentId)
    {
        try
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Where(e => e.DepartmentId == departmentId)
                .OrderBy(e => e.FullName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employees by department {DepartmentId}", departmentId);
            throw;
        }
    }
}