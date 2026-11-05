
namespace ThuyetMinh.Data.Models;
public class ListeningTransaction
{
    public long Id { get; set; }
    public long AnonymousVisitorId { get; set; }
    public AnonymousVisitor AnonymousVisitor { get; set; } = null!;
    public ListeningTransactionType Type { get; set; }
    public long ChangeSeconds { get; set; }
    public long BalanceAfterSeconds { get; set; }
    public long? PaymentOrderId { get; set; }
    public PaymentOrder? PaymentOrder { get; set; }
    public long? ListeningSessionId { get; set; }
    public ListeningSession? ListeningSession { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
