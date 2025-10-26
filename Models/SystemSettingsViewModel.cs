namespace buildone.Models
{
    public class SystemSettingsViewModel
    {
        public CompanyInfo CompanyInfo { get; set; } = new();
        public SecuritySettings SecuritySettings { get; set; } = new();
        public MaintenanceSettings MaintenanceSettings { get; set; } = new();
        public EmailSettings EmailSettings { get; set; } = new();
    }

    public class CompanyInfo
    {
        public string CompanyName { get; set; } = "BuildOne Asset Management";
        public string Address { get; set; } = "";
        public string City { get; set; } = "";
        public string State { get; set; } = "";
        public string ZipCode { get; set; } = "";
        public string Country { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string Website { get; set; } = "";
    }

    public class SecuritySettings
    {
        public int PasswordMinLength { get; set; } = 6;
        public bool RequireDigit { get; set; } = true;
        public bool RequireLowercase { get; set; } = true;
        public bool RequireUppercase { get; set; } = true;
        public bool RequireNonAlphanumeric { get; set; } = true;
        public int MaxFailedAccessAttempts { get; set; } = 5;
        public int LockoutTimeSpanMinutes { get; set; } = 15;
        public int SessionTimeoutMinutes { get; set; } = 60;
    }

    public class MaintenanceSettings
    {
        public bool MaintenanceMode { get; set; } = false;
        public string MaintenanceMessage { get; set; } = "System is under maintenance. Please try again later.";
        public DateTime? ScheduledMaintenanceStart { get; set; }
        public DateTime? ScheduledMaintenanceEnd { get; set; }
        public bool AutoBackupEnabled { get; set; } = true;
        public int BackupRetentionDays { get; set; } = 30;
        public string BackupTime { get; set; } = "02:00";
    }

    public class EmailSettings
    {
        public string SmtpServer { get; set; } = "";
        public int SmtpPort { get; set; } = 587;
        public string SmtpUsername { get; set; } = "";
        public string SmtpPassword { get; set; } = "";
        public bool EnableSsl { get; set; } = true;
        public string FromEmail { get; set; } = "";
        public string FromName { get; set; } = "";
        public bool NotificationsEnabled { get; set; } = true;
        public bool WarrantyAlerts { get; set; } = true;
        public bool ImagingJobAlerts { get; set; } = true;
        public bool InventoryAlerts { get; set; } = true;
        public int LowStockThreshold { get; set; } = 10;
        public int OutOfStockThreshold { get; set; } = 0;
        public string InventoryAlertRecipients { get; set; } = "";
    }
}