namespace QLCSV.Models
{
    public class Major
    {
        public int Id { get; set; }

        public int FacultyId { get; set; }
        public Faculty Faculty { get; set; } = null!;

        public string Name { get; set; } = null!;
        public string? Code { get; set; }

        // Navi
        public ICollection<AlumniProfile> AlumniProfiles { get; set; } = new List<AlumniProfile>();
    }
}
