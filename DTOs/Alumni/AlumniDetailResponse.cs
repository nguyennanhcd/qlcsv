namespace QLCSV.DTOs.Alumni
{
    public class AlumniDetailResponse
    {
        public long Id { get; set; }

        public long UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? AvatarUrl { get; set; }

        public string? StudentId { get; set; }
        public int GraduationYear { get; set; }

        public int FacultyId { get; set; }
        public string FacultyName { get; set; } = null!;

        public int MajorId { get; set; }
        public string MajorName { get; set; } = null!;

        public string? CurrentPosition { get; set; }
        public string? Company { get; set; }
        public string? CompanyIndustry { get; set; }
        public string? City { get; set; }
        public string Country { get; set; } = null!;

        public string? LinkedinUrl { get; set; }
        public string? FacebookUrl { get; set; }
        public string? Bio { get; set; }

        public bool IsPublic { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
