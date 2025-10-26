using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using buildone.Data;

namespace buildone.Pages.Audit;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<AssetAudit> Audits { get; set; } = new();
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? AuditedBy { get; set; }
    
    // Statistics
    public int TotalAudits { get; set; }
    public int NewAssetsCount { get; set; }
    public int StatusChangesCount { get; set; }
    public int LocationChangesCount { get; set; }

    public async Task OnGetAsync(DateTime? startDate, DateTime? endDate, string? auditedBy)
    {
        StartDate = startDate;
        EndDate = endDate;
        AuditedBy = auditedBy;

        var query = _context.AssetAudits.AsQueryable();

        // Apply filters
        if (startDate.HasValue)
        {
            query = query.Where(a => a.AuditDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(a => a.AuditDate <= endOfDay);
        }

        if (!string.IsNullOrWhiteSpace(auditedBy))
        {
            query = query.Where(a => a.AuditedBy.Contains(auditedBy));
        }

        Audits = await query
            .OrderByDescending(a => a.AuditDate)
            .Take(100) // Limit to most recent 100 for performance
            .ToListAsync();

        // Calculate statistics
        TotalAudits = Audits.Count;
        NewAssetsCount = Audits.Count(a => a.IsNewAsset);
        StatusChangesCount = Audits.Count(a => a.PreviousStatus != null);
        LocationChangesCount = Audits.Count(a => !string.IsNullOrEmpty(a.PreviousLocation));
    }
}
