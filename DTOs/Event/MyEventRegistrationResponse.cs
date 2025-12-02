namespace QLCSV.DTOs.Event
{
    public class MyEventRegistrationResponse
    {
        public long EventId { get; set; }
        public string Title { get; set; } = null!;
        public DateTimeOffset EventDate { get; set; }

        public string? Location { get; set; }
        public bool IsOnline { get; set; }
        public string? ThumbnailUrl { get; set; }

        public DateTimeOffset RegisteredAt { get; set; }
        public string Status { get; set; } = null!;
    }
}
