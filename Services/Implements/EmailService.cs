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
    // Note: SmtpClient is not thread-safe for concurrent SendMailAsync. Create per-send instances instead of sharing one.

    public EmailService(
        ILogger<EmailService> logger,
        IOptions<EmailSettings> emailSettings,
        IOptions<AppSettings> appSettings)
    {
        _logger = logger;
        _emailSettings = emailSettings.Value;
        _appSettings = appSettings.Value;

        // Intentionally not creating a shared SmtpClient here to avoid concurrency issues
    }

    private SmtpClient CreateSmtpClient()
    {
        return new SmtpClient
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
                From = new MailAddress(_emailSettings.From, "Mon Amour"),
                Subject = "Xác thực tài khoản Mon Amour",
                IsBodyHtml = true,
                Body = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='utf-8'>
                        <style>
                            body {{ font-family: 'Noto Serif', Arial, sans-serif; line-height: 1.6; color: #62000d; background-color: #fbf1e6; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: white; border-radius: 12px; box-shadow: 0 4px 6px rgba(98, 0, 13, 0.1); }}
                            .header {{ background-color: #62000d; padding: 24px; text-align: center; color: #fbf1e6; border-radius: 8px 8px 0 0; }}
                            .content {{ padding: 24px; }}
                            .button {{ display: inline-block; padding: 12px 32px; background-color: #62000d; color: #fbf1e6; text-decoration: none; border-radius: 8px; font-weight: 600; transition: all 0.3s ease; }}
                            .button:hover {{ background-color: #4a0009; }}
                            .footer {{ background-color: #fbf1e6; padding: 20px; text-align: center; font-size: 12px; color: #62000d; border-radius: 0 0 8px 8px; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h2>Xác thực tài khoản Mon Amour</h2>
                            </div>
                            <div class='content'>
                                <p>Chào mừng bạn đến với Mon Amour!</p>
                                <p>Vui lòng click vào nút dưới đây để xác thực tài khoản của bạn:</p>
                                <p style='text-align: center; margin: 30px 0;'>
                                    <a href='{verificationLink}' class='button'>Xác thực tài khoản</a>
                                </p>
                                <p>Hoặc copy link sau vào trình duyệt:</p>
                                <p style='word-break: break-all; background-color: #f8f9fa; padding: 10px; border-radius: 3px;'>{verificationLink}</p>
                                <p><strong>Lưu ý:</strong> Link xác thực sẽ hết hạn sau 24 giờ.</p>
                                <p>Nếu bạn không đăng ký tài khoản tại Mon Amour, vui lòng bỏ qua email này.</p>
                            </div>
                            <div class='footer'>
                                <p>Trân trọng,<br><strong>Mon Amour Team</strong></p>
                                <p>© 2024 Mon Amour. All rights reserved.</p>
                            </div>
                        </div>
                    </body>
                    </html>"
            };
            mailMessage.To.Add(email);

            using var smtp = CreateSmtpClient();
            await smtp.SendMailAsync(mailMessage);
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
                            body {{ font-family: 'Noto Serif', Arial, sans-serif; line-height: 1.6; color: #62000d; background-color: #fbf1e6; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: white; border-radius: 12px; box-shadow: 0 4px 6px rgba(98, 0, 13, 0.1); }}
                            .header {{ background-color: #62000d; padding: 24px; text-align: center; color: #fbf1e6; border-radius: 8px 8px 0 0; }}
                            .content {{ padding: 24px; }}
                            .button {{ display: inline-block; padding: 12px 32px; background-color: #62000d; color: #fbf1e6; text-decoration: none; border-radius: 8px; font-weight: 600; transition: all 0.3s ease; }}
                            .button:hover {{ background-color: #4a0009; }}
                            .footer {{ background-color: #fbf1e6; padding: 20px; text-align: center; font-size: 12px; color: #62000d; border-radius: 0 0 8px 8px; }}
                            .warning {{ background-color: #fbf1e6; border: 1px solid #62000d; padding: 16px; border-radius: 8px; margin: 20px 0; }}
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

            using var smtp = CreateSmtpClient();
            await smtp.SendMailAsync(mailMessage);
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
                From = new MailAddress(_emailSettings.From, "Mon Amour"),
                Subject = "Chào mừng đến với Mon Amour! 🎉",
                IsBodyHtml = true,
                Body = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='utf-8'>
                        <style>
                            body {{ font-family: 'Noto Serif', Arial, sans-serif; line-height: 1.6; color: #62000d; background-color: #fbf1e6; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: white; border-radius: 12px; box-shadow: 0 4px 6px rgba(98, 0, 13, 0.1); }}
                            .header {{ background-color: #62000d; padding: 24px; text-align: center; color: #fbf1e6; border-radius: 8px 8px 0 0; }}
                            .content {{ padding: 24px; }}
                            .button {{ display: inline-block; padding: 12px 32px; background-color: #62000d; color: #fbf1e6; text-decoration: none; border-radius: 8px; font-weight: 600; transition: all 0.3s ease; }}
                            .button:hover {{ background-color: #4a0009; }}
                            .footer {{ background-color: #fbf1e6; padding: 20px; text-align: center; font-size: 12px; color: #62000d; border-radius: 0 0 8px 8px; }}
                            .features {{ background-color: #fbf1e6; padding: 24px; border-radius: 8px; margin: 24px 0; }}
                            .feature-item {{ margin: 12px 0; padding: 16px; background-color: white; border-radius: 8px; border-left: 4px solid #62000d; box-shadow: 0 2px 4px rgba(98, 0, 13, 0.05); }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>🎉 Chào mừng đến với Mon Amour!</h1>
                                <p>Xin chào <strong>{name}</strong>!</p>
                            </div>
                            <div class='content'>
                                <p>Chúng tôi rất vui mừng khi bạn đã trở thành thành viên của Mon Amour! Tài khoản của bạn đã được xác thực thành công.</p>
                                
                                <div class='features'>
                                    <h3>🌟 Tại Mon Amour, bạn có thể:</h3>
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
                                
                                <p>Nếu bạn có bất kỳ câu hỏi nào, đừng ngần ngại liên hệ với chúng tôi. Team hỗ trợ Mon Amour luôn sẵn sàng giúp đỡ bạn!</p>
                            </div>
                            <div class='footer'>
                                    <p>
        📧 
        <a href=""mailto:booking.monamour@gmail.com"" class=""hover:underline"">
            booking.monamour@gmail.com
        </a>
    </p>
    <p>
        📞 
        <a href=""tel:0985613906"" class=""hover:underline"">
            0985613906
        </a>
    </p>
    <p>📍 Ngõ 83 Đào Tấn, Giảng Võ, Hà Nội</p>
                            </div>
                        </div>
                    </body>
                    </html>"
            };
            mailMessage.To.Add(email);

            using var smtp = CreateSmtpClient();
            await smtp.SendMailAsync(mailMessage);
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

            using var smtp = CreateSmtpClient();
            await smtp.SendMailAsync(mailMessage);
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

            using var smtp = CreateSmtpClient();
            await smtp.SendMailAsync(mailMessage);
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
                From = new MailAddress(_emailSettings.From, "Mon Amour"),
                Subject = string.IsNullOrWhiteSpace(subject) ? "Yêu cầu tư vấn mới" : subject,
                IsBodyHtml = true,
                Body = htmlBody
            };
            mailMessage.To.Add(adminEmail);

            using var smtp = CreateSmtpClient();
            await smtp.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending admin contact notification");
            throw;
        }
    }

    public async Task SendOrderConfirmedEmailAsync(string email, string orderCode, decimal totalAmount, string? note = null)
    {
        try
        {
            _logger.LogInformation("Sending order confirmed email to: {Email}, order: {OrderCode}", email, orderCode);

            var orderLink = $"{_appSettings.GetFullUrl()}/Order/Details?code={Uri.EscapeDataString(orderCode)}";

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.From, "Mon Amour"),
                Subject = $"Xác nhận đơn hàng #{orderCode}",
                IsBodyHtml = true,
                Body = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='utf-8'>
                        <style>
                            body {{ font-family: 'Noto Serif', Arial, sans-serif; line-height: 1.6; color: #62000d; background-color: #fbf1e6; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: white; border-radius: 12px; box-shadow: 0 4px 6px rgba(98, 0, 13, 0.1); }}
                            .header {{ background-color: #62000d; padding: 24px; text-align: center; color: #fbf1e6; border-radius: 8px 8px 0 0; }}
                            .content {{ padding: 24px; }}
                            .button {{ display: inline-block; padding: 12px 32px; background-color: #62000d; color: #fbf1e6; text-decoration: none; border-radius: 8px; font-weight: 600; transition: all 0.3s ease; }}
                            .button:hover {{ background-color: #4a0009; }}
                            .footer {{ background-color: #fbf1e6; padding: 20px; text-align: center; font-size: 12px; color: #62000d; border-radius: 0 0 8px 8px; }}
                            .summary {{ background-color: #fbf1e6; padding: 16px; border-radius: 8px; margin: 20px 0; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h2>Đơn hàng của bạn đã được xác nhận</h2>
                            </div>
                            <div class='content'>
                                <p>Cảm ơn bạn đã đặt hàng tại Mon Amour!</p>
                                <div class='summary'>
                                    <p><strong>Mã đơn hàng:</strong> #{orderCode}</p>
                                    <p><strong>Tổng tiền:</strong> {totalAmount:N0} ₫</p>
                                    {(string.IsNullOrWhiteSpace(note) ? string.Empty : $"<p><strong>Ghi chú:</strong> {System.Net.WebUtility.HtmlEncode(note)}</p>")}
                                </div>
                                <p>Chúng tôi đang chuẩn bị đơn hàng của bạn. Bạn có thể theo dõi trạng thái đơn hàng tại liên kết dưới đây.</p>
                                <p style='text-align: center; margin: 30px 0;'>
                                    <a href='{orderLink}' class='button'>Xem chi tiết đơn hàng</a>
                                </p>
                            </div>
                            <div class='footer'>
                                <p>Trân trọng,<br><strong>Mon Amour Team</strong></p>
                                <p>© 2024 Mon Amour. All rights reserved.</p>
                            </div>
                        </div>
                    </body>
                    </html>"
            };
            mailMessage.To.Add(email);

            using var smtp = CreateSmtpClient();
            await smtp.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending order confirmed email to {Email}", email);
            throw;
        }
    }

    public async Task SendOrderShippingEmailAsync(string email, string orderCode, string carrierName, string trackingNumber)
    {
        try
        {
            _logger.LogInformation("Sending order shipping email to: {Email}, order: {OrderCode}", email, orderCode);

            var trackingLink = $"{_appSettings.GetFullUrl()}/Order/Track?code={Uri.EscapeDataString(orderCode)}";

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.From, "Mon Amour"),
                Subject = $"Đơn hàng #{orderCode} đang được giao",
                IsBodyHtml = true,
                Body = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='utf-8'>
                        <style>
                            body {{ font-family: 'Noto Serif', Arial, sans-serif; line-height: 1.6; color: #62000d; background-color: #fbf1e6; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: white; border-radius: 12px; box-shadow: 0 4px 6px rgba(98, 0, 13, 0.1); }}
                            .header {{ background-color: #62000d; padding: 24px; text-align: center; color: #fbf1e6; border-radius: 8px 8px 0 0; }}
                            .content {{ padding: 24px; }}
                            .button {{ display: inline-block; padding: 12px 32px; background-color: #62000d; color: #fbf1e6; text-decoration: none; border-radius: 8px; font-weight: 600; transition: all 0.3s ease; }}
                            .button:hover {{ background-color: #4a0009; }}
                            .footer {{ background-color: #fbf1e6; padding: 20px; text-align: center; font-size: 12px; color: #62000d; border-radius: 0 0 8px 8px; }}
                            .summary {{ background-color: #fbf1e6; padding: 16px; border-radius: 8px; margin: 20px 0; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h2>Đơn hàng của bạn đang được giao</h2>
                            </div>
                            <div class='content'>
                                <div class='summary'>
                                    <p><strong>Mã đơn hàng:</strong> #{orderCode}</p>
                                    <p><strong>Đơn vị vận chuyển:</strong> {System.Net.WebUtility.HtmlEncode(carrierName)}</p>
                                    <p><strong>Mã vận đơn:</strong> {System.Net.WebUtility.HtmlEncode(trackingNumber)}</p>
                                </div>
                                <p>Bạn có thể theo dõi hành trình giao hàng bằng cách nhấn vào nút dưới đây.</p>
                                <p style='text-align: center; margin: 30px 0;'>
                                    <a href='{trackingLink}' class='button'>Theo dõi đơn hàng</a>
                                </p>
                            </div>
                            <div class='footer'>
                                <p>Trân trọng,<br><strong>Mon Amour Team</strong></p>
                                <p>© 2024 Mon Amour. All rights reserved.</p>
                            </div>
                        </div>
                    </body>
                    </html>"
            };
            mailMessage.To.Add(email);

            using var smtp = CreateSmtpClient();
            await smtp.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending order shipping email to {Email}", email);
            throw;
        }
    }

    public async Task SendOrderCompletedEmailAsync(string email, string orderCode, DateTime completedAt)
    {
        try
        {
            _logger.LogInformation("Sending order completed email to: {Email}, order: {OrderCode}", email, orderCode);

            var orderLink = $"{_appSettings.GetFullUrl()}/Order/Details?code={Uri.EscapeDataString(orderCode)}";

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.From, "Mon Amour"),
                Subject = $"Đơn hàng #{orderCode} đã hoàn thành",
                IsBodyHtml = true,
                Body = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='utf-8'>
                        <style>
                            body {{ font-family: 'Noto Serif', Arial, sans-serif; line-height: 1.6; color: #62000d; background-color: #fbf1e6; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: white; border-radius: 12px; box-shadow: 0 4px 6px rgba(98, 0, 13, 0.1); }}
                            .header {{ background-color: #62000d; padding: 24px; text-align: center; color: #fbf1e6; border-radius: 8px 8px 0 0; }}
                            .content {{ padding: 24px; }}
                            .button {{ display: inline-block; padding: 12px 32px; background-color: #62000d; color: #fbf1e6; text-decoration: none; border-radius: 8px; font-weight: 600; transition: all 0.3s ease; }}
                            .button:hover {{ background-color: #4a0009; }}
                            .footer {{ background-color: #fbf1e6; padding: 20px; text-align: center; font-size: 12px; color: #62000d; border-radius: 0 0 8px 8px; }}
                            .summary {{ background-color: #fbf1e6; padding: 16px; border-radius: 8px; margin: 20px 0; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h2>Đơn hàng đã hoàn thành</h2>
                            </div>
                            <div class='content'>
                                <div class='summary'>
                                    <p><strong>Mã đơn hàng:</strong> #{orderCode}</p>
                                    <p><strong>Thời gian hoàn thành:</strong> {completedAt:HH:mm dd/MM/yyyy}</p>
                                </div>
                                <p>Cảm ơn bạn đã mua sắm tại Mon Amour. Rất mong nhận được đánh giá của bạn về trải nghiệm mua hàng.</p>
                                <p style='text-align: center; margin: 30px 0;'>
                                    <a href='{orderLink}' class='button'>Xem đơn hàng</a>
                                </p>
                            </div>
                            <div class='footer'>
                                <p>Trân trọng,<br><strong>Mon Amour Team</strong></p>
                                <p>© 2024 Mon Amour. All rights reserved.</p>
                            </div>
                        </div>
                    </body>
                    </html>"
            };
            mailMessage.To.Add(email);

            using var smtp = CreateSmtpClient();
            await smtp.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending order completed email to {Email}", email);
            throw;
        }
    }

    public async Task SendOrderCancelledEmailAsync(string email, string orderCode, string? reason = null)
    {
        try
        {
            _logger.LogInformation("Sending order cancelled email to: {Email}, order: {OrderCode}", email, orderCode);

            var supportLink = $"mailto:{_emailSettings.From}";

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.From, "Mon Amour"),
                Subject = $"Đơn hàng #{orderCode} đã bị hủy",
                IsBodyHtml = true,
                Body = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='utf-8'>
                        <style>
                            body {{ font-family: 'Noto Serif', Arial, sans-serif; line-height: 1.6; color: #62000d; background-color: #fbf1e6; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: white; border-radius: 12px; box-shadow: 0 4px 6px rgba(98, 0, 13, 0.1); }}
                            .header {{ background-color: #62000d; padding: 24px; text-align: center; color: #fbf1e6; border-radius: 8px 8px 0 0; }}
                            .content {{ padding: 24px; }}
                            .button {{ display: inline-block; padding: 12px 32px; background-color: #62000d; color: #fbf1e6; text-decoration: none; border-radius: 8px; font-weight: 600; transition: all 0.3s ease; }}
                            .button:hover {{ background-color: #4a0009; }}
                            .footer {{ background-color: #fbf1e6; padding: 20px; text-align: center; font-size: 12px; color: #62000d; border-radius: 0 0 8px 8px; }}
                            .summary {{ background-color: #fbf1e6; padding: 16px; border-radius: 8px; margin: 20px 0; }}
                            .warning {{ background-color: #fff4f4; border: 1px solid #dc3545; color: #721c24; padding: 12px 16px; border-radius: 8px; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h2>Đơn hàng đã bị hủy</h2>
                            </div>
                            <div class='content'>
                                <div class='summary'>
                                    <p><strong>Mã đơn hàng:</strong> #{orderCode}</p>
                                    {(string.IsNullOrWhiteSpace(reason) ? string.Empty : $"<p class='warning'><strong>Lý do:</strong> {System.Net.WebUtility.HtmlEncode(reason)}</p>")}
                                </div>
                                <p>Nếu bạn cần hỗ trợ thêm, vui lòng liên hệ đội ngũ Mon Amour.</p>
                                <p style='text-align: center; margin: 30px 0;'>
                                    <a href='{supportLink}' class='button'>Liên hệ hỗ trợ</a>
                                </p>
                            </div>
                            <div class='footer'>
                                <p>Trân trọng,<br><strong>Mon Amour Team</strong></p>
                                <p>© 2024 Mon Amour. All rights reserved.</p>
                            </div>
                        </div>
                    </body>
                    </html>"
            };
            mailMessage.To.Add(email);

            using var smtp = CreateSmtpClient();
            await smtp.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending order cancelled email to {Email}", email);
            throw;
        }
    }
}
