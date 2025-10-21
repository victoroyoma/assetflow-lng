using buildone.Data;
using buildone.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace buildone.Services;

public class ImagingJobService : IImagingJobService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ImagingJobService> _logger;

    public ImagingJobService(ApplicationDbContext context, ILogger<ImagingJobService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ImagingJob>> GetAllImagingJobsAsync()
    {
        try
        {
            return await _context.ImagingJobs
                .Include(ij => ij.Asset)
                .Include(ij => ij.Technician)
                .OrderByDescending(ij => ij.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all imaging jobs");
            throw;
        }
    }

    public async Task<ImagingJob?> GetImagingJobByIdAsync(int id)
    {
        try
        {
            return await _context.ImagingJobs
                .Include(ij => ij.Asset)
                .Include(ij => ij.Technician)
                .FirstOrDefaultAsync(ij => ij.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving imaging job with ID {ImagingJobId}", id);
            throw;
        }
    }

    public async Task<ImagingJob> CreateImagingJobAsync(ImagingJob imagingJob)
    {
        try
        {
            imagingJob.CreatedAt = DateTime.UtcNow;
            _context.ImagingJobs.Add(imagingJob);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Imaging job created with ID {ImagingJobId}", imagingJob.Id);
            return imagingJob;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating imaging job");
            throw;
        }
    }

    public async Task<bool> UpdateImagingJobAsync(ImagingJob imagingJob)
    {
        try
        {
            _context.Entry(imagingJob).State = EntityState.Modified;
            var result = await _context.SaveChangesAsync();
            _logger.LogInformation("Imaging job updated with ID {ImagingJobId}", imagingJob.Id);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating imaging job with ID {ImagingJobId}", imagingJob.Id);
            throw;
        }
    }

    public async Task<bool> DeleteImagingJobAsync(int id)
    {
        try
        {
            var imagingJob = await _context.ImagingJobs.FindAsync(id);
            if (imagingJob != null)
            {
                _context.ImagingJobs.Remove(imagingJob);
                var result = await _context.SaveChangesAsync();
                _logger.LogInformation("Imaging job deleted with ID {ImagingJobId}", id);
                return result > 0;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting imaging job with ID {ImagingJobId}", id);
            throw;
        }
    }

    public async Task<bool> ImagingJobExistsAsync(int id)
    {
        try
        {
            return await _context.ImagingJobs.AnyAsync(ij => ij.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if imaging job exists with ID {ImagingJobId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<ImagingJob>> GetImagingJobsByAssetAsync(int assetId)
    {
        try
        {
            return await _context.ImagingJobs
                .Include(ij => ij.Asset)
                .Include(ij => ij.Technician)
                .Where(ij => ij.AssetId == assetId)
                .OrderByDescending(ij => ij.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving imaging jobs by asset {AssetId}", assetId);
            throw;
        }
    }

    public async Task<IEnumerable<ImagingJob>> GetImagingJobsByTechnicianAsync(int technicianId)
    {
        try
        {
            return await _context.ImagingJobs
                .Include(ij => ij.Asset)
                .Include(ij => ij.Technician)
                .Where(ij => ij.TechnicianId == technicianId)
                .OrderByDescending(ij => ij.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving imaging jobs by technician {TechnicianId}", technicianId);
            throw;
        }
    }

    public async Task<IEnumerable<ImagingJob>> GetImagingJobsByStatusAsync(JobStatus status)
    {
        try
        {
            return await _context.ImagingJobs
                .Include(ij => ij.Asset)
                .Include(ij => ij.Technician)
                .Where(ij => ij.Status == status)
                .OrderByDescending(ij => ij.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving imaging jobs by status {Status}", status);
            throw;
        }
    }

    public async Task<IEnumerable<ImagingJob>> GetPendingImagingJobsAsync()
    {
        try
        {
            return await _context.ImagingJobs
                .Include(ij => ij.Asset)
                .Include(ij => ij.Technician)
                .Where(ij => ij.Status == JobStatus.Pending)
                .OrderBy(ij => ij.ScheduledAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending imaging jobs");
            throw;
        }
    }

    public async Task<IEnumerable<ImagingJob>> GetInProgressImagingJobsAsync()
    {
        try
        {
            return await _context.ImagingJobs
                .Include(ij => ij.Asset)
                .Include(ij => ij.Technician)
                .Where(ij => ij.Status == JobStatus.InProgress)
                .OrderBy(ij => ij.StartedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving in-progress imaging jobs");
            throw;
        }
    }

    public async Task<IEnumerable<ImagingJob>> GetOverdueImagingJobsAsync()
    {
        try
        {
            var now = DateTime.UtcNow;
            return await _context.ImagingJobs
                .Include(ij => ij.Asset)
                .Include(ij => ij.Technician)
                .Where(ij => ij.Status == JobStatus.Pending && 
                            ij.ScheduledAt.HasValue && 
                            ij.ScheduledAt.Value < now)
                .OrderBy(ij => ij.ScheduledAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue imaging jobs");
            throw;
        }
    }

    public async Task<IEnumerable<ImagingJob>> SearchImagingJobsAsync(string searchTerm)
    {
        try
        {
            return await _context.ImagingJobs
                .Include(ij => ij.Asset)
                .Include(ij => ij.Technician)
                .Where(ij => (ij.Asset != null && ij.Asset.AssetTag.Contains(searchTerm)) ||
                            (ij.ImageVersion != null && ij.ImageVersion.Contains(searchTerm)) ||
                            (ij.Technician != null && ij.Technician.FullName.Contains(searchTerm)) ||
                            (ij.Notes != null && ij.Notes.Contains(searchTerm)))
                .OrderByDescending(ij => ij.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching imaging jobs with term {SearchTerm}", searchTerm);
            throw;
        }
    }

    public async Task<bool> StartImagingJobAsync(int jobId, int technicianId)
    {
        try
        {
            var job = await _context.ImagingJobs.FindAsync(jobId);
            if (job == null || job.Status != JobStatus.Pending) return false;

            job.Status = JobStatus.InProgress;
            job.TechnicianId = technicianId;
            job.StartedAt = DateTime.UtcNow;

            var result = await _context.SaveChangesAsync();
            _logger.LogInformation("Imaging job {ImagingJobId} started by technician {TechnicianId}", jobId, technicianId);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting imaging job {ImagingJobId}", jobId);
            throw;
        }
    }

    public async Task<bool> CompleteImagingJobAsync(int jobId, string? notes = null)
    {
        try
        {
            var job = await _context.ImagingJobs.FindAsync(jobId);
            if (job == null || job.Status != JobStatus.InProgress) return false;

            job.Status = JobStatus.Completed;
            job.CompletedAt = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(notes))
            {
                job.Notes = job.Notes != null ? $"{job.Notes}\n{notes}" : notes;
            }

            var result = await _context.SaveChangesAsync();
            _logger.LogInformation("Imaging job {ImagingJobId} completed", jobId);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing imaging job {ImagingJobId}", jobId);
            throw;
        }
    }

    public async Task<bool> FailImagingJobAsync(int jobId, string? notes = null)
    {
        try
        {
            var job = await _context.ImagingJobs.FindAsync(jobId);
            if (job == null) return false;

            job.Status = JobStatus.Failed;
            if (!string.IsNullOrEmpty(notes))
            {
                job.Notes = job.Notes != null ? $"{job.Notes}\nFAILED: {notes}" : $"FAILED: {notes}";
            }

            var result = await _context.SaveChangesAsync();
            _logger.LogInformation("Imaging job {ImagingJobId} marked as failed", jobId);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error failing imaging job {ImagingJobId}", jobId);
            throw;
        }
    }

    public async Task<bool> AssignTechnicianAsync(int jobId, int technicianId)
    {
        try
        {
            var job = await _context.ImagingJobs.FindAsync(jobId);
            if (job == null) return false;

            job.TechnicianId = technicianId;

            var result = await _context.SaveChangesAsync();
            _logger.LogInformation("Technician {TechnicianId} assigned to imaging job {ImagingJobId}", technicianId, jobId);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning technician {TechnicianId} to imaging job {ImagingJobId}", technicianId, jobId);
            throw;
        }
    }

    public async Task<ImagingJob?> GetActiveJobForAssetAsync(int assetId)
    {
        try
        {
            return await _context.ImagingJobs
                .Include(ij => ij.Asset)
                .Include(ij => ij.Technician)
                .Where(ij => ij.AssetId == assetId && 
                            (ij.Status == JobStatus.Pending || ij.Status == JobStatus.InProgress || ij.Status == JobStatus.Scheduled))
                .OrderByDescending(ij => ij.CreatedAt)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active job for asset {AssetId}", assetId);
            throw;
        }
    }

    public async Task<IEnumerable<int>> GetAssetsWithActiveJobsAsync()
    {
        try
        {
            return await _context.ImagingJobs
                .Where(ij => ij.Status == JobStatus.Pending || ij.Status == JobStatus.InProgress || ij.Status == JobStatus.Scheduled)
                .Select(ij => ij.AssetId)
                .Distinct()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assets with active jobs");
            throw;
        }
    }

    public async Task<IEnumerable<object>> GetAllJobsAsync()
    {
        try
        {
            return await _context.ImagingJobs
                .Include(ij => ij.Asset)
                .Include(ij => ij.Technician)
                .OrderByDescending(ij => ij.CreatedAt)
                .Select(ij => new
                {
                    Id = ij.Id,
                    AssetId = ij.AssetId,
                    AssetTag = ij.Asset != null ? ij.Asset.AssetTag : "",
                    TechnicianId = ij.TechnicianId,
                    TechnicianName = ij.Technician != null ? ij.Technician.FullName : "",
                    ImagingType = ij.ImagingType,
                    ImageVersion = ij.ImageVersion ?? "",
                    Status = ij.Status,
                    ScheduledAt = ij.ScheduledAt,
                    StartedAt = ij.StartedAt,
                    CompletedAt = ij.CompletedAt,
                    Notes = ij.Notes ?? "",
                    CreatedAt = ij.CreatedAt,
                    UpdatedAt = ij.UpdatedAt
                })
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all jobs as objects");
            throw;
        }
    }
}