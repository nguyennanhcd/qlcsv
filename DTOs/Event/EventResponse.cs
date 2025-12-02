namespace QLCSV.DTOs.Event
{
    public class EventResponse
    {
        public long Id { get; set; }

        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        public DateTimeOffset EventDate { get; set; }

        public string? Location { get; set; }
        public bool IsOnline { get; set; }
        public string? MeetLink { get; set; }
        public string? ThumbnailUrl { get; set; }

        public long CreatedBy { get; set; }
        public string CreatedByName { get; set; } = null!;

        public DateTimeOffset CreatedAt { get; set; }
        public int? MaxParticipants { get; set; }
        public int RegisteredCount { get; set; }

        // Optional: trạng thái đăng ký của current user (nếu có)
        public string? MyRegistrationStatus { get; set; }
    }
}
