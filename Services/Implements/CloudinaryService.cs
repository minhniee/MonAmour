using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MonAmour.Services.Interfaces;
using MonAmour.Util;

namespace MonAmour.Services.Implements
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly CloudinarySettings _settings;
        private readonly ILogger<CloudinaryService> _logger;

        public CloudinaryService(IOptions<CloudinarySettings> settings, ILogger<CloudinaryService> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            var account = new Account(
                _settings.CloudName,
                _settings.ApiKey,
                _settings.ApiSecret
            );

            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = _settings.UseSecureUrl;
        }

        public async Task<string?> UploadImageAsync(IFormFile file, string? folder = null, string? publicId = null)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("File is null or empty");
                    return null;
                }

                // Validate file size
                if (file.Length > _settings.MaxFileSize)
                {
                    _logger.LogWarning("File size {FileSize} exceeds maximum {MaxSize}", file.Length, _settings.MaxFileSize);
                    return null;
                }

                // Validate file format
                var fileExtension = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
                if (!_settings.AllowedFormats.Contains(fileExtension))
                {
                    _logger.LogWarning("File format {Format} is not allowed", fileExtension);
                    return null;
                }

                using var stream = file.OpenReadStream();
                return await UploadImageStreamAsync(stream, file.FileName, folder, publicId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image file: {FileName}", file?.FileName);
                return null;
            }
        }

        public async Task<string?> UploadImageAsync(byte[] imageBytes, string fileName, string? folder = null, string? publicId = null)
        {
            try
            {
                if (imageBytes == null || imageBytes.Length == 0)
                {
                    _logger.LogWarning("Image bytes is null or empty");
                    return null;
                }

                // Validate file size
                if (imageBytes.Length > _settings.MaxFileSize)
                {
                    _logger.LogWarning("Image size {ImageSize} exceeds maximum {MaxSize}", imageBytes.Length, _settings.MaxFileSize);
                    return null;
                }

                using var stream = new MemoryStream(imageBytes);
                return await UploadImageStreamAsync(stream, fileName, folder, publicId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image bytes: {FileName}", fileName);
                return null;
            }
        }

        private async Task<string?> UploadImageStreamAsync(Stream stream, string fileName, string? folder = null, string? publicId = null)
        {
            try
            {
                var folderPath = folder ?? _settings.DefaultFolder;
                var finalPublicId = publicId ?? $"{folderPath}/{Guid.NewGuid()}";

                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(fileName, stream),
                    PublicId = finalPublicId,
                    Folder = folderPath,
                    Transformation = new Transformation()
                        .Width(_settings.MaxWidth)
                        .Height(_settings.MaxHeight)
                        .Crop("limit")
                        .Quality(_settings.Quality)
                        .FetchFormat("auto"),
                    Overwrite = true,
                    UseFilename = false,
                    UniqueFilename = true
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    _logger.LogInformation("Image uploaded successfully: {PublicId}, URL: {Url}", uploadResult.PublicId, uploadResult.SecureUrl);
                    return uploadResult.SecureUrl?.ToString();
                }
                else
                {
                    _logger.LogError("Failed to upload image: {Error}", uploadResult.Error?.Message);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image stream: {FileName}", fileName);
                return null;
            }
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(publicId))
                {
                    _logger.LogWarning("Public ID is null or empty");
                    return false;
                }

                var deleteParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deleteParams);

                if (result.StatusCode == System.Net.HttpStatusCode.OK && result.Result == "ok")
                {
                    _logger.LogInformation("Image deleted successfully: {PublicId}", publicId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to delete image: {PublicId}, Result: {Result}", publicId, result.Result);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image: {PublicId}", publicId);
                return false;
            }
        }

        public string GetOptimizedImageUrl(string publicId, int? width = null, int? height = null, string? quality = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(publicId))
                {
                    return string.Empty;
                }

                var transformation = new Transformation();

                if (width.HasValue)
                    transformation = transformation.Width(width.Value);

                if (height.HasValue)
                    transformation = transformation.Height(height.Value);

                if (width.HasValue || height.HasValue)
                    transformation = transformation.Crop("limit");

                transformation = transformation
                    .Quality(quality ?? _settings.Quality)
                    .FetchFormat("auto");

                var url = _cloudinary.Api.UrlImgUp.Transform(transformation).BuildUrl(publicId);
                return url ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating optimized URL for: {PublicId}", publicId);
                return string.Empty;
            }
        }

        public string? ExtractPublicIdFromUrl(string cloudinaryUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cloudinaryUrl))
                    return null;

                // Check if it's a valid Cloudinary URL
                if (!cloudinaryUrl.Contains("cloudinary.com"))
                {
                    _logger.LogWarning("URL is not a Cloudinary URL, skipping deletion: {Url}", cloudinaryUrl);
                    return null;
                }

                // Example URL: https://res.cloudinary.com/demo/image/upload/v1234567890/sample.jpg
                // Extract: sample (without extension)
                
                var uri = new Uri(cloudinaryUrl);
                var pathSegments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                
                // Find the segment after 'upload'
                var uploadIndex = Array.IndexOf(pathSegments, "upload");
                if (uploadIndex >= 0 && uploadIndex < pathSegments.Length - 1)
                {
                    var publicIdWithExtension = pathSegments[uploadIndex + 1];
                    
                    // Skip version if present (starts with 'v' followed by numbers)
                    if (publicIdWithExtension.StartsWith("v") && publicIdWithExtension.Length > 1 && 
                        publicIdWithExtension.Substring(1).All(char.IsDigit))
                    {
                        if (uploadIndex < pathSegments.Length - 2)
                        {
                            publicIdWithExtension = pathSegments[uploadIndex + 2];
                        }
                    }
                    
                    // Remove file extension
                    var lastDotIndex = publicIdWithExtension.LastIndexOf('.');
                    if (lastDotIndex > 0)
                    {
                        return publicIdWithExtension.Substring(0, lastDotIndex);
                    }
                    
                    return publicIdWithExtension;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting public ID from URL: {Url}", cloudinaryUrl);
                return null;
            }
        }
    }
}
