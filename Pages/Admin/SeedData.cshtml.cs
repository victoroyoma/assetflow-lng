using buildone.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace buildone.Pages.Admin;

[Authorize(Policy = "CanAccessSystemSettings")]
public class SeedDataModel : PageModel
{
    private readonly IDataSeedingService _dataSeedingService;
    private readonly IBulkDataSeedingService _bulkDataSeedingService;
    private readonly ILogger<SeedDataModel> _logger;

    public SeedDataModel(
        IDataSeedingService dataSeedingService,
        IBulkDataSeedingService bulkDataSeedingService,
        ILogger<SeedDataModel> logger)
    {
        _dataSeedingService = dataSeedingService;
        _bulkDataSeedingService = bulkDataSeedingService;
        _logger = logger;
    }

    public (int Departments, int Employees, int Assets, int ImagingJobs)? Statistics { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            Statistics = await _bulkDataSeedingService.GetSeedingStatisticsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load statistics");
            TempData["ErrorMessage"] = "Failed to load statistics. Please refresh the page.";
        }
    }

    public async Task<IActionResult> OnPostSeedBasicAsync()
    {
        try
        {
            _logger.LogInformation("Basic data seeding initiated by {User}", User.Identity?.Name);
            await _dataSeedingService.SeedDataAsync();
            
            TempData["SuccessMessage"] = "Basic data seeding completed successfully! Roles and admin user have been created.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed basic data");
            TempData["ErrorMessage"] = $"Failed to seed basic data: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSeedBulkAsync()
    {
        try
        {
            _logger.LogInformation("Bulk data seeding initiated by {User}", User.Identity?.Name);
            
            var statsBefore = await _bulkDataSeedingService.GetSeedingStatisticsAsync();
            
            await _dataSeedingService.SeedBulkDataAsync();
            
            var statsAfter = await _bulkDataSeedingService.GetSeedingStatisticsAsync();
            
            var departmentsAdded = statsAfter.Departments - statsBefore.Departments;
            var employeesAdded = statsAfter.Employees - statsBefore.Employees;
            var assetsAdded = statsAfter.Assets - statsBefore.Assets;
            var jobsAdded = statsAfter.ImagingJobs - statsBefore.ImagingJobs;
            
            TempData["SuccessMessage"] = $"Bulk data seeding completed successfully! " +
                $"Added: {departmentsAdded} departments, {employeesAdded} employees, " +
                $"{assetsAdded} assets, and {jobsAdded} imaging jobs.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed bulk data");
            TempData["ErrorMessage"] = $"Failed to seed bulk data: {ex.Message}";
        }

        return RedirectToPage();
    }
}
