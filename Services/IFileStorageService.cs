namespace buildone.Services;

public interface IFileStorageService
{
    /// <summary>
    /// Saves an uploaded file to storage
    /// </summary>
    /// <param name="file">The uploaded file</param>
    /// <param name="folder">Folder path (e.g., "jobs", "assets")</param>
    /// <returns>Relative file path</returns>
    Task<string> SaveFileAsync(IFormFile file, string folder);

    /// <summary>
    /// Deletes a file from storage
    /// </summary>
    /// <param name="filePath">Relative file path</param>
    Task DeleteFileAsync(string filePath);

    /// <summary>
    /// Gets the physical path for a file
    /// </summary>
    /// <param name="filePath">Relative file path</param>
    /// <returns>Full physical path</returns>
    string GetPhysicalPath(string filePath);

    /// <summary>
    /// Checks if a file exists
    /// </summary>
    /// <param name="filePath">Relative file path</param>
    Task<bool> FileExistsAsync(string filePath);

    /// <summary>
    /// Gets file stream for download
    /// </summary>
    /// <param name="filePath">Relative file path</param>
    Task<Stream> GetFileStreamAsync(string filePath);
}
