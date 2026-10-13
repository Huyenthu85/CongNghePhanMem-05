
namespace ThuyetMinh.Data.Models
{
    public class Translation
    {
        public int Id { get; set; }
        public long ShopSubmissionId { get; set; }
        public ShopSubmission ShopSubmission { get; set; } = null!;
        public int LanguageId { get; set; }
        public Language Language { get; set; } = null!;
        public string TranslatedText { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
    }
}
