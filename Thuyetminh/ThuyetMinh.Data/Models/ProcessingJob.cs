namespace ThuyetMinh.Data.Models
{
    public class ProcessingJob
    {
        public int Id { get; set; }

        public int PoiId { get; set; }

        public int VersionId { get; set; }

        public int LanguageId { get; set; }

        public string Status { get; set; } = "Pending";

        public string? ErrorMessage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }
    }
}
