
using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Data.Models;

namespace ThuyetMinh.Data.Repositories
{
    public class LanguageRepository
    {
        private readonly AppDbContext _context;
        public LanguageRepository(AppDbContext context)
        {
            _context = context;
        }
        public List<Language> GetAll()
        {
            return _context.Languages.AsNoTracking().ToList();
        }
        public Language? GetByCode(string code)
        {
            return _context.Languages.AsNoTracking()
                .FirstOrDefault(x => x.Code == code.ToLower());
        }
    }
}
