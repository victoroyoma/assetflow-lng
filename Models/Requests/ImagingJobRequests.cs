using buildone.Data.Enums;

namespace buildone.Models.Requests
{
    public class CreateImagingJobRequest
    {
        public int AssetId { get; set; }
        public int? TechnicianId { get; set; }
        public ImagingType ImagingType { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public string? ImageVersion { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateImagingJobRequest
    {
        public JobStatus? Status { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? Notes { get; set; }
        public string? ImageVersion { get; set; }
    }
}