using buildone.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace buildone.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SearchController> _logger;

    public SearchController(
        ApplicationDbContext context,
        ILogger<SearchController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Global search across assets, employees, and departments
    /// </summary>
    /// <param name="q">Search query</param>
    /// <returns>Search results grouped by category</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search([FromQuery] string? q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
        {
            return BadRequest(new { error = "Search query must be at least 2 characters" });
        }

        try
        {
            var searchTerm = q.ToLower().Trim();
            
            // Search Assets
            var assets = await _context.Assets
                .Include(a => a.AssignedEmployee)
                .Include(a => a.Department)
                .Where(a => 
                    (a.AssetTag != null && a.AssetTag.ToLower().Contains(searchTerm)) ||
                    (a.PcId != null && a.PcId.ToLower().Contains(searchTerm)) ||
                    (a.SerialNumber != null && a.SerialNumber.ToLower().Contains(searchTerm)) ||
                    (a.Brand != null && a.Brand.ToLower().Contains(searchTerm)) ||
                    (a.Model != null && a.Model.ToLower().Contains(searchTerm)) ||
                    (a.Type != null && a.Type.ToLower().Contains(searchTerm)))
                .Take(10)
                .Select(a => new
                {
                    id = a.Id,
                    type = "asset",
                    title = a.AssetTag ?? "Unknown",
                    subtitle = $"{a.Brand} {a.Model} - {a.Type}",
                    description = a.Status.ToString(),
                    url = $"/Assets/Details/{a.Id}",
                    icon = "fa-laptop"
                })
                .ToListAsync();

            // Search Employees
            var employees = await _context.Employees
                .Include(e => e.Department)
                .Where(e => 
                    (e.FullName != null && e.FullName.ToLower().Contains(searchTerm)) ||
                    (e.Email != null && e.Email.ToLower().Contains(searchTerm)) ||
                    (e.Username != null && e.Username.ToLower().Contains(searchTerm)))
                .Take(10)
                .Select(e => new
                {
                    id = e.Id,
                    type = "employee",
                    title = e.FullName ?? "Unknown",
                    subtitle = e.Email ?? "",
                    description = e.Department != null ? e.Department.Name : "No Department",
                    url = $"/Employees/Details/{e.Id}",
                    icon = "fa-user"
                })
                .ToListAsync();

            // Search Departments
            var departments = await _context.Departments
                .Where(d => 
                    (d.Name != null && d.Name.ToLower().Contains(searchTerm)) ||
                    (d.Code != null && d.Code.ToLower().Contains(searchTerm)))
                .Take(5)
                .Select(d => new
                {
                    id = d.Id,
                    type = "department",
                    title = d.Name ?? "Unknown",
                    subtitle = d.Code ?? "",
                    description = $"{d.Employees.Count} employees",
                    url = $"/Departments/Details/{d.Id}",
                    icon = "fa-building"
                })
                .ToListAsync();

            // Search Imaging Jobs
            var imagingJobs = await _context.ImagingJobs
                .Include(j => j.Asset)
                .Include(j => j.Technician)
                .Where(j => 
                    (j.Asset != null && j.Asset.AssetTag != null && j.Asset.AssetTag.ToLower().Contains(searchTerm)) ||
                    (j.ImageVersion != null && j.ImageVersion.ToLower().Contains(searchTerm)))
                .Take(10)
                .Select(j => new
                {
                    id = j.Id,
                    type = "imaging_job",
                    title = $"Job #{j.Id} - {(j.Asset != null ? j.Asset.AssetTag : "Unknown")}",
                    subtitle = j.ImageVersion ?? "",
                    description = j.Status.ToString(),
                    url = $"/Imaging/Details/{j.Id}",
                    icon = "fa-compact-disc"
                })
                .ToListAsync();

            var totalResults = assets.Count + employees.Count + departments.Count + imagingJobs.Count;

            return Ok(new
            {
                query = q,
                totalResults,
                results = new
                {
                    assets,
                    employees,
                    departments,
                    imagingJobs
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing search for query: {Query}", q);
            return StatusCode(500, new { error = "An error occurred while searching" });
        }
    }
}
