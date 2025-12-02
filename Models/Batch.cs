namespace QLCSV.Models
{
    public class Batch
    {
        public int Id { get; set; }

        public int GraduationYear { get; set; }
        public string Name { get; set; } = null!;
        public int? StartYear { get; set; }
        public string? Description { get; set; }

        // Navi
        public ICollection<AlumniBatch> AlumniBatches { get; set; } = new List<AlumniBatch>();
    }
}
