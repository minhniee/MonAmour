namespace MonAmour.Util
{
    public class CloudinarySettings
    {
        public string CloudName { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://res.cloudinary.com";
        public bool UseSecureUrl { get; set; } = true;
        
        // Upload settings
        public string DefaultFolder { get; set; } = "monamour";
        public int MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB
        public string[] AllowedFormats { get; set; } = { "jpg", "jpeg", "png", "gif", "webp" };
        
        // Transformation settings
        public int MaxWidth { get; set; } = 1920;
        public int MaxHeight { get; set; } = 1080;
        public string Quality { get; set; } = "auto:good";
    }
}
