
namespace ThuyetMinh.Data.Models;

public class AnonymousVisitor
{
    public long Id { get; set; }
    public string TokenHash { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
