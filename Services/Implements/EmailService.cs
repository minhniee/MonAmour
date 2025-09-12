using Microsoft.Extensions.Options;
using MonAmour.Services.Interfaces;
using MonAmour.Util;
using System.Net;
using System.Net.Mail;

namespace MonAmour.Services.Implements;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailSettings _emailSettings;
    private readonly AppSettings _appSettings;
    private readonly SmtpClient _smtpClient;

    public EmailService(
        ILogger<EmailService> logger,
        IOptions<EmailSettings> emailSettings,
        IOptions<AppSettings> appSettings)
    {
        _logger = logger;
        _emailSettings = emailSettings.Value;
        _appSettings = appSettings.Value;

        _smtpClient = new SmtpClient
        {
            Host = _emailSettings.Host,
            Port = _emailSettings.Port,
            EnableSsl = true,
            Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password)
        };
    }

    public async Task SendVerificationEmailAsync(string email, string token)
    {
        try
        {
            _logger.LogInformation("Sending verification email to: {Email}", email);

            var verificationLink = $"{_appSettings.AppUrl}/Auth/VerifyEmail?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(email)}";

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.From, "MonAmour"),
                Subject = "Xác thực tài khoản MonAmour",
                IsBodyHtml = true,
                Body = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='utf-8'>
                        <style>
                            body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                            .header {{ background-color: #f8f9fa; padding: 20px; text-align: center; }}
                            .content {{ padding: 20px; }}
                            .button {{ display: inline-block; padding: 12px 24px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px; }}
                            .footer {{ background-color: #f8f9fa; padding: 15px; text-align: center; font-size: 12px; color: #666; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h2>Xác thực tài khoản MonAmour</h2>
                            </div>
                            <div class='content'>
                                <p>Chào mừng bạn đến với MonAmour!</p>
                                <p>Vui lòng click vào nút dưới đây để xác thực tài khoản của bạn:</p>
                                <p style='text-align: center; margin: 30px 0;'>
                                    <a href='{verificationLink}' class='button'>Xác thực tài khoản</a>
                                </p>
                                <p>Hoặc copy link sau vào trình duyệt:</p>
                                <p style='word-break: break-all; background-color: #f8f9fa; padding: 10px; border-radius: 3px;'>{verificationLink}</p>
                                <p><strong>Lưu ý:</strong> Link xác thực sẽ hết hạn sau 24 giờ.</p>
                                <p>Nếu bạn không đăng ký tài khoản tại MonAmour, vui lòng bỏ qua email này.</p>
                            </div>
                            <div class='footer'>
                                <p>Trân trọng,<br><strong>MonAmour Team</strong></p>
                                <p>© 2024 MonAmour. All rights reserved.</p>
                            </div>
                        </div>
                    </body>
                    </html>"
            };
            mailMessage.To.Add(email);

            await _smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("Verification email sent successfully to: {Email}", email);
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "SMTP error sending verification email to {Email}: {Error}", email, ex.Message);
            throw new Exception($"Failed to send verification email: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending verification email to {Email}", email);
            throw new Exception("Failed to send verification email due to system error", ex);
        }
    }

    public async Task SendPasswordResetEmailAsync(string email, string token)
    {
        try
        {
            _logger.LogInformation("Sending password reset email to: {Email}", email);

            var resetLink = $"{_appSettings.GetFullUrl()}/Auth/ResetPassword?token={Uri.EscapeDataString(token)}";

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.From, "MonAmour"),
                Subject = "Đặt lại mật khẩu MonAmour",
                IsBodyHtml = true,
                Body = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='utf-8'>
                        <style>
                            body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                            .header {{ background-color: #f8f9fa; padding: 20px; text-align: center; }}
                            .content {{ padding: 20px; }}
                            .button {{ display: inline-block; padding: 12px 24px; background-color: #dc3545; color: white; text-decoration: none; border-radius: 5px; }}
                            .footer {{ background-color: #f8f9fa; padding: 15px; text-align: center; font-size: 12px; color: #666; }}
                            .warning {{ background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 10px; border-radius: 5px; margin: 15px 0; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h2>Đặt lại mật khẩu MonAmour</h2>
                            </div>
                            <div class='content'>
                                <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản MonAmour.</p>
                                <p>Vui lòng click vào nút dưới đây để đặt lại mật khẩu:</p>
                                <p style='text-align: center; margin: 30px 0;'>
                                    <a href='{resetLink}' class='button'>Đặt lại mật khẩu</a>
                                </p>
                                <p>Hoặc copy link sau vào trình duyệt:</p>
                                <p style='word-break: break-all; background-color: #f8f9fa; padding: 10px; border-radius: 3px;'>{resetLink}</p>
                                <div class='warning'>
                                    <p><strong>⚠️ Lưu ý quan trọng:</strong></p>
                                    <ul>
                                        <li>Link đặt lại mật khẩu sẽ hết hạn sau <strong>1 giờ</strong></li>
                                        <li>Chỉ sử dụng link này nếu bạn thực sự yêu cầu đặt lại mật khẩu</li>
                                        <li>Nếu bạn không yêu cầu, vui lòng bỏ qua email này</li>
                                    </ul>
                                </div>
                            </div>
                            <div class='footer'>
                                <p>Trân trọng,<br><strong>MonAmour Team</strong></p>
                                <p>© 2024 MonAmour. All rights reserved.</p>
                            </div>
                        </div>
                    </body>
                    </html>"
            };
            mailMessage.To.Add(email);

            await _smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("Password reset email sent successfully to: {Email}", email);
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "SMTP error sending password reset email to {Email}: {Error}", email, ex.Message);
            throw new Exception($"Failed to send password reset email: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending password reset email to {Email}", email);
            throw new Exception("Failed to send password reset email due to system error", ex);
        }
    }

    public async Task SendWelcomeEmailAsync(string email, string name)
    {
        try
        {
            _logger.LogInformation("Sending welcome email to: {Email} for user: {Name}", email, name);

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.From, "MonAmour"),
                Subject = "Chào mừng đến với MonAmour! 🎉",
                IsBodyHtml = true,
                Body = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='utf-8'>
                        <style>
                            body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                            .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; color: white; }}
                            .content {{ padding: 30px; }}
                            .button {{ display: inline-block; padding: 15px 30px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; text-decoration: none; border-radius: 25px; font-weight: bold; }}
                            .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666; }}
                            .features {{ background-color: #f8f9fa; padding: 20px; border-radius: 10px; margin: 20px 0; }}
                            .feature-item {{ margin: 10px 0; padding: 10px; background-color: white; border-radius: 5px; border-left: 4px solid #667eea; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>🎉 Chào mừng đến với MonAmour!</h1>
                                <p>Xin chào <strong>{name}</strong>!</p>
                            </div>
                            <div class='content'>
                                <p>Chúng tôi rất vui mừng khi bạn đã trở thành thành viên của MonAmour! Tài khoản của bạn đã được xác thực thành công.</p>
                                
                                <div class='features'>
                                    <h3>🌟 Tại MonAmour, bạn có thể:</h3>
                                    <div class='feature-item'>
                                        <strong>📸 Khám phá concepts chụp ảnh độc đáo</strong><br>
                                        Tìm hiểu các phong cách chụp ảnh đa dạng và sáng tạo
                                    </div>
                                    <div class='feature-item'>
                                        <strong>📅 Đặt lịch với nhiếp ảnh gia chuyên nghiệp</strong><br>
                                        Lựa chọn và đặt lịch với các photographer tài năng
                                    </div>
                                    <div class='feature-item'>
                                        <strong>🛍️ Mua sắm thời trang và phụ kiện</strong><br>
                                        Khám phá bộ sưu tập sản phẩm thời trang độc đáo
                                    </div>
                                    <div class='feature-item'>
                                        <strong>💎 Và nhiều điều thú vị khác!</strong><br>
                                        Trải nghiệm các tính năng đặc biệt dành riêng cho thành viên
                                    </div>
                                </div>
                                
                                <p style='text-align: center; margin: 40px 0;'>
                                    <a href='{_appSettings.GetFullUrl()}' class='button'>🚀 Bắt đầu khám phá ngay!</a>
                                </p>
                                
                                <p>Nếu bạn có bất kỳ câu hỏi nào, đừng ngần ngại liên hệ với chúng tôi. Team hỗ trợ MonAmour luôn sẵn sàng giúp đỡ bạn!</p>
                            </div>
                            <div class='footer'>
                                <p>Trân trọng,<br><strong>MonAmour Team</strong></p>
                                <p>📧 Email: support@monamour.com | 📞 Hotline: 1900 xxxx</p>
                                <p>© 2024 MonAmour. All rights reserved.</p>
                            </div>
                        </div>
                    </body>
                    </html>"
            };
            mailMessage.To.Add(email);

            await _smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("Welcome email sent successfully to: {Email} for user: {Name}", email, name);
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "SMTP error sending welcome email to {Email}: {Error}", email, ex.Message);
            throw new Exception($"Failed to send welcome email: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending welcome email to {Email}", email);
            throw new Exception("Failed to send welcome email due to system error", ex);
        }
    }

    public async Task SendAdminPaymentIssueReportAsync(string adminEmail, string subject, string htmlBody)
    {
        try
        {
            _logger.LogInformation("Sending admin payment issue report to: {Email}", adminEmail);

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.From, "MonAmour"),
                Subject = string.IsNullOrWhiteSpace(subject) ? "Báo cáo sự cố thanh toán" : subject,
                IsBodyHtml = true,
                Body = htmlBody
            };
            mailMessage.To.Add(adminEmail);

            await _smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("Admin payment issue report sent successfully to: {Email}", adminEmail);
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "SMTP error sending admin payment issue report to {Email}: {Error}", adminEmail, ex.Message);
            throw new Exception($"Failed to send admin payment issue report: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending admin payment issue report to {Email}", adminEmail);
            throw new Exception("Failed to send admin payment issue report due to system error", ex);
        }
    }

    public async Task SendContactConfirmationEmailAsync(string customerEmail, string customerName, string htmlBody)
    {
        try
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.From, "MonAmour"),
                Subject = "Xác nhận yêu cầu tư vấn - MonAmour",
                IsBodyHtml = true,
                Body = htmlBody
            };
            mailMessage.To.Add(customerEmail);

            await _smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending contact confirmation to {Email}", customerEmail);
            throw;
        }
    }

    public async Task SendAdminContactNotificationEmailAsync(string htmlBody, string? subject = null)
    {
        try
        {
            var adminEmail = _emailSettings.From;
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.From, "MonAmour"),
                Subject = string.IsNullOrWhiteSpace(subject) ? "Yêu cầu tư vấn mới" : subject,
                IsBodyHtml = true,
                Body = htmlBody
            };
            mailMessage.To.Add(adminEmail);

            await _smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending admin contact notification");
            throw;
        }
    }
}
