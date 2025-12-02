namespace QLCSV.DTOs.Major
{
    public class MajorResponse
    {
        public int Id { get; set; }

        public int FacultyId { get; set; }
        public string FacultyName { get; set; } = null!;

        public string Name { get; set; } = null!;
        public string? Code { get; set; }

        public int AlumniCount { get; set; }
    }
}
