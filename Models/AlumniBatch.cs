namespace QLCSV.Models
{
    public class AlumniBatch
    {
        public long AlumniId { get; set; }
        public AlumniProfile Alumni { get; set; } = null!;

        public int BatchId { get; set; }
        public Batch Batch { get; set; } = null!;
    }
}
