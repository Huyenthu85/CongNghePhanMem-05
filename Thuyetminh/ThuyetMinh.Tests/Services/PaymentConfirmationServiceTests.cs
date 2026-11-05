
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Business.Services;
using ThuyetMinh.Data;
using ThuyetMinh.Data.Models;
using Xunit;
namespace ThuyetMinh.Tests.Services;

public class PaymentConfirmationServiceTests
{
    private static async Task<(SqliteConnection Connection, AppDbContext Db)> CreateDatabaseAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();
        return (connection, db);
    }
    private static async Task<PaymentOrder> CreateOrderAsync(AppDbContext db)
    {
        var visitor = new AnonymousVisitor
        {
            TokenHash = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow,
            IsActive = true
        };
        db.AnonymousVisitors.Add(visitor);
        await db.SaveChangesAsync();
        var order = new PaymentOrder
        {
            OrderCode = "TM" + Guid.NewGuid().ToString("N"),
            AnonymousVisitorId = visitor.Id,
            AmountVnd = 20000,
            GrantedSeconds = 900,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };
        db.PaymentOrders.Add(order);
        await db.SaveChangesAsync();
        return order;
    }
    [Fact]
    public async Task ConfirmAsync_ValidPayment_Adds900Seconds()
    {
        var setup = await CreateDatabaseAsync();
        await using var connection = setup.Connection;
        await using var db = setup.Db;
        var order = await CreateOrderAsync(db);
        var service = new PaymentConfirmationService(db);
        bool result = await service.ConfirmAsync(order.OrderCode, "BANK-001", 20000);
        Assert.True(result);
        db.ChangeTracker.Clear();
        var balance = await db.ListeningBalances.SingleAsync();
        var savedOrder = await db.PaymentOrders.SingleAsync();
        var transaction = await db.ListeningTransactions.SingleAsync();
        Assert.Equal(900L, balance.RemainingSeconds);
        Assert.Equal(PaymentStatus.Paid, savedOrder.Status);
        Assert.NotNull(savedOrder.GrantedAt);
        Assert.Equal(900L, transaction.ChangeSeconds);
        Assert.Equal(ListeningTransactionType.PaymentCredit, transaction.Type);
    }
    [Fact]
    public async Task ConfirmAsync_SameTransaction_DoesNotAddTwice()
    {
        var setup = await CreateDatabaseAsync();
        await using var connection = setup.Connection;
        await using var db = setup.Db;
        var order = await CreateOrderAsync(db);
        var service = new PaymentConfirmationService(db);
        bool first = await service.ConfirmAsync(order.OrderCode, "BANK-002", 20000);
        bool second = await service.ConfirmAsync(order.OrderCode, "BANK-002", 20000);
        Assert.True(first);
        Assert.True(second);
        db.ChangeTracker.Clear();
        var balance = await db.ListeningBalances.SingleAsync();
        int transactionCount = await db.ListeningTransactions.CountAsync();
        Assert.Equal(900L, balance.RemainingSeconds);
        Assert.Equal(1, transactionCount);
    }
    [Fact]
    public async Task ConfirmAsync_WrongAmount_DoesNotAddSeconds()
    {
        var setup = await CreateDatabaseAsync();
        await using var connection = setup.Connection;
        await using var db = setup.Db;
        var order = await CreateOrderAsync(db);
        var service = new PaymentConfirmationService(db);
        bool result = await service.ConfirmAsync(order.OrderCode, "BANK-003", 15000);
        Assert.False(result);
        db.ChangeTracker.Clear();
        var savedOrder = await db.PaymentOrders.SingleAsync();
        Assert.Equal(PaymentStatus.Pending, savedOrder.Status);
        Assert.Empty(await db.ListeningTransactions.ToListAsync());
        Assert.Empty(await db.ListeningBalances.ToListAsync());
    }
    [Fact]
    public async Task ConfirmAsync_ReusedProviderTransaction_IsRejected()
    {
        var setup = await CreateDatabaseAsync();
        await using var connection = setup.Connection;
        await using var db = setup.Db;
        var firstOrder = await CreateOrderAsync(db);
        var secondOrder = await CreateOrderAsync(db);
        var service = new PaymentConfirmationService(db);
        bool first = await service.ConfirmAsync(firstOrder.OrderCode, "BANK-004", 20000);
        bool second = await service.ConfirmAsync(secondOrder.OrderCode, "BANK-004", 20000);
        Assert.True(first);
        Assert.False(second);
        db.ChangeTracker.Clear();
        var savedSecondOrder = await db.PaymentOrders.SingleAsync(x => x.Id == secondOrder.Id);
        Assert.Equal(PaymentStatus.Pending, savedSecondOrder.Status);
        Assert.Equal(1, await db.ListeningTransactions.CountAsync());
    }

    [Fact]
    public async Task ConfirmAsync_ExpiredOrder_IsRejected()
    {
        var setup = await CreateDatabaseAsync();
        await using var connection = setup.Connection;
        await using var db = setup.Db;
        var order = await CreateOrderAsync(db);
        order.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        await db.SaveChangesAsync();
        var service = new PaymentConfirmationService(db);
        bool result = await service.ConfirmAsync(order.OrderCode, "BANK-005", 20000);
        Assert.False(result);
        db.ChangeTracker.Clear();
        var savedOrder = await db.PaymentOrders.SingleAsync();
        Assert.Equal(PaymentStatus.Pending, savedOrder.Status);
        Assert.Empty(await db.ListeningTransactions.ToListAsync());
        Assert.Empty(await db.ListeningBalances.ToListAsync());
    }
    [Fact]
    public async Task ConfirmAsync_DifferentTransactionForPaidOrder_IsRejected()
    {
        var setup = await CreateDatabaseAsync();
        await using var connection = setup.Connection;
        await using var db = setup.Db;
        var order = await CreateOrderAsync(db);
        var service = new PaymentConfirmationService(db);
        bool first = await service.ConfirmAsync(order.OrderCode, "BANK-006", 20000);
        bool second = await service.ConfirmAsync(order.OrderCode, "BANK-007", 20000);
        Assert.True(first);
        Assert.False(second);
        db.ChangeTracker.Clear();
        var balance = await db.ListeningBalances.SingleAsync();
        Assert.Equal(900L, balance.RemainingSeconds);
        Assert.Equal(1, await db.ListeningTransactions.CountAsync());
    }
    [Fact]
    public async Task ConfirmAsync_UnknownOrder_IsRejected()
    {
        var setup = await CreateDatabaseAsync();
        await using var connection = setup.Connection;
        await using var db = setup.Db;
        var service = new PaymentConfirmationService(db);
        bool result = await service.ConfirmAsync("TM-NOT-EXIST", "BANK-008", 20000);
        Assert.False(result);
        Assert.Empty(await db.ListeningBalances.ToListAsync());
        Assert.Empty(await db.ListeningTransactions.ToListAsync());
    }

}
