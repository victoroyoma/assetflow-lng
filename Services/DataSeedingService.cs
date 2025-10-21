using buildone.Data;
using Microsoft.AspNetCore.Identity;

namespace buildone.Services;

public interface IDataSeedingService
{
    Task SeedDataAsync();
    Task SeedBulkDataAsync();
}

public class DataSeedingService : IDataSeedingService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IBulkDataSeedingService _bulkDataSeedingService;
    private readonly ILogger<DataSeedingService> _logger;

    public DataSeedingService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IBulkDataSeedingService bulkDataSeedingService,
        ILogger<DataSeedingService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _bulkDataSeedingService = bulkDataSeedingService;
        _logger = logger;
    }

    public async Task SeedDataAsync()
    {
        try
        {
            // Seed roles first
            await SeedRolesAsync();
            
            // Then seed admin user
            await SeedAdminUserAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding data");
            throw;
        }
    }

    public async Task SeedBulkDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting comprehensive bulk data seeding...");
            
            // First ensure roles and admin user exist
            await SeedDataAsync();
            
            // Then seed bulk data (departments, employees, assets, imaging jobs)
            await _bulkDataSeedingService.SeedBulkDataAsync();
            
            _logger.LogInformation("Comprehensive bulk data seeding completed successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding bulk data");
            throw;
        }
    }

    private async Task SeedRolesAsync()
    {
        var roles = new[]
        {
            new { Name = "Administrator", Description = "Full system access and user management" },
            new { Name = "User", Description = "Standard user access with limited permissions" },
            new { Name = "Technician", Description = "Asset management and imaging job access" }
        };

        foreach (var roleData in roles)
        {
            if (!await _roleManager.RoleExistsAsync(roleData.Name))
            {
                var role = new ApplicationRole(roleData.Name, roleData.Description);
                var result = await _roleManager.CreateAsync(role);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("Created role: {RoleName}", roleData.Name);
                }
                else
                {
                    _logger.LogError("Failed to create role {RoleName}: {Errors}", 
                        roleData.Name, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }

    private async Task SeedAdminUserAsync()
    {
        const string adminEmail = "admin@buildone.com";
        const string adminPassword = "Admin123!";

        var existingUser = await _userManager.FindByEmailAsync(adminEmail);
        
        if (existingUser == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Administrator");
                _logger.LogInformation("Created admin user: {Email}", adminEmail);
            }
            else
            {
                _logger.LogError("Failed to create admin user: {Errors}", 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            // Check if admin user has the correct password
            var isPasswordValid = await _userManager.CheckPasswordAsync(existingUser, adminPassword);
            if (!isPasswordValid)
            {
                _logger.LogInformation("Updating admin user password");
                var token = await _userManager.GeneratePasswordResetTokenAsync(existingUser);
                await _userManager.ResetPasswordAsync(existingUser, token, adminPassword);
            }

            // Ensure admin user has Administrator role
            if (!await _userManager.IsInRoleAsync(existingUser, "Administrator"))
            {
                await _userManager.AddToRoleAsync(existingUser, "Administrator");
                _logger.LogInformation("Added Administrator role to existing admin user");
            }
        }
    }
}