
using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Data.Models;

namespace ThuyetMinh.Data.Repositories
{
    public class AudioRepository
    {
        private readonly AppDbContext _context;
        public AudioRepository(AppDbContext context)
        {
            _context = context;
        }
        public List<Audio> GetAll()
        {
            return _context.Audios.AsNoTracking().ToList();
        }
        public Audio? GetById(int id)
        {
            return _context.Audios.AsNoTracking()
                .FirstOrDefault(x => x.Id == id);
        }
        public Audio? GetByTranslationId(int translationId)
        {
            return _context.Audios.AsNoTracking()
                .FirstOrDefault(x => x.TranslationId == translationId);
        }
        public Audio Add(Audio audio)
        {
            if (_context.Audios.Any(x => x.TranslationId == audio.TranslationId))
                throw new InvalidOperationException("Ban dich nay da co audio.");
            _context.Audios.Add(audio);
            _context.SaveChanges();
            return audio;
        }

        public Audio Update(Audio audio)
        {
            var existing = _context.Audios
                .FirstOrDefault(x => x.Id == audio.Id);
            if (existing == null)
                throw new KeyNotFoundException("Khong tim thay audio.");
            existing.FilePath = audio.FilePath;
            existing.DurationSeconds = audio.DurationSeconds;
            existing.Status = audio.Status;
            _context.SaveChanges();
            return existing;
        }
    }
}
