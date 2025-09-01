using Microsoft.EntityFrameworkCore;
using MonAmourDb_BE.DTOs;
using MonAmourDb_BE.Models;
using MonAmourDb_BE.Service.Interfaces;

namespace MonAmourDb_BE.Service.Implementations
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly MonAmourDbContext _db;

        public EmailTemplateService(MonAmourDbContext db)
        {
            _db = db;
        }



        public async Task<EmailTemplate?> GetActiveTemplateAsync(string templateType, string name)
        {
            return await _db.EmailTemplates.FirstOrDefaultAsync(t => t.TemplateType == templateType && t.Name == name && t.IsActive == true);
        }


        public Task<EmailTemplate?> CreateTemplateAsync(EmailTemplateDto dto)
        {
            throw new NotImplementedException();
        }


        public Task<EmailTemplate?> UpdateTemplateAsync(int templateId, EmailTemplateDto dto)
        {
            throw new NotImplementedException();
        }


        public Task<bool> DeactivateTemplateAsync(int templateId)
        {
            throw new NotImplementedException();
        }

    }
}


