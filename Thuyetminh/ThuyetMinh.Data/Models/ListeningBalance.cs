
namespace ThuyetMinh.Data.Models;
public class ListeningBalance
{
    public long Id { get; set; }
    public long AnonymousVisitorId { get; set; }
    public AnonymousVisitor AnonymousVisitor { get; set; } = null!;
    public long RemainingSeconds { get; set; } = 0;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
