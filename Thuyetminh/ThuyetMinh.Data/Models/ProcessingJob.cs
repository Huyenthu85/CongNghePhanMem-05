
namespace ThuyetMinh.Data.Models
{
    public class ProcessingJob
    {
        public int Id { get; set; }
        public long ShopSubmissionId { get; set; }
        public ShopSubmission ShopSubmission { get; set; } = null!;
        public int LanguageId { get; set; }
        public Language Language { get; set; } = null!;
        public string Status { get; set; } = "Pending";
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }
}
