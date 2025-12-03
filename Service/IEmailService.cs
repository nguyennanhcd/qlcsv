namespace QLCSV.Service
{
    public interface IEmailService
    {
        Task SendEmailVerificationAsync(string toEmail, string fullName, string verificationToken);
        Task SendPasswordResetAsync(string toEmail, string fullName, string resetToken);
    }
}
