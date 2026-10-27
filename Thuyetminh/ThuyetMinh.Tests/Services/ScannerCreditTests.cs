using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Business.Services;
using ThuyetMinh.Data;
using ThuyetMinh.Data.Models;
using Xunit;

namespace ThuyetMinh.Tests.Services;

public class ScannerCreditTests
{
    [Fact]
    public async Task QrDemo_CreditsScannerAndNotLaptop_ItsOrderIsStillPaidOnce()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        await using var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();
        var now = DateTime.UtcNow;
        var laptop = new AnonymousVisitor { TokenHash = Guid.NewGuid().ToString("N"), CreatedAt = now, LastSeenAt = now, IsActive = true };
        var phone = new AnonymousVisitor { TokenHash = Guid.NewGuid().ToString("N"), CreatedAt = now, LastSeenAt = now, IsActive = true };
        db.AnonymousVisitors.AddRange(laptop, phone);
        await db.SaveChangesAsync();
        db.ListeningBalances.Add(new ListeningBalance { AnonymousVisitorId = laptop.Id, RemainingSeconds = 0, UpdatedAt = now });
        var order = new PaymentOrder {
            OrderCode = "TM" + Guid.NewGuid().ToString("N"),
            AnonymousVisitorId = laptop.Id,
            AmountVnd = 20000,
            GrantedSeconds = 900,
            Status = PaymentStatus.Pending,
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(15)
        };
        db.PaymentOrders.Add(order);
        await db.SaveChangesAsync();
        var service = new PaymentConfirmationService(db);
        bool first = await service.ConfirmAsync(order.OrderCode, "DEMO-SCAN-001", 20000, phone.Id);
        Assert.True(first);
        bool second = await service.ConfirmAsync(order.OrderCode, "DEMO-SCAN-001", 20000, phone.Id);
        Assert.True(second);
        db.ChangeTracker.Clear();
        Assert.Equal(0L, (await db.ListeningBalances.SingleAsync(x => x.AnonymousVisitorId == laptop.Id)).RemainingSeconds);
        Assert.Equal(900L, (await db.ListeningBalances.SingleAsync(x => x.AnonymousVisitorId == phone.Id)).RemainingSeconds);
        var credit = await db.ListeningTransactions.SingleAsync();
        Assert.Equal(phone.Id, credit.AnonymousVisitorId);
        Assert.Equal(ListeningTransactionType.PaymentCredit, credit.Type);
        Assert.Equal(900L, credit.ChangeSeconds);
        Assert.Equal(PaymentStatus.Paid, (await db.PaymentOrders.SingleAsync()).Status);
    }
}