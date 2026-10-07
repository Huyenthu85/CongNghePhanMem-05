namespace ThuyetMinh.Data.Models
{
    public class Audio
    {
        public int Id { get; set; }

        public int TranslationId { get; set; }

        public string FilePath { get; set; } = string.Empty;

        public int DurationSeconds { get; set; }

        public string Status { get; set; } = "Pending";
    }
}