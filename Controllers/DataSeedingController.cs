using buildone.Authorization;
using buildone.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace buildone.Controllers;

[Authorize(Policy = Policies.CanAccessSystemSettings)]
[Route("api/[controller]")]
[ApiController]
public class DataSeedingController : ControllerBase
{
    private readonly IDataSeedingService _dataSeedingService;
    private readonly IBulkDataSeedingService _bulkDataSeedingService;
    private readonly IExcelDataSeedingService _excelDataSeedingService;
    private readonly ILogger<DataSeedingController> _logger;

    public DataSeedingController(
        IDataSeedingService dataSeedingService,
        IBulkDataSeedingService bulkDataSeedingService,
        IExcelDataSeedingService excelDataSeedingService,
        ILogger<DataSeedingController> logger)
    {
        _dataSeedingService = dataSeedingService;
        _bulkDataSeedingService = bulkDataSeedingService;
        _excelDataSeedingService = excelDataSeedingService;
        _logger = logger;
    }

    /// <summary>
    /// Seeds basic data (roles and admin user)
    /// </summary>
    /// <returns>Success message</returns>
    [HttpPost("seed-basic")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SeedBasicData()
    {
        try
        {
            _logger.LogInformation("Basic data seeding initiated by {User}", User.Identity?.Name);
            await _dataSeedingService.SeedDataAsync();
            
            return Ok(new 
            { 
                message = "Basic data seeding completed successfully",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed basic data");
            return StatusCode(500, new 
            { 
                error = "Failed to seed basic data", 
                details = ex.Message 
            });
        }
    }

    /// <summary>
    /// Seeds bulk data (departments, employees, assets, and imaging jobs)
    /// This will create 10 employees, 400 assets, and 400 imaging jobs
    /// </summary>
    /// <returns>Success message with statistics</returns>
    [HttpPost("seed-bulk")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SeedBulkData()
    {
        try
        {
            _logger.LogInformation("Bulk data seeding initiated by {User}", User.Identity?.Name);
            
            await _dataSeedingService.SeedBulkDataAsync();
            
            var stats = await _bulkDataSeedingService.GetSeedingStatisticsAsync();
            
            return Ok(new 
            { 
                message = "Bulk data seeding completed successfully",
                statistics = new
                {
                    departments = stats.Departments,
                    employees = stats.Employees,
                    assets = stats.Assets,
                    imagingJobs = stats.ImagingJobs
                },
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed bulk data");
            return StatusCode(500, new 
            { 
                error = "Failed to seed bulk data", 
                details = ex.Message 
            });
        }
    }

    /// <summary>
    /// Gets current database statistics
    /// </summary>
    /// <returns>Database statistics</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var stats = await _bulkDataSeedingService.GetSeedingStatisticsAsync();
            
            return Ok(new 
            { 
                statistics = new
                {
                    departments = stats.Departments,
                    employees = stats.Employees,
                    assets = stats.Assets,
                    imagingJobs = stats.ImagingJobs
                },
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve statistics");
            return StatusCode(500, new 
            { 
                error = "Failed to retrieve statistics", 
                details = ex.Message 
            });
        }
    }

    /// <summary>
    /// Seeds data from an uploaded Excel (.xlsx) file. Accepts a multipart/form-data file upload.
    /// Supports sheets named: Departments, Employees, Assets, Inventory. Sheets not present will be skipped.
    /// </summary>
    [HttpPost("seed-from-excel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SeedFromExcel([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "No file uploaded or file is empty." });
        }

        try
        {
            _logger.LogInformation("Excel seeding initiated by {User}. Filename={FileName}", User.Identity?.Name, file.FileName);

            using var stream = file.OpenReadStream();
            var result = await _excelDataSeedingService.SeedFromExcelAsync(stream, User.Identity?.Name);

            return Ok(new
            {
                message = "Excel seeding completed",
                departments = result.Departments,
                employees = result.Employees,
                assets = result.Assets,
                inventory = result.Inventory,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed from Excel");
            return StatusCode(500, new { error = "Failed to seed from Excel", details = ex.Message });
        }
    }
}
