namespace QLCSV.DTOs.Event
{
    public class EventRegistrationUserResponse
    {
        public long UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;

        public DateTimeOffset RegisteredAt { get; set; }
        public string Status { get; set; } = null!;
    }
}
