namespace MonAmour.Services.Interfaces;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string email, string token);
    Task SendPasswordResetEmailAsync(string email, string token);
    Task SendWelcomeEmailAsync(string email, string name);

    Task SendAdminPaymentIssueReportAsync(string adminEmail, string subject, string htmlBody);

    // Contact form emails
    Task SendContactConfirmationEmailAsync(string customerEmail, string customerName, string htmlBody);
    Task SendAdminContactNotificationEmailAsync(string htmlBody, string? subject = null);


}
