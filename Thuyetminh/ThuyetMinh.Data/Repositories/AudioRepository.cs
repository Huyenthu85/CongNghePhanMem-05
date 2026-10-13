
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
            _context.Audios.Add(audio);
            _context.SaveChanges();
            return audio;
        }
    }
}
