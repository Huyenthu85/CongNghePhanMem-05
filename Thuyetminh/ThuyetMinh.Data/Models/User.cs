namespace ThuyetMinh.Data.Models;

public class User
{
    public long Id { get; set; }
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string? PhoneNumber { get; set; } // Số điện thoại
public string? Address { get; set; }     // Địa chỉ nhà thực
    public Role Role { get; set; }
    public string? FullName { get; set; }
    public bool Active { get; set; } = true;

    public ICollection<Shop> Shops { get; set; } = new List<Shop>();
}
