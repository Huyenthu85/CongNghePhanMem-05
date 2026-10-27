using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Data;
using ThuyetMinh.Data.Models;

namespace ThuyetMinh.Business.Services;

public sealed record ListeningSnapshot(long SessionId, long RemainingSeconds, string Status);

public interface IListeningService
{
    Task<ListeningSnapshot?> StartAsync(long visitorId, long shopId);
    Task<ListeningSnapshot?> UpdateAsync(long visitorId, long sessionId, string action);
}

public sealed class ListeningService : IListeningService
{
    private readonly AppDbContext _db;
    private const long MaxChargePerRequestSeconds = 8;
    public ListeningService(AppDbContext db) => _db = db;

    public async Task<ListeningSnapshot?> StartAsync(long visitorId, long shopId)
    {
        if (visitorId <= 0 || shopId <= 0 || !await _db.Shops.AnyAsync(x => x.Id == shopId)) return null;
        await using var tx = await _db.Database.BeginTransactionAsync();
        var balance = await _db.ListeningBalances.FirstOrDefaultAsync(x => x.AnonymousVisitorId == visitorId);
        if (balance == null || balance.RemainingSeconds <= 0) return null;
        var now = DateTime.UtcNow;
        var previous = await _db.ListeningSessions
            .FirstOrDefaultAsync(x => x.AnonymousVisitorId == visitorId && x.Status != ListeningSessionStatus.Ended);
        if (previous != null)
        {
            Charge(previous, balance, now);
            previous.Status = ListeningSessionStatus.Ended;
            previous.EndedAt = now;
            await _db.SaveChangesAsync();
        }
        if (balance.RemainingSeconds <= 0)
        {
            await tx.CommitAsync();
            return null;
        }
        var session = new ListeningSession
        {
            AnonymousVisitorId = visitorId,
            ShopId = shopId,
            Status = ListeningSessionStatus.Playing,
            StartedAt = now,
            LastHeartbeatAt = now
        };
        _db.ListeningSessions.Add(session);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return Snapshot(session, balance);
    }

    public async Task<ListeningSnapshot?> UpdateAsync(long visitorId, long sessionId, string action)
    {
        if (visitorId <= 0 || sessionId <= 0 ||
            action != "heartbeat" && action != "pause" && action != "resume" && action != "end")
            return null;
        await using var tx = await _db.Database.BeginTransactionAsync();
        var session = await _db.ListeningSessions.FirstOrDefaultAsync(x => x.Id == sessionId && x.AnonymousVisitorId == visitorId);
        var balance = await _db.ListeningBalances.FirstOrDefaultAsync(x => x.AnonymousVisitorId == visitorId);
        if (session == null || balance == null) return null;
        if (session.Status == ListeningSessionStatus.Ended)
            return Snapshot(session, balance);
        var now = DateTime.UtcNow;
        if (session.Status == ListeningSessionStatus.Playing) Charge(session, balance, now);
        if (balance.RemainingSeconds <= 0 || action == "end")
        {
            session.Status = ListeningSessionStatus.Ended;
            session.EndedAt = now;
        }
        else if (action == "pause") session.Status = ListeningSessionStatus.Paused;
        else if (action == "resume") session.Status = ListeningSessionStatus.Playing;
        if (action == "resume" || session.Status != ListeningSessionStatus.Playing)
            session.LastHeartbeatAt = now;
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return Snapshot(session, balance);
    }

    private void Charge(ListeningSession session, ListeningBalance balance, DateTime now)
    {
        long elapsed = (long)Math.Floor(Math.Max(0, (now - session.LastHeartbeatAt).TotalSeconds));
        long seconds = Math.Min(Math.Min(elapsed, MaxChargePerRequestSeconds), balance.RemainingSeconds);
        if (seconds <= 0) return;
        balance.RemainingSeconds -= seconds;
        balance.UpdatedAt = now;
        session.ChargedSeconds += seconds;
        session.LastHeartbeatAt = now;
        _db.ListeningTransactions.Add(new ListeningTransaction
        {
            AnonymousVisitorId = session.AnonymousVisitorId,
            ListeningSession = session,
            Type = ListeningTransactionType.ListeningDebit,
            ChangeSeconds = -seconds,
            BalanceAfterSeconds = balance.RemainingSeconds,
            CreatedAt = now
        });
    }

    private static ListeningSnapshot Snapshot(ListeningSession s, ListeningBalance b) =>
        new(s.Id, b.RemainingSeconds, s.Status.ToString());
}
