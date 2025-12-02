namespace QLCSV.Models
{
    public class Event
    {
        public long Id { get; set; }

        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        public DateTimeOffset EventDate { get; set; }

        public string? Location { get; set; }
        public bool IsOnline { get; set; } = false;
        public string? MeetLink { get; set; }
        public string? ThumbnailUrl { get; set; }

        public long CreatedBy { get; set; }
        public User CreatedByUser { get; set; } = null!;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public int? MaxParticipants { get; set; }

        // Navi
        public ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();
    }
}
