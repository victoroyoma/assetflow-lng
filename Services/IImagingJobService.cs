using buildone.Data;
using buildone.Data.Enums;

namespace buildone.Services;

public interface IImagingJobService
{
    Task<IEnumerable<ImagingJob>> GetAllImagingJobsAsync();
    Task<ImagingJob?> GetImagingJobByIdAsync(int id);
    Task<ImagingJob> CreateImagingJobAsync(ImagingJob imagingJob);
    Task<bool> UpdateImagingJobAsync(ImagingJob imagingJob);
    Task<bool> DeleteImagingJobAsync(int id);
    Task<bool> ImagingJobExistsAsync(int id);
    Task<IEnumerable<ImagingJob>> GetImagingJobsByAssetAsync(int assetId);
    Task<IEnumerable<ImagingJob>> GetImagingJobsByTechnicianAsync(int technicianId);
    Task<IEnumerable<ImagingJob>> GetImagingJobsByStatusAsync(JobStatus status);
    Task<IEnumerable<ImagingJob>> GetPendingImagingJobsAsync();
    Task<IEnumerable<ImagingJob>> GetInProgressImagingJobsAsync();
    Task<IEnumerable<ImagingJob>> GetOverdueImagingJobsAsync();
    Task<IEnumerable<ImagingJob>> SearchImagingJobsAsync(string searchTerm);
    Task<bool> StartImagingJobAsync(int jobId, int technicianId);
    Task<bool> CompleteImagingJobAsync(int jobId, string? notes = null);
    Task<bool> FailImagingJobAsync(int jobId, string? notes = null);
    Task<bool> AssignTechnicianAsync(int jobId, int technicianId);
    Task<ImagingJob?> GetActiveJobForAssetAsync(int assetId);
    Task<IEnumerable<int>> GetAssetsWithActiveJobsAsync();
    Task<IEnumerable<object>> GetAllJobsAsync();
}