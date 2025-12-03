using System.Net;
using System.Net.Mail;

namespace QLCSV.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailVerificationAsync(string toEmail, string fullName, string verificationToken)
        {
            var subject = "Xác thực tài khoản QLCSV";
            var verifyUrl = $"{_config["AppUrl"]}/verify-email?token={verificationToken}";
            
            var body = $@"
                <h2>Xin chào {fullName},</h2>
                <p>Cảm ơn bạn đã đăng ký tài khoản QLCSV.</p>
                <p>Vui lòng nhấn vào link dưới đây để xác thực email của bạn:</p>
                <p><a href='{verifyUrl}'>Xác thực email</a></p>
                <p>Hoặc sao chép link sau vào trình duyệt:</p>
                <p>{verifyUrl}</p>
                <p>Link này sẽ hết hạn sau 24 giờ.</p>
                <br/>
                <p>Nếu bạn không đăng ký tài khoản này, vui lòng bỏ qua email này.</p>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendPasswordResetAsync(string toEmail, string fullName, string resetToken)
        {
            var subject = "Đặt lại mật khẩu QLCSV";
            var resetUrl = $"{_config["AppUrl"]}/reset-password?token={resetToken}";
            
            var body = $@"
                <h2>Xin chào {fullName},</h2>
                <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
                <p>Vui lòng nhấn vào link dưới đây để đặt lại mật khẩu:</p>
                <p><a href='{resetUrl}'>Đặt lại mật khẩu</a></p>
                <p>Hoặc sao chép link sau vào trình duyệt:</p>
                <p>{resetUrl}</p>
                <p>Link này sẽ hết hạn sau 1 giờ.</p>
                <br/>
                <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpHost = _config["Smtp:Host"];
            var smtpPort = _config.GetValue<int>("Smtp:Port");
            var smtpUser = _config["Smtp:Username"];
            var smtpPass = _config["Smtp:Password"];
            var fromEmail = _config["Smtp:FromEmail"];
            var fromName = _config["Smtp:FromName"];

            if (string.IsNullOrWhiteSpace(smtpHost))
            {
                _logger.LogWarning("SMTP not configured. Email not sent to {Email}", toEmail);
                return;
            }

            try
            {
                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUser, smtpPass)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail ?? smtpUser!, fromName ?? "QLCSV"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw;
            }
        }
    }
}
