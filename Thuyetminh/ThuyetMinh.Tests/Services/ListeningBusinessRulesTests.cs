using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Business.Services;
using ThuyetMinh.Data;
using ThuyetMinh.Data.Models;
using Xunit;
namespace ThuyetMinh.Tests.Services;
public class ListeningBusinessRulesTests
{
    private static async Task<(SqliteConnection, AppDbContext, long, long, long)> Setup(long seconds)
    {
        var cn = new SqliteConnection("Data Source=:memory:");
        await cn.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(cn).Options;
        var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();
        var owner = new User { Username = "test-owner", PasswordHash = "test", Role = Role.Owner };
        db.Users.Add(owner);
        await db.SaveChangesAsync();
        var shop = new Shop { OwnerId = owner.Id, Name = "Test Shop", Address = "Demo", PublishStatus = PublishStatus.Approved };
        var a = new AnonymousVisitor { TokenHash = Guid.NewGuid().ToString("N") };
        var b = new AnonymousVisitor { TokenHash = Guid.NewGuid().ToString("N") };
        db.Shops.Add(shop);
        db.AnonymousVisitors.AddRange(a, b);
        await db.SaveChangesAsync();
        db.ListeningBalances.AddRange(
            new ListeningBalance { AnonymousVisitorId = a.Id, RemainingSeconds = seconds },
            new ListeningBalance { AnonymousVisitorId = b.Id, RemainingSeconds = 0 }
        );
        await db.SaveChangesAsync();
        return (cn, db, a.Id, b.Id, shop.Id);
    }
    [Fact]
    public async Task TC05_ThietBiKhacKhongDuocDieuKhienPhien()
    {
        var (cn, db, a, b, shop) = await Setup(900);
        await using var c = cn;
        await using var d = db;
        var service = new ListeningService(db);
        var started = await service.StartAsync(a, shop);
        Assert.NotNull(started);
        var result = await service.UpdateAsync(b, started.SessionId, "end");
        Assert.Null(result);
        var session = await db.ListeningSessions.SingleAsync();
        Assert.Equal(ListeningSessionStatus.Playing, session.Status);
        Assert.Equal(900L, (await db.ListeningBalances
            .SingleAsync(x => x.AnonymousVisitorId == b)).RemainingSeconds == 0
            ? (await db.ListeningBalances.SingleAsync(x => x.AnonymousVisitorId == a)).RemainingSeconds
            : -1L);
    }
    [Fact]
    public async Task TC06_PhiênMoiKetThucPhienCu()
    {
        var (cn, db, a, _, shop) = await Setup(900);
        await using var c = cn;
        await using var d = db;
        var service = new ListeningService(db);
        var first = await service.StartAsync(a, shop);
        Assert.NotNull(first);
        var second = await service.StartAsync(a, shop);
        Assert.NotNull(second);
        Assert.NotEqual(first.SessionId, second.SessionId);
        Assert.Equal(2, await db.ListeningSessions.CountAsync());
        Assert.Equal(1, await db.ListeningSessions
            .CountAsync(x => x.Status != ListeningSessionStatus.Ended));
        Assert.Equal(ListeningSessionStatus.Ended,
            (await db.ListeningSessions.SingleAsync(x => x.Id == first.SessionId)).Status);
    }
    [Fact]
    public async Task TC07_TamDungKhongTruGiayVaTiepTuc()
    {
        var (cn, db, a, _, shop) = await Setup(900);
        await using var c = cn;
        await using var d = db;
        var service = new ListeningService(db);
        var started = await service.StartAsync(a, shop);
        Assert.NotNull(started);
        var session = await db.ListeningSessions.SingleAsync();
        session.LastHeartbeatAt = DateTime.UtcNow.AddSeconds(-4);
        await db.SaveChangesAsync();
        var paused = await service.UpdateAsync(a, session.Id, "pause");
        Assert.NotNull(paused);
        Assert.Equal("Paused", paused.Status);
        long balanceAtPause = paused.RemainingSeconds;
        session.LastHeartbeatAt = DateTime.UtcNow.AddSeconds(-30);
        await db.SaveChangesAsync();
        var duringPause = await service.UpdateAsync(a, session.Id, "heartbeat");
        Assert.NotNull(duringPause);
        Assert.Equal(balanceAtPause, duringPause.RemainingSeconds);
        var resumed = await service.UpdateAsync(a, session.Id, "resume");
        Assert.NotNull(resumed);
        Assert.Equal("Playing", resumed.Status);
        Assert.Equal(balanceAtPause, resumed.RemainingSeconds);
        session.LastHeartbeatAt = DateTime.UtcNow.AddSeconds(-3);
        await db.SaveChangesAsync();
        var heartbeat = await service.UpdateAsync(a, session.Id, "heartbeat");
        Assert.NotNull(heartbeat);
        Assert.True(heartbeat.RemainingSeconds < balanceAtPause);
    }
    [Fact]
    public async Task TC08_HetThoiLuongKhongDuocTruAm()
    {
        var (cn, db, a, _, shop) = await Setup(3);
        await using var c = cn;
        await using var d = db;
        var service = new ListeningService(db);
        var started = await service.StartAsync(a, shop);
        Assert.NotNull(started);
        var session = await db.ListeningSessions.SingleAsync();
        session.LastHeartbeatAt = DateTime.UtcNow.AddSeconds(-20);
        await db.SaveChangesAsync();
        var result = await service.UpdateAsync(a, session.Id, "heartbeat");
        Assert.NotNull(result);
        Assert.Equal(0L, result.RemainingSeconds);
        Assert.Equal("Ended", result.Status);
        var again = await service.UpdateAsync(a, session.Id, "heartbeat");
        Assert.NotNull(again);
        Assert.Equal(0L, again.RemainingSeconds);
        Assert.Equal(3L, (await db.ListeningSessions.SingleAsync()).ChargedSeconds);
        Assert.Equal(-3L, (await db.ListeningTransactions.SingleAsync()).ChangeSeconds);
    }
}
