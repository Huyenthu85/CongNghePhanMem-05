
using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Data;
using ThuyetMinh.Data.Models;
namespace ThuyetMinh.Business.Services;
public interface IPaymentConfirmationService
{
    Task<bool> ConfirmAsync(string orderCode, string providerTransactionId, long paidAmountVnd, long? creditedVisitorId = null);
}
public class PaymentConfirmationService : IPaymentConfirmationService
{
    private readonly AppDbContext _db;
    public PaymentConfirmationService(AppDbContext db)
    {
        _db = db;
    }
    public async Task<bool> ConfirmAsync(string orderCode, string providerTransactionId, long paidAmountVnd, long? creditedVisitorId = null)
    {
        if (string.IsNullOrWhiteSpace(orderCode) || string.IsNullOrWhiteSpace(providerTransactionId))
            return false;
        await using var transaction = await _db.Database.BeginTransactionAsync();
        var order = await _db.PaymentOrders.FirstOrDefaultAsync(x => x.OrderCode == orderCode);
        if (order == null)
            return false;
        if (order.Status == PaymentStatus.Paid && order.GrantedAt != null)
            return order.ProviderTransactionId == providerTransactionId;
        if (order.Status != PaymentStatus.Pending || order.GrantedAt != null)
            return false;
        if (order.ExpiresAt <= DateTime.UtcNow || paidAmountVnd != order.AmountVnd)
            return false;
        if (await _db.PaymentOrders.AnyAsync(x => x.ProviderTransactionId == providerTransactionId && x.Id != order.Id))
            return false;
        // Listening credit belongs to the browser that created the order and displayed the QR.
        // Reject attempts to transfer the credit to a different visitor.
        if (creditedVisitorId.HasValue && creditedVisitorId.Value != order.AnonymousVisitorId)
            return false;
        long receiverId = order.AnonymousVisitorId;
        if (receiverId <= 0 || !await _db.AnonymousVisitors.AnyAsync(v => v.Id == receiverId && v.IsActive))
            return false;
        var balance = await _db.ListeningBalances
            .FirstOrDefaultAsync(x => x.AnonymousVisitorId == receiverId);
        if (balance == null)
        {
            balance = new ListeningBalance
            {
                AnonymousVisitorId = receiverId,
                RemainingSeconds = 0
            };
            _db.ListeningBalances.Add(balance);
        }
        var now = DateTime.UtcNow;
        long newBalance = checked(balance.RemainingSeconds + order.GrantedSeconds);
        balance.RemainingSeconds = newBalance;
        balance.UpdatedAt = now;
        order.Status = PaymentStatus.Paid;
        order.PaidAt = now;
        order.GrantedAt = now;
        order.ProviderTransactionId = providerTransactionId;
        _db.ListeningTransactions.Add(new ListeningTransaction
        {
            AnonymousVisitorId = receiverId,
            Type = ListeningTransactionType.PaymentCredit,
            ChangeSeconds = order.GrantedSeconds,
            BalanceAfterSeconds = newBalance,
            PaymentOrder = order,
            CreatedAt = now
        });
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
    }
}
