using buildone.Data;
using buildone.Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace buildone.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        ApplicationDbContext context,
        ILogger<NotificationsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets recent notifications including warranty alerts, job updates, and system events
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications()
    {
        try
        {
            var notifications = new List<object>();
            
            // 1. Warranty Expiring Soon (within 30 days)
            var expiringWarranties = await _context.Assets
                .Where(a => a.WarrantyExpiry.HasValue && 
                           a.WarrantyExpiry.Value <= DateTime.UtcNow.AddDays(30) &&
                           a.WarrantyExpiry.Value > DateTime.UtcNow)
                .CountAsync();

            if (expiringWarranties > 0)
            {
                notifications.Add(new
                {
                    id = "warranty-alert",
                    type = "warning",
                    icon = "fa-exclamation-triangle",
                    iconColor = "warning",
                    title = "Warranty Expiring",
                    message = $"{expiringWarranties} asset{(expiringWarranties > 1 ? "s" : "")} expire within 30 days",
                    time = "System Alert",
                    url = "/Reports/WarrantyAlerts",
                    isRead = false
                });
            }

            // 2. Failed Imaging Jobs (last 7 days)
            var failedJobs = await _context.ImagingJobs
                .Where(j => j.Status == JobStatus.Failed && 
                           j.CreatedAt >= DateTime.UtcNow.AddDays(-7))
                .OrderByDescending(j => j.CreatedAt)
                .Take(3)
                .Include(j => j.Asset)
                .ToListAsync();

            foreach (var job in failedJobs)
            {
                var timeAgo = GetTimeAgo(job.CreatedAt);
                notifications.Add(new
                {
                    id = $"failed-job-{job.Id}",
                    type = "error",
                    icon = "fa-times-circle",
                    iconColor = "danger",
                    title = "Imaging Job Failed",
                    message = $"{job.Asset?.AssetTag ?? "Unknown Asset"} - {job.ImageVersion}",
                    time = timeAgo,
                    url = $"/Imaging/Details/{job.Id}",
                    isRead = false
                });
            }

            // 3. Recently Completed Jobs (last 24 hours)
            var recentCompletedJobs = await _context.ImagingJobs
                .Where(j => j.Status == JobStatus.Completed && 
                           j.CompletedAt.HasValue &&
                           j.CompletedAt.Value >= DateTime.UtcNow.AddHours(-24))
                .OrderByDescending(j => j.CompletedAt)
                .Take(2)
                .Include(j => j.Asset)
                .ToListAsync();

            foreach (var job in recentCompletedJobs)
            {
                var timeAgo = GetTimeAgo(job.CompletedAt ?? job.CreatedAt);
                notifications.Add(new
                {
                    id = $"completed-job-{job.Id}",
                    type = "success",
                    icon = "fa-check-circle",
                    iconColor = "success",
                    title = "Imaging Job Complete",
                    message = $"{job.Asset?.AssetTag ?? "Unknown Asset"} imaging successful",
                    time = timeAgo,
                    url = $"/Imaging/Details/{job.Id}",
                    isRead = true
                });
            }

            // 4. Jobs In Progress
            var inProgressJobs = await _context.ImagingJobs
                .Where(j => j.Status == JobStatus.InProgress)
                .CountAsync();

            if (inProgressJobs > 0)
            {
                notifications.Add(new
                {
                    id = "in-progress-jobs",
                    type = "info",
                    icon = "fa-spinner",
                    iconColor = "info",
                    title = "Jobs In Progress",
                    message = $"{inProgressJobs} imaging job{(inProgressJobs > 1 ? "s" : "")} currently running",
                    time = "System Status",
                    url = "/Imaging/Index",
                    isRead = true
                });
            }

            // 5. Recently Assigned Assets (last 7 days)
            var recentlyAssigned = await _context.Assets
                .Where(a => a.Status == AssetStatus.Assigned && 
                           a.UpdatedAt.HasValue &&
                           a.UpdatedAt.Value >= DateTime.UtcNow.AddDays(-7))
                .OrderByDescending(a => a.UpdatedAt)
                .Take(2)
                .Include(a => a.AssignedEmployee)
                .ToListAsync();

            foreach (var asset in recentlyAssigned)
            {
                var timeAgo = GetTimeAgo(asset.UpdatedAt ?? asset.CreatedAt);
                notifications.Add(new
                {
                    id = $"assigned-asset-{asset.Id}",
                    type = "success",
                    icon = "fa-user-check",
                    iconColor = "success",
                    title = "Asset Deployed",
                    message = $"{asset.AssetTag} assigned to {asset.AssignedEmployee?.FullName ?? "employee"}",
                    time = timeAgo,
                    url = $"/Assets/Details/{asset.Id}",
                    isRead = true
                });
            }

            // Calculate unread count
            var unreadCount = notifications.Count(n => 
            {
                var notif = n as dynamic;
                return notif != null && !(bool)((dynamic)n).GetType()
                    .GetProperty("isRead")?.GetValue(n) ?? true;
            });

            // Count unread properly
            unreadCount = notifications
                .Select(n => n.GetType().GetProperty("isRead")?.GetValue(n))
                .Count(isRead => isRead != null && !(bool)isRead);

            return Ok(new
            {
                unreadCount,
                totalCount = notifications.Count,
                notifications = notifications.Take(10).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading notifications");
            return Ok(new
            {
                unreadCount = 0,
                totalCount = 0,
                notifications = new List<object>()
            });
        }
    }

    private string GetTimeAgo(DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime;

        if (timeSpan.TotalMinutes < 1)
            return "Just now";
        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes} minute{((int)timeSpan.TotalMinutes != 1 ? "s" : "")} ago";
        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours} hour{((int)timeSpan.TotalHours != 1 ? "s" : "")} ago";
        if (timeSpan.TotalDays < 7)
            return $"{(int)timeSpan.TotalDays} day{((int)timeSpan.TotalDays != 1 ? "s" : "")} ago";
        
        return dateTime.ToString("MMM dd, yyyy");
    }
}