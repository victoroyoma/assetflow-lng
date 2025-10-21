using buildone.Data;

namespace buildone.Services;

public interface IDepartmentService
{
    Task<IEnumerable<Department>> GetAllDepartmentsAsync();
    Task<IEnumerable<Department>> GetAllDepartmentsWithEmployeesAsync();
    Task<Department?> GetDepartmentByIdAsync(int id);
    Task<Department?> GetDepartmentByCodeAsync(string code);
    Task<Department> CreateDepartmentAsync(Department department);
    Task<bool> UpdateDepartmentAsync(Department department);
    Task<bool> DeleteDepartmentAsync(int id);
    Task<bool> DepartmentExistsAsync(int id);
    Task<bool> DepartmentCodeExistsAsync(string code, int? excludeId = null);
    Task<bool> DepartmentNameExistsAsync(string name, int? excludeId = null);
    Task<IEnumerable<Department>> SearchDepartmentsAsync(string searchTerm);
}