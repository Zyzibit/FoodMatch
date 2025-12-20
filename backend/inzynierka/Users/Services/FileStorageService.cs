namespace inzynierka.Users.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileStorageService> _logger;
    private readonly long _maxFileSize = 5 * 1024 * 1024; // 5 MB
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private readonly string _uploadFolder = "uploads/profile-pictures";

    public FileStorageService(
        IWebHostEnvironment environment,
        ILogger<FileStorageService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public bool IsValidImageFile(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return false;
        }

        if (file.Length > _maxFileSize)
        {
            _logger.LogWarning("File size exceeds maximum allowed size: {Size}", file.Length);
            return false;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            _logger.LogWarning("File extension not allowed: {Extension}", extension);
            return false;
        }

        return true;
    }

    public async Task<string> SaveProfilePictureAsync(IFormFile file, string userId)
    {
        try
        {
            if (!IsValidImageFile(file))
            {
                throw new ArgumentException("Invalid file");
            }

            var uploadPath = Path.Combine(_environment.ContentRootPath, _uploadFolder);
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Generate unique filename
            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{userId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative URL
            var relativeUrl = $"/{_uploadFolder}/{fileName}";
            _logger.LogInformation("Profile picture saved successfully: {Url}", relativeUrl);
            
            return relativeUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving profile picture for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> DeleteProfilePictureAsync(string fileUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(fileUrl))
            {
                return false;
            }

            // Remove leading slash if present
            var relativePath = fileUrl.TrimStart('/');
            var filePath = Path.Combine(_environment.ContentRootPath, relativePath);

            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
                _logger.LogInformation("Profile picture deleted successfully: {Path}", filePath);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting profile picture: {Url}", fileUrl);
            return false;
        }
    }
}

