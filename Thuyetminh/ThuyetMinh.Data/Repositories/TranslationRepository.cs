
using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Data.Models;

namespace ThuyetMinh.Data.Repositories
{
    public class TranslationRepository
    {
        private readonly AppDbContext _context;
        public TranslationRepository(AppDbContext context)
        {
            _context = context;
        }
        public List<Translation> GetAll()
        {
            return _context.Translations.AsNoTracking().ToList();
        }
        public Translation? GetById(int id)
        {
            return _context.Translations.AsNoTracking()
                .FirstOrDefault(x => x.Id == id);
        }
        public Translation Add(Translation translation)
        {
            _context.Translations.Add(translation);
            _context.SaveChanges();
            return translation;
        }
        public Translation? GetBySubmissionLanguage(long shopSubmissionId, int languageId)
        {
            return _context.Translations.AsNoTracking()
                .FirstOrDefault(x =>
                    x.ShopSubmissionId == shopSubmissionId &&
                    x.LanguageId == languageId);
        }
    }
}
