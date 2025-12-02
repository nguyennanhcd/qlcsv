namespace QLCSV.DTOs.Auth
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public string? Token { get; set; }
        public string? Role { get; set; }
        public bool ProfileCompleted { get; set; }   // true nếu đã có AlumniProfile
    }
}
