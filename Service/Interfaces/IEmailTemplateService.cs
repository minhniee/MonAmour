using MonAmourDb_BE.DTOs;
using MonAmourDb_BE.Models;

namespace MonAmourDb_BE.Service.Interfaces
{
    public interface IEmailTemplateService
    {
        Task<EmailTemplate?> GetActiveTemplateAsync(string templateType, string name);
        Task<EmailTemplate?> CreateTemplateAsync(EmailTemplateDto dto);
        Task<EmailTemplate?> UpdateTemplateAsync(int templateId, EmailTemplateDto dto);
        Task<bool> DeactivateTemplateAsync(int templateId);
    }
}


