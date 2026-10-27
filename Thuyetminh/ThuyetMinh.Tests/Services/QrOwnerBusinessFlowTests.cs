using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Business.Services;
using ThuyetMinh.Data;
using ThuyetMinh.Data.Models;
using Xunit;

namespace ThuyetMinh.Tests.Services;

public class QrOwnerBusinessFlowTests
{
    private static async Task<(SqliteConnection, AppDbContext)> Setup()
    {
        var cn = new SqliteConnection("Data Source=:memory:");
        await cn.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(cn).Options;
        var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();
        return (cn, db);
    }

    private static async Task<long> Visitor(AppDbContext db)
    {
        var v = new AnonymousVisitor
        {
            TokenHash = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow,
            IsActive = true
        };
        db.AnonymousVisitors.Add(v);
        await db.SaveChangesAsync();
        return v.Id;
    }

    private static async Task<string> Order(AppDbContext db, long owner)
    {
        var o = new PaymentOrder
        {
            OrderCode = "TM" + Guid.NewGuid().ToString("N"),
            AnonymousVisitorId = owner,
            AmountVnd = 20000,
            GrantedSeconds = 900,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };
        db.PaymentOrders.Add(o);
        await db.SaveChangesAsync();
        return o.OrderCode;
    }

    [Fact]
    public async Task TC01_MayTaoQrNhan900Giay()
    {
        var (cn, db) = await Setup();
        await using var c = cn;
        await using var d = db;
        var owner = await Visitor(db);
        var scanner = await Visitor(db);
        var code = await Order(db, owner);
        var service = new PaymentConfirmationService(db);
        Assert.True(await service.ConfirmAsync(code, "DEMO-" + Guid.NewGuid(), 20000));
        db.ChangeTracker.Clear();
        var credit = await db.ListeningTransactions.SingleAsync();
        Assert.Equal(owner, credit.AnonymousVisitorId);
        Assert.NotEqual(scanner, credit.AnonymousVisitorId);
        Assert.Equal(900L, credit.ChangeSeconds);
        Assert.Equal(900L, (await db.ListeningBalances.SingleAsync()).RemainingSeconds);
    }

    [Fact]
    public async Task TC02_KhongDuocChuyen900GiaySangMayQuet()
    {
        var (cn, db) = await Setup();
        await using var c = cn;
        await using var d = db;
        var owner = await Visitor(db);
        var scanner = await Visitor(db);
        Assert.NotEqual(owner, scanner);
        var code = await Order(db, owner);
        var savedOrder = await db.PaymentOrders.SingleAsync(x => x.OrderCode == code);
        Assert.Equal(owner, savedOrder.AnonymousVisitorId);
        Assert.Equal(PaymentStatus.Pending, savedOrder.Status);
        var service = new PaymentConfirmationService(db);
        Assert.False(await service.ConfirmAsync(
            code, "DEMO-" + Guid.NewGuid(), 20000, scanner));
        Assert.Empty(await db.ListeningTransactions.ToListAsync());
        Assert.Empty(await db.ListeningBalances.ToListAsync());
    }

    [Fact]
    public async Task TC03_XacNhanLaiKhongCongTrung()
    {
        var (cn, db) = await Setup();
        await using var c = cn;
        await using var d = db;
        var owner = await Visitor(db);
        var code = await Order(db, owner);
        var service = new PaymentConfirmationService(db);
        var transaction = "DEMO-" + Guid.NewGuid();
        Assert.True(await service.ConfirmAsync(code, transaction, 20000));
        Assert.True(await service.ConfirmAsync(code, transaction, 20000));
        db.ChangeTracker.Clear();
        Assert.Equal(900L, (await db.ListeningBalances.SingleAsync()).RemainingSeconds);
        Assert.Equal(1, await db.ListeningTransactions.CountAsync());
    }

    [Fact]
    public async Task TC04_HaiDonHopLeDuocCong1800Giay()
    {
        var (cn, db) = await Setup();
        await using var c = cn;
        await using var d = db;
        var owner = await Visitor(db);
        var first = await Order(db, owner);
        var second = await Order(db, owner);
        var service = new PaymentConfirmationService(db);
        Assert.True(await service.ConfirmAsync(first, "DEMO-" + Guid.NewGuid(), 20000));
        Assert.True(await service.ConfirmAsync(second, "DEMO-" + Guid.NewGuid(), 20000));
        db.ChangeTracker.Clear();
        Assert.Equal(1800L, (await db.ListeningBalances.SingleAsync()).RemainingSeconds);
        Assert.Equal(2, await db.ListeningTransactions.CountAsync());
    }
}
