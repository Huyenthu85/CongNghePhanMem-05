using ThuyetMinh.Data.Models;

namespace ThuyetMinh.Data.Repositories
{
    public class TranslationRepository
    {
        private static readonly List<Translation> _translations = new();

        public List<Translation> GetAll()
        {
            return _translations;
        }

        public Translation? GetById(int id)
        {
            return _translations.FirstOrDefault(x => x.Id == id);
        }

        public Translation Add(Translation translation)
        {
            translation.Id = _translations.Count + 1;
            _translations.Add(translation);

            return translation;
        }

        public Translation? GetByPoiVersionLanguage(
            int poiId,
            int versionId,
            int languageId)
        {
            return _translations.FirstOrDefault(x =>
                x.PoiId == poiId &&
                x.VersionId == versionId &&
                x.LanguageId == languageId);
        }
    }
}