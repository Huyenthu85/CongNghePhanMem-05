using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Business.Services;
using ThuyetMinh.Data;
using ThuyetMinh.Data.Models;
using Xunit;

namespace ThuyetMinh.Tests.Services;

public class ListeningServiceTests
{
    private static async Task<(SqliteConnection Connection, AppDbContext Db, long VisitorId, long ShopId)> SetupAsync(long seconds)
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();
        var owner = new User { Username = "demo-owner", PasswordHash = "test", Role = Role.Owner };
        db.Users.Add(owner);
        await db.SaveChangesAsync();
        var shop = new Shop { OwnerId = owner.Id, Name = "Địa điểm mẫu", Address = "Demo", PublishStatus = PublishStatus.Approved };
        var visitor = new AnonymousVisitor { TokenHash = Guid.NewGuid().ToString("N") };
        db.Shops.Add(shop);
        db.AnonymousVisitors.Add(visitor);
        await db.SaveChangesAsync();
        db.ListeningBalances.Add(new ListeningBalance { AnonymousVisitorId = visitor.Id, RemainingSeconds = seconds });
        await db.SaveChangesAsync();
        return (connection, db, visitor.Id, shop.Id);
    }

    [Fact]
    public async Task StartAsync_NoBalance_DoesNotStart()
    {
        var test = await SetupAsync(0);
        await using var connection = test.Connection;
        await using var db = test.Db;
        var service = new ListeningService(db);
        Assert.Null(await service.StartAsync(test.VisitorId, test.ShopId));
        Assert.Equal(0, await db.ListeningSessions.CountAsync());
    }

    [Fact]
    public async Task Heartbeat_DebitsOnlyElapsedServerTime_AndPauseStopsCharging()
    {
        var test = await SetupAsync(900);
        await using var connection = test.Connection;
        await using var db = test.Db;
        var service = new ListeningService(db);
        var started = await service.StartAsync(test.VisitorId, test.ShopId);
        Assert.NotNull(started);
        var session = await db.ListeningSessions.SingleAsync();
        session.LastHeartbeatAt = DateTime.UtcNow.AddSeconds(-4);
        await db.SaveChangesAsync();
        var heartbeat = await service.UpdateAsync(test.VisitorId, session.Id, "heartbeat");
        Assert.NotNull(heartbeat);
        Assert.InRange(heartbeat.RemainingSeconds, 892L, 899L);
        var paused = await service.UpdateAsync(test.VisitorId, session.Id, "pause");
        Assert.NotNull(paused);
        var afterPause = paused.RemainingSeconds;
        session.LastHeartbeatAt = DateTime.UtcNow.AddSeconds(-20);
        await db.SaveChangesAsync();
        var whilePaused = await service.UpdateAsync(test.VisitorId, session.Id, "heartbeat");
        Assert.NotNull(whilePaused);
        Assert.Equal(afterPause, whilePaused.RemainingSeconds);
        var ledger = await db.ListeningTransactions.ToListAsync();
        Assert.Contains(ledger, x => x.Type == ListeningTransactionType.ListeningDebit && x.ChangeSeconds < 0);
    }
}
