using buildone.Data;
using Microsoft.EntityFrameworkCore;

namespace buildone.Services;

public interface IJobCommentService
{
    Task<IEnumerable<JobComment>> GetCommentsByJobIdAsync(int jobId);
    Task<JobComment> AddCommentAsync(int jobId, int employeeId, string comment, bool isSystemGenerated = false);
    Task<bool> DeleteCommentAsync(int commentId);
}

public class JobCommentService : IJobCommentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<JobCommentService> _logger;

    public JobCommentService(ApplicationDbContext context, ILogger<JobCommentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<JobComment>> GetCommentsByJobIdAsync(int jobId)
    {
        try
        {
            return await _context.JobComments
                .Include(c => c.Employee)
                .Where(c => c.ImagingJobId == jobId)
                .OrderByDescending(c => c.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving comments for job {JobId}", jobId);
            return Enumerable.Empty<JobComment>();
        }
    }

    public async Task<JobComment> AddCommentAsync(int jobId, int employeeId, string comment, bool isSystemGenerated = false)
    {
        try
        {
            var jobComment = new JobComment
            {
                ImagingJobId = jobId,
                EmployeeId = employeeId,
                Comment = comment,
                IsSystemGenerated = isSystemGenerated,
                CreatedAt = DateTime.UtcNow
            };

            _context.JobComments.Add(jobComment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Comment added to job {JobId} by employee {EmployeeId}", jobId, employeeId);
            return jobComment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment to job {JobId}", jobId);
            throw;
        }
    }

    public async Task<bool> DeleteCommentAsync(int commentId)
    {
        try
        {
            var comment = await _context.JobComments.FindAsync(commentId);
            if (comment == null)
            {
                _logger.LogWarning("Comment {CommentId} not found", commentId);
                return false;
            }

            // Don't allow deletion of system-generated comments
            if (comment.IsSystemGenerated)
            {
                _logger.LogWarning("Attempted to delete system-generated comment {CommentId}", commentId);
                return false;
            }

            _context.JobComments.Remove(comment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Comment {CommentId} deleted", commentId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comment {CommentId}", commentId);
            return false;
        }
    }
}
