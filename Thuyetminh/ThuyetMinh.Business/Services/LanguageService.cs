using ThuyetMinh.Data.Models;
using ThuyetMinh.Data.Repositories;

namespace ThuyetMinh.Business.Services
{
    public class LanguageService
    {
        private readonly LanguageRepository _languageRepository;

        public LanguageService()
        {
            _languageRepository = new LanguageRepository();
        }

        public List<Language> GetAllLanguages()
        {
            return _languageRepository.GetAll()
                .Where(x => x.IsActive)
                .ToList();
        }

        public Language? GetLanguageByCode(string code)
        {
            return _languageRepository.GetByCode(code);
        }
    }
}