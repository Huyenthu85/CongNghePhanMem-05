
using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Data;
using ThuyetMinh.Data.Models;
using ThuyetMinh.Data.Repositories;

namespace ThuyetMinh.Business.Services
{
    public class TranslationService
    {
        private readonly TranslationRepository _translationRepository;
        private readonly AppDbContext _context;
        public TranslationService(TranslationRepository translationRepository, AppDbContext context)
        {
            _translationRepository = translationRepository;
            _context = context;
        }
        public List<Translation> GetAllTranslations()
        {
            return _translationRepository.GetAll();
        }
        public Translation? GetTranslationById(int id)
        {
            return _translationRepository.GetById(id);
        }
        public Translation CreateTranslation(long shopSubmissionId, int languageId, string translatedText)
        {
            if (string.IsNullOrWhiteSpace(translatedText))
                throw new ArgumentException("Noi dung ban dich khong duoc de trong.");
            var sub = _context.ShopSubmissions
                .Include(x => x.Shop)
                .FirstOrDefault(x => x.Id == shopSubmissionId);
            if (sub == null)
                throw new KeyNotFoundException("Khong tim thay noi dung can dich.");
            if (sub.Status != SubmissionStatus.Approved ||
                sub.Shop.PublishedVersionId != sub.Id ||
                sub.Shop.PublishStatus != PublishStatus.Approved)
                throw new InvalidOperationException("Noi dung chua du dieu kien de dich.");
            var language = _context.Languages
                .FirstOrDefault(x => x.Id == languageId && x.IsActive);
            if (language == null)
                throw new ArgumentException("Ngon ngu khong ton tai hoac da ngung hoat dong.");
            var existing = _translationRepository.GetBySubmissionLanguage(shopSubmissionId, languageId);
            if (existing != null)
                throw new InvalidOperationException("Ban dich cho ngon ngu nay da ton tai.");
            var translation = new Translation
            {
                ShopSubmissionId = shopSubmissionId,
                LanguageId = languageId,
                TranslatedText = translatedText.Trim(),
                Status = "Success"
            };
            return _translationRepository.Add(translation);
        }
        public Translation? GetTranslation(long shopSubmissionId, int languageId)
        {
            return _translationRepository.GetBySubmissionLanguage(shopSubmissionId, languageId);
        }
    }
}
