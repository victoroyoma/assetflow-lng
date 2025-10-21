using buildone.Data;

namespace buildone.Services;

public interface IEmployeeService
{
    Task<IEnumerable<Employee>> GetAllEmployeesAsync();
    Task<Employee?> GetEmployeeByIdAsync(int id);
    Task<Employee?> GetEmployeeByUsernameAsync(string username);
    Task<Employee?> GetEmployeeByEmailAsync(string email);
    Task<Employee> CreateEmployeeAsync(Employee employee);
    Task<bool> UpdateEmployeeAsync(Employee employee);
    Task<bool> DeleteEmployeeAsync(int id);
    Task<bool> EmployeeExistsAsync(int id);
    Task<bool> UsernameExistsAsync(string username, int? excludeId = null);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);
    Task<IEnumerable<Employee>> SearchEmployeesAsync(string searchTerm);
    Task<IEnumerable<Employee>> GetEmployeesByDepartmentAsync(int departmentId);
}