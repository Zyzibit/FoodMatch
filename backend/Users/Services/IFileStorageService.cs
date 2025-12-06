namespace inzynierka.Users.Services;

public interface IFileStorageService
{
    Task<string> SaveProfilePictureAsync(IFormFile file, string userId);
    Task<bool> DeleteProfilePictureAsync(string fileUrl);
    bool IsValidImageFile(IFormFile? file);
}

