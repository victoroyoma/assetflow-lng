namespace buildone.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileStorageService> _logger;
    private const string UploadsFolder = "uploads";

    public FileStorageService(IWebHostEnvironment environment, ILogger<FileStorageService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> SaveFileAsync(IFormFile file, string folder)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty or null", nameof(file));

        // Create uploads directory structure
        var uploadsPath = Path.Combine(_environment.WebRootPath, UploadsFolder, folder);
        Directory.CreateDirectory(uploadsPath);

        // Generate unique filename to avoid conflicts
        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var filePath = Path.Combine(uploadsPath, fileName);

        try
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path for database storage
            var relativePath = Path.Combine(UploadsFolder, folder, fileName).Replace("\\", "/");
            _logger.LogInformation("File saved: {FilePath}", relativePath);
            
            return relativePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file: {FileName}", file.FileName);
            throw;
        }
    }

    public async Task DeleteFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return;

        try
        {
            var physicalPath = GetPhysicalPath(filePath);
            
            if (File.Exists(physicalPath))
            {
                await Task.Run(() => File.Delete(physicalPath));
                _logger.LogInformation("File deleted: {FilePath}", filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            // Don't throw - file might already be deleted or not exist
        }
    }

    public string GetPhysicalPath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be empty", nameof(filePath));

        // Normalize path separators
        filePath = filePath.Replace("/", Path.DirectorySeparatorChar.ToString());
        
        return Path.Combine(_environment.WebRootPath, filePath);
    }

    public async Task<bool> FileExistsAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        try
        {
            var physicalPath = GetPhysicalPath(filePath);
            return await Task.Run(() => File.Exists(physicalPath));
        }
        catch
        {
            return false;
        }
    }

    public async Task<Stream> GetFileStreamAsync(string filePath)
    {
        var physicalPath = GetPhysicalPath(filePath);
        
        if (!File.Exists(physicalPath))
            throw new FileNotFoundException("File not found", filePath);

        return await Task.Run(() => (Stream)new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read));
    }
}
