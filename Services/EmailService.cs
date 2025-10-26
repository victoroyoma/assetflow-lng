using System.Net;
using System.Net.Mail;

namespace buildone.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        try
        {
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUser = _configuration["Email:SmtpUser"];
            var smtpPassword = _configuration["Email:SmtpPassword"];
            var fromEmail = _configuration["Email:FromEmail"] ?? smtpUser;
            var fromName = _configuration["Email:FromName"] ?? "BuildOne Asset Management";
            var enableSsl = bool.Parse(_configuration["Email:EnableSsl"] ?? "true");

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPassword))
            {
                _logger.LogWarning("Email configuration is incomplete. Skipping email send.");
                return;
            }

            using var message = new MailMessage();
            message.From = new MailAddress(fromEmail, fromName);
            message.To.Add(to);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = isHtml;

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPassword),
                EnableSsl = enableSsl
            };

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent successfully to {To} with subject: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {To} with subject: {Subject}", to, subject);
        }
    }

    public async Task SendInventoryLowStockAlertAsync(string itemName, int currentQuantity, int minimumQuantity, string? location = null)
    {
        var recipients = _configuration["Email:InventoryAlertRecipients"] ?? "";
        if (string.IsNullOrEmpty(recipients))
        {
            _logger.LogWarning("No inventory alert recipients configured.");
            return;
        }

        var subject = $"⚠️ Low Stock Alert: {itemName}";
        var locationInfo = location != null ? $"<div class='info-row'><span class='label'>Location:</span> {location}</div>" : "";
        
        var body = $@"<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ background-color: #f44336; color: white; padding: 20px; border-radius: 10px 10px 0 0; margin: -30px -30px 20px; }}
        .header h1 {{ margin: 0; font-size: 24px; }}
        .alert {{ background-color: #ffebee; padding: 15px; border-left: 4px solid #f44336; margin: 20px 0; }}
        .info-row {{ padding: 10px 0; border-bottom: 1px solid #e0e0e0; }}
        .info-row:last-child {{ border-bottom: none; }}
        .label {{ color: #666; font-weight: bold; }}
        .value {{ color: #f44336; font-weight: bold; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 2px solid #e0e0e0; color: #666; font-size: 12px; text-align: center; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>⚠️ Low Stock Alert</h1>
        </div>
        <div class='alert'>
            <p><strong>Item ""{itemName}""</strong> is running low on stock and requires immediate attention.</p>
        </div>
        
        <div class='info-row'>
            <span class='label'>Item Name:</span> <strong>{itemName}</strong>
        </div>
        <div class='info-row'>
            <span class='label'>Current Quantity:</span> <span class='value'>{currentQuantity}</span>
        </div>
        <div class='info-row'>
            <span class='label'>Minimum Quantity:</span> {minimumQuantity}
        </div>
        {locationInfo}
        <div class='info-row'>
            <span class='label'>Alert Time:</span> {DateTime.Now:MMMM dd, yyyy hh:mm tt}
        </div>
        
        <div style='margin-top: 20px; padding: 15px; background-color: #e7f3ff; border-left: 4px solid #2196F3;'>
            <p style='margin: 0;'><strong>Action Required:</strong> Please restock this item as soon as possible to maintain inventory levels.</p>
        </div>
        <div class='footer'>
            <p>This is an automated notification from BuildOne Asset Management System</p>
        </div>
    </div>
</body>
</html>";

        foreach (var recipient in recipients.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            await SendEmailAsync(recipient.Trim(), subject, body, true);
        }
    }

    public async Task SendInventoryRestockedNotificationAsync(string itemName, int addedQuantity, int newQuantity, string restockedBy, DateTime restockDate, string? reference = null)
    {
        var recipients = _configuration["Email:InventoryAlertRecipients"] ?? "";
        if (string.IsNullOrEmpty(recipients))
        {
            _logger.LogWarning("No inventory alert recipients configured.");
            return;
        }

        var subject = $"✅ Inventory Restocked: {itemName}";
        var referenceInfo = reference != null ? $"<div class='info-row'><span class='label'>Reference:</span> {reference}</div>" : "";
        
        var body = $@"<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; border-radius: 10px 10px 0 0; margin: -30px -30px 20px; }}
        .header h1 {{ margin: 0; font-size: 24px; }}
        .success {{ background-color: #e8f5e9; padding: 15px; border-left: 4px solid #4CAF50; margin: 20px 0; }}
        .info-row {{ padding: 10px 0; border-bottom: 1px solid #e0e0e0; }}
        .info-row:last-child {{ border-bottom: none; }}
        .label {{ color: #666; font-weight: bold; }}
        .value {{ color: #4CAF50; font-weight: bold; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 2px solid #e0e0e0; color: #666; font-size: 12px; text-align: center; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✅ Inventory Restocked</h1>
        </div>
        <div class='success'>
            <p><strong>Item ""{itemName}""</strong> has been successfully restocked.</p>
        </div>
        
        <div class='info-row'>
            <span class='label'>Item Name:</span> <strong>{itemName}</strong>
        </div>
        <div class='info-row'>
            <span class='label'>Quantity Added:</span> <span class='value'>+{addedQuantity}</span>
        </div>
        <div class='info-row'>
            <span class='label'>New Total Quantity:</span> {newQuantity}
        </div>
        <div class='info-row'>
            <span class='label'>Restocked By:</span> {restockedBy}
        </div>
        <div class='info-row'>
            <span class='label'>Restock Date:</span> {restockDate:MMMM dd, yyyy hh:mm tt}
        </div>
        {referenceInfo}
        
        <div class='footer'>
            <p>This is an automated notification from BuildOne Asset Management System</p>
        </div>
    </div>
</body>
</html>";

        foreach (var recipient in recipients.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            await SendEmailAsync(recipient.Trim(), subject, body, true);
        }
    }

    public async Task SendInventoryWithdrawnNotificationAsync(string itemName, int withdrawnQuantity, int remainingQuantity, string withdrawnBy, DateTime withdrawDate, string? purpose = null)
    {
        var recipients = _configuration["Email:InventoryAlertRecipients"] ?? "";
        if (string.IsNullOrEmpty(recipients))
        {
            _logger.LogWarning("No inventory alert recipients configured.");
            return;
        }

        var subject = $"📦 Inventory Withdrawn: {itemName}";
        var purposeInfo = purpose != null ? $"<div class='info-row'><span class='label'>Purpose:</span> {purpose}</div>" : "";
        
        var body = $@"<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ background-color: #2196F3; color: white; padding: 20px; border-radius: 10px 10px 0 0; margin: -30px -30px 20px; }}
        .header h1 {{ margin: 0; font-size: 24px; }}
        .info-box {{ background-color: #e3f2fd; padding: 15px; border-left: 4px solid #2196F3; margin: 20px 0; }}
        .info-row {{ padding: 10px 0; border-bottom: 1px solid #e0e0e0; }}
        .info-row:last-child {{ border-bottom: none; }}
        .label {{ color: #666; font-weight: bold; }}
        .value {{ color: #2196F3; font-weight: bold; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 2px solid #e0e0e0; color: #666; font-size: 12px; text-align: center; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📦 Inventory Withdrawn</h1>
        </div>
        <div class='info-box'>
            <p><strong>Item ""{itemName}""</strong> has been withdrawn from inventory.</p>
        </div>
        
        <div class='info-row'>
            <span class='label'>Item Name:</span> <strong>{itemName}</strong>
        </div>
        <div class='info-row'>
            <span class='label'>Quantity Withdrawn:</span> <span class='value'>-{withdrawnQuantity}</span>
        </div>
        <div class='info-row'>
            <span class='label'>Remaining Quantity:</span> {remainingQuantity}
        </div>
        <div class='info-row'>
            <span class='label'>Withdrawn By:</span> {withdrawnBy}
        </div>
        <div class='info-row'>
            <span class='label'>Withdraw Date:</span> {withdrawDate:MMMM dd, yyyy hh:mm tt}
        </div>
        {purposeInfo}
        
        <div class='footer'>
            <p>This is an automated notification from BuildOne Asset Management System</p>
        </div>
    </div>
</body>
</html>";

        foreach (var recipient in recipients.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            await SendEmailAsync(recipient.Trim(), subject, body, true);
        }
    }
}
