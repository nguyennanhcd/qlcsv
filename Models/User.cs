using Microsoft.Extensions.Logging;

namespace QLCSV.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? AvatarUrl { get; set; }

        // alumni | admin | pending
        public string Role { get; set; } = "pending";

        public bool IsActive { get; set; } = true;
        
        // Email verification
        public bool EmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
        public DateTimeOffset? EmailVerificationTokenExpiry { get; set; }
        
        // Password reset
        public string? PasswordResetToken { get; set; }
        public DateTimeOffset? PasswordResetTokenExpiry { get; set; }
        
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navi
        public AlumniProfile? AlumniProfile { get; set; }
        public ICollection<Event> EventsCreated { get; set; } = new List<Event>();
        public ICollection<EventRegistration> EventRegistrations { get; set; } = new List<EventRegistration>();
    }
}
