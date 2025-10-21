using buildone.Data;
using buildone.Data.Enums;

namespace buildone.Services;

public interface INotificationService
{
    Task SendJobAssignedNotificationAsync(ImagingJob job, Employee technician);
    Task SendJobStatusChangeNotificationAsync(ImagingJob job, JobStatus oldStatus, JobStatus newStatus);
    Task SendJobOverdueNotificationAsync(ImagingJob job);
    Task SendDailySummaryAsync(Employee technician);
}

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly IJobCommentService _commentService;

    public NotificationService(ILogger<NotificationService> logger, IJobCommentService commentService)
    {
        _logger = logger;
        _commentService = commentService;
    }

    public async Task SendJobAssignedNotificationAsync(ImagingJob job, Employee technician)
    {
        try
        {
            _logger.LogInformation("Notification: Job {JobId} assigned to technician {TechnicianId}", 
                job.Id, technician.Id);

            // Add system comment
            await _commentService.AddCommentAsync(
                job.Id,
                technician.Id,
                $"Job assigned to {technician.FullName}",
                isSystemGenerated: true
            );

            // TODO: Implement actual email sending when SMTP is configured
            // await _emailService.SendEmailAsync(technician.Email, "New Job Assigned", emailBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending job assigned notification for job {JobId}", job.Id);
        }
    }

    public async Task SendJobStatusChangeNotificationAsync(ImagingJob job, JobStatus oldStatus, JobStatus newStatus)
    {
        try
        {
            _logger.LogInformation("Notification: Job {JobId} status changed from {OldStatus} to {NewStatus}", 
                job.Id, oldStatus, newStatus);

            // Add system comment if technician is assigned
            if (job.TechnicianId.HasValue)
            {
                await _commentService.AddCommentAsync(
                    job.Id,
                    job.TechnicianId.Value,
                    $"Status changed from {oldStatus} to {newStatus}",
                    isSystemGenerated: true
                );
            }

            // TODO: Send email notification
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending status change notification for job {JobId}", job.Id);
        }
    }

    public async Task SendJobOverdueNotificationAsync(ImagingJob job)
    {
        try
        {
            _logger.LogWarning("Job {JobId} is overdue", job.Id);

            // Add system comment
            if (job.TechnicianId.HasValue)
            {
                await _commentService.AddCommentAsync(
                    job.Id,
                    job.TechnicianId.Value,
                    "⚠️ This job is overdue",
                    isSystemGenerated: true
                );
            }

            // TODO: Send email notification
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending overdue notification for job {JobId}", job.Id);
        }
    }

    public async Task SendDailySummaryAsync(Employee technician)
    {
        try
        {
            _logger.LogInformation("Sending daily summary to technician {TechnicianId}", technician.Id);
            
            // TODO: Generate and send daily summary email
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending daily summary to technician {TechnicianId}", technician.Id);
        }
    }
}
