
using ThuyetMinh.Data.Models;
using ThuyetMinh.Data.Repositories;

namespace ThuyetMinh.Business.Services
{
    public class LanguageService
    {
        private readonly LanguageRepository _languageRepository;
        public LanguageService(LanguageRepository languageRepository)
        {
            _languageRepository = languageRepository;
        }
        public List<Language> GetAllLanguages()
        {
            return _languageRepository.GetAll()
                .Where(x => x.IsActive)
                .ToList();
        }
        public Language? GetLanguageByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;
            return _languageRepository.GetByCode(code.Trim().ToLowerInvariant());
        }
    }
}
