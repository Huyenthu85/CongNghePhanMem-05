
namespace ThuyetMinh.Data.Models;
public class PaymentOrder
{
    public long Id { get; set; }
    public string OrderCode { get; set; } = null!;
    public long AnonymousVisitorId { get; set; }
    public AnonymousVisitor AnonymousVisitor { get; set; } = null!;
    public long AmountVnd { get; set; }
    public long GrantedSeconds { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? GrantedAt { get; set; }
    public string? ProviderTransactionId { get; set; }
}
