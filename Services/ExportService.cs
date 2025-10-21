using System.Text;
using buildone.Data;
using buildone.Data.Enums;

namespace buildone.Services;

public interface IExportService
{
    Task<byte[]> ExportJobsToCsvAsync(IEnumerable<ImagingJob> jobs);
    string GenerateJobQueueReport(IEnumerable<ImagingJob> jobs, DateTime? startDate = null, DateTime? endDate = null);
}

public class ExportService : IExportService
{
    private readonly ILogger<ExportService> _logger;

    public ExportService(ILogger<ExportService> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> ExportJobsToCsvAsync(IEnumerable<ImagingJob> jobs)
    {
        try
        {
            var csv = new StringBuilder();
            
            // CSV Header
            csv.AppendLine("Job ID,Asset Tag,PC ID,Status,Priority,Imaging Type,Technician,Created Date,Scheduled At,Started At,Completed At,Duration,Notes");

            // CSV Rows
            foreach (var job in jobs)
            {
                var duration = job.Duration?.ToString(@"hh\:mm") ?? "";
                var technician = job.Technician?.FullName ?? "Unassigned";
                var notes = job.Notes?.Replace("\"", "\"\"") ?? ""; // Escape quotes

                csv.AppendLine($"\"{job.Id}\"," +
                             $"\"{job.Asset?.AssetTag ?? ""}\"," +
                             $"\"{job.Asset?.PcId ?? ""}\"," +
                             $"\"{job.Status}\"," +
                             $"\"{job.Priority}\"," +
                             $"\"{job.ImagingType}\"," +
                             $"\"{technician}\"," +
                             $"\"{job.CreatedAt:yyyy-MM-dd HH:mm}\"," +
                             $"\"{job.ScheduledAt?.ToString("yyyy-MM-dd HH:mm") ?? ""}\"," +
                             $"\"{job.StartedAt?.ToString("yyyy-MM-dd HH:mm") ?? ""}\"," +
                             $"\"{job.CompletedAt?.ToString("yyyy-MM-dd HH:mm") ?? ""}\"," +
                             $"\"{duration}\"," +
                             $"\"{notes}\"");
            }

            _logger.LogInformation("Exported {Count} jobs to CSV", jobs.Count());
            return await Task.FromResult(Encoding.UTF8.GetBytes(csv.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting jobs to CSV");
            throw;
        }
    }

    public string GenerateJobQueueReport(IEnumerable<ImagingJob> jobs, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var jobsList = jobs.ToList();
            var report = new StringBuilder();

            report.AppendLine("═══════════════════════════════════════════");
            report.AppendLine("         JOB QUEUE REPORT");
            report.AppendLine("═══════════════════════════════════════════");
            report.AppendLine();
            report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
            
            if (startDate.HasValue || endDate.HasValue)
            {
                report.AppendLine($"Period: {startDate?.ToString("yyyy-MM-dd") ?? "Start"} to {endDate?.ToString("yyyy-MM-dd") ?? "End"}");
            }
            
            report.AppendLine();
            report.AppendLine("───────────────────────────────────────────");
            report.AppendLine("SUMMARY STATISTICS");
            report.AppendLine("───────────────────────────────────────────");
            report.AppendLine($"Total Jobs: {jobsList.Count}");
            report.AppendLine($"Pending: {jobsList.Count(j => j.Status == JobStatus.Pending)}");
            report.AppendLine($"In Progress: {jobsList.Count(j => j.Status == JobStatus.InProgress)}");
            report.AppendLine($"Completed: {jobsList.Count(j => j.Status == JobStatus.Completed)}");
            report.AppendLine($"Failed: {jobsList.Count(j => j.Status == JobStatus.Failed)}");
            report.AppendLine($"Cancelled: {jobsList.Count(j => j.Status == JobStatus.Cancelled)}");
            report.AppendLine();

            // Priority breakdown
            report.AppendLine("───────────────────────────────────────────");
            report.AppendLine("BY PRIORITY");
            report.AppendLine("───────────────────────────────────────────");
            report.AppendLine($"Urgent: {jobsList.Count(j => j.Priority == JobPriority.Urgent)}");
            report.AppendLine($"High: {jobsList.Count(j => j.Priority == JobPriority.High)}");
            report.AppendLine($"Normal: {jobsList.Count(j => j.Priority == JobPriority.Normal)}");
            report.AppendLine($"Low: {jobsList.Count(j => j.Priority == JobPriority.Low)}");
            report.AppendLine();

            // Imaging type breakdown
            report.AppendLine("───────────────────────────────────────────");
            report.AppendLine("BY IMAGING TYPE");
            report.AppendLine("───────────────────────────────────────────");
            foreach (var type in Enum.GetValues<ImagingType>())
            {
                if (type == ImagingType.None) continue;
                report.AppendLine($"{type}: {jobsList.Count(j => j.ImagingType == type)}");
            }
            report.AppendLine();

            // Average completion time
            var completedJobs = jobsList.Where(j => j.Duration.HasValue).ToList();
            if (completedJobs.Any())
            {
                var avgDuration = TimeSpan.FromMinutes(completedJobs.Average(j => j.Duration!.Value.TotalMinutes));
                report.AppendLine("───────────────────────────────────────────");
                report.AppendLine("PERFORMANCE METRICS");
                report.AppendLine("───────────────────────────────────────────");
                report.AppendLine($"Average Completion Time: {avgDuration:hh\\:mm}");
                report.AppendLine($"Fastest Job: {completedJobs.Min(j => j.Duration):hh\\:mm}");
                report.AppendLine($"Slowest Job: {completedJobs.Max(j => j.Duration):hh\\:mm}");
                report.AppendLine();
            }

            // Top technicians
            var technicianStats = jobsList
                .Where(j => j.Technician != null)
                .GroupBy(j => j.Technician!.FullName)
                .Select(g => new { Technician = g.Key, Count = g.Count(), Completed = g.Count(j => j.Status == JobStatus.Completed) })
                .OrderByDescending(t => t.Completed)
                .Take(5);

            if (technicianStats.Any())
            {
                report.AppendLine("───────────────────────────────────────────");
                report.AppendLine("TOP TECHNICIANS");
                report.AppendLine("───────────────────────────────────────────");
                foreach (var tech in technicianStats)
                {
                    report.AppendLine($"{tech.Technician}: {tech.Completed}/{tech.Count} completed");
                }
                report.AppendLine();
            }

            report.AppendLine("═══════════════════════════════════════════");

            _logger.LogInformation("Generated job queue report with {Count} jobs", jobsList.Count);
            return report.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating job queue report");
            throw;
        }
    }
}
