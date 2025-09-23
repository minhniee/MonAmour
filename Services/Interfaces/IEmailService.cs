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

    // Order status emails
    Task SendOrderConfirmedEmailAsync(string email, string orderCode, decimal totalAmount, string? note = null);
    Task SendOrderShippingEmailAsync(string email, string orderCode, string carrierName, string trackingNumber);
    Task SendOrderCompletedEmailAsync(string email, string orderCode, DateTime completedAt);
    Task SendOrderCancelledEmailAsync(string email, string orderCode, string? reason = null);


}
