using ThuyetMinh.Data.Models;

namespace ThuyetMinh.Data.Repositories
{
    public class LanguageRepository
    {
        private readonly List<Language> _languages = new()
        {
            new Language { Id = 1, Code = "vi", Name = "Tiếng Việt" },
            new Language { Id = 2, Code = "en", Name = "Tiếng Anh" },
            new Language { Id = 3, Code = "zh", Name = "Tiếng Trung" },
            new Language { Id = 4, Code = "km", Name = "Tiếng Khmer" }
        };

        public List<Language> GetAll()
        {
            return _languages;
        }

        public Language? GetByCode(string code)
        {
            return _languages.FirstOrDefault(
                x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase)
            );
        }
    }
}