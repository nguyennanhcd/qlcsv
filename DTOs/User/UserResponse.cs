namespace QLCSV.DTOs.User
{
    public class UserResponse
    {
        public long Id { get; set; }

        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? AvatarUrl { get; set; }

        public string Role { get; set; } = null!;
        public bool IsActive { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        // Thông tin hồ sơ (nếu có)
        public bool HasProfile { get; set; }
        public string? StudentId { get; set; }
        public int? GraduationYear { get; set; }

        public int? FacultyId { get; set; }
        public string? FacultyName { get; set; }

        public int? MajorId { get; set; }
        public string? MajorName { get; set; }
    }
}
