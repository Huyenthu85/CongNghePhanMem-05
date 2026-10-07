using ThuyetMinh.Data.Models;
using ThuyetMinh.Data.Repositories;

namespace ThuyetMinh.Business.Services
{
    public class TranslationService
    {
        private readonly TranslationRepository _translationRepository;

        public TranslationService()
        {
            _translationRepository = new TranslationRepository();
        }

        public List<Translation> GetAllTranslations()
        {
            return _translationRepository.GetAll();
        }

        public Translation? GetTranslationById(int id)
        {
            return _translationRepository.GetById(id);
        }

        public Translation CreateTranslation(
            int poiId,
            int versionId,
            int languageId,
            string translatedText)
        {
            var translation = new Translation
            {
                PoiId = poiId,
                VersionId = versionId,
                LanguageId = languageId,
                TranslatedText = translatedText,
                Status = "Success"
            };

            return _translationRepository.Add(translation);
        }

        public Translation? GetTranslation(
            int poiId,
            int versionId,
            int languageId)
        {
            return _translationRepository.GetByPoiVersionLanguage(
                poiId,
                versionId,
                languageId);
        }
    }
}