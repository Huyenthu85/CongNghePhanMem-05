
using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Data.Models;
namespace ThuyetMinh.Data.Repositories;
public interface IListeningBalanceRepository
{
    Task<ListeningBalance> GetOrCreateAsync(long anonymousVisitorId);
}
public class ListeningBalanceRepository : IListeningBalanceRepository
{
    private readonly AppDbContext _db;
    public ListeningBalanceRepository(AppDbContext db)
    {
        _db = db;
    }
    public async Task<ListeningBalance> GetOrCreateAsync(long anonymousVisitorId)
    {
        var balance = await _db.ListeningBalances
            .FirstOrDefaultAsync(x => x.AnonymousVisitorId == anonymousVisitorId);
        if (balance != null)
        {
            return balance;
        }
        balance = new ListeningBalance
        {
            AnonymousVisitorId = anonymousVisitorId,
            RemainingSeconds = 0,
            UpdatedAt = DateTime.UtcNow
        };
        _db.ListeningBalances.Add(balance);
        await _db.SaveChangesAsync();
        return balance;
    }
}
