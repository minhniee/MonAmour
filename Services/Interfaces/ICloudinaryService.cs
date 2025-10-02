using Microsoft.AspNetCore.Http;

namespace MonAmour.Services.Interfaces
{
    public interface ICloudinaryService
    {
        /// <summary>
        /// Upload image to Cloudinary
        /// </summary>
        /// <param name="file">Image file to upload</param>
        /// <param name="folder">Folder name (optional)</param>
        /// <param name="publicId">Custom public ID (optional)</param>
        /// <returns>Cloudinary URL of uploaded image</returns>
        Task<string?> UploadImageAsync(IFormFile file, string? folder = null, string? publicId = null);
        
        /// <summary>
        /// Upload image from byte array
        /// </summary>
        /// <param name="imageBytes">Image byte array</param>
        /// <param name="fileName">Original file name</param>
        /// <param name="folder">Folder name (optional)</param>
        /// <param name="publicId">Custom public ID (optional)</param>
        /// <returns>Cloudinary URL of uploaded image</returns>
        Task<string?> UploadImageAsync(byte[] imageBytes, string fileName, string? folder = null, string? publicId = null);
        
        /// <summary>
        /// Delete image from Cloudinary
        /// </summary>
        /// <param name="publicId">Public ID of image to delete</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteImageAsync(string publicId);
        
        /// <summary>
        /// Get optimized image URL with transformations
        /// </summary>
        /// <param name="publicId">Public ID of image</param>
        /// <param name="width">Width (optional)</param>
        /// <param name="height">Height (optional)</param>
        /// <param name="quality">Quality (optional)</param>
        /// <returns>Optimized image URL</returns>
        string GetOptimizedImageUrl(string publicId, int? width = null, int? height = null, string? quality = null);
        
        /// <summary>
        /// Extract public ID from Cloudinary URL
        /// </summary>
        /// <param name="cloudinaryUrl">Full Cloudinary URL</param>
        /// <returns>Public ID</returns>
        string? ExtractPublicIdFromUrl(string cloudinaryUrl);
    }
}
