using buildone.Data;
using Microsoft.EntityFrameworkCore;

namespace buildone.Services;

public class DepartmentService : IDepartmentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DepartmentService> _logger;

    public DepartmentService(ApplicationDbContext context, ILogger<DepartmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
    {
        try
        {
            return await _context.Departments
                .OrderBy(d => d.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all departments");
            throw;
        }
    }

    public async Task<IEnumerable<Department>> GetAllDepartmentsWithEmployeesAsync()
    {
        try
        {
            return await _context.Departments
                .Include(d => d.Employees)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all departments with employees");
            throw;
        }
    }

    public async Task<Department?> GetDepartmentByIdAsync(int id)
    {
        try
        {
            return await _context.Departments
                .Include(d => d.Employees)
                .Include(d => d.Assets)
                .FirstOrDefaultAsync(d => d.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving department with ID {DepartmentId}", id);
            throw;
        }
    }

    public async Task<Department?> GetDepartmentByCodeAsync(string code)
    {
        try
        {
            return await _context.Departments
                .Include(d => d.Employees)
                .Include(d => d.Assets)
                .FirstOrDefaultAsync(d => d.Code == code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving department with code {DepartmentCode}", code);
            throw;
        }
    }

    public async Task<Department> CreateDepartmentAsync(Department department)
    {
        try
        {
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Department created with ID {DepartmentId}", department.Id);
            return department;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating department");
            throw;
        }
    }

    public async Task<bool> UpdateDepartmentAsync(Department department)
    {
        try
        {
            _context.Entry(department).State = EntityState.Modified;
            var result = await _context.SaveChangesAsync();
            _logger.LogInformation("Department updated with ID {DepartmentId}", department.Id);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating department with ID {DepartmentId}", department.Id);
            throw;
        }
    }

    public async Task<bool> DeleteDepartmentAsync(int id)
    {
        try
        {
            var department = await _context.Departments.FindAsync(id);
            if (department != null)
            {
                _context.Departments.Remove(department);
                var result = await _context.SaveChangesAsync();
                _logger.LogInformation("Department deleted with ID {DepartmentId}", id);
                return result > 0;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting department with ID {DepartmentId}", id);
            throw;
        }
    }

    public async Task<bool> DepartmentExistsAsync(int id)
    {
        try
        {
            return await _context.Departments.AnyAsync(d => d.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if department exists with ID {DepartmentId}", id);
            throw;
        }
    }

    public async Task<bool> DepartmentCodeExistsAsync(string code, int? excludeId = null)
    {
        try
        {
            if (string.IsNullOrEmpty(code)) return false;
            
            var query = _context.Departments.Where(d => d.Code == code);
            if (excludeId.HasValue)
            {
                query = query.Where(d => d.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if department code exists {DepartmentCode}", code);
            throw;
        }
    }

    public async Task<bool> DepartmentNameExistsAsync(string name, int? excludeId = null)
    {
        try
        {
            if (string.IsNullOrEmpty(name)) return false;
            
            var query = _context.Departments.Where(d => d.Name == name);
            if (excludeId.HasValue)
            {
                query = query.Where(d => d.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if department name exists {DepartmentName}", name);
            throw;
        }
    }

    public async Task<IEnumerable<Department>> SearchDepartmentsAsync(string searchTerm)
    {
        try
        {
            return await _context.Departments
                .Where(d => d.Name.Contains(searchTerm) ||
                           (d.Code != null && d.Code.Contains(searchTerm)))
                .OrderBy(d => d.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching departments with term {SearchTerm}", searchTerm);
            throw;
        }
    }
}