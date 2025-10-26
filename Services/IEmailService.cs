namespace buildone.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task SendInventoryLowStockAlertAsync(string itemName, int currentQuantity, int minimumQuantity, string? location = null);
    Task SendInventoryRestockedNotificationAsync(string itemName, int addedQuantity, int newQuantity, string restockedBy, DateTime restockDate, string? reference = null);
    Task SendInventoryWithdrawnNotificationAsync(string itemName, int withdrawnQuantity, int newQuantity, string withdrawnBy, DateTime withdrawDate, string reason);
}
