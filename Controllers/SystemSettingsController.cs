using buildone.Authorization;
using buildone.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace buildone.Controllers
{
    [Authorize(Policy = Policies.CanAccessSystemSettings)]
    public class SystemSettingsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SystemSettingsController> _logger;

        public SystemSettingsController(IConfiguration configuration, ILogger<SystemSettingsController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // GET: SystemSettings
        public IActionResult Index()
        {
            var settings = LoadSystemSettings();
            return View(settings);
        }

        // POST: SystemSettings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(SystemSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    SaveSystemSettings(model);
                    TempData["SuccessMessage"] = "System settings updated successfully.";
                    _logger.LogInformation("System settings updated by user {User}", User.Identity?.Name);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating system settings");
                    TempData["ErrorMessage"] = "Error updating system settings. Please try again.";
                }
            }

            return View(model);
        }

        // GET: SystemSettings/Database
        public IActionResult Database()
        {
            var model = new DatabaseManagementViewModel
            {
                LastBackup = GetLastBackupDate(),
                DatabaseSize = GetDatabaseSize(),
                TotalUsers = GetTotalUsers(),
                TotalAssets = GetTotalAssets(),
                TotalEmployees = GetTotalEmployees()
            };

            return View(model);
        }

        // POST: SystemSettings/CreateBackup
        [HttpPost]
        public IActionResult CreateBackup()
        {
            try
            {
                // In a real application, you would implement actual backup logic here
                _logger.LogInformation("Manual backup initiated by user {User}", User.Identity?.Name);
                TempData["SuccessMessage"] = "Database backup created successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating database backup");
                TempData["ErrorMessage"] = "Error creating backup. Please try again.";
            }

            return RedirectToAction(nameof(Database));
        }

        // GET: SystemSettings/Security
        public IActionResult Security()
        {
            var settings = LoadSystemSettings();
            return View(settings.SecuritySettings);
        }

        // POST: SystemSettings/Security
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Security(SecuritySettings model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var settings = LoadSystemSettings();
                    settings.SecuritySettings = model;
                    SaveSystemSettings(settings);
                    
                    TempData["SuccessMessage"] = "Security settings updated successfully.";
                    _logger.LogInformation("Security settings updated by user {User}", User.Identity?.Name);
                    return RedirectToAction(nameof(Security));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating security settings");
                    TempData["ErrorMessage"] = "Error updating security settings. Please try again.";
                }
            }

            return View(model);
        }

        private SystemSettingsViewModel LoadSystemSettings()
        {
            // In a real application, you would load these from a database or configuration file
            // For now, we'll use default values with some configuration overrides
            return new SystemSettingsViewModel
            {
                CompanyInfo = new CompanyInfo
                {
                    CompanyName = _configuration["CompanyInfo:Name"] ?? "BuildOne Asset Management",
                    Address = _configuration["CompanyInfo:Address"] ?? "",
                    City = _configuration["CompanyInfo:City"] ?? "",
                    State = _configuration["CompanyInfo:State"] ?? "",
                    ZipCode = _configuration["CompanyInfo:ZipCode"] ?? "",
                    Country = _configuration["CompanyInfo:Country"] ?? "",
                    Phone = _configuration["CompanyInfo:Phone"] ?? "",
                    Email = _configuration["CompanyInfo:Email"] ?? "",
                    Website = _configuration["CompanyInfo:Website"] ?? ""
                },
                SecuritySettings = new SecuritySettings
                {
                    PasswordMinLength = int.Parse(_configuration["Security:PasswordMinLength"] ?? "6"),
                    RequireDigit = bool.Parse(_configuration["Security:RequireDigit"] ?? "true"),
                    RequireLowercase = bool.Parse(_configuration["Security:RequireLowercase"] ?? "true"),
                    RequireUppercase = bool.Parse(_configuration["Security:RequireUppercase"] ?? "true"),
                    RequireNonAlphanumeric = bool.Parse(_configuration["Security:RequireNonAlphanumeric"] ?? "true"),
                    MaxFailedAccessAttempts = int.Parse(_configuration["Security:MaxFailedAccessAttempts"] ?? "5"),
                    LockoutTimeSpanMinutes = int.Parse(_configuration["Security:LockoutTimeSpanMinutes"] ?? "15"),
                    SessionTimeoutMinutes = int.Parse(_configuration["Security:SessionTimeoutMinutes"] ?? "60")
                },
                MaintenanceSettings = new MaintenanceSettings
                {
                    MaintenanceMode = bool.Parse(_configuration["Maintenance:MaintenanceMode"] ?? "false"),
                    MaintenanceMessage = _configuration["Maintenance:Message"] ?? "System is under maintenance. Please try again later.",
                    AutoBackupEnabled = bool.Parse(_configuration["Maintenance:AutoBackupEnabled"] ?? "true"),
                    BackupRetentionDays = int.Parse(_configuration["Maintenance:BackupRetentionDays"] ?? "30"),
                    BackupTime = _configuration["Maintenance:BackupTime"] ?? "02:00"
                },
                EmailSettings = new EmailSettings
                {
                    SmtpServer = _configuration["Email:SmtpServer"] ?? "",
                    SmtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587"),
                    SmtpUsername = _configuration["Email:SmtpUsername"] ?? "",
                    EnableSsl = bool.Parse(_configuration["Email:EnableSsl"] ?? "true"),
                    FromEmail = _configuration["Email:FromEmail"] ?? "",
                    FromName = _configuration["Email:FromName"] ?? "",
                    NotificationsEnabled = bool.Parse(_configuration["Email:NotificationsEnabled"] ?? "true"),
                    WarrantyAlerts = bool.Parse(_configuration["Email:WarrantyAlerts"] ?? "true"),
                    ImagingJobAlerts = bool.Parse(_configuration["Email:ImagingJobAlerts"] ?? "true"),
                    InventoryAlerts = bool.Parse(_configuration["Email:InventoryAlerts"] ?? "true"),
                    LowStockThreshold = int.Parse(_configuration["Email:LowStockThreshold"] ?? "10"),
                    OutOfStockThreshold = int.Parse(_configuration["Email:OutOfStockThreshold"] ?? "0"),
                    InventoryAlertRecipients = _configuration["Email:InventoryAlertRecipients"] ?? ""
                }
            };
        }

        private void SaveSystemSettings(SystemSettingsViewModel settings)
        {
            // In a real application, you would save these to a database or configuration file
            // For now, we'll just log the action
            _logger.LogInformation("System settings saved: {Settings}", System.Text.Json.JsonSerializer.Serialize(settings));
        }

        private DateTime? GetLastBackupDate()
        {
            // In a real application, you would get this from your backup system
            return DateTime.UtcNow.AddDays(-1);
        }

        private string GetDatabaseSize()
        {
            // In a real application, you would query the database for its size
            return "15.2 MB";
        }

        private int GetTotalUsers()
        {
            // In a real application, you would count from the database
            return 1; // We have the admin user
        }

        private int GetTotalAssets()
        {
            // In a real application, you would count from the database
            return 0; // No sample data
        }

        private int GetTotalEmployees()
        {
            // In a real application, you would count from the database
            return 0; // No sample data
        }
    }

    public class DatabaseManagementViewModel
    {
        public DateTime? LastBackup { get; set; }
        public string DatabaseSize { get; set; } = "";
        public int TotalUsers { get; set; }
        public int TotalAssets { get; set; }
        public int TotalEmployees { get; set; }
    }
}