using Microsoft.AspNetCore.Http;

namespace MonAmour.Services.Interfaces
{
    public interface IFileUploadService
    {
        Task<string?> UploadBannerImageAsync(IFormFile file, string bannerType);
        Task<bool> DeleteBannerImageAsync(string imagePath);
        Task<string?> UpdateBannerImageAsync(IFormFile file, string oldImagePath, string bannerType);
        Task<string?> UploadBlogImageAsync(IFormFile file);
        Task<bool> DeleteBlogImageAsync(string imagePath);
        Task<string?> UpdateBlogImageAsync(IFormFile file, string oldImagePath);
        bool IsValidImageFile(IFormFile file);
        string GetBannerImagePath(string bannerType);
        string GetBlogImagePath();
    }
}
