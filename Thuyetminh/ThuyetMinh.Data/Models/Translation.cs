namespace ThuyetMinh.Data.Models
{
    public class Translation
    {
        public int Id { get; set; }

        public int PoiId { get; set; }

        public int VersionId { get; set; }

        public int LanguageId { get; set; }

        public string TranslatedText { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";
    }
}