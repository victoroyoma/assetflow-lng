using buildone.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace buildone.Pages;

public class DbTestModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DbTestModel> _logger;

    public DbTestModel(ApplicationDbContext context, ILogger<DbTestModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public bool ConnectionSuccessful { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            await _context.Database.CanConnectAsync();
            ConnectionSuccessful = true;
            _logger.LogInformation("Database connection test successful");
        }
        catch (Exception ex)
        {
            ConnectionSuccessful = false;
            ErrorMessage = ex.Message;
            _logger.LogError(ex, "Database connection test failed");
        }
    }
}