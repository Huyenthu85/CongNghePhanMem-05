
namespace ThuyetMinh.Data.Models;
public class ListeningSession
{
    public long Id { get; set; }
    public long AnonymousVisitorId { get; set; }
    public AnonymousVisitor AnonymousVisitor { get; set; } = null!;
    public long ShopId { get; set; }
    public Shop Shop { get; set; } = null!;
    public int? AudioId { get; set; }
    public Audio? Audio { get; set; }
    public ListeningSessionStatus Status { get; set; } = ListeningSessionStatus.Playing;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastHeartbeatAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public long ChargedSeconds { get; set; } = 0;
}
