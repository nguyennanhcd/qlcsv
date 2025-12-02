namespace QLCSV.Models
{
    public class Faculty
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? ShortName { get; set; }
        public string? Description { get; set; }

        // Navi
        public ICollection<Major> Majors { get; set; } = new List<Major>();
        public ICollection<AlumniProfile> AlumniProfiles { get; set; } = new List<AlumniProfile>();
    }
}
