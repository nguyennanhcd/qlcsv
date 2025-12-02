namespace QLCSV.DTOs.Faculty
{
    public class FacultyResponse
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
        public string? ShortName { get; set; }
        public string? Description { get; set; }

        public int MajorCount { get; set; }
    }
}
