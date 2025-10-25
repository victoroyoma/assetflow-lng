using System.IO;
using System.Threading.Tasks;

namespace buildone.Services;

public interface IExcelDataSeedingService
{
    /// <summary>
    /// Seeds data from an Excel workbook stream. The workbook may contain one or more sheets
    /// named: Departments, Employees, Assets, Inventory, ImagingJobs. Sheets that are missing
    /// will be skipped.
    /// </summary>
    /// <param name="stream">Input stream for the uploaded Excel (.xlsx) file</param>
    /// <param name="uploadedBy">Optional user who uploaded the file (Audit)</param>
    /// <returns>Counts of records processed for each category</returns>
    Task<(int Departments, int Employees, int Assets, int Inventory)> SeedFromExcelAsync(Stream stream, string? uploadedBy = null);
}
