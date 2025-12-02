namespace QLCSV.Models
{
    public class AlumniProfile
    {
        public long Id { get; set; }

        public long UserId { get; set; }
        public User User { get; set; } = null!;

        public string? StudentId { get; set; }
        public int GraduationYear { get; set; }

        public int FacultyId { get; set; }
        public Faculty Faculty { get; set; } = null!;

        public int MajorId { get; set; }
        public Major Major { get; set; } = null!;

        public string? CurrentPosition { get; set; }
        public string? Company { get; set; }
        public string? CompanyIndustry { get; set; }
        public string? City { get; set; }
        public string Country { get; set; } = "Việt Nam";
        public string? LinkedinUrl { get; set; }
        public string? FacebookUrl { get; set; }
        public string? Bio { get; set; }

        public bool IsPublic { get; set; } = false;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navi
        public ICollection<AlumniBatch> AlumniBatches { get; set; } = new List<AlumniBatch>();
    }
}
