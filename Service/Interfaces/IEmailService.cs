using System.Threading.Tasks;

namespace MonAmourDb_BE.Service.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(string toEmail, string subject, string htmlBody);
    }
}


