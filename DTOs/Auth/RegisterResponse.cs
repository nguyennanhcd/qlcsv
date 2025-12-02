namespace QLCSV.DTOs.Auth
{
    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public string? Token { get; set; }
        public long UserId { get; set; }
    }
}