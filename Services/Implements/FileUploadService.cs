using Microsoft.AspNetCore.Http;
using MonAmour.Services.Interfaces;

namespace MonAmour.Services.Implements
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileUploadService> _logger;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

        public FileUploadService(IWebHostEnvironment environment, ILogger<FileUploadService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string?> UploadBannerImageAsync(IFormFile file, string bannerType)
        {
            try
            {
                if (!IsValidImageFile(file))
                {
                    return null;
                }

                var uploadPath = GetBannerImagePath(bannerType);
                var fileName = GenerateUniqueFileName(file.FileName);
                var filePath = Path.Combine(uploadPath, fileName);

                // Ensure directory exists
                Directory.CreateDirectory(uploadPath);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var relativePath = $"/Imagine/banner/{bannerType}/{fileName}";
                _logger.LogInformation("Banner image uploaded successfully: {Path}", relativePath);
                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading banner image");
                return null;
            }
        }

        public async Task<bool> DeleteBannerImageAsync(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                    return true;

                var fullPath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/'));
                
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("Banner image deleted: {Path}", imagePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting banner image: {Path}", imagePath);
                return false;
            }
        }

        public async Task<string?> UpdateBannerImageAsync(IFormFile file, string oldImagePath, string bannerType)
        {
            try
            {
                // Delete old image first
                await DeleteBannerImageAsync(oldImagePath);

                // Upload new image
                return await UploadBannerImageAsync(file, bannerType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating banner image");
                return null;
            }
        }

        public bool IsValidImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            if (file.Length > _maxFileSize)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return _allowedExtensions.Contains(extension);
        }

        public string GetBannerImagePath(string bannerType)
        {
            return Path.Combine(_environment.WebRootPath, "Imagine", "banner", bannerType.ToLower());
        }

        public async Task<string?> UploadBlogImageAsync(IFormFile file)
        {
            try
            {
                if (!IsValidImageFile(file))
                {
                    return null;
                }

                var uploadPath = GetBlogImagePath();
                var fileName = GenerateUniqueFileName(file.FileName);
                var filePath = Path.Combine(uploadPath, fileName);

                // Ensure directory exists
                Directory.CreateDirectory(uploadPath);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var relativePath = $"/Imagine/blog/{fileName}";
                _logger.LogInformation("Blog image uploaded successfully: {Path}", relativePath);
                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading blog image");
                return null;
            }
        }

        public async Task<bool> DeleteBlogImageAsync(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                    return true;

                var fullPath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/'));
                
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("Blog image deleted: {Path}", imagePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting blog image: {Path}", imagePath);
                return false;
            }
        }

        public async Task<string?> UpdateBlogImageAsync(IFormFile file, string oldImagePath)
        {
            try
            {
                // Delete old image first
                await DeleteBlogImageAsync(oldImagePath);

                // Upload new image
                return await UploadBlogImageAsync(file);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating blog image");
                return null;
            }
        }

        public string GetBlogImagePath()
        {
            return Path.Combine(_environment.WebRootPath, "Imagine", "blog");
        }

        private string GenerateUniqueFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var fileName = Path.GetFileNameWithoutExtension(originalFileName);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var random = Guid.NewGuid().ToString("N")[..8];
            
            return $"{fileName}_{timestamp}_{random}{extension}";
        }
    }
}
