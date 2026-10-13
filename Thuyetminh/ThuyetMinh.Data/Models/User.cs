namespace ThuyetMinh.Data.Models;

public class User
{
    public long Id { get; set; }
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public Role Role { get; set; }
    public string? FullName { get; set; }
    public bool Active { get; set; } = true;

    public ICollection<Shop> Shops { get; set; } = new List<Shop>();
}
