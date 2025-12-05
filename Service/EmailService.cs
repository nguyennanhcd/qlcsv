using System.Net.Http.Json;

namespace QLCSV.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;
        private readonly HttpClient _httpClient;

        public EmailService(IConfiguration config, ILogger<EmailService> logger, HttpClient httpClient)
        {
            _config = config;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task SendEmailVerificationAsync(string toEmail, string fullName, string verificationToken)
        {
            var subject = "Xác thực tài khoản QLCSV";
            var appUrl = Environment.GetEnvironmentVariable("APP_URL") ?? "http://localhost:5083";
            var verifyUrl = $"{appUrl}/verify-email?token={verificationToken}";

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
            var appUrl = Environment.GetEnvironmentVariable("APP_URL") ?? "http://localhost:5083";
            var resetUrl = $"{appUrl}/reset-password?token={resetToken}";

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
            var apiKey = Environment.GetEnvironmentVariable("RESEND_API_KEY");

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("Resend API not configured. Email not sent to {Email}", toEmail);
                return;
            }

            var fromEmail = Environment.GetEnvironmentVariable("RESEND_FROM_EMAIL") ?? "onboarding@resend.dev";
            var fromName = Environment.GetEnvironmentVariable("RESEND_FROM_NAME") ?? "QLCSV Alumni System";

            try
            {
                var payload = new
                {
                    from = $"{fromName} <{fromEmail}>",
                    to = new[] { toEmail },
                    subject = subject,
                    html = body
                };

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
                request.Headers.Add("Authorization", $"Bearer {apiKey}");
                request.Content = JsonContent.Create(payload);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

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
