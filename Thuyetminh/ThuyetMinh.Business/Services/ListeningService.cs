using System.Data;
using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Data;
using ThuyetMinh.Data.Models;

namespace ThuyetMinh.Business.Services;

public sealed record ListeningSnapshot(long SessionId, long RemainingSeconds, string Status, long ChargedSeconds);

public interface IListeningService
{
    Task<ListeningSnapshot?> StartAsync(long visitorId, long shopId, int? audioId = null);
    Task<ListeningSnapshot?> UpdateAsync(long visitorId, long sessionId, string action, long? playedSeconds = null);
}

public sealed class ListeningService : IListeningService
{
    private readonly AppDbContext _db;
    private const long MaxChargePerRequestSeconds = 8;
    public const int MaxHeartbeatAgeSeconds = 25;
    public ListeningService(AppDbContext db) => _db = db;

    public async Task<ListeningSnapshot?> StartAsync(long visitorId, long shopId, int? audioId = null)
    {
        if (visitorId <= 0 || shopId <= 0 || !await _db.Shops.AnyAsync(x => x.Id == shopId)) return null;
        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var balance = await _db.ListeningBalances.FirstOrDefaultAsync(x => x.AnonymousVisitorId == visitorId);
        if (balance == null) return null;
        var now = DateTime.UtcNow;
        var previousSessions = await _db.ListeningSessions
            .Where(x => x.AnonymousVisitorId == visitorId &&
                x.Status != ListeningSessionStatus.Ended)
            .ToListAsync();

        foreach (var previous in previousSessions)
        {
            previous.Status = ListeningSessionStatus.Ended;
            previous.EndedAt = now;
            previous.LastHeartbeatAt = now;
        }

        if (previousSessions.Count > 0)
            await _db.SaveChangesAsync();
        if (balance.RemainingSeconds <= 0)
        {
            await tx.CommitAsync();
            return null;
        }
        var session = new ListeningSession
        {
            AnonymousVisitorId = visitorId,
            ShopId = shopId,
            AudioId = audioId,
            Status = ListeningSessionStatus.Playing,
            StartedAt = now,
            LastHeartbeatAt = now
        };
        _db.ListeningSessions.Add(session);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return Snapshot(session, balance);
    }

    public async Task<ListeningSnapshot?> UpdateAsync(long visitorId, long sessionId, string action, long? playedSeconds = null)
    {
        if (visitorId <= 0 || sessionId <= 0 || playedSeconds is < 0 ||
            action != "heartbeat" && action != "pause" && action != "resume" && action != "end")
            return null;
        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var session = await _db.ListeningSessions.FirstOrDefaultAsync(x => x.Id == sessionId && x.AnonymousVisitorId == visitorId);
        var balance = await _db.ListeningBalances.FirstOrDefaultAsync(x => x.AnonymousVisitorId == visitorId);
        if (session == null || balance == null) return null;
        if (session.Status == ListeningSessionStatus.Ended)
            return Snapshot(session, balance);
        var now = DateTime.UtcNow;
        bool expired = session.Status == ListeningSessionStatus.Playing &&
            session.LastHeartbeatAt < now.AddSeconds(-MaxHeartbeatAgeSeconds);
        if (session.Status == ListeningSessionStatus.Playing && !expired)
        {
            if (session.AudioId != null && playedSeconds.HasValue)
            {
                ChargeReportedPlayback(
                    session, balance, now, playedSeconds.Value);
            }
            else if (session.AudioId == null && action == "heartbeat")
            {
                Charge(session, balance, now);
            }
        }
        if (balance.RemainingSeconds <= 0 || action == "end" || expired)
        {
            session.Status = ListeningSessionStatus.Ended;
            session.EndedAt = now;
        }
        else if (action == "pause") session.Status = ListeningSessionStatus.Paused;
        else if (action == "resume") session.Status = ListeningSessionStatus.Playing;
        if (session.AudioId != null || action == "resume" || session.Status != ListeningSessionStatus.Playing)
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
        session.LastHeartbeatAt = now;
        Debit(session, balance, now, seconds);
    }

    // The client reports cumulative whole seconds of actual playback, not its seek position.
    // The persisted charged count makes retries idempotent; server time bounds each increase.
    private void ChargeReportedPlayback(ListeningSession session, ListeningBalance balance, DateTime now, long reported)
    {
        var totalLimit = (long)Math.Ceiling(Math.Max(0, (now - session.StartedAt).TotalSeconds));
        var intervalLimit = (long)Math.Ceiling(Math.Max(0, (now - session.LastHeartbeatAt).TotalSeconds)) + 2;
        long delta = Math.Max(0, Math.Min(reported, totalLimit) - session.ChargedSeconds);
        long seconds = Math.Min(Math.Min(delta, intervalLimit), balance.RemainingSeconds);
        if (seconds > 0) Debit(session, balance, now, seconds);
    }

    private void Debit(ListeningSession session, ListeningBalance balance, DateTime now, long seconds)
    {
        balance.RemainingSeconds -= seconds;
        balance.UpdatedAt = now;
        session.ChargedSeconds += seconds;
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
        new(s.Id, b.RemainingSeconds, s.Status.ToString(), s.ChargedSeconds);
}
