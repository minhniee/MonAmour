using MonAmour.Models;

namespace MonAmour.Services.Interfaces
{
    public interface IVietQRService
    {
        Task<string> GenerateQRCodeAsync(decimal amount, string content, string template = "compact");
        string GetQRCodeUrl(decimal amount, string content, string template = "compact");
    }
}
