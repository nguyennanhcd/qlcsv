namespace QLCSV.DTOs.Batch
{
    public class BatchResponse
    {
        public int Id { get; set; }

        public int GraduationYear { get; set; }
        public string Name { get; set; } = null!;
        public int? StartYear { get; set; }
        public string? Description { get; set; }

        public int AlumniCount { get; set; }
    }
}
