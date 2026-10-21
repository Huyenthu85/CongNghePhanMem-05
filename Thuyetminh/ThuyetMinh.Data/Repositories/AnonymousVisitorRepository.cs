
using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Data.Models;
namespace ThuyetMinh.Data.Repositories;
public interface IAnonymousVisitorRepository
{
    Task<AnonymousVisitor?> GetByTokenHashAsync(string tokenHash);
    Task<AnonymousVisitor> CreateAsync(string tokenHash);
    Task UpdateLastSeenAsync(AnonymousVisitor visitor);
}
public class AnonymousVisitorRepository : IAnonymousVisitorRepository
{
    private readonly AppDbContext _db;
    public AnonymousVisitorRepository(AppDbContext db)
    {
        _db = db;
    }
    public async Task<AnonymousVisitor?> GetByTokenHashAsync(string tokenHash)
    {
        return await _db.AnonymousVisitors
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash && x.IsActive);
    }
    public async Task<AnonymousVisitor> CreateAsync(string tokenHash)
    {
        var visitor = new AnonymousVisitor
        {
            TokenHash = tokenHash,
            CreatedAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow,
            IsActive = true
        };
        _db.AnonymousVisitors.Add(visitor);
        await _db.SaveChangesAsync();
        return visitor;
    }
    public async Task UpdateLastSeenAsync(AnonymousVisitor visitor)
    {
        visitor.LastSeenAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}
