namespace MonAmour.Models;

public class AppSettings
{
    public string AppName { get; set; } = "MonAmour";
    public string AppUrl { get; set; } = "";
    public string SupportEmail { get; set; } = "";
    public bool EnableEmailVerification { get; set; } = true;
    public int SessionTimeoutMinutes { get; set; } = 30;
    public bool EnableRememberMe { get; set; } = true;
    public int RememberMeDays { get; set; } = 30;

    public string GetFullUrl()
    {
        return AppUrl;
    }
}
